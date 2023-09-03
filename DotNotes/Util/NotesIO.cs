using DotNotes.Models;
using System.IO.Compression;
using System.Xml;

namespace DotNotes.Util
{
    public class NotesIO
    {
        public static Note createNote(string name)
        {
            if (String.IsNullOrEmpty(name)) return null;

            name = name.Replace(" ", "_");

            DateTime now = DateTime.Now;
            string fileName = name + ".xml";
            string filePath = $"{FileSystem.Current.AppDataDirectory}/NoteStorage/{fileName}";

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) return null;

            Note note = new Note(9999, name, new DateOnly(now.Year, now.Month, now.Day), "", new FileInfo(filePath));
            bool result = saveNote(note, false);
            return result ? note : null;
        }

        public static bool saveNote(Note note, bool ovr = true)
        {
            string fileName = note.FileInfo.FullName;
            string newFileName = fileName;

            int i = 1;
            while (!ovr && File.Exists(newFileName))
            {
                if (i > 255)
                {
                    return false;
                }

                if (fileName.IndexOf('.') >= 0)
                {
                    newFileName = fileName.Insert(fileName.IndexOf('.'), " (" + i.ToString() + ")");
                }
                else
                {
                    return false;
                }

                i++;
            }

            try
            {
                if (File.Exists(newFileName)) File.Delete(newFileName);

                using (XmlWriter writer = XmlWriter.Create(newFileName))
                {
                    writer.WriteStartElement("note");
                    writer.WriteElementString("name", note.Name);
                    writer.WriteElementString("date", note.Date.ToShortDateString());
                    writer.WriteElementString("body", note.Body);
                    writer.WriteEndElement();
                    writer.Flush();
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static List<Note> loadNotes()
        {
            List<Note> notes = new List<Note>();

            string notesFolder = $"{FileSystem.Current.AppDataDirectory}/NoteStorage";
            if (!Directory.Exists(notesFolder)) Directory.CreateDirectory(notesFolder);

            notes = new List<Note>();

            DirectoryInfo d = new DirectoryInfo(notesFolder);
            foreach (FileInfo file in d.GetFiles("*.xml"))
            {
                using (StreamReader reader = file.OpenText())
                {
                    XmlDocument xml = new XmlDocument();
                    xml.PreserveWhitespace = true;

                    try
                    {
                        xml.Load(reader);
                    }
                    catch
                    {
                        xml.LoadXml("<?xml version=\"1.0\"?>");
                    }

                    try
                    {
                        XmlNodeList xnList = xml.SelectNodes("/note");
                        for (int i = 0; i < xnList.Count; i++)
                        {
                            XmlNode xn = xnList[i];
                            string name = xn["name"].InnerText;
                            string date = xn["date"].InnerText;
                            string body = xn["body"].InnerText;

                            if (String.IsNullOrEmpty(body)) body = "";

                            if (!(String.IsNullOrEmpty(name) || String.IsNullOrEmpty(date)))
                            {
                                notes.Add(new Note(i, name, DateOnly.Parse(date), body, file));
                            }
                        }
                    }
                    catch { }
                }
            }

            return notes;
        }

        public static bool deleteNotes(List<Note> notes)
        {
            bool successful = true;
            foreach (Note note in notes)
            {
                try
                {
                    if (note.FileInfo is null || !File.Exists(note.FileInfo.FullName))
                    {
                        successful = false;
                    }
                    else
                    {
                        note.FileInfo.Delete();
                    }
                }
                catch
                {
                    successful = false;
                }
            }
            return successful;
        }

        public static object shareNotes(List<Note> notes)
        {
            string folderPath = FileSystem.Current.AppDataDirectory + "/NoteDistribution";
            string zipPath = folderPath + "/current.zip";

            try
            {
                if (Directory.Exists(folderPath)) Directory.Delete(folderPath, true);
                Directory.CreateDirectory(folderPath);

                using (FileStream zipStream = new FileStream(zipPath, FileMode.Create))
                using (ZipArchive zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
                {
                    foreach (Note note in notes)
                    {
                        FileInfo file = note.FileInfo;
                        ZipArchiveEntry entry = zipArchive.CreateEntry(file.Name);
                        using (Stream stream = entry.Open())
                        using (FileStream fileStream = file.OpenRead())
                        {
                            fileStream.CopyTo(stream);
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            if (!File.Exists(zipPath)) return false;

            return zipPath;
        }

        public static bool importNotes(string zipCurrPath)
        {
            bool successful = true;
            string tempFolderPath = FileSystem.Current.AppDataDirectory + "/NoteImportData";
            string noteFolderPath = FileSystem.Current.AppDataDirectory + "/NoteStorage";
            string zipDestPath = tempFolderPath + "/current.zip";

            if (!File.Exists(zipCurrPath)) return false;

            try
            {
                if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
                Directory.CreateDirectory(tempFolderPath);

                File.Copy(zipCurrPath, zipDestPath, true);

                if (!File.Exists(zipDestPath)) return false;

                /* Check if zip only has *.xml files and no folders */
                using (ZipArchive zip = ZipFile.OpenRead(zipDestPath))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        string content = "";
                        using (Stream stream = entry.Open())
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                content = reader.ReadToEnd();
                            }
                        }

                        if (!entry.FullName.EndsWith(".xml"))
                        {
                            successful = false; /* Safety procedure (anti-malware) */
                        }
                        else if (!checkSyntax(content))
                        {
                            successful = false;
                        }
                        else
                        {
                            string destFilePath = Path.Combine(noteFolderPath, entry.FullName);

                            if (!Directory.Exists(noteFolderPath))
                            {
                                Directory.CreateDirectory(noteFolderPath);
                            }

                            string newDestFilePath = destFilePath;

                            int i = 1;
                            while (File.Exists(newDestFilePath))
                            {
                                if (i > 255)
                                {
                                    successful = false;
                                    break;
                                }

                                if (destFilePath.IndexOf('.') >= 0)
                                {
                                    newDestFilePath = destFilePath.Insert(destFilePath.IndexOf('.'), " (" + i.ToString() + ")");
                                }

                                i++;
                            }

                            if (i <= 255)
                            {
                                entry.ExtractToFile(newDestFilePath, false);
                            }
                        }
                    }
                }
            }
            catch
            {
                return false;
            }

            return successful;
        }

        public static bool checkSyntax(string content)
        {
            XmlDocument xml = new XmlDocument();
            xml.PreserveWhitespace = true;

            try
            {
                xml.LoadXml(content);
            }
            catch
            {
                return false;
            }

            XmlNodeList xnList = xml.SelectNodes("/note");
            if (xnList.Count == 0)
            {
                return false;
            }

            XmlNode xn = xnList[0];
            string name = xn["name"].InnerText;
            string date = xn["date"].InnerText;
            string body = xn["body"].InnerText;

            if (String.IsNullOrEmpty(body)) body = "";

            if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(date))
            {
                return false;
            }

            return true;
        }
    }
}
