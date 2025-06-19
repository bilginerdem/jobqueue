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