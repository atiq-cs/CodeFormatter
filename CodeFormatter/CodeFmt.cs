// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;
  using System.IO;

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
    /// Value being empty/null indicats an error state and most methods should
    /// not be called in error state
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
      bool fileExists = File.Exists(path);
      if (fileExists || Directory.Exists(path)) {
        Path = path;
        IsDirectory = fileExists ? false : true;
        return true;
      }
      return false;
    }

    /// <summary>
    ///
    /// </summary>
    public void ReplaceTabs(string filePath, byte numChars = 2) {
      if (string.IsNullOrEmpty(Path))
        return;
      // simuate for dir now
      Console.WriteLine(" " + filePath);
      // dotnet run: relative to location from where executable is run from
      string[] lines = File.ReadAllLines(filePath);
      for (int i = 0; i < lines.Length; i++) {
        var line = lines[i];
      }
      string spaceString = "  ";
      string replaceString="";
      int n = numChars / spaceString.Length;
      for (int i = 0; i < n; i++)
        replaceString += spaceString;
      var fileContents = File.ReadAllText(filePath);
      fileContents = fileContents.Replace("\t", replaceString);
      File.WriteAllText(filePath, fileContents);
    }

    /// <summary>
    /// Set an action based on user choice and perform action to specified file
    /// This action is stream (file content) editing for the file. Right now,
    /// following caveats are not taken into account,
    ///  Unmanageable file size: >= 1 GB, check read files (API in msdn) limit
    /// Actions:
    /// - Replace Tabs
    /// - Do all styling to apply modern format in source code
    /// </summary>
    public void ProcessFile(string filePath) {
      ReplaceTabs(filePath);
    }

    /// <summary>
    /// Process provided directory (recurse)
    /// </summary>
    public void ProcessDirectory(string dirPath) {
      // Process the list of files found in the directory.
      string[] fileEntries = Directory.GetFiles(dirPath);
      foreach (string fileName in fileEntries)
        ProcessFile(fileName);

      // Recurse into subdirectories of this directory.
      string[] subdirectoryEntries = Directory.GetDirectories(dirPath);
      foreach (string subdirectory in subdirectoryEntries)
        ProcessDirectory(subdirectory);
    }

    /// <summary>
    /// Initiate the ToDo Action for the app
    /// </summary>
    public void Run() {
      Console.WriteLine("Path is a " + (IsDirectory ? "Dir" : "File"));
      Console.WriteLine("Selected Action: replacing tabs");
      if (IsDirectory) {
        ProcessDirectory(Path);
      } else
        ProcessFile(Path);
    }
  }
}
