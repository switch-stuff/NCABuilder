using RunProcessAsTask;
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
        public static async void Standard_Application
(
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            byte CryptoType,
            byte NCAType,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK,
            RichTextBox TB
            )
        {
            uint CounterModifier;
            byte ExeCounter;
            if (CryptoType == CryptoType_Original)
            {
                CounterModifier = CryptoType_Original;
            }
            else
            {
                CounterModifier = CryptoType_Updated;
            }
            if (CounterModifier == 2)
            {
                ExeCounter = 1;
            }
            else
            {
                ExeCounter = 0;
            }

            Directory.CreateDirectory($"{RootPath}/Temp");
            TB.AppendText("Created temporary directory...\r\n");
            TB.ScrollToCaret();

            uint Section2Offset = 0x4000;

            TB.AppendText("Building logo partition...\r\n");
            TB.ScrollToCaret();

            var BuildLogo = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                Arguments = $"\"{RootPath}/Logo\" \"{RootPath}/Temp/Logo.pfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildLogo.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildLogo.Process.WaitForExit();
            TB.AppendText("Successfully built logo partition!\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();
            var LogoCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/Logo.pfs", 0x1000, 1, Crypto_None, 0);
            var LogoSize = new FileInfo($@"{RootPath}/Temp/Logo.pfs").Length;

            uint PaddedLogoLength = (uint)(((int)Math.Ceiling((decimal)(LogoSize + LogoCalcs.Item2.Length) / 0x4000) * 0x4000));

            uint Section1Offset = 0x4000 + PaddedLogoLength;

            TB.AppendText("Building RomFS partition...\r\n");
            TB.ScrollToCaret();

            var BuildRomFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildRomFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildRomFS.Process.WaitForExit();
            TB.AppendText("Successfully built RomFS partition!\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();
            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, (byte)CounterModifier);
            var RomLength = RomHead.Item2.Length;

            var InRom = File.Open($"{RootPath}/Temp/RomFS.romfs", FileMode.Open);

            uint PaddedRomLength = (uint)(((int)Math.Ceiling((decimal)(InRom.Length + RomLength) / 0x4000) * 0x4000));

            TB.AppendText("Building ExeFS partition...\r\n");
            TB.ScrollToCaret();

            var BuildPFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildPFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildPFS.Process.WaitForExit();

            TB.AppendText("Successfully built ExeFS partition!\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();

            var ExeCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, ExeCounter);
            var ExeSize = new FileInfo($@"{RootPath}/Temp/ExeFS.pfs").Length;

            uint PaddedExeLength = (uint)(((int)Math.Ceiling((decimal)(ExeSize + ExeCalcs.Item2.Length) / 0x4000) * 0x4000));

            uint Section0Offset = 0x4000 + PaddedLogoLength + PaddedRomLength;

            TB.AppendText("Generating entries...\r\n");
            TB.ScrollToCaret();

            byte[] Section2 = Entry(Section2Offset, 0x4000 + PaddedLogoLength);
            byte[] Section1 = Entry(Section1Offset, 0x4000 + PaddedRomLength + PaddedLogoLength);
            byte[] Section0 = Entry(Section0Offset, 0x4000 + PaddedRomLength + PaddedExeLength + PaddedLogoLength);

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            TB.AppendText("Generating body key for encryption...\r\n");
            TB.ScrollToCaret();

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            TB.AppendText($"Body key: {Utils.BytesToString(Key)}\r\n");
            TB.ScrollToCaret();

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            TB.AppendText("Generating NCA header...\r\n");
            TB.ScrollToCaret();

            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType,
                KeyIdx,
                0x4000 + PaddedRomLength + PaddedExeLength + PaddedLogoLength,
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
                CryptoInitialisers.GenSHA256Hash(LogoCalcs.Item1),
                Utils.Pad(0x20),
                Keys
                );

            byte[] Final = NCAHeader
                (
                Head,
                ExeCalcs.Item1,
                RomHead.Item1,
                LogoCalcs.Item1,
                Utils.Pad(0x200)
                );

            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);
            TB.AppendText($"Opened NCA for writing...\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Encrypting and writing header to NCA...\r\n");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);
            TB.AppendText("Writing logo partition to NCA...\r\n");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes("000000000000000000000000000000C0"), Utils.Pad(0x3400)), 0, 0x3400);
            int LogoPadLen = ((int)Math.Ceiling((decimal)PaddedLogoLength / 0x4000) * 0x4000) - (int)(LogoSize + LogoCalcs.Item2.Length);
            Output.Write(LogoCalcs.Item2.Concat(File.ReadAllBytes($"{RootPath}/Temp/Logo.pfs").Concat(Utils.Pad(LogoPadLen))).ToArray(), 0, (int)PaddedLogoLength);
            uint Counter = 0x400 + (PaddedLogoLength >> 4);
            TB.AppendText("Encrypting and writing RomFS partition to NCA...\r\n");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{CounterModifier.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);
            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);
            byte[] CryptoBuffer = new byte[0x4000];
            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)) / 0x4000))
            {
                InRom.Read(CryptoBuffer, 0, 0x4000);
                Utils.Align(ref CryptoBuffer, 0x4000);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{CounterModifier.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x4000);
                Counter = Counter + 0x400;
            }
            TB.AppendText("Encrypting and writing ExeFS to NCA...\r\n");
            TB.ScrollToCaret();
            int ExePadLen = ((int)Math.Ceiling((decimal)PaddedExeLength / 0x4000) * 0x4000) - (int)(ExeSize + ExeCalcs.Item2.Length);
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{ExeCounter.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), ExeCalcs.Item2.Concat(File.ReadAllBytes($"{RootPath}/Temp/ExeFS.pfs").Concat(Utils.Pad(ExePadLen))).ToArray()), 0, (int)PaddedExeLength);
            Output.Dispose();
            InRom.Dispose();
            var Openforhashing = File.OpenRead($"{RootPath}/Output/Generated.nca");
            TB.AppendText("Calculating NCA hash...");
            TB.ScrollToCaret();
            var Filename = Utils.BytesToString(CryptoInitialisers.GenSHA256StrmHash(Openforhashing)).Substring(0, 32).ToLower();
            Openforhashing.Dispose();
            File.Move($"{RootPath}/Output/Generated.nca", $"{RootPath}/Output/{Filename}.nca");
            TB.AppendText("Done!");
            TB.ScrollToCaret();
            MessageBox.Show("Done!");
        }

        public static async void ExeFS_RomFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            byte CryptoType,
            byte NCAType,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK,
            RichTextBox TB
            )
        {
            uint CounterModifier;
            byte ExeCounter;
            if (CryptoType == CryptoType_Original)
            {
                CounterModifier = CryptoType_Original;
            }
            else
            {
                CounterModifier = CryptoType_Updated;
            }
            if (CounterModifier == 2)
            {
                ExeCounter = 1;
            }
            else
            {
                ExeCounter = 0;
            }

            Directory.CreateDirectory($"{RootPath}/Temp");
            TB.AppendText("Created temporary directory...\r\n");
            TB.ScrollToCaret();
            uint Section1Offset = 0xC00;
            TB.AppendText("Building RomFS partition...\r\n");
            TB.ScrollToCaret();
            var BuildRomFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildRomFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildRomFS.Process.WaitForExit();
            TB.AppendText("Successfully built RomFS partition!\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();
            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, (byte)CounterModifier);
            var RomLength = RomHead.Item2.Length;

            var InRom = File.Open($"{RootPath}/Temp/RomFS.romfs", FileMode.Open);

            uint PaddedRomLength = (uint)(((int)Math.Ceiling((decimal)(InRom.Length + RomLength) / 0x4000) * 0x4000));

            TB.AppendText("Building ExeFS partition...\r\n");
            TB.ScrollToCaret();
            var BuildPFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildPFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildPFS.Process.WaitForExit();
            TB.AppendText("Successfully built ExeFS partition!\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();
            var ExeCalcs = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, ExeCounter);
            var ExeSize = new FileInfo($@"{RootPath}/Temp/ExeFS.pfs").Length;

            uint PaddedExeLength = (uint)(((int)Math.Ceiling((decimal)(ExeSize + ExeCalcs.Item2.Length) / 0x4000) * 0x4000));

            uint Section0Offset = 0xC00 + PaddedRomLength;

            TB.AppendText("Generating entries..\r\n");
            TB.ScrollToCaret();
            byte[] Section1 = Entry(Section1Offset, 0xC00 + PaddedRomLength);
            byte[] Section0 = Entry(Section0Offset, 0xC00 + PaddedRomLength + PaddedExeLength);

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            TB.AppendText($"Generating body key for encryption...\r\n");
            TB.ScrollToCaret();
            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);
            TB.AppendText($"Body key: {Utils.BytesToString(Key)}\r\n");
            TB.ScrollToCaret();
            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);
            TB.AppendText($"Generating NCA header...\r\n");
            TB.ScrollToCaret();
            byte[] Head = Header(
                Utils.Pad(0x100),
                Utils.Pad(0x100),
                NCA3,
                NCAType_Digital,
                ContentType,
                CryptoType,
                KeyIdx,
                0xC00 + PaddedRomLength + PaddedExeLength,
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
                Keys
                );

            byte[] Final = NCAHeader
                (
                Head,
                ExeCalcs.Item1,
                RomHead.Item1,
                Utils.Pad(0x200),
                Utils.Pad(0x200)
                );

            Directory.CreateDirectory($"{RootPath}/Output");
            var Output = File.Open($"{RootPath}/Output/Generated.nca", FileMode.Create);
            TB.AppendText("Opened NCA for writing...\r\n");
            TB.ScrollToCaret();
            TB.AppendText("Encrypting and writing header to NCA...\r\n");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);
            uint Counter = 0xC0;
            TB.AppendText("Encrypting and writing RomFS to NCA...\r\n");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{CounterModifier.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), RomHead.Item2), 0, RomHead.Item2.Length);
            Counter = Counter + ((uint)RomHead.Item2.Length >> 4);
            byte[] CryptoBuffer = new byte[0x4000];
            foreach (int i in Enumerable.Range(0, (((int)Math.Ceiling((decimal)InRom.Length / 0x4000) * 0x4000)) / 0x4000))
            {
                InRom.Read(CryptoBuffer, 0, 0x4000);
                Utils.Align(ref CryptoBuffer, 0x4000);
                Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{CounterModifier.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), CryptoBuffer), 0, 0x4000);
                Counter = Counter + 0x400;
            }
            TB.AppendText("Encrypting and writing ExeFS to NCA...\r\n");
            TB.ScrollToCaret();
            int ExePadLen = ((int)Math.Ceiling((decimal)PaddedExeLength / 0x4000) * 0x4000) - (int)(ExeSize + ExeCalcs.Item2.Length);
            Output.Write(CryptoInitialisers.AES_CTR(Key, Utils.StringToBytes($"{ExeCounter.ToString("X8")}0000000000000000{Counter.ToString("X8")}"), ExeCalcs.Item2.Concat(File.ReadAllBytes($"{RootPath}/Temp/ExeFS.pfs").Concat(Utils.Pad(ExePadLen))).ToArray()), 0, (int)PaddedExeLength);
            Output.Dispose();
            InRom.Dispose();
            var Openforhashing = File.OpenRead($"{RootPath}/Output/Generated.nca");
            TB.AppendText("Calculating NCA hash...");
            TB.ScrollToCaret();
            var Filename = Utils.BytesToString(CryptoInitialisers.GenSHA256StrmHash(Openforhashing)).Substring(0, 32).ToLower();
            Openforhashing.Dispose();
            File.Move($"{RootPath}/Output/Generated.nca", $"{RootPath}/Output/{Filename}.nca");
            TB.AppendText("Done!");
            TB.ScrollToCaret();
            MessageBox.Show("Done!");
        }

        public static async void RomFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            byte CryptoType,
            byte NCAType,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK,
            RichTextBox TB
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            TB.AppendText("Created temporary directory...\r\n");
            TB.ScrollToCaret();
            uint Section0Offset = 0xC00;
            TB.AppendText("Building RomFS partition...\r\n");
            TB.ScrollToCaret();
            var BuildRomFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_romfs.exe",
                Arguments = $"\"{RootPath}/RomFS\" \"{RootPath}/Temp/RomFS.romfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildRomFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildRomFS.Process.WaitForExit();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();
            var RomHead = RomFSConstructor.MakeRomFS($"{RootPath}/Temp/RomFS.romfs", 0, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/RomFS.romfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)RomLength + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x4000) * 0x4000)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));
            TB.AppendText("Generating body key for encryption...\r\n");
            TB.ScrollToCaret();
            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);
            TB.AppendText($"Body key: {Utils.BytesToString(Key)}\r\n");
            TB.ScrollToCaret();
            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);
            TB.AppendText($"Generating NCA header...\r\n");
            TB.ScrollToCaret();
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
            TB.AppendText($"Opened NCA for writing...\r\n");
            TB.ScrollToCaret();
            TB.AppendText($"Encrypting and writing header to NCA...");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;
            TB.AppendText($"Encrypting and writing RomFS to NCA...");
            TB.ScrollToCaret();
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
            var Openforhashing = File.OpenRead($"{RootPath}/Output/Generated.nca");
            TB.AppendText("Calculating NCA hash...");
            TB.ScrollToCaret();
            var Filename = Utils.BytesToString(CryptoInitialisers.GenSHA256StrmHash(Openforhashing)).Substring(0, 32).ToLower();
            Openforhashing.Dispose();
            File.Move($"{RootPath}/Output/Generated.nca", $"{RootPath}/Output/{Filename}.nca");
            TB.AppendText("Done!");
            TB.ScrollToCaret();
            MessageBox.Show("Done!");
        }

        public static async void ExeFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            byte CryptoType,
            byte NCAType,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK,
            RichTextBox TB
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            TB.AppendText("Created temporary directory...\r\n");
            TB.ScrollToCaret();
            uint Section0Offset = 0xC00;
            TB.AppendText("Building ExeFS partition...\r\n");
            TB.ScrollToCaret();
            var BuildPFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                Arguments = $"\"{RootPath}/ExeFS\" \"{RootPath}/Temp/ExeFS.pfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildPFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildPFS.Process.WaitForExit();
            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();

            var RomHead = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/ExeFS.pfs", 0x8000, 1, Crypto_CTR, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/ExeFS.pfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            TB.AppendText("Generating body key for encryption...\r\n");
            TB.ScrollToCaret();

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            TB.AppendText($"Body key: {Utils.BytesToString(Key)}\r\n");
            TB.ScrollToCaret();

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            TB.AppendText($"Generating NCA header...\r\n");
            TB.ScrollToCaret();

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

            TB.AppendText($"Opened NCA for writing...\r\n");
            TB.ScrollToCaret();
            TB.AppendText($"Encrypting and writing header to NCA...");
            TB.ScrollToCaret();
            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            TB.AppendText($"Encrypting and writing ExeFS to NCA...");
            TB.ScrollToCaret();

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
            var Openforhashing = File.OpenRead($"{RootPath}/Output/Generated.nca");
            TB.AppendText("Calculating NCA hash...");
            TB.ScrollToCaret();
            var Filename = Utils.BytesToString(CryptoInitialisers.GenSHA256StrmHash(Openforhashing)).Substring(0, 32).ToLower();
            Openforhashing.Dispose();
            File.Move($"{RootPath}/Output/Generated.nca", $"{RootPath}/Output/{Filename}.nca");
            TB.AppendText("Done!");
            TB.ScrollToCaret();
            MessageBox.Show("Done!");
        }

        public static async void PFS
            (
            byte ContentType,
            byte KeyIdx,
            byte KeyGeneration,
            byte CryptoType,
            byte NCAType,
            string RootPath,
            ulong TitleID,
            uint Ver,
            string Headerkey,
            string KAEK,
            RichTextBox TB
            )
        {
            Directory.CreateDirectory($"{RootPath}/Temp");
            TB.AppendText("Created temporary directory...\r\n");
            TB.ScrollToCaret();

            uint Section0Offset = 0xC00;
            TB.AppendText("Building PFS0 partition...\r\n");
            TB.ScrollToCaret();

            var BuildPFS = await ProcessEx.RunAsync(new ProcessStartInfo
            {
                FileName = $@"{Directory.GetCurrentDirectory()}/Utilities/build_pfs0.exe",
                Arguments = $"\"{RootPath}/PFS\" \"{RootPath}/Temp/PFS.pfs\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            foreach (var Line in BuildPFS.StandardOutput)
            {
                TB.Invoke(new Action(() => TB.AppendText(Line + "\r\n")));
                TB.ScrollToCaret();
            }
            BuildPFS.Process.WaitForExit();

            TB.AppendText("Calculating hashes...\r\n");
            TB.ScrollToCaret();

            var RomHead = PFS0Constructor.MakePFS0($@"{RootPath}/Temp/PFS.pfs", 0x1000, 1, Crypto_CTR, 0);
            var RomLength = RomHead.Item2.Length;

            var InCrypt = File.Open($"{RootPath}/Temp/PFS.pfs", FileMode.Open);

            byte[] Section0 = Entry(Section0Offset, Section0Offset + (uint)(((int)Math.Ceiling((decimal)InCrypt.Length / 0x200) * 0x200)));

            byte[] HeaderKey1 = Utils.StringToBytes(Headerkey.Substring(0, 32));
            byte[] HeaderKey2 = Utils.StringToBytes(Headerkey.Substring(32, 32));

            TB.AppendText("Generating body key for encryption...\r\n");
            TB.ScrollToCaret();

            byte[] Key = CryptoInitialisers.GenerateRandomKey(0x10);

            TB.AppendText($"Body key: {Utils.BytesToString(Key)}\r\n");
            TB.ScrollToCaret();

            byte[] Keys = KeyArea(Utils.StringToBytes(KAEK), Key);

            TB.AppendText($"Generating NCA header...\r\n");
            TB.ScrollToCaret();

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

            TB.AppendText($"Opened NCA for writing...\r\n");
            TB.ScrollToCaret();
            TB.AppendText($"Encrypting and writing header to NCA...");
            TB.ScrollToCaret();


            Output.Write(CryptoInitialisers.AES_XTS(HeaderKey1, HeaderKey2, 0x200, Final, 0), 0, Final.Length);

            uint Counter = 0xC0;

            TB.AppendText($"Encrypting and writing PFS to NCA...");
            TB.ScrollToCaret();

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
            var Openforhashing = File.OpenRead($"{RootPath}/Output/Generated.nca");
            TB.AppendText("Calculating NCA hash...");
            TB.ScrollToCaret();
            var Filename = Utils.BytesToString(CryptoInitialisers.GenSHA256StrmHash(Openforhashing)).Substring(0, 32).ToLower();
            Openforhashing.Dispose();
            File.Move($"{RootPath}/Output/Generated.nca", $"{RootPath}/Output/{Filename}.nca");
            TB.AppendText("Done!");
            TB.ScrollToCaret();
            MessageBox.Show("Done!");
        }
    }
}