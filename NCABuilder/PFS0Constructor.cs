using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace NCABuilder
{
    internal class PFS0Constructor
    {
        public static Tuple<byte[], byte[]> MakePFS0(string InputFile, int HashSize, uint Section, byte Crypto, byte CTRO)
        {
            byte[] Buf = new byte[HashSize];
            var Input = File.Open(InputFile, FileMode.Open);
            ulong PreAlignmentSize = (ulong)new FileInfo(InputFile).Length;
            var MemStrm = new MemoryStream();
            var Writer = new BinaryWriter(MemStrm);
            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)Input.Length / HashSize) * HashSize) / HashSize)))
            {
                var SHA = new SHA256Managed();
                if (i < ((int)Math.Ceiling((decimal)(((int)Math.Ceiling((decimal)Input.Length / HashSize) * HashSize) / HashSize) - 1)))
                {
                    Input.Read(Buf, 0, HashSize);
                    Utils.Align(ref Buf, HashSize);
                    Writer.Write(SHA.ComputeHash(Buf, 0, HashSize));
                }
                else
                {
                    Input.Read(Buf, 0, HashSize - (((int)Math.Ceiling((decimal)Input.Length / HashSize) * HashSize) - (int)PreAlignmentSize));
                    Writer.Write(SHA.ComputeHash(Buf, 0, HashSize - ((int)Math.Ceiling((decimal)(((int)Math.Ceiling((decimal)Input.Length / HashSize) * HashSize) - (int)PreAlignmentSize)))));
                }
            }
            var HashArray = MemStrm.ToArray();
            Writer.Dispose();
            MemStrm.Dispose();
            Input.Dispose();

            var Header = Structs.PFS0(CryptoInitialisers.GenSHA256Hash(HashArray), (uint)HashSize, 2, 0, (ulong)HashArray.Length, (ulong)HashArray.Length, PreAlignmentSize, CTRO);
            var FullHdr = Structs.SectionHeader(Structs.Type_PFS0, (byte)Section, Crypto, Header);
            return Tuple.Create(FullHdr, HashArray);
        }
    }
}