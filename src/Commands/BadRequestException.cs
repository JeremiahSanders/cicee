using System;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands;

[ExcludeFromCodeCoverage]
public class BadRequestException : Exception
{
  private BadRequestException(string message, Exception? innerException = null)
    : base(message, innerException)
  {
  }

  public static BadRequestException FromMessage(string message)
  {
    return new BadRequestException(message);
  }

  public static BadRequestException FromException(string message, Exception innterException)
  {
    return new BadRequestException(message, innterException);
  }
}
