using System;

namespace Cicee.Exec
{
  public class BadRequestException : Exception
  {
    public BadRequestException(string message, Exception? innerException = null) : base(message, innerException) { }
  }
}
