using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FileFormat.Chunks
{
    public class FNTF : FNT
    {
        #region Public properties
        public uint SizeChunk
        {
            get
            {
                uint size = 0;

                foreach (Resource.FontChar FontChar in _fontChars)
                {
                    size += (uint)FontChar.ToByte().Length;
                }

                return (uint)(size + ID.Length);
            }
        }
        #endregion
        #region Constructor
        public FNTF(char[] id, byte[] data)
            : base(id, data)
        {
            ProcessHeader(data);
        }
        #endregion
        #region Private methods
        private void ProcessHeader(byte[] data)
        {
            byte[] charsData = new byte[] { };

            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            using (bin)
            {
                _maxWidth = bin.ReadByte();
                _maxHeight = bin.ReadByte();
                _startChar = bin.ReadByte();
                _totalChars = bin.ReadByte();

                charsData = bin.ReadBytes(data.Length - 4);
            }

            ProcessCharacters(charsData);
        }
        private void ProcessCharacters(byte[] data)
        {
            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            byte[] charData = new byte[] { };

            using (bin)
            {
                for (int i = 0; i < _totalChars; i++)
                {
                    charData = bin.ReadBytes(_maxHeight);

                    _fontChars.Add(new Resource.FontChar(_maxWidth, _maxHeight, charData));
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

                _fontChars.Add(new Resource.FontChar(_maxWidth, _maxHeight, emptyChar));
            }

            _totalChars = 0xe8;
        }

        public byte[] ToByte()
        {
            List<byte> data = new List<byte>();

            data.AddRange(Encoding.Default.GetBytes(ID));
            data.AddRange(BitConverter.GetBytes(SizeChunk));
            data.Add(_maxWidth);
            data.Add(_maxHeight);
            data.Add(_startChar);
            data.Add(_totalChars);

            for (int i = 0; i < _totalChars; i++)
            {
                data.AddRange(_fontChars[i].ToByte());
            }

            return data.ToArray();
        }
        #endregion
    }
}
