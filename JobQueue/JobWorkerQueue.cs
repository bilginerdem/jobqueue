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
    public string Key { get; set; }
    public event EventHandler<JobErrorArgs> Error;

    private readonly IList<IJob> _jobs;
    private readonly BlockingCollection<T> _blockingCollection = new BlockingCollection<T>();

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

    public void Push(T data)
    {
      if (JobState == JobState.Running)
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
      _blockingCollection.CompleteAdding();
      foreach (var job in _jobs)
      {
        job.Dispose();
      }
      GC.Collect();
    }

    public JobState JobState => _jobs[0].JobState;

    public int Count
    {
      get
      {
        return _blockingCollection.Count;
      }
    }

    public void Start()
    {
      foreach (var job in _jobs)
      {
        job.Start();
        JobManager.AddJob(job.Key, this);
      }
    }

    public void Cancel()
    {
      foreach (var job in _jobs)
      {
        job.Cancel();
        JobManager.DeleteJob(job.Key);
      }
      _blockingCollection.CompleteAdding();
    }

    public void Resume()
    {
      foreach (var job in _jobs)
      {
        job.Resume();
      }
    }

    public void Suspend()
    {
      foreach (var job in _jobs)
      {
        job.Suspend();
      }
    }

    public void Join()
    {
      Cancel();
      foreach (var job in _jobs)
      {
        job.Join();
      }
    }

    public void Wait(TimeSpan timeSpan)
    {
      IList<Task> tasks = new List<Task>(_jobs.Count);
      foreach (var job in _jobs)
      {
        tasks.Add(Task.Run(() => job.Wait(timeSpan)));
      }

      Task.WaitAll(tasks.ToArray());
    }

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