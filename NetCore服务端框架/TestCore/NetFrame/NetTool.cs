using System;
using System.Collections.Generic;
using System.Text;

namespace NetFrame
{
    public static class NetTool{
        /// <summary>使用GUID生成字符串ID </summary>
        public static string GenerateGuidID() {
            Guid guid = Guid.NewGuid();
            string GuidS = guid.ToString();
            //Console.WriteLine("guid.ToString() " + GuidS);
            string id = guid.ToString("N");//去除—（短线）得到32位字母和数字生成的字符串
            //Console.WriteLine("guid.ToString(\"N\") " + id);
            return id;
        }
        /// <summary>使用GUID生成数字ID</summary>
        public static long GenerateIntID() {
            byte[] buffer = Guid.NewGuid().ToByteArray();
            for (int i = 0; i < buffer.Length; i++) {
                //Console.WriteLine(buffer[i]);
            }
            return BitConverter.ToInt64(buffer, 0);
        }
    }
}
