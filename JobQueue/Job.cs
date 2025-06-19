using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using JobQueue.Interfaces;
using JobQueue.Strategies;
 
namespace JobQueue
{
  [DebuggerDisplay(JobDebuggerDisplayKeys.Job)]
  public sealed class Job : IJobWork
  {
    private const int DefaultIntervalMs = 300;

    public event EventHandler<JobErrorArgs> Error;

    private readonly object _signal = new object();

    #region FIELDS

    private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    private readonly JobStart _jobStart;
    private readonly IExecutionStrategy _nonExpcetionStrategy;

    internal volatile int State = JobStateConst.NONE;

    private Thread _thread;
    private bool _continously;
    private JobWorkType _workType;
    private TimeSpan _interval;

    #endregion

    public Job(JobStart action)
    {
      Guard.NotNull(action, "OnAction");

      _jobStart = action;

      _nonExpcetionStrategy = new DefaultExecutionStrategy();
    }

    private void InternalStartAction(object data)
    {
      try
      {
        do
        {
          InternalAction(data);
          _cancellationTokenSource.Token.ThrowIfCancellationRequested();
        } while (!_cancellationTokenSource.IsCancellationRequested && _continously);
      }
      catch (OperationCanceledException opc)
      {
        Interlocked.Exchange(ref State, JobStateConst.CANCELLED);
        OnError(opc);
      }
      catch (ObjectDisposedException ode)
      {
        Interlocked.Exchange(ref State, JobStateConst.DISPOSED);
        OnError(ode);
      }
      catch (Exception ex)
      {
        OnError(ex);
      }
    }

    private void InternalAction(object data)
    {
      lock (_signal)
      {
        if (State == JobStateConst.SUSPEND)
          Monitor.Wait(_signal);

        if (_workType == JobWorkType.Schedule)
          Monitor.Wait(_signal, _interval);
      }

      _jobStart((IJobDataContext)data);
    }

    public IJobDataContext DataContext { get; set; }

    public bool Continously
    {
      get => _continously;
      set
      {
        CheckSetProperty();
        _continously = _workType == JobWorkType.Schedule || value;
      }
    }

    public JobWorkType WorkType
    {
      get => _workType;
      set
      {
        CheckSetProperty();
        _workType = value;

        if (_workType == JobWorkType.Schedule)
          _continously = true;
      }
    }

    public TimeSpan Interval
    {
      get => _interval;
      set
      {
        CheckSetProperty();
        _interval = value;
      }
    }

    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    public JobState JobState => (JobState)State;
    public int Count => 1;


    public string Key { get; set; }

    public void Start()
    {
      if (!CheckStart())
        return;

      if (_interval == TimeSpan.Zero && _workType == JobWorkType.Schedule)
        _interval = TimeSpan.FromMilliseconds(DefaultIntervalMs);

      _thread = new Thread(InternalStartAction)
      {
        IsBackground = true,
        Name = Key
      };

      _thread.Start(DataContext);
      
      JobManager.AddJob(Key, this);
    }

    public void Cancel()
    {
      _cancellationTokenSource.Cancel();
    }

    public void Join()
    {
      _nonExpcetionStrategy.Execute("PulseAll", () =>
      {
        lock (_signal)
        {
          Monitor.PulseAll(_signal);
        }
      });

      JobManager.DeleteJob(Key);
      _thread.Join(1000);
      Cancel();
    }

    public void Wait(TimeSpan timeSpan)
    {
      Monitor.Wait(_signal, timeSpan);
    }
     

    public void Suspend()
    {
      Interlocked.CompareExchange(ref State, JobStateConst.SUSPEND, JobStateConst.RUNNING);
    }

    public void Resume()
    {
      lock (_signal)
      {
        if (CheckAndSetRunning())
        {
          Monitor.Pulse(_signal);
        }
      }
      Interlocked.CompareExchange(ref State, JobStateConst.RUNNING, JobStateConst.SUSPEND);
    }

    private bool CheckAndSetRunning()
    {
      return Interlocked.Exchange(ref State, JobStateConst.RUNNING) == JobStateConst.SUSPEND;
    }
    private bool CheckStart()
    {
      return Interlocked.Exchange(ref State, JobStateConst.RUNNING) == JobStateConst.NONE;
    }

    private void CheckSetProperty([CallerMemberName] string propertyName = null)
    {
      if (State != JobStateConst.NONE)
      {
        OnError(new ArgumentException($"{propertyName} is error"));
      }
    }


    private void OnError(Exception exception)
    {
      Error?.Invoke(this, new JobErrorArgs(exception));
    }

    public void Dispose()
    { 
      _nonExpcetionStrategy.Execute("JT.Dispose [Join]", Join);
      _nonExpcetionStrategy.Execute("JT.Dispose [Cancel.Dispose]", _cancellationTokenSource.Dispose);

      Interlocked.Exchange(ref State, JobStateConst.DISPOSED);
    }
  }
}