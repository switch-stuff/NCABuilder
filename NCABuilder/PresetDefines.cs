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
        //public static void Standard_Application
        //    (
        //    byte ContentType,
        //    byte KeyIdx,
        //    byte KeyGeneration,
        //    string RootPath,
        //    ulong TitleID,
        //    uint Ver,
        //    string Headerkey,
        //    string KAEK
        //    )
        //{
        //    uint Section2Offset = 0x4000;
        //    Directory.CreateDirectory($"{RootPath}/Temp");
        //    var BuildLogo = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
        //            Arguments = $"\"{RootPath}/Logo\" \"{RootPath}/Temp/Logo.pfs\"",
        //            WindowStyle = ProcessWindowStyle.Hidden
        //        }
        //    };
        //    BuildLogo.Start();
        //    BuildLogo.WaitForExit();
        //    var PFSCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/Logo.pfs", 0x1000, 1, Crypto_None, 0);

        //    uint Section1Offset = 0x4000 + (uint)PFSCalcs.Item2.Length;
        //    var BuildRomFS = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
        //            Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
        //            WindowStyle = ProcessWindowStyle.Hidden
        //        }
        //    };
        //    BuildRomFS.Start();
        //    BuildRomFS.WaitForExit();
        //    var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, 2);
        //    var RomLength = RomHead.Item2.Length;
        //    uint Section0Offset = 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength;
        //    var BuildExe = new Process
        //    {
        //        StartInfo = new ProcessStartInfo
        //        {
        //            FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
        //            Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
        //            WindowStyle = ProcessWindowStyle.Hidden
        //        }
        //    };
        //    BuildExe.Start();
        //    BuildExe.WaitForExit();
        //    var ExeCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, 1);
        //    byte[] Exe = ExeCalcs.Item2;
        //    Utils.Align(ref Exe, 0x8000);

        //    byte[] Section2 = Entry(Section2Offset, 0x4000 + (uint)PFSCalcs.Item2.Length);
        //    byte[] Section1 = Entry(Section1Offset, 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength);
        //    byte[] Section0 = Entry(Section0Offset, 0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength + (uint)Exe.Length);

        //    byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
        //    byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

        //    byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

        //    byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

        //    byte[] Head = Header(
        //        Utils.Pad(0x100),
        //        Utils.Pad(0x100),
        //        NCA3,
        //        NCAType_Digital,
        //        ContentType,
        //        CryptoType_Updated,
        //        KeyIdx,
        //        0x4000 + (uint)PFSCalcs.Item2.Length + (uint)RomLength + (uint)Exe.Length,
        //        TitleID,
        //        Ver,
        //        KeyGeneration,
        //        Utils.Pad(0x10),
        //        Section0,
        //        Section1,
        //        Section2,
        //        Utils.Pad(0x10),
        //        CryptoInitialisers.GenSHA256Hash(ExeCalcs.Item1),
        //        CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
        //        CryptoInitialisers.GenSHA256Hash(PFSCalcs.Item1),
        //        Utils.Pad(0x20),
        //        Keys);

        //    byte[] Final = NCAHeader(
        //        Head,
        //        ExeCalcs.Item1,
        //        RomHead.Item1,
        //        PFSCalcs.Item1,
        //        Utils.Pad(0x200));

        //    Directory.CreateDirectory($"{RootPath}/Output");
        //    var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);
        //    Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);
        //    Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000000000C0"), Utils.Pad(0x3400)), 0, 0x3400);
        //    Output.Write(PFSCalcs.Item2, 0, PFSCalcs.Item2.Length);
        //    Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000020000000000000000{Utils.BytesToString(BitConverter.GetBytes(Section1Offset >> 4).Reverse().ToArray())}"), RomHead.Item2), 0, RomHead.Item2.Length);
        //    Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000010000000000000000{Utils.BytesToString(BitConverter.GetBytes(Section0Offset >> 4).Reverse().ToArray())}"), Exe), 0, ExeCalcs.Item2.Length);
        //    Output.Dispose();
        //    MessageBox.Show("Done!");
        //}

        public static void ExeFS_RomFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");

            uint Section1Offset = 0xC00;

            var BuildRomFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                    Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            BuildRomFS.Start();
            BuildRomFS.WaitForExit();

            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, 2);
            var RomLength = RomHead.Item2.Length;

            var InRom = File.Open($"{RootPath}/Temp/RomFS.romfs", FileMode.Open);

            uint Section0Offset = 0xC00 + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000));

            var BuildPFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                    Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            BuildPFS.Start();
            BuildPFS.WaitForExit();

            var ExeCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, 1);
            var ExeSize = new FileInfo($@"{RootPath}/Temp/ExeFS.pfs").Length;

            byte[] Section1 = Entry(Section1Offset, 0xC00 + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)));
            byte[] Section0 = Entry(Section0Offset, 0xC00 + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)) + (uint)ExeCalcs.Item2.Length + (uint)(((int)Math.Ceiling((decimal)ExeSize / 0x4000) * 0x4000)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType_Updated,
                KeyIdx,
                0xC00 + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)) + (uint)ExeCalcs.Item2.Length + ((uint)(((int)Math.Ceiling((decimal)ExeSize / 0x200) * 0x200))),
                TitleID,
                Ver,
                KeyGeneration,
                Utils.Pad(0x10),
                Section0,
                Section1,
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                CryptoInitialisers.GenSHA256Hash(ExeCalcs.Item1),
                CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Keys);

            byte[] Final = NCAHeader(
                Head,
                ExeCalcs.Item1,
                RomHead.Item1,
                Utils.Pad(0x200),
                Utils.Pad(0x200));

            byte[] CryptoBuffer = new byte[0x4000];

            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);

            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000020000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);

            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);

            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)) / 0x4000))
            {
                InRom.Read(CryptoBuffer, 0, 0x4000);
                Utils.Align(ref CryptoBuffer, 0x4000);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000020000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x4000);
                Counter = Counter + 0x400;
            }

            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000010000000000000000{Counter.ToString("X8")}"), ExeCalcs.Item2.Concat(File.ReadAllBytes($"{RootPath}/Temp/ExeFS.pfs")).ToArray()), 0, ExeCalcs.Item2.Length + (int)ExeSize);

            Counter = Counter + (uint)((ExeSize + (uint)ExeCalcs.Item2.Length) >> 4);

            Output.Write(CryptoInitialisers.AES_CTR(
                    Key,
                    Utils.StringToBytes($"000000010000000000000000" +
                    $"{Counter.ToString("X8")}"),
                    Utils.Pad(((int)Math.Ceiling((decimal)Output.Length / 0x4000) * 0x4000) - (int)Output.Length)), 0, ((int)Math.Ceiling((decimal)Output.Length / 0x4000) * 0x4000) - (int)Output.Length);

            Output.Dispose();
            InRom.Dispose();
            MessageBox.Show("Done!");
        }

        public static void RomFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            uint Section0Offset = 0xC00;
            var BuildRomFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                    Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            BuildRomFS.Start();
            BuildRomFS.WaitForExit();

            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/RomFS.romfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x4000) * 0x4000)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType_Updated,
                KeyIdx,
                0xC00 + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x4000) * 0x4000)),
                TitleID,
                Ver,
                KeyGeneration,
                Utils.Pad(0x10),
                Section0,
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Keys);

            byte[] Final = NCAHeader(
                Head,
                RomHead.Item1,
                Utils.Pad(0x200),
                Utils.Pad(0x200),
                Utils.Pad(0x200));

            byte[] CryptoBuffer = new byte[0x4000];
            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);

            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);

            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);

            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InCrypt.Length / 0x4000) * 0x4000)) / 0x4000))
            {
                InCrypt.Read(CryptoBuffer, 0, 0x4000);
                Utils.Align(ref CryptoBuffer, 0x4000);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x4000);
                Counter = Counter + 0x400;
            }

            Output.Dispose();
            InCrypt.Dispose();
            MessageBox.Show("Done!");
        }

        public static void ExeFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            uint Section0Offset = 0xC00;
            var BuildPFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                    Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            BuildPFS.Start();
            BuildPFS.WaitForExit();

            var RomHead = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/ExeFS.pfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType_Updated,
                KeyIdx,
                0xC00 + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)),
                TitleID,
                Ver,
                KeyGeneration,
                Utils.Pad(0x10),
                Section0,
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Keys);

            byte[] Final = NCAHeader(
                Head,
                RomHead.Item1,
                Utils.Pad(0x200),
                Utils.Pad(0x200),
                Utils.Pad(0x200));

            byte[] CryptoBuffer = new byte[0x10];
            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);

            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);

            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);

            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InCrypt.Length / 0x10) * 0x10)) / 0x10))
            {
                InCrypt.Read(CryptoBuffer, 0, 0x10);
                Utils.Align(ref CryptoBuffer, 0x10);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x10);
                Counter = Counter + 1;
            }
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), Utils.Pad(((int)Math.Ceiling((decimal)Output.Length / 0x200) * 0x200) - (int)Output.Length)), 0, ((int)Math.Ceiling((decimal)Output.Length / 0x200) * 0x200) - (int)Output.Length);
            Output.Dispose();
            InCrypt.Dispose();
            MessageBox.Show("Done!");
        }

        public static void PFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            uint Section0Offset = 0xC00;
            var BuildPFS = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                    Arguments = $"\"{RootPath}/PFS0\" \"{RootPath}/Temp/PFS.pfs\"",
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            };
            BuildPFS.Start();
            BuildPFS.WaitForExit();

            var RomHead = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/PFS.pfs", 0x1000, 1, Crypto_CTR, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/PFS.pfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType_Updated,
                KeyIdx,
                0xC00 + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)),
                TitleID,
                Ver,
                KeyGeneration,
                Utils.Pad(0x10),
                Section0,
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                Utils.Pad(0x10),
                CryptoInitialisers.GenSHA256Hash(RomHead.Item1),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Utils.Pad(0x20),
                Keys);

            byte[] Final = NCAHeader(
                Head,
                RomHead.Item1,
                Utils.Pad(0x200),
                Utils.Pad(0x200),
                Utils.Pad(0x200));

            byte[] CryptoBuffer = new byte[0x10];
            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);

            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);

            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);

            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InCrypt.Length / 0x10) * 0x10)) / 0x10))
            {
                InCrypt.Read(CryptoBuffer, 0, 0x10);
                Utils.Align(ref CryptoBuffer, 0x10);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x10);
                Counter = Counter + 1;
            }
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"000000000000000000000000{Counter.ToString("X8")}"), Utils.Pad(((int)Math.Ceiling((decimal)Output.Length / 0x200) * 0x200) - (int)Output.Length)), 0, ((int)Math.Ceiling((decimal)Output.Length / 0x200) * 0x200) - (int)Output.Length);
            Output.Dispose();
            InCrypt.Dispose();
            MessageBox.Show("Done!");
        }
    }
}