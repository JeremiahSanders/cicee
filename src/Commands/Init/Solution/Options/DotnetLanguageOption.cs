using System;
using System.CommandLine;

namespace Cicee.Commands.Init.Solution.Options;

public static class DotnetLanguageOption
{
  public enum DotnetLanguage
  {
    CSharp,
    FSharp
  }

  public static Option<DotnetLanguage?> Create()
  {
    return new Option<DotnetLanguage?>(new[] {"--language"}, argumentResult =>
      {
        DotnetLanguage? lang = null;
        foreach (var token in argumentResult.Tokens)
        {
          var value = token.Value.Trim().ToLowerInvariant();
          switch (value)
          {
            case "c#":
            case "csharp":
              lang = DotnetLanguage.CSharp;
              break;
            case "f#":
            case "fsharp":
              lang = DotnetLanguage.FSharp;
              break;
          }
        }

        return lang;
      },
      isDefault: true,
      ".NET Language. Allowed values: C#, F#"
    );
  }

  public static string GetLanguage(DotnetLanguage requestLanguage)
  {
    return requestLanguage switch
    {
      DotnetLanguage.CSharp => "C#",
      DotnetLanguage.FSharp => "F#",
      _ => throw new ArgumentOutOfRangeException(nameof(requestLanguage), requestLanguage, message: null)
    };
  }
}
