using System;

namespace JobQueue.Strategies
{
  public class ExecutionStrategyKey
  { 
    public ExecutionStrategyKey(string provider, string appName)
    {
      Guard.NotEmpty(provider, "providerInvariantName");

      Provider = provider;
      AppName = appName;
    } 
    public string Provider { get; }
     
    public string AppName { get; }
    
    public override bool Equals(object obj)
    {
      var otherKey = obj as ExecutionStrategyKey;
      if (ReferenceEquals(otherKey, null))
      {
        return false;
      }

      return Provider.Equals(otherKey.Provider, StringComparison.Ordinal)
             && ((AppName == null && otherKey.AppName == null) ||
                 (AppName != null && AppName.Equals(otherKey.AppName, StringComparison.Ordinal)));
    }
    
    public override int GetHashCode()
    {
      if (AppName != null)
      {
        return Provider.GetHashCode() ^ AppName.GetHashCode();
      }

      return Provider.GetHashCode();
    }
  }
}
