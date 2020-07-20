using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsUnityDebugHelper
{
    public static class OuputParseUtil
    {
        public static string[] SplitAndTrim(this string input, params char[] separator)
        {
            return input.Split(separator).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, char[] separator, int count)
        {
            return input.Split(separator, count).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, char[] separator, StringSplitOptions options)
        {
            return input.Split(separator, options).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, char[] separator, int count, StringSplitOptions options)
        {
            return input.Split(separator, count, options).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, string separator)
        {
            return input.Split(new string[] { separator }, StringSplitOptions.None).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, string separator, StringSplitOptions options)
        {
            return input.Split(new string[] { separator }, options).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, string[] separator, StringSplitOptions options)
        {
            return input.Split(separator, options).Select(str => str.Trim()).ToArray();
        }

        public static string[] SplitAndTrim(this string input, string[] separator, int count, StringSplitOptions options)
        {
            return input.Split(separator, count, options).Select(str => str.Trim()).ToArray();
        }

        public static string[] GetGameObjectNamesFromInfo(this string infoString)
        {
            string[] objInfos = infoString.SplitAndTrim("\r\n", StringSplitOptions.RemoveEmptyEntries);

            if (!objInfos[0].StartsWith("UnityEngine.GameObject"))
            {
                throw new ArgumentException($"Unexpected Type name is printed\n\tExpected: UnityEngine.GameObject\n\tReal: {objInfos[0]}");
            }

            // last element is always ">"
            if (objInfos.Length == 2)
            {
                return null;
            }

            string[] objNames = new string[objInfos.Length - 2];

            int startIdx, endIdx;
            for (int i = 0; i < objNames.Length; ++i)
            {
                startIdx = objInfos[i + 1].IndexOf('"') + 1;
                endIdx = objInfos[i + 1].LastIndexOf('(') - 2;

                objNames[i] = objInfos[i + 1].Substring(startIdx, endIdx - startIdx + 1);
            }

            return objNames;
        }

        public static (string name, string value) TryGetFieldInfo(this string fieldInfo, out bool isSuccess)
        {
            int colonIndex = fieldInfo.IndexOf(':');
            if(colonIndex == -1)
            {
                isSuccess = false;
                return ("", "");
            }

            string name = fieldInfo.Substring(0, colonIndex);
            string value;
            try 
            {
                value = fieldInfo.Substring(colonIndex + 2);
            } 
            catch(IndexOutOfRangeException)
            {
                value = "";
            }
            isSuccess = true;
            return (name, value);
        }
    }
}
