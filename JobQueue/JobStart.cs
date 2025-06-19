using JobQueue.Interfaces;

namespace JobQueue
{
  public delegate void JobStart(IJobDataContext jobDataContext);
}