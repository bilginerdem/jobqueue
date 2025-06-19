using System;

namespace JobQueue
{
  public sealed class JobErrorArgs : EventArgs
  {
    public JobErrorArgs(Exception exception)
    {
      Exception = exception;
    }


    public JobErrorArgs(string message)
    {
      Exception = new Exception(message);
    }

    public Exception Exception { get; }
  }
}