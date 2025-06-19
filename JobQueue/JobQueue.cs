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
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using JobQueue.Interfaces;

namespace JobQueue
{

  [DebuggerDisplay(JobDebuggerDisplayKeys.JobQueueThread)]
  public sealed class JobQueue<T> : IJobQueue<T>
  {
    public string Key { get; set; }

    public event EventHandler<JobErrorArgs> Error;

    private readonly Job _job;

    private readonly Action<T> _queueAction;

    private readonly BlockingCollection<T> _blockingCollection = new BlockingCollection<T>();

    public JobQueue(string key, Action<T> queueAction)
    {
      _queueAction = queueAction;

      _job = (Job)JobBuilder.Create()
                                   .WithKey(key)
                                   .Continously(true)
                                   .OnAction(QueueCallback)
                                   .Build();
      Key = key;
      _job.Error += _job_Error;
    }

    private void _job_Error(object sender, JobErrorArgs e)
    {
      Error?.Invoke(sender, e);
    }

    private void QueueCallback(IJobDataContext jobdatacontext)
    {
      foreach (var element in _blockingCollection.GetConsumingEnumerable())
      {
        if (element == null)
          return;

        _queueAction(element);
      }

      //if (_blockingCollection.TryTake(out T element, TimeSpan.FromMilliseconds(1)))
      //{
      //  if (element == null)
      //    return;

      //  _queueAction(element);
      //}
    }

    public void Push(T data)
    {
      if (_job.State == JobStateConst.RUNNING)
      {
        _blockingCollection.Add(data);
      }
    }

    public void Push(T data, bool force)
    {
      if (force)
      {
        _blockingCollection.Add(data);
      }
      else
      {
        Push(data);
      }
    }
    public void Dispose()
    {
      try
      {
        _blockingCollection.CompleteAdding();
        _job.Dispose();
        GC.Collect();
      }
      catch
      {
        // ignored
      }
    }

    public JobState JobState => _job.JobState;

    public int Count
    {
      get
      {
        return _blockingCollection.Count;
      }
    }

    public void Start()
    {
      _job.Start();
      JobManager.AddJob(_job.Key, this);
    }

    public void Cancel()
    {
      _job.Cancel();

      _blockingCollection.CompleteAdding();
    }

    public void Resume()
    {
      _job.Resume();
    }

    public void Suspend()
    {
      _job.Suspend();
    }

    public void Join()
    {
      Cancel();
      _job.Join();
    }

    public void Wait(TimeSpan timeSpan)
    {
      _job.Wait(timeSpan);
      Thread.Yield();
    }

    public void Interrupt()
    {
      _job.Join();
    }
  }


  public sealed class JobQueue : IJobQueue
  {
    private readonly IJobQueue<object> _jobQueue;
    public JobQueue(string key, Action<object> queueAction)
    {
      Key = key;
      _jobQueue = new JobQueue<object>(key, queueAction);
      _jobQueue.Error += _jobQueue_Error;
    }

    private void _jobQueue_Error(object sender, JobErrorArgs e)
    {
      Error?.Invoke(sender, e);
    }

    public string Key { get; set; }
    public event EventHandler<JobErrorArgs> Error;
    public JobState JobState => _jobQueue.JobState;

    public int Count
    {
      get
      {
        return _jobQueue.Count;
      }
    }

    public void Start()
    {
      _jobQueue.Start();
    }

    public void Cancel()
    {
      _jobQueue.Cancel();
    }

    public void Resume()
    {
      _jobQueue.Resume();
    }

    public void Suspend()
    {
      _jobQueue.Suspend();
    }

    public void Join()
    {
      _jobQueue.Join();
    }

    public void Wait(TimeSpan timeSpan)
    {
      _jobQueue.Wait(timeSpan);
    }
     

    public void Push(object data)
    {
      _jobQueue.Push(data);
    }

    public void Push(object data, bool force)
    {
      _jobQueue.Push(data, force);
    }

    public void Dispose()
    {
      _jobQueue.Dispose();
    }
  }
}