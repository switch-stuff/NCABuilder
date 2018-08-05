using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NCABuilder
{
    internal class PFS0Constructor
    {      
        public static Tuple<byte[], byte[]> MakePFS0(string InputFile, int HashSize, uint Section, byte Crypto, byte CTRO)
        {
            byte[] ReadInputFile = File.ReadAllBytes(InputFile);
            ulong PreAlignmentSize = (ulong)ReadInputFile.LongLength;
            byte[] Aligned = Utils.Align(ref ReadInputFile, HashSize);
            var MemStrm = new MemoryStream();
            var Writer = new BinaryWriter(MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)ReadInputFile.Length / HashSize)))
            {
                var SHA = new SHA256Managed();
                if (i < ((int)Math.Ceiling((decimal)Aligned.Length / HashSize) - 1))
                {                 
                    Writer.Write(SHA.ComputeHash(Aligned, i * HashSize, HashSize));
                    Writer.Flush();
                }
                else
                {
                    Writer.Write(SHA.ComputeHash(Aligned, Aligned.Length - HashSize, HashSize - (Aligned.Length - (int)PreAlignmentSize)));
                    Writer.Flush();
                }
            }
            var HashArray = MemStrm.ToArray();
            Writer.Dispose();
            MemStrm.Dispose();

            var Header = Structs.PFS0(CryptoInitialisers.GenSHA256Hash(HashArray), (uint)HashSize, 2, 0, (ulong)HashArray.Length, (ulong)HashSize, PreAlignmentSize, CTRO);
            var FullHdr = Structs.SectionHeader(Structs.Type_PFS0, (byte)Section, Crypto, Header);
            return Tuple.Create(FullHdr, Utils.Align(ref HashArray, HashSize).Concat(Aligned).ToArray());
        }
    }
}