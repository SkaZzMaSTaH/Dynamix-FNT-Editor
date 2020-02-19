using System.IO;

namespace Lib
{
    public static class Unpack
    {
        public static byte[] DecompressLZW(uint sz, byte[] data)
        {
            LZW LZW = new LZW();
            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            return LZW.Decompress(sz, bin);
        }
        public static FileFormat.Chunks.FNT Font(byte[] fileData)
        {
            FileFormat.Chunks.FNT FontImport = null;

            BinaryReader bin = new BinaryReader(new MemoryStream(fileData));

            char[] id = new char[4];
            uint size;
            byte[] data = new byte[] { };

            using (bin)
            {
                id = bin.ReadChars(4);
                size = bin.ReadUInt32();
                data = bin.ReadBytes((int)size);
            }

            if (data[0] != FileFormat.Chunks.FNTP.MagicalNumber)
            {
                FontImport = new FileFormat.Chunks.FNTF(id, data);
            }
            else
            {
                FontImport = new FileFormat.Chunks.FNTP(id, data);
            }

            return FontImport;
        }
    }
}
