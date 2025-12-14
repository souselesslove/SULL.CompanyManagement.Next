using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace VOL.Core.Utilities
{
    public static class VerificationCode
    {
        private static readonly string[] Chars = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "P", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        private static readonly string[] Numbers = new string[] { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static string RandomText(int type = 1)
        {
            string code = "";//产生的随机数
            int temp = -1;
            Random rand = new Random();
            if (type == 0)
            {
                for (int i = 1; i < 5; i++)
                {
                    if (temp != -1)
                    {
                        rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));
                    }
                    int t = rand.Next(61);
                    if (temp != -1 && temp == t)
                    {
                        return RandomText(0);
                    }
                    temp = t;
                    code += Chars[t];
                }
            } 
            else if (type == 1)
            {
                for (int i = 1; i < 5; i++)
                {
                    if (temp != -1)
                    {
                        rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));
                    }
                    int t = rand.Next(10);
                    if (temp != -1 && temp == t)
                    {
                        return RandomText(1);
                    }
                    temp = t;
                    code += Numbers[t];
                }
            }
            return code;
        }
        public static string CreateBase64Image(string code)
        {
            return VerificationCodeServices.CreateBase64Image(code);
        }
    }
}
