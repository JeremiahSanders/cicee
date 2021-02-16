using System;
using System.Diagnostics.CodeAnalysis;

namespace Cicee.Commands
{
  [ExcludeFromCodeCoverage]
  public class BadRequestException : Exception
  {
    public BadRequestException(string message, Exception? innerException = null) : base(message, innerException) { }
  }
}
