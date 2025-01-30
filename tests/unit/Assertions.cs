using System;

using LanguageExt.Common;

using Shouldly;

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
              actual.ShouldBeEquivalentTo(expected);
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
              actual.ShouldBeOfType(exception.GetType());
              actual.Message.ShouldBe(exception.Message);
            }
          );
        }
      );
    }
  }
}
