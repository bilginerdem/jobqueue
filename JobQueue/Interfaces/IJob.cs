using System;

namespace JobQueue.Interfaces
{
  public interface IJob : IDisposable
  {
    string Key { get; set;  }
    event EventHandler<JobErrorArgs> Error;
    JobState JobState { get; }
    int Count { get; }
    void Start();
    void Cancel();
    void Resume();
    void Suspend();
    void Join();
    void Wait(TimeSpan timeSpan);
  }
}