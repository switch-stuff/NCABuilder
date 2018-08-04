using System.IO;

namespace NCABuilder
{
    internal class CNMTConstructor
    {
        public static byte[] CNMTHead(ulong TitleID, uint Version, byte Type, short NumOfContentEntries, uint MinFirmVer)
        {
            var Final = new BinaryWriter(new MemoryStream());
            Final.Write(TitleID);
            Final.Write(Version);
            Final.Write(0);
            Final.Write(Type);
            Final.Write((byte)0);
            Final.Write((short)0x10);
            Final.Write(NumOfContentEntries);
            Final.Write(Utils.Pad(0xE));
            Final.Dispose();
            return new MemoryStream().ToArray();
        }
    }
}