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
  public static class Guard
  {
    public static T NotNull<T>(T value, string parameterName) where T : class
    {
      if (value == null)
      {
        throw new ArgumentNullException(parameterName);
      }

      return value;
    }

    public static T? NotNull<T>(T? value, string parameterName) where T : struct
    {
      if (value == null)
      {
        throw new ArgumentNullException(parameterName);
      }

      return value;
    }

    public static string NotEmpty(string value, string parameterName)
    {
      if (string.IsNullOrWhiteSpace(value))
      {
        throw new ArgumentException($"'{parameterName}' is null or empty (include space)");
      }

      return value;
    }
 
    public static void ArgumentNotNull(object argumentValue, string argumentName)
    {
      if (argumentValue == null) throw new ArgumentNullException(argumentName);
    }
 
    public static void ArgumentNotNullOrEmpty(string argumentValue, string argumentName)
    {
      if (argumentValue == null) throw new ArgumentNullException(argumentName);
      if (argumentValue.Length == 0) throw new ArgumentException("ExceptionStringEmpty", argumentName);
    }
  }
}