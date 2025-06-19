using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using JobQueue.Interfaces;

namespace JobQueue
{
  public sealed class JobQueueAsync<T> : IJobQueue<T>
  {
    public string Key { get; set; }

    public event EventHandler<JobErrorArgs> Error;

    private Task _task;

    private readonly Channel<T> _channel = Channel.CreateUnbounded<T>();
    private readonly Action<T> _queueAction;

    public JobQueueAsync(string key, Action<T> queueAction)
    {
      _queueAction = queueAction;
      Key = key;
    }


    private async void QueueCallback()
    {
      try
      {
        await foreach (var element in _channel.Reader.ReadAllAsync())
        {
          if (element == null)
            return;

          _queueAction(element);
        }
      }
      catch (Exception e)
      {
        Error?.Invoke(this, new JobErrorArgs(e));
      }
    }

    public void Push(T data)
    {
      if (_task.Status == TaskStatus.Running)
      {
        _channel.Writer.TryWrite(data);
      }
    }

    public void Push(T data, bool force)
    {
      if (force)
      {
        _channel.Writer.TryWrite(data);
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
        _channel.Writer.Complete(); 
        _task.Dispose();
        GC.Collect();
      }
      catch
      {
        // ignored
      }
    }

    public JobState JobState
    {
      get
      {
        return _task.Status switch
        {
          TaskStatus.Running => JobState.Running,
          TaskStatus.Canceled => JobState.Cancelled,
          TaskStatus.Faulted => JobState.Failed,
          TaskStatus.RanToCompletion => JobState.Disposed,
          _ => JobState.None
        };
      }
    }

    public int Count => _channel.Reader.Count;

    public void Start()
    {
      _task = Task.Factory.StartNew(QueueCallback, TaskCreationOptions.LongRunning);
    }

    public void Cancel()
    { 
      Dispose();
    }

    public void Resume()
    {
      throw new NotSupportedException();
    }

    public void Suspend()
    {
      throw new NotSupportedException();
    }

    public void Join()
    { 
      Dispose();
    }

    public void Wait(TimeSpan timeSpan)
    {
      _task.Wait(timeSpan);
      Thread.Yield();
    }

    public void Interrupt()
    { 
      Dispose();
    }

  }
}
 