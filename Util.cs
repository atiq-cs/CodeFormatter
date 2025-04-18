﻿// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license
// This file just stays as reference code

namespace ConsoleApp {
  using System;

  class Util {
    const int MaxNumLinesToProbe = 16;
    const int MinIndentLengthExpected = 2;
    const int MaxColumnWrapLength = 120;

    public Util() {
    }

    /// <summary>
    /// Get number of space count in indentation in specified source
    /// </summary>
    /// <param name="lines">Lines of source file</param>
    public bool IndentAndDocumentationFix(string[] lines, bool indent, int
        numIndentSpaces, byte numChars, bool simulate) {
      string spaceString = "  ";
      string sourceString = "";
      int n = numIndentSpaces / spaceString.Length;
      for (int i = 0; i < n; i++)
        sourceString += spaceString;
      string replaceString = "";
      n = numChars / spaceString.Length;
      for (int i = 0; i < n; i++)
        replaceString += spaceString;

      bool isInsideBlockComment = false, done = false;
      bool isModified = false;
      for (int i = 0; i < lines.Length; i++) {
        var line = (simulate || indent==false)? lines[i]: lines[i].Replace(sourceString, replaceString);
        if (done == false) {
          if (isBlockCommentStatusToggling(line, isInsideBlockComment)) {
            isInsideBlockComment = isInsideBlockComment ? false : true;
            if (isInsideBlockComment == false)
              done = true;
          }
          string oldline = line;
          line = ReadAndFormat(line);
          if (oldline != line)
            isModified = true;
          if (simulate == false)
            line = oldline;
          }
        lines[i] = line;
      }
      return isModified;
    }

    /// <summary>
    /// Write the string that we wanna write to fix old comment/docu style
    /// 
    /// if no key is found or found key cannot be mapped (has a length greater
    /// than limit) consider it as continuation multi-line value for previouso
    /// key
    /// </summary>
    /// <param name="lines">Lines of source file</param>
    private string ReadAndFormat(string line) {
      // trimstart https://docs.microsoft.com/en-us/dotnet/api/system.string.trimstart?view=netframework-4.7.2
      if (line.TrimStart(' ').StartsWith("/*"))
        return line;
      if (line.TrimEnd(' ').EndsWith("*/"))
        return line;
      if (line.StartsWith("*"))
        line = line.Substring(1);
      else if (line.StartsWith(" *"))
        line = line.Substring(2);
      if (string.IsNullOrEmpty(line))
        return "*";
      // if on first line, key not found that's an error
      var tokens = line.TrimStart().Split(':');
      var key = tokens[0].TrimStart(' ').TrimEnd(' ');
      // if key is not recognized then entire line is val for key that was previously recognized
      var val = tokens.Length>1?tokens[1].TrimStart(' ').TrimEnd(' '):"";
      Console.WriteLine("key: " + key + " val: " + val);
      return line;
      // var dict = new Dictionary<string, string>();
    }

    /// <summary>
    /// Get number of space count in indentation in specified source
    /// </summary>
    /// <param name="lines">Lines of source file</param>
    public int GetIndentAmount(string[] lines) {
      // Look for lines starting with 2 spaces, find the first non-whitespace char
      // Inside comment block this might not work. Hence, try outside of comment
      // blocks.
      // Display the file contents by using a foreach loop.
      // Console.WriteLine("Contents:");
      bool isInsideBlockComment = false;
      int length = MaxColumnWrapLength;   // max column wrap number
      int currentLength;

      // oci: outside comment line index
      for (int i = 0, oci = 0; oci < MaxNumLinesToProbe && i < lines.Length; i++) {
        var line = lines[i];
        if (isBlockCommentStatusToggling(line, isInsideBlockComment)) {
          isInsideBlockComment = isInsideBlockComment ? false : true;
          // Console.WriteLine(line + "\t[inside block comment? " + isInsideBlockComment + "]");
        }
        /*else {
          Console.WriteLine(line);
        }*/

        if (isInsideBlockComment == false)
          oci++;

        if ((currentLength = GetIndentationLengthFromLine(line, isInsideBlockComment)) > 0)
          length = Math.Min(length, currentLength);
        if (oci == MaxNumLinesToProbe && length == MaxColumnWrapLength)
          oci = 0;
      }
      return length==MaxColumnWrapLength?0:length;
    }

    /// <summary>
    /// Simplified method: check if we are going outside of comment block if
    /// previously we were inside. Or check an oppositve thing is happening..
    /// 
    /// Consider following test-cases where,
    /// \s stands for start, \e stands for end
    ///  \s\e
    ///  \e\s
    ///  \s\s
    ///  \e\e
    ///  
    /// Consider cases for testing for inside block comment,
    /// started in some previous line do we get an ending
    /// if last found /* is before last found */ then toggle
    /// </summary>
    /// <param name="line">Single line</param>
    /// <param name="isInsideBlockComment">Current status regarding being in block comment</param>
    /// <returns>
    /// True if toggling, false otherwise
    /// </returns>
    bool isBlockCommentStatusToggling(string line, bool isInsideBlockComment) {
      if (isInsideBlockComment) {
        int startPos = line.LastIndexOf("*/");
        if (startPos == -1)
          return false;
        int endPos = line.LastIndexOf("/*");
        if (endPos == -1)
          return true;
        return (startPos >= endPos + 2);
      }
      else {
        int startPos = line.LastIndexOf("/*");
        if (startPos == -1)
          return false;
        int endPos = line.LastIndexOf("*/");
        if (endPos == -1)
          return true;
        return (startPos <= endPos + 2);
      }
    }

    /// <summary>
    /// For a line of code outside of comment block, find number of spaces used for indentation
    /// </summary>
    /// <param name="line">Line to inspect</param>
    /// <param name="isInsideBlockComment">Indicates whether we are inside a block comment</param>
    /// <returns>
    /// On error (inside block comment or ...), returns 0
    /// </returns>
    int GetIndentationLengthFromLine(string line, bool isInsideBlockComment) {
      if (isInsideBlockComment == false && line.StartsWith("  ")) {
        int i = MinIndentLengthExpected;
        for (; i < line.Length; i++)
          if (line[i] != ' ')
            break;
        return i;
      }
      return 0;
    }
  }
}
