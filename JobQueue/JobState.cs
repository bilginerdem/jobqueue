namespace JobQueue
{
  public enum JobState
  {
    None = -1,
    Suspend = 0,
    Running = 1,
    Cancelled = 2,
    Disposed = 3,
    Failed = -99
  }

  internal struct JobStateConst
  {
    public const int NONE = -1;
    public const int SUSPEND = 0;
    public const int RUNNING = 1;
    public const int CANCELLED = 2;
    public const int DISPOSED = 3;
    public const int FAILED = 99;
  }
}