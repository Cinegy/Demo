using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Xml;

namespace ChartControlSample
{
    // class Variable - define the name, type and value
    public class Variable
    {
        public Variable(String _name, String _type, String _val)
        {
            name = _name;
            type = _type;
            var_value = _val;
        }

        private String name;
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        private String type;
        public String Type
        {
            get { return type; }
            set { type = value; }
        }

        private String var_value;
        public String Value
        {
            get { return var_value; }
            set { var_value = value; }
        }
    }

    public class RequestManager
    {
        // Create the request body (XML) and convert it to the byte array
        public static byte[] CreateXmlRequest(Array Variables)
        {
            MemoryStream stream = new MemoryStream();

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = false;
            settings.NewLineOnAttributes = false;
            settings.Encoding = Encoding.UTF8;

            XmlWriter writer = XmlWriter.Create(stream, settings);

            writer.WriteStartElement("PostRequest");            // <PostRequest>

            foreach (Variable var in Variables)
            {
                writer.WriteStartElement("SetValue");               // <SetValue>
                writer.WriteAttributeString("Name", var.Name);
                writer.WriteAttributeString("Type", var.Type);
                writer.WriteAttributeString("Value", var.Value);
                writer.WriteEndElement();                           // </SetValue>
            }

            writer.WriteEndElement();                           // </PostRequest>
            writer.Flush();

            return stream.ToArray();
        }

        // Send the HTTP request to the playout server
        public static bool SendPlayoutRequest(String server, int instance, byte[] data)
        {
            String url = String.Format("http://{0}:{1}/postbox", server, 5521 + instance);

            WebClient request = new WebClient();
            byte[] response;
            request.Headers.Add("Content-Type", "text/xml; utf-8");

            try
            {
                response = request.UploadData(url, "POST", data);
                return true;
            }
            catch (Exception e)
            {
                String resp = String.Format("Error sending request: {0}", e.Message);
                return false;
            }
        }
    }
}
