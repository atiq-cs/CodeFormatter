# Code Formatter
Formats source code to a consistent style that I follow for my competitive programming solution
source files. 
Following actions are automatically applied on the source file,
 - Tab replacement
 - Indentation Fix (both inside/outside comment)
 - Comment style fixing

Additionally, it supports simulation or `noop`.

Here's an example input file,

    /***************************************************************************
    * Problem Name: Palindromic Substrings
    * Problem URL : https://leetcode.com/problems/palindromic-substrings/
    * Date        : Oct 17 2017
    * Complexity  : O(n^3) Time, O(n) space
    * Author      : Atiq Rahman
    * Status      : Accepted
    * Notes       : Track palandrome for each index
    * meta        : tag-dp, tag-string, tag-leetcode-easy
    ***************************************************************************/

After formatting it looks like,

    /***************************************************************************************************
    * Title : Palindromic Substrings
    * URL   : https://leetcode.com/problems/palindromic-substrings/
    * Date  : 2017-10-17
    * Comp  : O(n^3) Time, O(n) space
    * Author: Atiq Rahman
    * Status: Accepted
    * Notes : Track palandrome for each index
    * meta  : tag-dp, tag-string, tag-leetcode-easy
    ***************************************************************************************************/

As we can see above it,
- changes max column wrap to 100,
- formats date
- Changes Key Value (on left side each line before :) to shorter format.

For More information on this software please have a look at the
[wiki](https://github.com/atiq-cs/CodeFormatter/wiki/Design-Requirements)