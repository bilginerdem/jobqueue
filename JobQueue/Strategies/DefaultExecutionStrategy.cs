using System;

namespace JobQueue.Strategies
{
  public class DefaultExecutionStrategy : IExecutionStrategy
  {
    public bool RetriesOnFailure => false;

    public void Execute(string name, Action operation)
    {
      operation();
    }
    public TResult Execute<TResult>(string name, Func<TResult> operation)
    {
      return operation();
    } 
  }
}
