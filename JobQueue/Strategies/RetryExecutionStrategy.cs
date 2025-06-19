using System;

namespace JobQueue.Strategies
{
  public class RetryExecutionStrategy : IExecutionStrategy
  {
    private int _maxRetryCount = 5;
    public bool RetriesOnFailure => true;

    public void SetMaxRetryCount(int count)
    {
      _maxRetryCount = count;
    }

    public void Execute(string name, Action operation)
    {
      Execute(name, () =>
      {
        operation();
        return (object)null;
      });
    }

    public TResult Execute<TResult>(string name, Func<TResult> operation)
    {
      var count = 0;

      while (count < _maxRetryCount)
      {
        try
        {
          return operation();
        }
        catch
        {
          count += 1;
        }
      }
      return default;
    }
  }
}
