using System;
using System.Linq;
using static NCABuilder.Utils;
using static NCABuilder.CryptoInitialisers;
using static NCABuilder.Structs;
using System.IO;
using System.Security.Cryptography;

namespace NCABuilder
{
    class RomFSConstructor
    {
        public static Tuple<byte[],byte[]> MakeRomFS(string InputFile, uint Section, byte CTRO)
        {
            byte[] ReadInputFile = File.ReadAllBytes(InputFile);

            ulong PreAlignmentSize = (ulong)ReadInputFile.LongLength;
            Align(ref ReadInputFile, 0x200);
            byte[] AlignForHashing = Align(ref ReadInputFile, 0x4000);
            var L4MemStrm = new MemoryStream();
            var L4Writer = new BinaryWriter(L4MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)AlignForHashing.Length / 0x4000)))
            {
                var SHA = new SHA256Managed();
                L4Writer.Write(SHA.ComputeHash(AlignForHashing, i * 0x4000, 0x4000));
                L4Writer.Flush();
            }
            byte[] Lvl4 = RomFSSetSizeLevelBody(L4MemStrm.ToArray(), ((int)Math.Ceiling((decimal)(PreAlignmentSize / 0x200) / 0x4000) * 0x4000) - (L4MemStrm.ToArray().Length));
            Align(ref Lvl4, 0x4000);
            L4Writer.Dispose();
            L4MemStrm.Dispose();

            var L3MemStrm = new MemoryStream();
            var L3Writer = new BinaryWriter(L3MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)Lvl4.Length / 0x4000)))
            {
                byte[] InputBlock = Lvl4.Skip(i * 0x4000).Take(0x4000).ToArray();
                L3Writer.Write(GenSHA256Hash(Align(ref InputBlock, 0x4000)));
            }
            byte[] Lvl3 = RomFSSetSizeLevelBody(L3MemStrm.ToArray(), ((int)Math.Ceiling((decimal)(Lvl4.Length / 0x200) / 0x4000) * 0x4000) - (L3MemStrm.ToArray().Length));
            Align(ref Lvl3, 0x4000);
            L3Writer.Dispose();
            L3MemStrm.Dispose();

            var L2MemStrm = new MemoryStream();
            var L2Writer = new BinaryWriter(L2MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)Lvl3.Length / 0x4000)))
            {
                byte[] InputBlock = Lvl3.Skip(i * 0x4000).Take(0x4000).ToArray();
                L2Writer.Write(GenSHA256Hash(Align(ref InputBlock, 0x4000)));
            }
            byte[] Lvl2 = RomFSSetSizeLevelBody(L2MemStrm.ToArray(), ((int)Math.Ceiling((decimal)(Lvl3.Length / 0x200) / 0x4000) * 0x4000) - (L2MemStrm.ToArray().Length));
            Align(ref Lvl2, 0x4000);
            L2Writer.Dispose();
            L2MemStrm.Dispose();

            var L1MemStrm = new MemoryStream();
            var L1Writer = new BinaryWriter(L1MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)Lvl2.Length / 0x4000)))
            {
                byte[] InputBlock = Lvl2.Skip(i * 0x4000).Take(0x4000).ToArray();
                L1Writer.Write(GenSHA256Hash(Align(ref InputBlock, 0x4000)));
            }
            var Lvl1 = RomFSSetSizeLevelBody(L1MemStrm.ToArray(), ((int)Math.Ceiling((decimal)(Lvl2.Length / 0x200) / 0x4000) * 0x4000) - (L1MemStrm.ToArray().Length));
            Align(ref Lvl1, 0x4000);
            L1Writer.Dispose();
            L1MemStrm.Dispose();

            var L0MemStrm = new MemoryStream();
            var L0Writer = new BinaryWriter(L0MemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)Lvl1.Length / 0x4000)))
            {
                byte[] InputBlock = Lvl1.Skip(i * 0x4000).Take(0x4000).ToArray();
                L0Writer.Write(GenSHA256Hash(Align(ref InputBlock, 0x4000)));
            }
            var Lvl0 = RomFSSetSizeLevelBody(L0MemStrm.ToArray(), ((int)Math.Ceiling((decimal)(Lvl1.Length / 0x200) / 0x4000) * 0x4000) - (L0MemStrm.ToArray().Length));
            Align(ref Lvl0, 0x4000);
            L0Writer.Dispose();
            L0MemStrm.Dispose();

            var SuperBlockHashMemStrm = new MemoryStream();
            var SuperBlockHashWriter = new BinaryWriter(SuperBlockHashMemStrm);
            foreach (int i in Enumerable.Range(0, (int)Math.Ceiling((decimal)Lvl0.Length / 0x4000)))
            {
                byte[] InputBlock = Lvl0.Skip(i * 0x4000).Take(0x4000).ToArray();
                SuperBlockHashWriter.Write(GenSHA256Hash(Align(ref InputBlock, 0x4000)));
            }
            var SuperBlockHash = SuperBlockHashMemStrm.ToArray();
            SuperBlockHashWriter.Dispose();
            SuperBlockHashMemStrm.Dispose();

            byte[] RomHdr = RomFSConstructor(SuperBlockHash, (ulong)Lvl0.Length, (ulong)Lvl1.Length, (ulong)Lvl2.Length, (ulong)Lvl3.Length, (ulong)Lvl4.Length, PreAlignmentSize, CTRO);

            byte[] Rom = RomFS(Lvl0, Lvl1, Lvl2, Lvl3, Lvl4, ReadInputFile);
            byte[] Hdr = SectionHeader(Type_RomFS, (byte)Section, Crypto_CTR, RomHdr);
            return Tuple.Create(Hdr, Rom);
        }
    }
}
