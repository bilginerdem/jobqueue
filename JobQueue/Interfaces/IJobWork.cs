using System;
using System.Threading;

namespace JobQueue.Interfaces
{
  public interface IJobWork : IJob
  {
    bool Continously { get; set; }  
    IJobDataContext DataContext { get; set; }
    JobWorkType WorkType { get; set; } 
    TimeSpan Interval { get; set; } 
    CancellationToken CancellationToken { get; }
  }
}
