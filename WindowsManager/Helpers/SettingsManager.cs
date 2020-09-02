using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace WindowsManager.Helpers
{
    public class SettingsManager
    {

        #region Fields


        private XDocument _Document = null;
        private IEnumerable<XElement> _Screens = null;
        private int _MoveStep = 25;
        private int _ResizeStep = 25;
        private int _ExplorerWidth = 1400;
        private int _ExplorerHeight = 800;
        private string _ImageViewerPath = null;

        #endregion Fields



        #region Properties

        internal bool SettingsExist => _Document != null;

        internal IEnumerable<XElement> Screens
        {
            get => _Screens;
            set
            {
                _Screens = value;
                Save();
            }
        }

        public int MoveStep
        {
            get => _MoveStep;
            set
            {
                _MoveStep = value;
                Save();
            }
        }

        public int ResizeStep
        {
            get => _ResizeStep;
            set
            {
                _ResizeStep = value;
                Save();
            }
        }

        public int ExplorerWidth
        {
            get => _ExplorerWidth;
            set
            {
                _ExplorerWidth = value;
                Save();
            }
        }

        public int ExplorerHeight
        {
            get => _ExplorerHeight;
            set
            {
                _ExplorerHeight = value;
                Save();
            }
        }

        public string ImageViewerPath
        {
            get => _ImageViewerPath;
            set
            {
                _ImageViewerPath = value;
                Save();
            }
        }

        #endregion Properties




        #region Constructor

        public SettingsManager()
        {
            if (File.Exists(Constants.SettingsFile))
                _Document = XDocument.Load(Constants.SettingsFile);
        } 

        #endregion Constructor



        internal void Load()
        {
            if (_Document is null)
                return;

            IEnumerable<XElement> elements = _Document.Root.Elements();
            _Screens = elements.Where(x => x.Name == "Screens").Elements();
            _MoveStep = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "MoveStep").Value);
            _ResizeStep = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "ResizeStep").Value);
            _ExplorerWidth = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "ExplorerWidth").Value);
            _ExplorerHeight = Convert.ToInt32(elements.FirstOrDefault(x => x.Name == "ExplorerHeight").Value);
            _ImageViewerPath = elements.FirstOrDefault(x => x.Name == "ImageViewerPath").Value;
        }



        private void Save()
        {
            // get the document and the root
            XDocument document = null;
            XElement root = null;

            if (_Document is XDocument)
            {
                document = _Document;
                root = _Document.Root;
                root.RemoveAll();
            }
            else
            {
                document = new XDocument();
                root = new XElement($"WindowsManager_{Constants.Version}");
                document.Add(root);
            }

            // screens
            XElement xScreens = new XElement($"Screens");
            root.Add(xScreens);
            Screens.ForEach(x => xScreens.Add(x));

            // hotkeys properties
            root.Add(new XElement("MoveStep", _MoveStep));
            root.Add(new XElement("ResizeStep", _ResizeStep));
            root.Add(new XElement("ExplorerWidth", _ExplorerWidth));
            root.Add(new XElement("ExplorerHeight", _ExplorerHeight));
            root.Add(new XElement("ImageViewerPath", _ImageViewerPath));

            // save the document
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = true
            };

            Directory.CreateDirectory(Constants.SettingsFolder);
            XmlWriter writer = XmlWriter.Create(Constants.SettingsFile, settings);
            document.Save(writer);
            writer.Flush();
            writer.Close();
        }

    }
}


//TODO: save file async