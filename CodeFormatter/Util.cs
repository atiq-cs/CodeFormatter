// Copyright (c) FFTSys Inc. All rights reserved.
// Use of this source code is governed by a GPL-v3 license

namespace ConsoleApp {
  using System;

  class Util {
    const int MaxNumLinesToProbe = 8;

    public Util() {
    }

    /// <summary>
    /// Can be space or tabs
    /// </summary>
    /// <returns>
    /// Returns true if tabs
    /// </returns>
    public bool GetIndentationType(string[] lines) {
      bool isInsideBlockComment = false;
      for (int i = 0, oci = 0; oci < MaxNumLinesToProbe && i < lines.Length; i++) {
        var line = lines[i];
        if (isBlockCommentStatusToggling(line, isInsideBlockComment))
          isInsideBlockComment = isInsideBlockComment ? false : true;
        if (isInsideBlockComment == false)
          oci++;


      }
      return false;
    }

    public int IndentationSettingsFinder(string[] lines) {
      // Display the file contents by using a foreach loop.
      Console.WriteLine("Contents:");
      bool isInsideBlockComment = false;
      int prevIndentLen = 0, indentLength=0;
      for (int i=0; i<MaxNumLinesToProbe && i<lines.Length; i++) {
        var line = lines[i];
        if (isBlockCommentStatusToggling(line, isInsideBlockComment)) {
          isInsideBlockComment = isInsideBlockComment ? false : true;
          Console.WriteLine(line + "\t[inside block comment? " + isInsideBlockComment + "]");
        }
        else { // Use a tab to indent each line of the file.
          Console.WriteLine(line);
        }
        
        int len = GetIndentationLength(line, isInsideBlockComment);
        if (len > 0) {
          indentLength = len;
          prevIndentLen = indentLength;
        }
      }

      return 0;
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
    /// </summary>
    /// <param name="line">Single line</param>
    /// <param name="isInsideBlockComment">Current status regarding being in block comment</param>
    /// <returns>
    /// True if toggling, false otherwise
    /// </returns>
    bool isBlockCommentStatusToggling(string line, bool isInsideBlockComment) {
      if (isInsideBlockComment) {
        // /* started in some previous line
        // do we get an ending */ ?
        // if last found /* is before last found */ then toggle
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

    /* 
     * return value
     * -1 - Error
     * 0 - no indentation
     * 
     */
    int GetIndentationLength(string line, bool isInsideBlockComment) {
      if (isInsideBlockComment) {
        if (line.StartsWith("**"))
          return -1;

      } else {
        // int indentl
      }
      return 0;
    }
  }
}
