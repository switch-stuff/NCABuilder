using System;
using System.Collections.Generic;
using System.Linq;

namespace NCABuilder
{
    internal class Utils
    {

        // Produces a zero-byte array of a specified length.
        public static byte[] Pad(int Count)
        {
            return Enumerable.Repeat((byte)0x00, Count).ToArray();
        }

        public static byte[] Align(ref byte[] Input, int Pad)
        {
            int Length = (Input.Length + Pad - 1) / Pad * Pad;
            Array.Resize(ref Input, Length);
            return Input;
        }

        public static string BytesToString(byte[] Bytes)
        {
            return BitConverter.ToString(Bytes).Replace("-", "");
        }

        public static byte[] StringToBytes(string String)
        {
            return Enumerable.Range(0, String.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(String.Substring(x, 2), 16))
                .ToArray();
        }
    }
}