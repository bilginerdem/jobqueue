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

namespace JobQueue.Strategies
{
  public class RetryExecutionStrategy : IExecutionStrategy
  {
    private int _maxRetryCount = 5;
    /// <summary>
    /// Indicates if the strategy retries on failure.
    /// </summary>
    public bool RetriesOnFailure => true;

    /// <summary>
    /// Sets the maximum retry count.
    /// </summary>
    public void SetMaxRetryCount(int count)
    {
      _maxRetryCount = count;
    }

    /// <summary>
    /// Executes the operation with retry logic.
    /// </summary>
    public void Execute(string name, Action operation)
    {
      Execute(name, () =>
      {
        operation();
        return (object)null;
      });
    }

    /// <summary>
    /// Executes the operation with retry logic and returns a result.
    /// </summary>
    public TResult Execute<TResult>(string name, Func<TResult> operation)
    {
      var count = 0;

      while (count < _maxRetryCount)
      {
        try
        {
          return operation();
        }
        catch
        {
          count += 1;
        }
      }
      return default;
    }
  }
}
