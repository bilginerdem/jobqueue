// Copyright (c) 2025 Erdem Bilgin
// 
// This source code is licensed under the MIT license found in the
// LICENSE file in the root directory of this source tree.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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

    /// <summary>
    /// Creates a new instance of JobBuilder.
    /// </summary>
    public static JobBuilder Create()
    {
      return new JobBuilder();
    }

    /// <summary>
    /// Creates a new instance of JobBuilder with the specified key.
    /// </summary>
    public static JobBuilder Create(string key)
    { 
      return Create().WithKey(key); 
    }

    /// <summary>
    /// Sets the job key.
    /// </summary>
    public JobBuilder WithKey(string key)
    {
      _key = key;
      return this;
    }

    /// <summary>
    /// Sets whether the job should run continuously.
    /// </summary>
    public JobBuilder Continously(bool continously)
    {
      _continously = continously;
      return this;
    }
    /// <summary>
    /// Sets the schedule interval for the job.
    /// </summary>
    public JobBuilder Schedule(TimeSpan interval)
    {
      _jobWorkType = JobWorkType.Schedule;
      _interval = interval;

      return this;
    }

    /// <summary>
    /// Sets the job action.
    /// </summary>
    public JobBuilder OnAction(JobStart jobStart)
    {
      _jobStart = jobStart;
      return this;
    } 

    /// <summary>
    /// Sets the data context for the job.
    /// </summary>
    public JobBuilder SetDataContext(IJobDataContext jobDataContext)
    {
      _jobDataContext = jobDataContext;
      return this;
    }

    /// <summary>
    /// Builds and returns the job.
    /// </summary>
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
