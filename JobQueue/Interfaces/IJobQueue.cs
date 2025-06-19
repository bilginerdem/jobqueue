namespace JobQueue.Interfaces
{

  public interface IJobQueue<in T> : IJob
  { 
    void Push(T data);
    void Push(T data, bool force);
  }
  public interface IJobQueue : IJobQueue<object>
  {
  }
}