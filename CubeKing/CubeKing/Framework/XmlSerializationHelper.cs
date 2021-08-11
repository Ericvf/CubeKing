using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using System.IO.IsolatedStorage;

namespace CubeKing
{
    public static class XmlSerializationHelper
    {
        /// <summary>
        /// Deserializes an instance of T from the stringXml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlContents"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        private static T Deserialize<T>(string xmlContents)
        {
            // Create a serializer
            using (StringReader s = new StringReader(xmlContents))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(s);
            }
        }

        /// <summary>
        /// Deserializes the file
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlFile"></param>
        /// <returns></returns>
        public static T DeserializeFile<T>(string fileName)
        {
            var returnValue = default(T);

            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (isf.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(fileName, FileMode.Open, isf))
                    {
                        using (StreamReader sr = new StreamReader(isfs))
                        {
                            var fileContents = sr.ReadToEnd();
                            if (!string.IsNullOrEmpty(fileContents))
                                returnValue = Deserialize<T>(fileContents);
                        }
                    }
                }
            }

            return returnValue;
        }

        /// <summary>
        /// Serializes the object of type T to the filePath
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="filePath"></param>
        public static void Serialize<T>(T serializableObject, string filePath)
        {
            Serialize(serializableObject, filePath, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="serializableObject"></param>
        /// <param name="filePath"></param>
        /// <param name="encoding"></param>
        public static void Serialize<T>(T serializableObject, string filePath, Encoding encoding)
        {
            using (IsolatedStorageFile isf = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream isfs = new IsolatedStorageFileStream(filePath, FileMode.Create, isf))
                {
                    using (StreamWriter sw = new StreamWriter(isfs))
                    {
                        //sw.Write(data); 
                        // Serialize the object to the writer
                        XmlSerializer serializer = new XmlSerializer(typeof(T));
                        serializer.Serialize(sw, serializableObject);
                        sw.Close();
                    }
                }
            }
        }
    }
}
