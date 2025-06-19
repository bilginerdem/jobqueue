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

namespace JobQueue
{
  public sealed class JobErrorArgs : EventArgs
  {
    /// <summary>
    /// Initializes a new instance of JobErrorArgs with an exception.
    /// </summary>
    public JobErrorArgs(Exception exception)
    {
      Exception = exception;
    }


    /// <summary>
    /// Initializes a new instance of JobErrorArgs with a message.
    /// </summary>
    public JobErrorArgs(string message)
    {
      Exception = new Exception(message);
    }

    /// <summary>
    /// Gets the exception associated with the error.
    /// </summary>
    public Exception Exception { get; }
  }
}