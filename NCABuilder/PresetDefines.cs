using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static NCABuilder.Structs;

namespace NCABuilder
{
    internal class PresetDefines
    {
        public static void Standard_Application(byte ContentType, byte KeyIdx, byte KeyGeneration, string RootPath, ulong TitleID, uint Ver, TextBox Headerkey, TextBox KAEK)
        {
            uint Section2Offset = 0x4000;
            Directory.CreateDirectory($"{RootPath}/Temp");
            var BuildLogo = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                    Arguments = $"\"{RootPath}/Logo\" \"{RootPath}/Temp/Logo.pfs\""
                }
            };
            BuildLogo.Start();
            BuildLogo.WaitForExit();
            var PFSCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/Logo.pfs", 0x1000, 1, Crypto_None, 0);

            uint Section1Offset = 0x4000 + (uint)PFSCalcs.Item2.Length;
            var BuildRomFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\""
                }
            };
            BuildRomFS.Start();
            BuildRomFS.WaitForExit();
            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, 2);
            var RomLength = RomHead.Item2.Length;
            uint Section0Offset = 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength;
            var BuildExe = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                    Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\""
                }
            };
            BuildExe.Start();
            BuildExe.WaitForExit();
            var ExeCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, 1);
            byte[] Exe = ExeCalcs.Item2;
            Utils.Align(ref Exe, 0x8000);

            byte[] Section2 = Entry(Section2Offset, 0x4000 + (uint)PFSCalcs.Item2.Length);
            byte[] Section1 = Entry(Section1Offset, 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength);
            byte[] Section0 = Entry(Section0Offset, 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength + (uint)Exe.Length);

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Text.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Text.Substring(32, 32));

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK.Text), Key);

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100), 
                NCA3, 
                NCAType_Digital, 
                ContentType, 
                CryptoType_Updated, 
                KeyIdx,
                0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength + (uint)Exe.Length, 
                TitleID, 
                Ver, 
                KeyGeneration, 
                Utils.Pad(0x10), 
                Section0, 
                Section1, 
                Section2, 
                Utils.Pad(0x10), 
                CryptoInitialisers.GenSHA256Hash(ExeCalcs.Item1),
                CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
                CryptoInitialisers.GenSHA256Hash(PFSCalcs.Item1), 
                Utils.Pad(0x20), 
                Keys);

            byte[] Final = NCAHeader(
                Head,
                ExeCalcs.Item1,
                RomHead.Item1,
                PFSCalcs.Item1, 
                Utils.Pad(0x200));

            File.WriteAllBytes($"{RootPath}/Temp/Header", CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0));

            File.WriteAllBytes($"{RootPath}/Temp/LogoPartition", PFSCalcs.Item2);

            File.WriteAllBytes($"{RootPath}/Temp/EncryptedNull", CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000000000C0"), Utils.Pad(0x3400)));

            File.WriteAllBytes($"{RootPath}/Temp/RomFSPartition", CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000020000000000000000" +
            $"{Utils.BytesToString(BitConverter.GetBytes(Section1Offset >> 4).Reverse().ToArray())}"), RomHead.Item2));

            File.WriteAllBytes($"{RootPath}/Temp/ExeFSPartition", CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000010000000000000000" +
                $"{Utils.BytesToString(BitConverter.GetBytes(Section0Offset >> 4).Reverse().ToArray())}"), Exe));

            Directory.CreateDirectory($"{RootPath}/Output");

            string[] Files =
            {
                $"{RootPath}/Temp/Header",
                $"{RootPath}/Temp/EncryptedNull",
                $"{RootPath}/Temp/LogoPartition",
                $"{RootPath}/Temp/RomFSPartition",
                $"{RootPath}/Temp/ExeFSPartition"
            };
            Utils.ConcatenateFiles($"{RootPath}/Output/Generated.nca", Files);
        }
    }
}