// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;
  using CommandLine;

  /// <summary>
  /// Entry Point class containing command line parser and instantiator main
  /// method
  /// </summary>
  class CodeFmtDemo {
    public class Options {
      [Option('t', "replaceTabs", Required = false, HelpText = "Replace tabs with two spaces.")]
      public bool ShouldReplaceTabs { get; set; }
      [Option('p', "path", Required = true, HelpText = "Location of source file/s.")]
      public string Path { get; set; }
    }

    static void Main(string[] args) {
      var app = new CodeFormatter();
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o => {
          if(o.ShouldReplaceTabs) {
            if (app.SetPath(o.Path) == false)
              throw new ArgumentException("Invalid path specified!");
            app.Run();
            return ;
          } else {
            Console.WriteLine($"Current Arguments: -v {o.ShouldReplaceTabs}");
            Console.WriteLine("Quick Start Example!");
          }
        });
      return;
      // Read each line of the file into a string array. Each element of the
      // array is one line of the file.

      // For debugging from VS
      // string[] lines = System.IO.File.ReadAllLines(@"D:\Code\CSharp\CodePrettifier\obj\Debug\Ex.cs");
      Util utilDemo = new Util();

      /* bool isTab = utilDemo.GetIndentationType();
      utilDemo.FixIndentation(isTab);
      int numIndentSpaces = utilDemo.IndentationSettingsFinder(lines); */
    }
  }
}
