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
      [Option('t', "NoTabReplace", Required = false, HelpText = "Disable replacement of tabs with two spaces.")]
      public bool NoTabReplace { get; set; }
      [Option('i', "NoIndent", Required = false, HelpText = "Disable indentation fix to two spaces.")]
      public bool NoIndent { get; set; }
      [Option("simulate", Required = false, HelpText = "Simulate an action.")]
      public bool ShouldSimulate { get; set; }
      [Option('p', "path", Required = true, HelpText = "Location of source file/s.")]
      public string Path { get; set; }
    }

    /// <summary>
    /// For now we are using to toogle boolean flags.
    /// Later ,check if we can actually do some sort of validation, the command line parser library
    /// should already provide it though
    /// </summary>
    static bool ValidateCommandLine(Options claOps) {
      string path = claOps.Path;
      if (string.IsNullOrEmpty(path) || (!System.IO.File.Exists(path) && !System.IO.Directory.
        Exists(path)))
        throw new ArgumentException("Invalid path specified!");

      claOps.NoTabReplace ^= true;
      claOps.NoIndent ^= true;
      return true;
    }

    static void Main(string[] args) {
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o => {
          if (ValidateCommandLine(o)) {
            var app = new CodeFormatter(o.Path.EndsWith(@"\")? o.Path.Substring(0, o.Path.Length -
              1) : o.Path, o.NoTabReplace, o.NoIndent,
              o.ShouldSimulate);
            app.Run();
            app.DisplaySummary();
          }
        });
      return;
    }
  }
}
