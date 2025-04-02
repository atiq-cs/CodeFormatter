// Copyright (c) FFTSys Inc. All rights reserved.
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
    /// Location of input source file or directory to process
    /// Value being empty/null indicates an error state. Most methods should
    /// not be called in such  error state
    /// </summary>
    private string Path { get; set; }
    private bool IsDirectory { get; set; }

    // States from CLA
    private bool ShouldReplaceTabs { get; set; }
    private bool ShouldIndent { get; set; }
    private bool ShouldSimulate { get; set; }

    private HashSet<string> ExclusionDirList;
    private HashSet<string> ExclusionExtList;

    // Actions Summary Related
    private HashSet<string> ExtList = new HashSet<string>();
    private int ModifiedFileCount = 0;

    // Debug related stuff
    private string VSPath = @"D:\PFiles_x86\MSVS\2017\Enterprise\Common7\IDE\devenv.exe";

    /// <summary>
    /// Props/methods related to single file processing
    /// <remarks>
    /// This internal class should not be aware of outer cless details such as
    /// <c> ShouldSimulate </c>
    /// </remarks>  
    /// </summary>
    class FileInfoType {
      public string Path { get; set; }
      public bool IsModified { get; set; }
      public string[] Lines { get; set; }

      public void Init(string Path) {
        this.Path = Path;
        IsModified = false;
        Lines = null;
        ModInfo = string.Empty;
      }
      public void ReadFile() {
        Lines = File.ReadAllLines(Path);
        if (Lines == null || Lines.Length == 0)
          throw new IOException("Unexpected file content!");
      }

      public void WriteFile() { File.WriteAllLines(Path, Lines); }
      public void SetDirtyFlag(string str) {
        if (IsModified) { ModInfo += ", " + str; }
        else { IsModified = true;
          ModInfo += str;
        }
      }

      // list of actions being performed in the file
      public string ModInfo { get; set; }
    }
    FileInfoType FileInfo = new FileInfoType();

    // Variables/settings related to Indentation
    private const int MaxColumnWrapLength = 120;    // max column wrap number
    private const int MinIndentLengthExpected = 2;

    // Variables/settings related to Comment Style/Documentation
    private Dictionary<string, string> CBOldKeysMap;
    private HashSet<string> CBKeysSet;
    private const int MaxKeyLength = 6;

    /// <summary>
    /// Constructor: sets first 5 properties
    /// </summary>
    public CodeFormatter(string Path, bool ShouldReplaceTabs, bool ShouldIndent,
        bool ShouldSimulate) {
      // Sets path member and directory flag
      IsDirectory = File.Exists(Path) ? false : true;
      this.Path = Path;
      this.ShouldReplaceTabs = ShouldReplaceTabs;
      this.ShouldIndent = ShouldIndent;
      this.ShouldSimulate = ShouldSimulate;

      /// Be aware, any dir named 'Workspace' will be ignored.
      ExclusionDirList = new HashSet<string>() { ".git", "Workspace" };
      ExclusionExtList = new HashSet<string>() { "csproj", "py", "txt" };
      CBOldKeysMap = new Dictionary<string, string>() { { "Problem Link", "URL" }, {"Problem Name", "Title" }, { "Problem Title", "Title" }, { "Problem", "Title" }, { "Problem URL", "URL" }, { "Complexity", "Comp" }, { "Desc", "Notes" }, { "Algorithm", "Algo" }, { "Occasion", "Occasn" }, { "Related", "rel" }, { "Rel", "rel" }, { "Ref", "ref" }, { "Credits", "Ack" } };
      CBKeysSet = new HashSet<string>() { "Ack", "Algo", "Author", "Comp", "Contst", "Date", "Email", "meta", "Notes", "Occasn", "ref", "rel", "Status", "Title", "URL" };
    }

    /// <summary>
    /// Replace tabs chars with spaces to specified file
    /// <remarks> This method has its own IO; doesn't use FileInfo </remarks>  
    /// </summary>
    public void ReplaceTabs(byte numChars = MinIndentLengthExpected) {
      var fileContents = File.ReadAllText(FileInfo.Path);
      if (fileContents.IndexOf('\t') != -1) {
        // +1 for '\'
        if (ShouldSimulate == false) {
          FileInfo.SetDirtyFlag("tabs");
          string spaceString = "  ";
          string replaceString = "";
          int n = numChars / spaceString.Length;
          for (int i = 0; i < n; i++)
            replaceString += spaceString;
          fileContents = fileContents.Replace("\t", replaceString);
          File.WriteAllText(FileInfo.Path, fileContents);
        }
      }
    }

    /// <summary>
    /// Indent with provided number of spaces, replace previous indentation chars
    /// Find default indent (number of spaces) in source file
    /// Avoid touching if found default indent count is less than 4 !
    /// Replaces indentation in every line regardless whether insice comment block or not
    /// </summary>
    public void Indent(byte numChars = 2) {
      int numIndentSpaces = GetIndentAmount();
      // 3 is unexpected! for my project
      if (numIndentSpaces == 0 || numIndentSpaces == 3) {
        // Console.WriteLine("indent unrecognized!");
      }
      else if (numIndentSpaces == numChars) {
        // Console.WriteLine(" already indented");
      }

      if (numIndentSpaces < 4)
        return ;

      FileInfo.SetDirtyFlag("indent");

      string spaceString = "  ", sourceString = spaceString;
      int n = numIndentSpaces / spaceString.Length;
      for (int i = 1; i < n; i++)
        sourceString += spaceString;
      string replaceString = "  ";
      n = numChars / spaceString.Length;
      for (int i = 1; i < n; i++)
        replaceString += spaceString;
      for (int i = 0; i < FileInfo.Lines.Length; i++)
        FileInfo.Lines[i] = FileInfo.Lines[i].Replace(sourceString, replaceString);
    }

    /// <summary>
    /// Fix comment style
    /// Can be callled followed by indent fix
    /// Detect if a line is modified and set a dirty flag based on that
    /// Use a separate modified flag because file might be flagged as modified by indenter method
    /// earlier
    /// </summary>
    public void FixDocumentation() {
      int startCBIndex, endCBIndex;
      GetSpecialCommentBlock(out startCBIndex, out endCBIndex);
      SetCBEdgeLine(true, startCBIndex);
      bool isModified = false;

      const int MaxCBLinesToProbe = 7;
      int cbi = 0;
      for (int i = startCBIndex + 1; i < endCBIndex; i++) {
        var line = FileInfo.Lines[i];
        ReadAndFormat(i);
        if (!isModified) {
          if (line == FileInfo.Lines[i]) {
            cbi++;
            if (cbi == MaxCBLinesToProbe)
              break;
          }
          else
            isModified = true;
        }
      }
      if (isModified)
        FileInfo.SetDirtyFlag("docu");
      SetCBEdgeLine(false, endCBIndex);
    }

    /// <summary>
    /// <param name="FileInfo.Path">Path of source file</param>  
    /// <returns> Returns true if soure contains tab.</returns>  
    /// <exception> <see cref="File::ReadAllText"/> </exception>
    /// </summary>
    private bool FileContainsTab() {
      return File.ReadAllText(FileInfo.Path).IndexOf('\t') != -1;
    }

    /// <summary>
    /// Get indentation space count in specified source
    /// </summary>
    /// <param name="lines">Lines of source file</param>
    private int GetIndentAmount() {
      // Look for lines starting with 2 spaces, find the first non-whitespace char
      // Inside comment block this might not work. Hence, try outside of comment
      // blocks.
      // Display the file contents by using a foreach loop.
      bool isInsideBlockComment = false;
      int length = MaxColumnWrapLength;   
      int currentLength;
      const int MaxNumLinesToProbe = 16;

      // oci: outside comment line index
      for (int i = 0, oci = 0; oci < MaxNumLinesToProbe && i < FileInfo.Lines.Length; i++) {
        var line = FileInfo.Lines[i];
        if (isBlockCommentStatusToggling(line, isInsideBlockComment))
          isInsideBlockComment = isInsideBlockComment ? false : true;
        if (isInsideBlockComment == false)
          oci++;
        if ((currentLength = GetIndentationLengthFromLine(line, isInsideBlockComment)) > 0)
          length = Math.Min(length, currentLength);
        if (oci == MaxNumLinesToProbe && length == MaxColumnWrapLength)
          oci = 0;
      }
      return length == MaxColumnWrapLength ? 0 : length;
    }

    /// <summary>
    /// For a line of code outside of comment block, find number of spaces used for indentation
    /// </summary>
    /// <param name="line">Line to inspect</param>
    /// <param name="isInsideBlockComment">Indicates whether we are inside a block comment</param>
    /// <returns>
    /// On error (inside block comment or ...), returns 0
    /// </returns>
    private int GetIndentationLengthFromLine(string line, bool isInsideBlockComment) {
      if (isInsideBlockComment == false && line.StartsWith("  ")) {
        int i = MinIndentLengthExpected;
        for (; i < line.Length; i++)
          if (line[i] != ' ')
            break;
        return i;
      }
      return 0;
    }

    /// <summary>
    /// Reason to spearate nasty commend finding details such as
    /// <c>isBlockCommentStatusToggling</c>. Verifies output values
    /// Right only verifies using our unique starting style and ending style, ensures there's a
    /// comment line in between.
    /// Later, may be verify if found block contains property as well.
    /// </summary>
    /// <param name="start"> start of result comment block</param>
    /// <param name="end"> end of result comment block</param>
    private void GetSpecialCommentBlock(out int start, out int end) {
      start = end = -1;
      for (int i = 0; i < FileInfo.Lines.Length; i++) {
        var line = FileInfo.Lines[i];
        if (start == -1) {
          if (line.
            StartsWith("/***********************************************************************")
            || line.StartsWith("/*")
          ) {
            start = i;
            continue;
          }
        }
        else if (line.
          EndsWith("***********************************************************************/") ||
          line.EndsWith("*/")) {
          end = i;
          if (end - start < 2) { start = end = -1; }
          else break;
        }
      }
      if (start == -1 || end == -1 || end - start < 2) {
        System.Diagnostics.Process.Start(VSPath, "/Edit \"" + FileInfo.Path + "\"");
        throw new InvalidOperationException("Invalid comment block for " + FileInfo.Path);
      }
    }

    /// <summary>
    /// Get key, value pair and write them. Value would need formatting
    /// 
    /// if no key is found or found key cannot be mapped (has a length greater than limit) consider
    /// it as continuation multi-line value for previouso key
    /// Using 'Split' is not the right approach here. If key is not known,
    /// - this can be string like http://, an explanation of prevoius term
    /// - : for other means
    /// - For now, fix by testing, for anything exceptional..
    /// 
    /// Should be easy to find the key since "Key" strictly cannot contain ':'
    /// 
    /// <remarks> Only for lines inside comment block..</remarks>  
    /// </summary>
    /// <param name="index"> line number to udpate </param>
    private void ReadAndFormat(int index) {
      var line = FileInfo.Lines[index];
      if (line.StartsWith("*"))
        line = line.Substring(1);
      else if (line.StartsWith(" *"))
        line = line.Substring(2);
      if (string.IsNullOrEmpty(line))
        line = "*";
      else {
        // if on first line, key not found that's an error
        // on this comment: we don't know if this is first line or whatever line it is!
        var pos = line.IndexOf(':');
        if (pos == -1 || pos > 16 || line.Substring(0, pos).EndsWith("http") || line.Substring(0,
            pos).EndsWith("https")) {
          // About 3 chars for indenting values (that don't have keys)
          line = "*   " + line.TrimStart().TrimEnd();
        }
        else {
          string key = line.Substring(0, pos).TrimStart().TrimEnd();
          string newKey = string.Empty;
          if (CBOldKeysMap.TryGetValue(key, out newKey) == false && CBKeysSet.Contains(key) == false) {
            System.Diagnostics.Process.Start(VSPath, "/Edit \"" + FileInfo.Path + "\"");
            throw new InvalidOperationException("key not found: " + key + ", file: " + FileInfo.Path);
          }
          var val = line.Substring(pos + 1).TrimStart().TrimEnd();
          if (string.IsNullOrEmpty(newKey) == false)
            key = newKey;
          if (key == "Date")
            val = FormatDate(val);
          int spaceCount = MaxKeyLength - key.Length;
          line = "* " + key + (spaceCount>0?new string(' ', spaceCount):"") + ": " + val;
        }
      }
      // Debug
      // Console.WriteLine(line);
      // Consider dirty flag setting some where near here..
      FileInfo.Lines[index] = line;
    }

    /// <summary>
    /// Sets the starting and ending line of the comment block
    /// </summary>
    /// <param name="isStart"> is it the start of comment block </param>
    /// <param name="index"> line number to udpate </param>
    private void SetCBEdgeLine(bool isStart, int index) {
      const string edgeMarker = 
      "***************************************************************************************************";
      FileInfo.Lines[index] = isStart? ("/" + edgeMarker) : (edgeMarker + "/");
    }

    /// <summary>
    /// <param name="FileInfo.Path">Path of source file</param>  
    /// <remarks> Currently applies only to files.</remarks>  
    /// <returns> Returns necessary suffix of file path.</returns>  
    /// </summary>
    private string GetSimplifiedPath(string path) {
        return IsDirectory ? (path.StartsWith(Path) ? path.
            Substring(Path.Length + 1) : string.IsNullOrEmpty(path) ? "." :
            path) : path;
    }

    /// <summary>
    /// <param name="FileInfo.Path">Path of source file</param>  
    /// <remarks> Currently applies only to files.</remarks>  
    /// <returns> Returns necessary suffix of file path.</returns>  
    /// </summary>
    private string FormatDate(string dateStr) {
      if (string.IsNullOrEmpty(dateStr))
        return dateStr;
      var pos = dateStr.IndexOf('(');
      string info = string.Empty;
      if (pos != -1) {
        info = " (" + dateStr.Substring(pos+1);
        dateStr = dateStr.Substring(0, pos);
      }
      try {
        DateTime parsedDate = DateTime.Parse(dateStr);
        System.Globalization.DateTimeFormatInfo dtfi = System.Globalization.CultureInfo.CreateSpecificCulture("en-US").DateTimeFormat;
        dtfi.DateSeparator = "-";
        dtfi.ShortDatePattern = @"yyyy/MM/dd";
        string result = parsedDate.ToString("d", dtfi);
        return (result.Length > dateStr.Length ? dateStr:result) + info;
      } catch (Exception e) {
        Console.WriteLine("invalid date string: " + dateStr + ", file: " + FileInfo.Path);
        System.Diagnostics.Process.Start(VSPath, "/Edit \"" + FileInfo.Path + "\"");
        throw e;
      }
    }

    /// <summary>
    /// ToDo:  replace this method
    /// Simplified method: check if we are going outside of comment block if previously we were
    /// inside. Or check an oppositve condition..
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
    /// ToDo: update for new design
    /// Set an action based on user choice and perform action to specified file
    /// This action is stream (file content) editing for the file. Right now,
    /// following caveats are not taken into account,
    ///  Unmanageable file size: >= 1 GB, check read files (API in msdn) limit
    /// Actions:
    /// - Replace Tabs
    /// - Do all styling to apply modern format in source code
    /// </summary>
    private void ProcessFile(string filePath) {
      var ext = new DirectoryInfo(filePath).Extension.Substring(1);
      if (IsInExclusionList(filePath, ext)) {
        Console.WriteLine(" [Ignored] " + GetSimplifiedPath(filePath));
        return;
      }
      FileInfo.Init(filePath);
      if (ShouldReplaceTabs)
        ReplaceTabs();
      FileInfo.ReadFile();
      if (ShouldIndent)
        Indent();
      FixDocumentation();
      if (FileInfo.IsModified) {
        Console.WriteLine(" " + GetSimplifiedPath(FileInfo.Path) + ": " + FileInfo.ModInfo);
        ModifiedFileCount++;
        ExtList.Add(ext);
        if (!ShouldSimulate)
          FileInfo.WriteFile();
      }
    }

    /// <summary>
    /// Check if directory qualifies to be in exclusion list
    /// Or if it is in extension list to be excluded for being a file
    /// </summary>
    private bool IsInExclusionList(string path, string extension="") {
      return string.IsNullOrEmpty(extension)? ExclusionDirList.Contains(new DirectoryInfo(path).
        Name) : ExclusionExtList.Contains(extension);
    }

    /// <summary>
    /// Process provided directory (recurse), due to recursion the parameter cannot be replaced
    /// with class property
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
      if (ShouldSimulate)
        Console.WriteLine("Simulated summary:");
      Console.WriteLine("Number of files modified: " + ModifiedFileCount);
      Console.WriteLine("Following source file types covered:");
      if (ExtList.Count == 0)
        Console.Write( " [Empty]");
      foreach (var ext in ExtList)
        Console.Write(" " + ext + ",");
      Console.WriteLine();
    }

    /// <summary>
    /// Run automation for the app
    /// </summary>
    public void Run() {
      Console.WriteLine("Processing " + (IsDirectory ? "Directory: " + Path + 
        ", File list:" : "File:"));
      if (IsDirectory) {
        ProcessDirectory(Path);
      } else
        ProcessFile(Path);
    }
  }
}
