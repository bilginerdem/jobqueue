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
 