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
  public class ExecutionStrategyKey
  { 
    public ExecutionStrategyKey(string provider, string appName)
    {
      Guard.NotEmpty(provider, "providerInvariantName");

      Provider = provider;
      AppName = appName;
    } 
    public string Provider { get; }
     
    public string AppName { get; }
    
    public override bool Equals(object obj)
    {
      var otherKey = obj as ExecutionStrategyKey;
      if (ReferenceEquals(otherKey, null))
      {
        return false;
      }

      return Provider.Equals(otherKey.Provider, StringComparison.Ordinal)
             && ((AppName == null && otherKey.AppName == null) ||
                 (AppName != null && AppName.Equals(otherKey.AppName, StringComparison.Ordinal)));
    }
    
    public override int GetHashCode()
    {
      if (AppName != null)
      {
        return Provider.GetHashCode() ^ AppName.GetHashCode();
      }

      return Provider.GetHashCode();
    }
  }
}
