// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;
  using CommandLine;

  /// <summary>
  /// A Code Formatting Application
  /// 
  /// Documentation (requirements/features/design decisions) at
  /// https://github.com/atiq-cs/CodeFormatter/wiki
  /// 
  /// Run Example, 
  /// </summary>
  class CodeFormatter {
    public class Options {
      [Option('t', "replaceTabs", Required = false, HelpText = "Replace tabs with two spaces.")]
      public bool claShouldReplaceTabs { get; set; }
    }

    static void Main(string[] args) {
      Parser.Default.ParseArguments<Options>(args)
        .WithParsed<Options>(o => {
          if(o.claShouldReplaceTabs) {
            Console.WriteLine("Replacing tabs..");
            return ;
          } else {
            Console.WriteLine($"Current Arguments: -v {o.claShouldReplaceTabs}");
            Console.WriteLine("Quick Start Example!");
          }
        });
      return;
      // Read each line of the file into a string array. Each element of the
      // array is one line of the file.
      // what if source file size is 1 GB or something, try to avoid reading
      // such files..

      // For debugging from VS
      // string[] lines = System.IO.File.ReadAllLines(@"D:\Code\CSharp\CodePrettifier\obj\Debug\Ex.cs");
      // dotnet run: relative to location from where executable is run from
      string[] lines = System.IO.File.ReadAllLines(@"obj\Debug\Ex.cs");
      Util utilDemo = new Util();

      /* bool isTab = utilDemo.GetIndentationType();
      utilDemo.FixIndentation(isTab);
      int numIndentSpaces = utilDemo.IndentationSettingsFinder(lines); */
    }
  }
}
