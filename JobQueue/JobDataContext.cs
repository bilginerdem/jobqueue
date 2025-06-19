using System.Collections.Concurrent;
using System.Collections.Generic;
using JobQueue.Interfaces;

namespace JobQueue
{
  public class JobDataContext : IJobDataContext
  {
    private readonly IDictionary<string, object> _dataDictionary = new ConcurrentDictionary<string, object>();

    public object GetData(string key)
    {
      _dataDictionary.TryGetValue(key, out var value);
      return value;
    }

    public void SetData(string key, object value)
    {
      _dataDictionary[key] = value;
    }
  }
}
