namespace JobQueue.Strategies
{
  public struct ExecutionStrategyKeys
  {
    public static readonly ExecutionStrategyKey Storage = new ExecutionStrategyKey("Storage", null);
    public static readonly ExecutionStrategyKey Logger = new ExecutionStrategyKey("Logger", null);
    public static readonly ExecutionStrategyKey Nonexception = new ExecutionStrategyKey("NoneExpcetion", null);
  }
}