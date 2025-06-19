using System.Collections.Concurrent;
using System.Collections.Generic;
using JobQueue.Interfaces;

namespace JobQueue
{
  public static class JobManager
  {
    public static readonly IDictionary<string, IJob> Jobs = new ConcurrentDictionary<string, IJob>();

    public static IJob GetJobThread(string key)
    {
      Jobs.TryGetValue(key, out IJob job);
      return job;
    }
     

    public static void Cancel(string key)
    {
      IJob job = GetJobThread(key); 
      job?.Cancel();
    }

    public static void Join(string key)
    {
      IJob job = GetJobThread(key); 
      job?.Join();
    }

    public static void JoinAll()
    {
      foreach (var job in Jobs)
      {
        job.Value.Join();
      }
    }

    internal static void AddJob(string key, IJob job)
    {
      Jobs[key] = job;
    }
    internal static void DeleteJob(string key)
    {
      Jobs.Remove(key);
    }
  }
}
