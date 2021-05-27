using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocToHTML.Extention;

namespace DocToHTML.Extention
{
    static class StringExtension
    {

        public static String Token(this String s, int size)
        {
            if (s.Length <= size)
            {
                return s;
            }

            string ss = "";
            string[] sdelim = new[] { " ", "," };

            while (s.Length > size)
            {
                string s1 = s.Substring(0, size);

                int i = size - 1;
                while (i > 0 && sdelim.Contains(s.Substring(i, 1)) == false)
                {
                    i--;
                }
                if (i > 0)
                {
                    ss += s.Substring(0, i + 1) + "\r\n";
                    s = s.Substring(i);
                }
                else
                {
                    ss += s.Substring(0, size) + "\r\n";
                    s = s.Substring(size + 1);
                }

            }

            return ss;
        }

        public static String Split(this String s, int size)
        {
            if (s.Length < size)
            {
                return s;
            }

            string ss = "";

            while (s.Length > 0)
            {
                int k = s.IndexOf("\r\n");
                if (k != -1)
                {
                    string stemp = Token(s.Substring(0, k), size);

                    ss += stemp + "\r\n";

                    s = s.Substring(k+2);

                }
                else
                {
                    ss += s;
                    s = "";
                }
            }


            s.IndexOf('\n');
            string separate = " ,";
            string str = "";
            int index = 0;

            str = s.Substring(0, size);
            index = str.Length;

            while (index > 0 && separate.Contains(str.Substring(index - 1, 1)) == false)
            {
                index--;
            }

            str = s.Substring(0, index) + "\r\n" + s.Substring(index);

            return str;
        }

    }
}
