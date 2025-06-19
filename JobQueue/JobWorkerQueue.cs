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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JobQueue.Interfaces;

namespace JobQueue
{
  [DebuggerDisplay(JobDebuggerDisplayKeys.JobQueueThread)]
  public sealed class JobWorkerQueue<T> : IJobQueue<T>
  {
    private readonly int _workerSize;
    private readonly Action<T> _queueAction;
    /// <summary>
    /// Gets or sets the unique identifier for the worker queue.
    /// </summary>
    public string Key { get; set; }
    /// <summary>
    /// Event triggered when an error occurs in the queue.
    /// </summary>
    public event EventHandler<JobErrorArgs> Error;

    private readonly IList<IJob> _jobs;
    private readonly BlockingCollection<T> _blockingCollection = new BlockingCollection<T>();

    /// <summary>
    /// Initializes a new instance of the JobWorkerQueue class with the specified key, worker size, and action.
    /// </summary>
    public JobWorkerQueue(string key, int workerSize, Action<T> queueAction)
    {
      _workerSize = workerSize;
      _queueAction = queueAction;
      Key = key;
      _jobs = new List<IJob>(workerSize);
      for (var i = 0; i < workerSize; i++)
      {
        var jobContext = new JobDataContext();
        jobContext.SetData("index", i);

        var job = (Job)JobBuilder.Create()
                                     .WithKey($"{key}_w{i}")
                                     .Continously(true)
                                     .SetDataContext(jobContext)
                                     .OnAction(QueueCallback)
                                     .Build();
        job.Error += _job_Error;
        _jobs.Add(job);
      }
    }

    private void _job_Error(object sender, JobErrorArgs e)
    {
      Error?.Invoke(sender, e);
    }

    private void QueueCallback(IJobDataContext jobdatacontext)
    {
      foreach (var item in _blockingCollection.GetConsumingEnumerable())
      {
        _queueAction(item);
      }
    }

    /// <summary>
    /// Adds a job to the worker queue.
    /// </summary>
    public void Push(T data)
    {
      if (JobState == JobState.Running)
      {
        _blockingCollection.Add(data);
      }
    }

    /// <summary>
    /// Adds a job to the worker queue, with an option to force enqueue.
    /// </summary>
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

    /// <summary>
    /// Releases resources used by the worker queue.
    /// </summary>
    public void Dispose()
    {
      _blockingCollection.CompleteAdding();
      foreach (var job in _jobs)
      {
        job.Dispose();
      }
      GC.Collect();
    }

    /// <summary>
    /// Gets the current state of the worker queue.
    /// </summary>
    public JobState JobState => _jobs[0].JobState;

    /// <summary>
    /// Gets the number of jobs in the worker queue.
    /// </summary>
    public int Count
    {
      get
      {
        return _blockingCollection.Count;
      }
    }

    /// <summary>
    /// Starts processing jobs in the worker queue.
    /// </summary>
    public void Start()
    {
      foreach (var job in _jobs)
      {
        job.Start();
        JobManager.AddJob(job.Key, this);
      }
    }

    /// <summary>
    /// Cancels the worker queue.
    /// </summary>
    public void Cancel()
    {
      foreach (var job in _jobs)
      {
        job.Cancel();
        JobManager.DeleteJob(job.Key);
      }
      _blockingCollection.CompleteAdding();
    }

    /// <summary>
    /// Resumes the worker queue if suspended.
    /// </summary>
    public void Resume()
    {
      foreach (var job in _jobs)
      {
        job.Resume();
      }
    }

    /// <summary>
    /// Suspends the worker queue.
    /// </summary>
    public void Suspend()
    {
      foreach (var job in _jobs)
      {
        job.Suspend();
      }
    }

    /// <summary>
    /// Waits for the worker queue to finish processing.
    /// </summary>
    public void Join()
    {
      Cancel();
      foreach (var job in _jobs)
      {
        job.Join();
      }
    }

    /// <summary>
    /// Waits for the worker queue to finish or for a timeout.
    /// </summary>
    public void Wait(TimeSpan timeSpan)
    {
      IList<Task> tasks = new List<Task>(_jobs.Count);
      foreach (var job in _jobs)
      {
        tasks.Add(Task.Run(() => job.Wait(timeSpan)));
      }

      Task.WaitAll(tasks.ToArray());
    }

    /// <summary>
    /// Interrupts the worker queue.
    /// </summary>
    public void Interrupt()
    {
      foreach (var job in _jobs)
      {
        try
        {
          job.Join();
        }
        catch
        {
          //ignored
        }
      }
    }
  }
}