using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Media;
using System.Linq;
using System.Windows.Forms;

namespace Dynamix_FNT_Editor
{
    public partial class Main : Form
    {
        Manager.Brain Pinky = null;

        public Main()
        {
            InitializeComponent();

            DisabledGUI();
        }

        #region Scripted events
        private void HandleError(Exception err)
        {
            MessageBox.Show(
                    err.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
        }

        private bool SaveFile(string fileName, string filter, byte[] data)
        {
            bool isSuccess = false;

            SaveFileDialog SFDialog = new SaveFileDialog();

            SFDialog.FileName = fileName;
            SFDialog.Filter = filter;

            if (SFDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllBytes(SFDialog.FileName, data);

                    isSuccess = true;
                }
                catch (Exception err)
                {
                    HandleError(err);
                }
            }

            return isSuccess;
        }
        private Dictionary<string, byte[]> OpenFile(string fileName, string filter)
        {
            Dictionary<string, byte[]> fileInfDat = new Dictionary<string, byte[]>();

            string info = "CANCEL!";
            byte[] data = new byte[1] { 0x00 };

            OpenFileDialog OFDialog = new OpenFileDialog();
            OFDialog.FileName = fileName;
            OFDialog.Filter = filter;

            if (OFDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    info = OFDialog.FileName;
                    data = File.ReadAllBytes(OFDialog.FileName);
                }
                catch (Exception err)
                {
                    HandleError(err);

                    info = "ERR!";
                    data = new byte[] { };
                }
            }

            fileInfDat.Add(info, data);

            return fileInfDat;
        }

        private void OpenFontFile()
        {
            string fileName = string.Empty;
            string filter = "Archivo de fuente|*.fnt|Todos los archivos|*.*";

            Dictionary<string, byte[]> fileInfDat = OpenFile(fileName, filter);

            string info = fileInfDat.ElementAt(0).Key;
            byte[] data = fileInfDat.ElementAt(0).Value;

            if (info == "CANCEL!") { return; }  // Cancel
            if (info != "ERR!")                 // Ok & data
            {
                Pinky = new Manager.Brain(info, Lib.Unpack.Font(data));
                Pinky.CharacterIndex = 0;
                Pinky.Zoom = 7;
                EnabledGUI();
                ShowInfoFile();
                ShowInfoFont();
                ShowInfoChar();
                ShowImageChar();

                MessageBox.Show(
                    "Archivo cargado con éxito.",
                    "Cargar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "El archivo está vacío.",
                    "Cargar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        private void SaveFontFile()
        {
            string fileName = Path.GetFileNameWithoutExtension(Pinky.FileName);
            string filter = "Archivo de fuente|*.fnt|Todos los archivos|*.*";
            byte[] data = (Pinky.IsFixed) ? (Pinky.FontOpened as FileFormat.Chunks.FNTF).ToByte() : (Pinky.FontOpened as FileFormat.Chunks.FNTP).ToByte();

            bool isSuccess = SaveFile(fileName, filter, data);

            if (isSuccess == true)
            {
                MessageBox.Show(
                    "Archivo guardado con éxito.",
                    "Guardar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void ImportCharFile()
        {
            string fileName = Path.GetFileNameWithoutExtension(Pinky.FileName) + "-CHAR-" + Convert.ToString(Pinky.CharacterIndex + Pinky.FontOpened.StartChar);
            string filter = "Archivo de carácter|*.char|Todos los archivos|*.*";

            Dictionary<string, byte[]> fileInfDat = OpenFile(fileName, filter);

            string info = fileInfDat.ElementAt(0).Key;
            byte[] data = fileInfDat.ElementAt(0).Value;

            byte type = 0x66, maxWidth = 0, maxHeight = 0, width = 0;
            byte[] charData = new byte[] { };

            BinaryReader bin = new BinaryReader(new MemoryStream(data));

            if (info == "CANCEL!") { return; }  // Cancel
            if (info != "ERR!")                 // Ok & data
            {
                try
                {
                    using (bin)
                    {
                        type = bin.ReadByte();
                        maxWidth = bin.ReadByte();
                        maxHeight = bin.ReadByte();
                        width = bin.ReadByte();
                        charData = bin.ReadBytes(maxHeight * ((width / 9) + 1));
                    }

                    #region Detection compatibility
                    if (type == 0x00 || type == 0xff)   // Check both fonts
                    {
                        if (maxHeight != Pinky.FontOpened.MaxHeight)
                        {
                            HandleError(new Exception("Este carácter no es válido. Debe tener el mismo alto máximo."));
                            return;
                        }
                    }
                    if (type == 0x00)       // Check with FontF
                    {
                        if (maxWidth != Pinky.FontOpened.MaxWidth || width != Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width)
                        {
                            HandleError(new Exception("Este carácter no es válido. Debe tener el mismo ancho máximo y de carácter."));
                            return;
                        }
                    }
                    if (type == 0xff)       // Check with FontP
                    {
                        if (maxWidth != Pinky.FontOpened.MaxWidth)
                        {
                            HandleError(new Exception("Este carácter no es válido. Debe tener el mismo ancho máximo."));
                            return;
                        }
                    }
                    #endregion

                    Pinky.FontOpened.FontChars[Pinky.CharacterIndex].ChangeWidth(width);
                    Pinky.FontOpened.FontChars[Pinky.CharacterIndex].ChangeCharData(charData);

                    ShowInfoChar();
                    ShowImageChar();

                    MessageBox.Show(
                        "Carácter importado con éxito.",
                        "Importar",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception err)
                {
                    HandleError(err);
                }
            }
            else
            {
                MessageBox.Show(
                    "El archivo está vacío.",
                    "Cargar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
        private void ExportCharFile()
        {
            string fileName = Path.GetFileNameWithoutExtension(Pinky.FileName) + "_CHAR" + Convert.ToString(Pinky.CharacterIndex + Pinky.FontOpened.StartChar);
            string filter = "Archivo de carácter|*.char|Todos los archivos|*.*";
            List<byte> data = new List<byte>();

            if (Pinky.IsFixed) { data.Add((byte)0x00); } else { data.Add((byte)0xff); }
            data.Add(Pinky.FontOpened.MaxWidth);
            data.Add(Pinky.FontOpened.MaxHeight);
            data.Add(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width);
            data.AddRange(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].ToByte());

            bool isSuccess = SaveFile(fileName, filter, data.ToArray());

            if (isSuccess == true)
            {
                MessageBox.Show(
                    "Carácter exportado con éxito.",
                    "Exportar",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void DisabledGUI()
        {
            const string DISABLED_STR = @"---";

            FileCaption.Text = DISABLED_STR;
            MaxWCaption.Text = DISABLED_STR;
            MaxHCaption.Text = DISABLED_STR;
            BaselineCaption.Text = DISABLED_STR;
            StartCharCaption.Text = DISABLED_STR;
            TotalCharsCaption.Text = DISABLED_STR;
            CharIndex.Text = DISABLED_STR;
            CharWCaption.Text = DISABLED_STR;
            CharHCaption.Text = DISABLED_STR;

            SaveButton.Enabled = false;
            SaveTButton.Enabled = false;
            ImportCharButton.Enabled = false;
            ExportCharButton.Enabled = false;
            PrevButton.Enabled = false;
            PrevTButton.Enabled = false;
            NextButton.Enabled = false;
            NextTButton.Enabled = false;
            ZoomTButton.Enabled = false;

            ExpandCharsButton.Enabled = false;
            IncreaseWButton.Enabled = false;
            DecreaseWButton.Enabled = false;
            ExpandWButton.Enabled = false;

            ColorActiveButton.Enabled = false;
            ColorPasiveButton.Enabled = false;
            ColorHiddenButton.Enabled = false;

            CharImage.Enabled = false;
        }
        private void EnabledGUI()
        {
            SaveButton.Enabled = true;
            SaveTButton.Enabled = true;
            ImportCharButton.Enabled = true;
            ExportCharButton.Enabled = true;
            PrevButton.Enabled = true;
            PrevTButton.Enabled = true;
            NextButton.Enabled = true;
            NextTButton.Enabled = true;
            ZoomTButton.Enabled = true;

            ExpandCharsButton.Enabled = true;
            IncreaseWButton.Enabled = (!Pinky.IsFixed) ? true : false;
            DecreaseWButton.Enabled = (!Pinky.IsFixed) ? true : false;
            ExpandWButton.Enabled = true;

            ColorActiveButton.Enabled = true;
            ColorPasiveButton.Enabled = true;
            ColorHiddenButton.Enabled = true;

            CharImage.Enabled = true;
        }

        private void ShowInfoFile()
        {
            FileCaption.Text = Pinky.FileName;
        }
        private void ShowInfoFont()
        {
            MaxWCaption.Text = Convert.ToString(Pinky.FontOpened.MaxWidth);
            MaxHCaption.Text = Convert.ToString(Pinky.FontOpened.MaxHeight);
            BaselineCaption.Text = (Pinky.IsFixed) ? "---" : Convert.ToString((Pinky.FontOpened as FileFormat.Chunks.FNTP).Baseline);
            StartCharCaption.Text = Convert.ToString(Pinky.FontOpened.StartChar);
            TotalCharsCaption.Text = Convert.ToString(Pinky.FontOpened.TotalChars);
        }
        private void ShowInfoChar()
        {
            CharIndex.Text = Convert.ToString(Pinky.CharacterIndex + Pinky.FontOpened.StartChar);
            CharWCaption.Text = Convert.ToString(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width);
            CharHCaption.Text = Convert.ToString(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Height);
        }

        private void ShowImageChar()
        {
            Bitmap image = Lib.Draw.FontChar(
                Pinky.Zoom,
                Pinky.FontOpened.FontChars[Pinky.CharacterIndex],
                new Color[3] {
                    ColorActiveButton.BackColor,
                    ColorPasiveButton.BackColor,
                    ColorHiddenButton.BackColor });

            CharImage.Width = image.Width;
            CharImage.Height = image.Height;
            CharImage.Image = image;
        }

        private void SetZoom()
        {
            Pinky.Zoom += 1;

            if (Pinky.Zoom > 10) { Pinky.Zoom = 5; }

            ShowImageChar();
        }

        private void ChangeCharWidth(byte width)
        {
            if (!Pinky.IsFixed)
            {
                Pinky.FontOpened.FontChars[Pinky.CharacterIndex].ChangeWidth(width);
            }

            ShowInfoChar();
            ShowImageChar();
        }
        #endregion
        #region System events
        private void ColorButton_Click(object sender, EventArgs e)
        {
            const DialogResult OPT_CANCEL = DialogResult.Cancel;

            ColorDialog.Color = (sender as Button).BackColor;
            
            if (ColorDialog.ShowDialog() == OPT_CANCEL) { return; }

            (sender as Button).BackColor = ColorDialog.Color;

            ShowImageChar();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            OpenFontFile();
        }
        private void OpenTButton_Click(object sender, EventArgs e)
        {
            OpenButton.PerformClick();
        }
        private void SaveButton_Click(object sender, EventArgs e)
        {
            SaveFontFile();
        }
        private void SaveTButton_Click(object sender, EventArgs e)
        {
            SaveButton.PerformClick();
        }
        private void ImportCharButton_Click(object sender, EventArgs e)
        {
            ImportCharFile();
        }
        private void ExportCharButton_Click(object sender, EventArgs e)
        {
            ExportCharFile();
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            int index = Pinky.CharacterIndex - 1;

            if (index < 0)
            {
                Pinky.CharacterIndex = 0;
            }
            else
            {
                Pinky.CharacterIndex = (byte)index;
            }

            ShowInfoChar();
            ShowImageChar();
        }
        private void PrevTButton_Click(object sender, EventArgs e)
        {
            PrevButton.PerformClick();
        }
        private void NextButton_Click(object sender, EventArgs e)
        {
            int index = Pinky.CharacterIndex + 1;

            if (index >= Pinky.FontOpened.TotalChars - 1)
            {
                Pinky.CharacterIndex = (byte)(Pinky.FontOpened.TotalChars - 1);
            }
            else
            {
                Pinky.CharacterIndex = (byte)index;
            }

            ShowInfoChar();
            ShowImageChar();
        }
        private void NextTButton_Click(object sender, EventArgs e)
        {
            NextButton.PerformClick();
        }

        private void ZoomTButton_Click(object sender, EventArgs e)
        {
            SetZoom();
        }

        private void ExpandCharsButton_Click(object sender, EventArgs e)
        {
            if (Pinky.IsFixed)
            {
                (Pinky.FontOpened as FileFormat.Chunks.FNTF).Expand();
            }
            else
            {
                (Pinky.FontOpened as FileFormat.Chunks.FNTP).Expand();
            }

            ShowInfoFont();
        }
        private void DecreaseWButton_Click(object sender, EventArgs e)
        {
            byte width = (byte)(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width - 1);

            if (width < 1) { width = 1; }

            ChangeCharWidth(width);
        }
        private void IncreaseWButton_Click(object sender, EventArgs e)
        {
            byte width = (byte)(Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width + 1);

            if (width > Pinky.FontOpened.MaxWidth) { width = Pinky.FontOpened.MaxWidth; }

            ChangeCharWidth(width);
        }
        private void ExpandWButton_Click(object sender, EventArgs e)
        {
            ChangeCharWidth(Pinky.FontOpened.MaxWidth);
        }

        private void CharImage_Click(object sender, EventArgs e)
        {
            if (CharImage.Enabled == false) { return; }

            Point coordinates = (e as MouseEventArgs).Location;
            Point arrayCoords = new Point((coordinates.X / Pinky.Zoom), (coordinates.Y / Pinky.Zoom));

            if (arrayCoords.X > Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Width)
            {
                SystemSounds.Hand.Play();

                return;
            }

            byte value = Pinky.FontOpened.FontChars[Pinky.CharacterIndex].Planes[arrayCoords.Y][arrayCoords.X];

            value = (value == 0x00) ? (byte)0x01 : (byte)0x00;

            Pinky.FontOpened.FontChars[Pinky.CharacterIndex].ChangeCharData(
                arrayCoords.X,
                arrayCoords.Y,
                value);

            ShowImageChar();
        }

        private void AboutTButton_Click(object sender, EventArgs e)
        {
            Manager.AssemblyInfo SoftInfo = new Manager.AssemblyInfo();

            MessageBox.Show(
                SoftInfo.Product + " " +
                "v" + SoftInfo.FileVersion + Environment.NewLine +
                SoftInfo.Description + Environment.NewLine + Environment.NewLine +
                SoftInfo.Company + Environment.NewLine +
                SoftInfo.Copyright + Environment.NewLine,
                SoftInfo.Title,
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        #endregion
    }
}
