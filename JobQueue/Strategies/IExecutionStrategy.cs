using System;

namespace JobQueue.Strategies
{
  public interface IExecutionStrategy
  {
    bool RetriesOnFailure { get; }
    void Execute(string name, Action operation);
    TResult Execute<TResult>(string name, Func<TResult> operation); 
  }
}
