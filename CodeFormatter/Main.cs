// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;
  using CommandLine;

  /// <summary>
  /// Entry Point class containing command line parser and instantiator main
  /// method
  /// Usage,
  ///  dotnet run -- --replaceTabs --path D:\MyApp --simulate
  ///  dotnet run -- --replaceTabs --path D:\MyApp
  /// </summary>
  class CodeFmtDemo {
    public class Options {
      [Option('t', "replaceTabs", Required = false, HelpText = "Replace tabs with two spaces.")]
      public bool ShouldReplaceTabs { get; set; }
      [Option('i', "indent", Required = false, HelpText = "Fix indentation to two spaces.")]
      public bool Indent { get; set; }
      [Option('p', "path", Required = true, HelpText = "Location of source file/s.")]
      public string Path { get; set; }
      [Option("simulate", Required = false, HelpText = "Simulate an action.")]
      public bool ShouldSimulate { get; set; }
    }

    static bool ValidateCommandLine(Options claOps) {
      return true;
    }

    static void Main(string[] args) {
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o => {
          if (ValidateCommandLine(o)) {
            var app = new CodeFormatter(o.Path, o.ShouldReplaceTabs, o.Indent, o.ShouldSimulate);
            app.Run();
            app.DisplaySummary();
          }
        });
      return;
    }
  }
}
