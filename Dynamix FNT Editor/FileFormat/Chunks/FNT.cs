using System.Collections.Generic;

namespace FileFormat.Chunks
{
    public abstract class FNT
    {
        #region Private vars
        private char[] _id = new char[4];
        #endregion
        #region Protected vars
        protected byte _maxWidth, _maxHeight, _startChar, _totalChars;

        protected List<Resource.FontChar> _fontChars = new List<Resource.FontChar>();
        #endregion
        #region Public properties
        public char[] ID { get { return _id; } }

        public byte MaxWidth { get { return _maxWidth; } }
        public byte MaxHeight { get { return _maxHeight; } }
        public byte StartChar { get { return _startChar; } }
        public byte TotalChars { get { return _totalChars; } }

        public List<Resource.FontChar> FontChars { get { return _fontChars; } }
        #endregion
        #region Constructors
        public FNT(char[] id, byte[] data)
        {
            _id = id;
        }
        #endregion
    }
}
