using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Pulse.Base
{
    public abstract class XmlSerializable
    {
        public virtual void Save(string path)
        {
            var w = new StreamWriter(path);
            var s = new XmlSerializer(this.GetType());
            s.Serialize(w, this);
            w.Close();
        }

        public static object Load(Type t, string path)
        {
            if (File.Exists(path))
            {
                var sr = new StreamReader(path);
                var xr = new XmlTextReader(sr);
                var xs = new XmlSerializer(t);
                object result = null;
                try
                {
                    result = xs.Deserialize(xr);
                }
                catch (Exception ex)
                {

                }

                xr.Close();
                sr.Close();
                return result;
            }
            return null;
        }
    }
}
