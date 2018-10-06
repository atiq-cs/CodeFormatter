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
  /// </summary>
  class CodeFormatter {
    /// <summary>
    /// Location of provided source files or directory to process
    /// Value being empty/null indicats an error state and most methods should not be called in
    /// error state
    /// </summary>
    public string Path { get; private set; }
    public bool IsDirectory { get; set; }

    public CodeFormatter() {
    }

    /// <summary>
    /// Validates and sets path member
    /// returns false if validation fails
    /// </summary>
    public bool SetPath(string path) {
      bool fileExists = System.IO.File.Exists(path);
      if (fileExists || System.IO.Directory.Exists(path)) {
        Path = path;
        IsDirectory = fileExists ? false : true;
        return true;
      }
      return false;
    }

    /// <summary>
    /// What if source file size is 1 GB or something, try to avoid reading such files..
    /// </summary>
    public void ReplaceTabs(string filePath, byte numChars=2) {
      if (string.IsNullOrEmpty(Path))
        return ;
      Console.WriteLine("Replacing tabs for " + Path + "..");
      // dotnet run: relative to location from where executable is run from
      /* string[] lines = System.IO.File.ReadAllLines(filePath);
      for (int i = 0; i < lines.Length; i++) {
        var line = lines[i];
      }*/
      string spaceString = "  ";
      string replaceString="";
      int n = numChars / spaceString.Length;
      for (int i = 0; i < n; i++)
        replaceString += spaceString;
      var fileContents = System.IO.File.ReadAllText(filePath);
      fileContents = fileContents.Replace("\t", replaceString);
      System.IO.File.WriteAllText(filePath, fileContents);
    }

    public void ProcessFile(string filePath) {
      ReplaceTabs(filePath);
    }

    /// <summary>
    /// Do something to a list of file (one file if provided path is a file)
    /// 
    /// Actions:
    /// - Replace Tabs
    /// - Do all styling to apply modern format in source code
    /// </summary>
    public void Run() {
      Console.WriteLine("Path is a " + (IsDirectory ? "Dir" : "File"));
      if (IsDirectory)
        throw new NotImplementedException();
      ProcessFile(Path);
    }
  }

  /// <summary>
  /// Entry Point
  /// 
  /// Handles command line
  /// instantiates Code Formatter Instance
  /// and calls required methods based on cmd line provided
  /// 
  /// Run Example, 
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
