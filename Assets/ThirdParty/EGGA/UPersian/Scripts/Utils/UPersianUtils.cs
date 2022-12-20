namespace UPersian.Utils
{
    public static class UPersianUtils{

        /// <summary>
        /// and extention for strings to fix RTL Languages to unity display format
        /// </summary>
        /// <param name="str">string to fix</param>
        /// <returns>corrected RTL string</returns>
        public static string RtlFix(this string str)
        {
            //ﺉﻚﻙﯤ
            str = str.Replace('ی', 'ﻱ');
            //str = str.Replace( 'ی','ﺉ');
            str = str.Replace('ک', 'ﻙ');
            //str = str.Replace('ﻚ', 'ک');
            str = ArabicSupport.ArabicFixer.Fix(str,true,false);
            str = str.Replace('ﺃ', 'آ');
            return str;
        }

        public static bool IsRtl(this string str)
        {
            var isRtl = false;
            foreach (var _char in str)
            {
                if ((_char >= 1536 && _char <= 1791) || (_char >= 65136 && _char <= 65279))
                {
                    isRtl = true;
                    break;
                }
            }
            return isRtl;
        }
    }
}
