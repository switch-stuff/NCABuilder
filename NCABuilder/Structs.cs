using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Numerics;
using static NCABuilder.Utils;

namespace NCABuilder
{
    internal class Structs
    {
        public static uint NCA2 = 0x3241434e;
        public static uint NCA3 = 0x3341434e;

        public static byte NCAType_Digital = 0;
        public static byte NCAType_Cartridge = 1;

        public static byte ContentType_Program = 0;
        public static byte ContentType_Meta = 1;
        public static byte ContentType_Control = 2;
        public static byte ContentType_Manual = 3;
        public static byte ContentType_Data = 4;
        public static byte ContentType_AOC = 5;

        public static byte CryptoType_Original = 0;
        public static byte CryptoType_Updated = 2;

        public static byte KeyIndex_Application = 0;
        public static byte KeyIndex_Ocean = 1;
        public static byte KeyIndex_System = 2;

        public static byte KeyGeneration_Firmware100_230 = 0;
        public static byte KeyGeneration_Firmware300 = 0;
        public static byte KeyGeneration_Firmware301_302 = 3;
        public static byte KeyGeneration_Firmware400_410 = 4;
        public static byte KeyGeneration_Firmware500 = 5;

        public static int MinFirmVersion_Firmware_100_230 = 0;
        public static int MinFirmVersion_Firmware_300 = 0xC000000;
        public static int MinFirmVersion_Firmware_301 = 0xC010000;
        public static int MinFirmVersion_Firmware_302 = 0xC020000;
        public static int MinFirmVersion_Firmware_400 = 0x10000000;
        public static int MinFirmVersion_Firmware_401 = 0x10010000;
        public static int MinFirmVersion_Firmware_410 = 0x10100000;
        public static int MinFirmVersion_Firmware_500 = 0x14000000;
        public static int MinFirmVersion_Firmware_501 = 0x14010000;
        public static int MinFirmVersion_Firmware_502 = 0x14020000;
        public static int MinFirmVersion_Firmware_510 = 0x14100000;

        public static byte[] Header
            (
            byte[] Signature1,
            byte[] Signature2,
            uint Magic,
            byte NCAType,
            byte ContentType,
            byte CryptoType,
            byte KeyIndex,
            ulong NCASize,
            ulong TitleID,
            uint SDKVer,
            byte KeyGeneration,
            byte[] RightsID,
            byte[] Entry1,
            byte[] Entry2,
            byte[] Entry3,
            byte[] Entry4,
            byte[] Hash1,
            byte[] Hash2,
            byte[] Hash3,
            byte[] Hash4,
            byte[] KeyArea
            )
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(Signature1);
            Final.Write(Signature2);
            Final.Write(Magic);
            Final.Write(NCAType);
            Final.Write(ContentType);
            Final.Write(CryptoType);
            Final.Write(KeyIndex);
            Final.Write(NCASize);
            Final.Write(TitleID);
            Final.Write(0);
            Final.Write(SDKVer);
            Final.Write(KeyGeneration);
            Final.Write(Pad(0xF));
            Final.Write(RightsID);
            Final.Write(Entry1);
            Final.Write(Entry2);
            Final.Write(Entry3);
            Final.Write(Entry4);
            Final.Write(Hash1);
            Final.Write(Hash2);
            Final.Write(Hash3);
            Final.Write(Hash4);
            Final.Write(KeyArea);
            Final.Write(Pad(0xC0));
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] Entry(uint StartOffset, uint EndOffset)
        {
            uint[] StartSector =
            {
                StartOffset / 0x200,
                EndOffset / 0x200,
                1,
                0
            };
            return StartSector.SelectMany(BitConverter.GetBytes).ToArray();
        }

        public static byte[] KeyArea(byte[] Key, byte[] Input)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(Pad(0x10));
            Final.Write(Pad(0x10));
            Final.Write(Input);
            Final.Write(Pad(0x10));
            Final.Dispose();
            return CryptoInitialisers.AES_EBC(Key, Mem.ToArray());
        }

        public static byte Type_PFS0 = 2;
        public static byte Type_RomFS = 3;

        public static byte Crypto_None = 1;
        public static byte Crypto_CTR = 3;

        public static byte[] NCAHeader
            (
            byte[] InitialHeader,
            byte[] PartitionHeader1,
            byte[] PartitionHeader2,
            byte[] PartitionHeader3,
            byte[] PartitionHeader4
            )
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(InitialHeader);
            Final.Write(PartitionHeader1);
            Final.Write(PartitionHeader2);
            Final.Write(PartitionHeader3);
            Final.Write(PartitionHeader4);
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] SectionHeader(byte FileSystem, byte Section, byte CryptoType, byte[] FSBlock)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write((byte)2);
            Final.Write((byte)0);
            Final.Write(Section);
            Final.Write(FileSystem);
            Final.Write(CryptoType);
            Final.Write(Pad(3));
            Final.Write(FSBlock);
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] PFS0(byte[] Hash, uint BlockSize, uint PFSType, ulong HashTableOffset, ulong HashTableSize, ulong RelativeOffset, ulong RelativeByteSize, uint TypeCTR)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(Hash);
            Final.Write(BlockSize);
            Final.Write(PFSType);
            Final.Write(HashTableOffset);
            Final.Write(HashTableSize);
            Final.Write(RelativeOffset);
            Final.Write(RelativeByteSize);
            Final.Write(Pad(0xF4));
            Final.Write(TypeCTR);
            Final.Write(Pad(0xB8));
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] RomFS(byte[] Level0, byte[] Level1, byte[] Level2, byte[] Level3, byte[] Level4, byte[] Level5)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(Level0);
            Final.Write(Level1);
            Final.Write(Level2);
            Final.Write(Level3);
            Final.Write(Level4);
            Final.Write(Level5);
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] RomFSSetSizeLevelBody(byte[] Hash, int Size)
        {
            return Hash.Concat(Pad(Size)).ToArray();
        }

        public static byte[] RomFS_IVFC(byte[] Hash, byte[] Level0Hdr, byte[] Level1Hdr, byte[] Level2Hdr, byte[] Level3Hdr, byte[] Level4Hdr, byte[] Level5Hdr, uint TypeCTR)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(0x43465649);
            Final.Write(0x20000);
            Final.Write(0x20);
            Final.Write(7);
            Final.Write(Level0Hdr);
            Final.Write(Level1Hdr);
            Final.Write(Level2Hdr);
            Final.Write(Level3Hdr);
            Final.Write(Level4Hdr);
            Final.Write(Level5Hdr);
            Final.Write(Pad(0x20));
            Final.Write(Hash);
            Final.Write(Pad(0x5C));
            Final.Write(TypeCTR);
            Final.Write(Pad(0xB8));
            return Mem.ToArray();
        }

        public static byte[] Level(ulong LevelOffset, ulong LevelSize, uint LevelBlockSize)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(LevelOffset);
            Final.Write(LevelSize);
            Final.Write(LevelBlockSize);
            Final.Write(0);
            Final.Dispose();
            return Mem.ToArray();
        }

        public static byte[] Level0(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] Level1(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] Level2(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] Level3(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] Level4(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] Level5(ulong Offset, ulong Size)
        {
            return Level(Offset, Size, 0xE);
        }

        public static byte[] RomFSConstructor(byte[] Hash, ulong SizeLevel0, ulong SizeLevel1, ulong SizeLevel2, ulong SizeLevel3, ulong SizeLevel4, ulong SizeLevel5, byte Section)
        {
            return RomFS_IVFC(
                Hash,
            Level0(0, SizeLevel0),
            Level1(SizeLevel0, SizeLevel1),
            Level2(SizeLevel0 + SizeLevel1, SizeLevel2),
            Level3(SizeLevel0 + SizeLevel1 + SizeLevel2, SizeLevel3),
            Level4(SizeLevel0 + SizeLevel1 + SizeLevel2 + SizeLevel3, SizeLevel4),
            Level5(SizeLevel0 + SizeLevel1 + SizeLevel2 + SizeLevel3 + SizeLevel4, SizeLevel5), Section);
        }

        public static byte[] NCA(byte[] Header, ref byte[] Body)
        {
            return Header.Concat(Align(ref Body, 0x200)).ToArray();
        }

        public static byte[] Ticket(string Issuer, byte[] Titlekey, byte Type, byte KeyRevision, byte[] RightsID)
        {
            var Mem = new MemoryStream();
            var Final = new BinaryWriter(Mem);
            Final.Write(0x10004);
            Final.Write(Pad(0x13C));
            Final.Write(Encoding.UTF8.GetBytes(Issuer));
            Final.Write(Pad(0x26));
            Final.Write(Titlekey);
            if (Titlekey.Length <= 0x10)
            {
                Final.Write(Pad(0xF0));
            };
            Final.Write(Type);
            Final.Write(0);
            Final.Write(KeyRevision);
            Final.Write(Pad(0x1A));
            Final.Write(RightsID);
            Final.Write((ulong)(0));
            Final.Write(0x2C0);
            Final.Dispose();
            return Mem.ToArray();
        }
    }
}