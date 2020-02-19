using System.IO;

namespace Manager
{
    public class Brain
    {
        private string _file;

        private FileFormat.Chunks.FNT _fontOpened = null;

        public string File { get { return _file; } }
        public string FileName { get { return Path.GetFileName(_file); } }
        public string FilePath { get { return Path.GetDirectoryName(_file); } }

        public FileFormat.Chunks.FNT FontOpened { get { return _fontOpened; } }

        public bool IsFixed
        {
            get
            {
                return (_fontOpened is FileFormat.Chunks.FNTF) ? true : false;
            }
        }

        public byte CharacterIndex { get; set; }
        public byte Zoom { get; set; }

        public Brain(string file, FileFormat.Chunks.FNT Font)
        {
            _file = file;
            _fontOpened = Font;
        }
    }
}
