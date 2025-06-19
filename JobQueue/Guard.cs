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