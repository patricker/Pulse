using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Pulse.Base
{
    public abstract class XmlSerializable<T>
        where T : class
    {
        public virtual void Save(string path)
        {
            using (FileStream f = File.Create(path))
            {
                Save(f);
            }
        }

        public virtual string Save()
        {
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                Save(sw);
            }

            return sb.ToString();
        }

        public virtual void Save(Stream stream)
        {
            using (var w = new StreamWriter(stream))
            {
                Save(w);
                w.Close();
            }
        }

        private void Save(TextWriter w)
        {
            var s = new XmlSerializer(typeof(T));
            s.Serialize(w, this);
        }

        public static T LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            using (StreamReader sr = new StreamReader(path))
            {
                return Load(sr);
            }
        }

        public static T LoadFromXML(string sb)
        {
            XmlTextReader xtr = new XmlTextReader(new StringReader(sb.ToString()));

            return Load(xtr);
        }

        public static T Load(StreamReader sr)
        {
            using (var xr = new XmlTextReader(sr))
            {                
                return Load(xr);
            }
        }

        public static T Load(XmlTextReader xr)
        {
            var xs = new XmlSerializer(typeof(T));
            T result = null;

            try
            {
                result = (T)xs.Deserialize(xr);
            }
            catch (Exception ex)
            {
            }

            return result;
        }
    }
}
