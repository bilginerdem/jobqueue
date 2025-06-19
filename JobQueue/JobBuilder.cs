using System;
using System.ComponentModel;
using JobQueue.Interfaces;

namespace JobQueue
{ 
  public sealed class JobBuilder
  {
    private string _key; 
    private IJobDataContext _jobDataContext;
    private JobStart _jobStart;
    private bool _continously;
    private JobWorkType _jobWorkType = JobWorkType.Normal;
    private TimeSpan _interval = TimeSpan.Zero; 

    public static JobBuilder Create()
    {
      return new JobBuilder();
    }

    public static JobBuilder Create(string key)
    { 
      return Create().WithKey(key); 
    }

    public JobBuilder WithKey(string key)
    {
      _key = key;
      return this;
    }

    public JobBuilder Continously(bool continously)
    {
      _continously = continously;
      return this;
    }
    public JobBuilder Schedule(TimeSpan interval)
    {
      _jobWorkType = JobWorkType.Schedule;
      _interval = interval;

      return this;
    }

    public JobBuilder OnAction(JobStart jobStart)
    {
      _jobStart = jobStart;
      return this;
    } 

    public JobBuilder SetDataContext(IJobDataContext jobDataContext)
    {
      _jobDataContext = jobDataContext;
      return this;
    }

    public IJobWork Build()
    {
      IJobWork jobWork = new Job(_jobStart);

      if (string.IsNullOrWhiteSpace(_key))
      {
        _key = Guid.NewGuid().ToString();
      }

      jobWork.Key = _key; 
      jobWork.WorkType = _jobWorkType; 
      jobWork.Continously = _jobWorkType == JobWorkType.Schedule || _continously;
      jobWork.Interval = _interval; 
      jobWork.DataContext = _jobDataContext;

      return jobWork;
    }


    #region EditorBrowsable

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override string ToString()
    {
      return base.ToString();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override bool Equals(object obj)
    {
      return base.Equals(obj);
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public override int GetHashCode()
    {
      return base.GetHashCode();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public new Type GetType()
    {
      return base.GetType();
    }

    #endregion
  }
}
