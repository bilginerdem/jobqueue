namespace JobQueue.Interfaces
{
  public interface IJobDataContext
  {
    object GetData(string key);
    void SetData(string key, object value);
  }
}