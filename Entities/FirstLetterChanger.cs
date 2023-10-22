using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VkCommentBot.Entities
{
    public static class FirstLetterChanger
    {    
        public static string ToUpperFirstLetter(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return string.Empty;
            // convert to char array of the string
            char[] letters = inputString.ToCharArray();
            // upper case the first char
            letters[0] = char.ToUpper(letters[0]);
            // return the array made of the new char array
            return new string(letters);
        }

        public static string ToLowerFirstLetter(string inputString)
        {
            if (string.IsNullOrEmpty(inputString))
                return string.Empty;
            // convert to char array of the string
            char[] letters = inputString.ToCharArray();
            // upper case the first char
            letters[0] = char.ToLower(letters[0]);
            // return the array made of the new char array
            return new string(letters);
        }
    }
}

