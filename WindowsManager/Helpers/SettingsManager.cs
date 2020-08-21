using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace WindowsManager.Helpers
{
    public class SettingsManager
    {

        internal bool SettingsExist()
        {
            return File.Exists(Constants.SettingsFile);
        }


        internal IEnumerable<XElement> Load()
        {
            XDocument document = XDocument.Load(Constants.SettingsFile);
            return document.Root.Elements();
        }

        internal void Save(List<XElement> SerializeScreens)
        {
            XDocument document = new XDocument();
            XElement xSettings = new XElement($"WindowsManager_{Constants.Version}");
            document.Add(xSettings);

            SerializeScreens.ForEach(x => xSettings.Add(x));

            // Save the formated document
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
