﻿// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;
  using System.IO;
  using System.Collections.Generic;

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
    private string Path { get; set; }
    private bool IsDirectory { get; set; }
    private bool ShouldReplaceTabs { get; set; }
    private bool ShouldIndent { get; set; }
    private bool ShouldSimulate { get; set; }
    private HashSet<string> ExtList = new HashSet<string>();
    private int ModifiedFileCount = 0;

    public CodeFormatter(string path, bool tabs, bool indent, bool simulate) {
      if (path.EndsWith(@"\"))
        path = path.Substring(0, path.Length - 1);
      // Validates and sets path member
      bool fileExists = File.Exists(path);
      if (fileExists || Directory.Exists(path)) {
        Path = path;
        IsDirectory = fileExists ? false : true;
      }
      else
        throw new ArgumentException("Invalid path specified!");
      ShouldReplaceTabs = tabs;
      ShouldIndent = indent;
      ShouldSimulate = simulate;
    }

    /// <summary>
    /// Replace tabs chars with spaces to specified file
    /// </summary>
    public void ReplaceTabs(string filePath, byte numChars = 2) {
      if (string.IsNullOrEmpty(Path))
        return;
      var fileContents = File.ReadAllText(filePath);
      if (fileContents.IndexOf('\t') != -1) {
        // +1 for '\'
        Console.WriteLine(" " + GetSimplifiedPath(filePath));
        ExtList.Add(new DirectoryInfo(filePath).Extension);
        ModifiedFileCount++;
        if (ShouldSimulate == false) {
          string spaceString = "  ";
          string replaceString = "";
          int n = numChars / spaceString.Length;
          for (int i = 0; i < n; i++)
            replaceString += spaceString;
          fileContents = fileContents.Replace("\t", replaceString);
          File.WriteAllText(filePath, fileContents);
        }
      }
    }

    /// <summary>
    /// Replace indentation with specified (2 by default) spaces to specified file
    /// 
    /// Find default indent (number of spaces) in source file
    /// </summary>
    public void IndentFix(string filePath, byte numChars = 2) {
      if (string.IsNullOrEmpty(Path))
        return;
      string[] lines = File.ReadAllLines(filePath);
      Util utilDemo = new Util();
      int numIndentSpaces = utilDemo.GetIndentAmount(lines);
      if (numIndentSpaces == 0) {
        Console.Write(" [Ignored] " + GetSimplifiedPath(filePath) + ": ");
        var color = Console.ForegroundColor;
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("indent unrecognized!");
        Console.ForegroundColor = color;
      }
      else if (numIndentSpaces == numChars)
        Console.WriteLine(" [Ignored] " + GetSimplifiedPath(filePath) + ": already indented");
      if (numIndentSpaces == 0 || numIndentSpaces == numChars) {
        return ;
      }
      Console.WriteLine(" " + GetSimplifiedPath(filePath) + ": indent " + numIndentSpaces);
      ModifiedFileCount++;
      ExtList.Add(new DirectoryInfo(filePath).Extension);
      if (ShouldSimulate == false) {
        string spaceString = "  ";
        string sourceString = "";
        int n = numIndentSpaces / spaceString.Length;
        for (int i = 0; i < n; i++)
          sourceString += spaceString;
        string replaceString = "";
        n = numChars / spaceString.Length;
        for (int i = 0; i < n; i++)
          replaceString += spaceString;
        for (int i = 0; i < lines.Length; i++)
          lines[i] = lines[i].Replace(sourceString, replaceString);
        File.WriteAllLines(filePath, lines);
      }
    }

    private string GetSimplifiedPath(string path) {
      return IsDirectory?(path.StartsWith(Path) ? path.
          Substring(Path.Length + 1) : string.IsNullOrEmpty(path) ? "." :
          path):path;
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
    private void ProcessFile(string filePath) {
      if (ShouldReplaceTabs)
        ReplaceTabs(filePath);
      else if (ShouldIndent)
        IndentFix(filePath);
    }

    /// <summary>
    /// Check if directory qualifies to be in exclusion list
    /// Caution: any dir named 'Workspace' will be ignored.
    /// </summary>
    private bool IsInExclusionList(string path) {
      string dirName = new DirectoryInfo(path).Name;
      var exclusionList = new HashSet<string>() { ".git", "Workspace" };
      return exclusionList.Contains(dirName);
    }

    /// <summary>
    /// Process provided directory (recurse)
    /// </summary>
    private void ProcessDirectory(string dirPath) {
      if (IsInExclusionList(dirPath)) {
        Console.WriteLine(" [Ignored] " + GetSimplifiedPath(dirPath));
        return ;
      }
      // Process the list of files found in the directory.
      string[] fileEntries = Directory.GetFiles(dirPath);
      foreach (string fileName in fileEntries)
        ProcessFile(fileName);

      // Recurse into subdirectories of this directory.
      string[] subdirectoryEntries = Directory.GetDirectories(dirPath);
      foreach (string subdirectory in subdirectoryEntries)
        ProcessDirectory(subdirectory);
    }

    public void DisplaySummary() {
      Console.WriteLine("Number of files modified: " + ModifiedFileCount);
      Console.WriteLine("Following source files covered:");
      if (ExtList.Count == 0)
        Console.Write( " [Empty]");
      foreach (var ext in ExtList)
        Console.Write(" " + ext + ",");
      Console.WriteLine();
    }

    /// <summary>
    /// Initiate the ToDo Action for the app
    /// </summary>
    public void Run() {
      if (ShouldReplaceTabs)
        Console.WriteLine("Selected Action: replacing tabs" + (ShouldSimulate?
          " (simulated)": ""));
      else if (ShouldIndent)
        Console.WriteLine("Selected Action: indentation fix" + (ShouldSimulate ?
          " (simulated)" : ""));
      Console.WriteLine("Processing " + (IsDirectory ? "Directory: " + Path + 
        ", File list:" : "File:"));
      if (IsDirectory) {
        ProcessDirectory(Path);
      } else
        ProcessFile(Path);
    }
  }
}
