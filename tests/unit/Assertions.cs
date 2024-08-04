using System;

using LanguageExt.Common;

using Xunit;

namespace Cicee.Tests.Unit;

public static class Assertions
{
  public static class Results
  {
    public static void Equal<T>(Result<T> expectedResult, Result<T> actualResult)
    {
      expectedResult.IfSucc(
        expected =>
        {
          actualResult.IfSucc(
            actual =>
            {
              Assert.Equal(expected, actual);
            }
          );
          _ = actualResult.IfFail(
            actualException => throw new Exception(message: "Should have succeeded.", actualException)
          );
        }
      );
      expectedResult.IfFail(
        exception =>
        {
          actualResult.IfSucc(actual => throw new Exception(message: "Should have failed"));
          actualResult.IfFail(
            actual =>
            {
              Assert.Equal(exception.GetType(), actual.GetType());
              Assert.Equal(exception.Message, actual.Message);
            }
          );
        }
      );
    }
  }
}
