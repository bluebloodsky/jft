using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JFT
{
    class MyMethod
    {
        public static string date2str(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return "";
            }
            return date.ToString(GlobalCache.CacheData.str_date_format);
        }

        public static string date2str_out(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return "";
            }
            return date.ToString(GlobalCache.CacheData.str_date_format_out);
        }

        public static DateTime str2Date(string str)
        {
            if (null == str || str.Trim() == "")
            {
                return DateTime.MinValue;
            }
            return DateTime.Parse(str);
        }
        public static DateTime obj2date(Object obj)
        {
            if (null == obj)
            {
                return DateTime.MinValue;
            }
            return (DateTime)obj;
        }
        public static Object date2obj(DateTime date)
        {
            if (date == DateTime.MinValue)
            {
                return null;
            }
            return date;
        }

        public static int obj2int(Object obj)
        {
            try
            {
                return int.Parse(obj.ToString());
            }
            catch
            {
                return 0;
            }
        }

        public static double obj2double(Object obj)
        {
            try
            {
                double result = double.Parse(obj.ToString());
                return result;
            }
            catch
            {
                return 0;
            }
        }
        public static Object double2obj(double orig)
        {
            if (orig == 0)
            {
                return null;
            }
            else
            {
                return orig;
            }

        }

        public static string int2chinese(int money)
        {
            Dictionary<char, string> cvt = new Dictionary<char, string>();
            cvt.Add('0', "零");
            cvt.Add('1', "壹");
            cvt.Add('2', "贰");
            cvt.Add('3', "叁");
            cvt.Add('4', "肆");
            cvt.Add('5', "伍");
            cvt.Add('6', "陆");
            cvt.Add('7', "柒");
            cvt.Add('8', "捌");
            cvt.Add('9', "玖");

            Dictionary<int, string> unit = new Dictionary<int, string>();
            unit.Add(0, "");
            unit.Add(1, "");
            unit.Add(2, "拾");
            unit.Add(3, "佰");
            unit.Add(4, "仟");
            unit.Add(5, "万");
            unit.Add(6, "拾");
            unit.Add(7, "佰");
            unit.Add(8, "仟");
            unit.Add(9, "亿");

            string s = money.ToString();
            StringBuilder sb = new StringBuilder();
            int l = s.Length;
            int zero_tag = s.TrimEnd('0').Length;

            foreach (var c in s)
            {
                if (--zero_tag < 0)
                    break;
                sb.Append(cvt[c]);
                sb.Append(unit[l--]);
            }
            return sb.ToString();
        
        }
    }
}
