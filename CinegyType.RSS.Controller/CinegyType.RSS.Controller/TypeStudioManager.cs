using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using CinegyType.RSS.Controller.Model;

namespace CinegyType.RSS.Controller
{
    internal class TypeStudioHelper
    {
        internal enum VariableType
        {
            Text = 1,
            File = 2,
            Float = 3,
            Int = 4,
            Bool = 5,
            Color = 6,
            Trigger = 7,
            Vector2D = 8,
            Vector3D = 9,
            Font = 10
        }

        internal static byte[] CreateXmlRequest(IEnumerable<Variable> variables)
        {
            var stream = new MemoryStream();

            var settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                NewLineOnAttributes = false,
                Encoding = Encoding.UTF8
            };

            var writer = XmlWriter.Create(stream, settings);

            writer.WriteStartElement("PostRequest"); // <PostRequest>

            foreach (var var in variables)
            {
                writer.WriteStartElement("SetValue"); // <SetValue>
                writer.WriteAttributeString("Name", var.Name);
                writer.WriteAttributeString("Type", var.Type.ToString());
                writer.WriteAttributeString("Value", var.Value);
                writer.WriteEndElement(); // </SetValue>
            }

            writer.WriteEndElement(); // </PostRequest>
            writer.Flush();

            return stream.ToArray();
        }

        internal static bool SendPlayoutRequest(PlayoutSettings playout, byte[] data)
        {
            //'.' is illegal in url
            if (playout.Hostname.Trim().Equals(".", StringComparison.InvariantCultureIgnoreCase))
                playout.Hostname = "localhost";

            var url = string.Format("http://{0}:{1}/postbox", playout.Hostname, 5521 + playout.Id);

            var request = new WebClient();
            request.Headers.Add("Content-Type", "text/xml; utf-8");

            try
            {
                request.UploadData(url, "POST", data);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("{0} - Failed on sending request. Server: {1}, instance: {2}, Error: {3}",
                    DateTime.Now.ToLongTimeString(),
                    playout.Hostname,
                    playout.Id,
                    e.Message);
                return false;
            }
        }

        internal static List<Variable> GetTemplateVariables(string filepath)
        {
            var variablesText = new List<Variable>();

            using (var reader = XmlReader.Create(filepath))
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(reader);
                var variables = xmlDoc.SelectSingleNode("Titler/Public/Variables");
                if (variables != null)
                {
                    var varNodes = variables.ChildNodes;

                    foreach (XmlNode element in varNodes)
                    {
                        try
                        {
                            var type = ToVariableType(element.Name);

                            if (element.Attributes != null)
                            {
                                var name = element.Attributes["NAME"].Value;
                                var value = element.Attributes["VALUE"].Value;

                                variablesText.Add(new Variable {Name = name, Type = type, Value = value});
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("{0} - Failed adding variable.\nInnerXml: {1}.\nError: {2}",
                                DateTime.Now.ToLongTimeString(),
                                element.InnerXml,
                                e.Message);
                        }
                    }
                }
            }

            return variablesText;
        }

        private static VariableType ToVariableType(string arg)
        {
            switch (arg)
            {
                case "Text":
                case "String":
                    return VariableType.Text;
                case "File":
                    return VariableType.File;
                case "Float":
                    return VariableType.Float;
                case "Int":
                    return VariableType.Int;
                case "Color":
                    return VariableType.Color;
                case "Bool":
                    return VariableType.Bool;
                case "Trigger":
                    return VariableType.Trigger;
                case "Vec2f":
                    return VariableType.Vector2D;
                case "Vec3f":
                    return VariableType.Vector3D;
                case "Font":
                    return VariableType.Font;
                default:
                    throw new ArgumentException(string.Format("Unexpected variable type: {0}", arg));
            }
        }

        [XmlRoot("var")]
        internal class Variable
        {
            #region Properties

            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "type")]
            public VariableType Type { get; set; }

            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }

            #endregion
        }
    }
}