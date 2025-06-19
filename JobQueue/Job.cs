// Copyright (c) 2025 Erdem Bilgin
// 
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

    /// <summary>
    /// Event triggered when an error occurs in the job.
    /// </summary>
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

    /// <summary>
    /// Initializes a new instance of the Job class with the specified action.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the data context for the job.
    /// </summary>
    public IJobDataContext DataContext { get; set; }

    /// <summary>
    /// Gets or sets whether the job runs continuously.
    /// </summary>
    public bool Continously
    {
      get => _continously;
      set
      {
        CheckSetProperty();
        _continously = _workType == JobWorkType.Schedule || value;
      }
    }

    /// <summary>
    /// Gets or sets the type of work for the job.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the interval for scheduled jobs.
    /// </summary>
    public TimeSpan Interval
    {
      get => _interval;
      set
      {
        CheckSetProperty();
        _interval = value;
      }
    }

    /// <summary>
    /// Gets the cancellation token for the job.
    /// </summary>
    public CancellationToken CancellationToken => _cancellationTokenSource.Token;

    /// <summary>
    /// Gets the current state of the job.
    /// </summary>
    public JobState JobState => (JobState)State;

    /// <summary>
    /// Gets the number of jobs (always 1 for a single job).
    /// </summary>
    public int Count => 1;

    /// <summary>
    /// Gets or sets the unique identifier for the job.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// Starts the job.
    /// </summary>
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

    /// <summary>
    /// Cancels the job.
    /// </summary>
    public void Cancel()
    {
      _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Waits for the job to finish processing.
    /// </summary>
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

    /// <summary>
    /// Waits for the job to finish or for a timeout.
    /// </summary>
    public void Wait(TimeSpan timeSpan)
    {
      Monitor.Wait(_signal, timeSpan);
    }
     
    /// <summary>
    /// Suspends the job.
    /// </summary>
    public void Suspend()
    {
      Interlocked.CompareExchange(ref State, JobStateConst.SUSPEND, JobStateConst.RUNNING);
    }

    /// <summary>
    /// Resumes the job if suspended.
    /// </summary>
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

    /// <summary>
    /// Releases resources used by the job.
    /// </summary>
    public void Dispose()
    { 
      _nonExpcetionStrategy.Execute("JT.Dispose [Join]", Join);
      _nonExpcetionStrategy.Execute("JT.Dispose [Cancel.Dispose]", _cancellationTokenSource.Dispose);

      Interlocked.Exchange(ref State, JobStateConst.DISPOSED);
    }
  }
}