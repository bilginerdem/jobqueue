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

using System.Collections.Concurrent;
using System.Collections.Generic;
using JobQueue.Interfaces;

namespace JobQueue
{
  public static class JobManager
  {
    public static readonly IDictionary<string, IJob> Jobs = new ConcurrentDictionary<string, IJob>();

    public static IJob GetJobThread(string key)
    {
      Jobs.TryGetValue(key, out IJob job);
      return job;
    }
     

    public static void Cancel(string key)
    {
      IJob job = GetJobThread(key); 
      job?.Cancel();
    }

    public static void Join(string key)
    {
      IJob job = GetJobThread(key); 
      job?.Join();
    }

    public static void JoinAll()
    {
      foreach (var job in Jobs)
      {
        job.Value.Join();
      }
    }

    internal static void AddJob(string key, IJob job)
    {
      Jobs[key] = job;
    }
    internal static void DeleteJob(string key)
    {
      Jobs.Remove(key);
    }
  }
}
