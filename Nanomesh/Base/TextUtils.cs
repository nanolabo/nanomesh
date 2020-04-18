using System;
using System.Collections.Generic;
using System.Text;

namespace Nanolabo
{
    public static class TextUtils
    {
        private static Dictionary<char, string> normalToStriked = new Dictionary<char, string>
        {
            { '0', "0̶" },
            { '1', "1̶" },
            { '2', "2̶" },
            { '3', "3̶" },
            { '4', "4̶" },
            { '5', "5̶" },
            { '6', "6̶" },
            { '7', "7̶" },
            { '8', "8̶" },
            { '9', "9̶" },
        };

        public static string StrikeThrough(int input)
        {
            //string inputStr = input.ToString();
            //StringBuilder strbldr = new StringBuilder();
            //strbldr.Append(' ');
            //for (int i = 0; i < inputStr.Length; i++)
            //{
            //    strbldr.Append(normalToStriked[inputStr[i]]);
            //}
            //return strbldr.ToString();
            return "(" + input + ")";
        }
    }
}
