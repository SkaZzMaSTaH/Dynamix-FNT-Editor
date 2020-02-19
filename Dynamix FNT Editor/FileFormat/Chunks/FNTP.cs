using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileFormat.Chunks
{
    public class FNTP : FNT
    {
        #region Private vars
        private byte _baseline;
        #endregion
        #region Public properties
        public byte Baseline { get { return _baseline; } }

        public uint SizeChunk
        {
            get
            {
                uint size = 0;

                foreach (Resource.FontChar FontChar in _fontChars)
                {
                    size += (uint)FontChar.ToByte().Length;
                }

                size += (uint)(_totalChars * 2);
                size += _totalChars;

                return size + 13;
            }
        }

        public static byte MagicalNumber { get { return 0xff; } }
        #endregion
        #region Constructor
        public FNTP(char[] id, byte[] data)
            : base(id, data)
        {
            ProcessHeader(data);
        }
        #endregion
        #region Private methods
        private void ProcessHeader(byte[] data)
        {
            byte[] compressData = new byte[] { };

            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            using (bin)
            {
                bin.BaseStream.Position = 1;

                _maxWidth = bin.ReadByte();
                _maxHeight = bin.ReadByte();
                _baseline = bin.ReadByte();
                _startChar = bin.ReadByte();
                _totalChars = bin.ReadByte();

                compressData = bin.ReadBytes(data.Length - 6);
            }

            ProcessDecompress(compressData);
        }
        private void ProcessDecompress(byte[] data)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            ushort size;
            byte typeCompression;
            uint sizeDecompress;

            byte[] decodeData;
            byte[] encodeData;

            using (bin)
            {
                size = bin.ReadUInt16();
                typeCompression = bin.ReadByte();
                sizeDecompress = bin.ReadUInt32();

                encodeData = bin.ReadBytes(data.Length - 7);
            }

            decodeData = (typeCompression == 0x02) ? Lib.Unpack.DecompressLZW(sizeDecompress, encodeData) : encodeData;

            ProcessCharacters(decodeData);
        }
        private void ProcessCharacters(byte[] data)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            ushort offset;
            byte width;
            byte[] charData = new byte[] { };

            using (bin)
            {
                for (int i = 0; i < _totalChars; i++)
                {
                    bin.BaseStream.Position = i * 2;
                    offset = bin.ReadUInt16();

                    bin.BaseStream.Position = i + (_totalChars * 2);
                    width = bin.ReadByte();

                    bin.BaseStream.Position = offset + (_totalChars * 2) + _totalChars;
                    charData = bin.ReadBytes(8 * ((width / 9) + 1));

                    _fontChars.Add(new Resource.FontChar(width, _maxHeight, charData));
                }
            }
        }
        #endregion
        #region Public methods
        public void Expand()
        {
            const byte EMPTY = 0x00;

            byte[] emptyChar;

            for (int i = _fontChars.Count; i < 0xe8; i++)
            {
                emptyChar = new byte[_maxHeight];

                for (int j = 0; j < _maxHeight; j++) { emptyChar[j] = EMPTY; }

                _fontChars.Add(new Resource.FontChar(1, _maxHeight, emptyChar));
            }

            _totalChars = 0xe8;
        }

        public byte[] ToByte()
        {
            List<byte> data = new List<byte>();
            ushort offset = 0x0000;

            data.AddRange(Encoding.Default.GetBytes(ID));
            data.AddRange(BitConverter.GetBytes(SizeChunk));
            data.Add(MagicalNumber);
            data.Add(_maxWidth);
            data.Add(_maxHeight);
            data.Add(_baseline);
            data.Add(_startChar);
            data.Add(_totalChars);
            data.AddRange(BitConverter.GetBytes((ushort)(SizeChunk - 13)));
            data.Add(0x00);     // Type compression = none
            data.AddRange(BitConverter.GetBytes(SizeChunk - 13));

            for (int i = 0; i < _totalChars; i++)
            {
                data.AddRange(BitConverter.GetBytes(offset));

                offset += (ushort)(_fontChars[i].ToByte().Length);
            }
            for (int i = 0; i < _totalChars; i++)
            {
                data.Add(_fontChars[i].Width);
            }
            for (int i = 0; i < _totalChars; i++)
            {
                data.AddRange(_fontChars[i].ToByte());
            }

            return data.ToArray();
        }
        #endregion
    }
}
