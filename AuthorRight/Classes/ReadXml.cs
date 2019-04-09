using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace AuthorRightClaim.Classes
{
    class ReadXml
    {
        private static string path { get; set; }

        public static int key = 0;

        public static Dictionary<string, string> dict = new Dictionary<string, string>();
        public static Dictionary<string, string> childDict = new Dictionary<string, string>();

        /// <summary>
        /// Read XML
        /// </summary>
        /// <param name="fileName"></param>
        public static bool getXmlData(string fileName)
        {
            //path = Utilities.ReadConfigFile("xmlPath");
            //string fullFilePath = path + fileName;
            try
            {
                bool success = false;
                path = fileName;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                string xml = xmlDoc.OuterXml.ToString();
                var doc = XDocument.Parse(xml);
                XmlNodeList nodes = xmlDoc.DocumentElement.ChildNodes;

                foreach (XmlNode node in nodes)
                {
                    key = 0;
                    dict.Add("Table", node.Name);
                    Console.WriteLine("Key = Table, Value = " + node.Name);

                    if (node.Attributes != null)
                    {
                        getNodeAttributeData(node);
                    }
                    if (node.HasChildNodes)
                    {
                        getChildNodeData(node);
                    }

                    Console.WriteLine("----------------------------------");

                    success = PopulateDB.updateDB(dict);
                    dict.Clear();
                }
                return success;
            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception Occurred : " + e);
                return false;
            }
        }

        /// <summary>
        /// Get child node data
        /// </summary>
        /// <param name="node"></param>
        private static void getChildNodeData(XmlNode node)
        {
            string keyName = "";
            string value = "";
            string prvValue = "";
            try
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    //int keyInt = 0;
                    keyName = childNode.Name;
                    value = childNode.InnerText.Trim();

                    if (keyName.Contains("-"))
                    {
                        keyName = keyName.Replace('-', '_');
                    }

                    while (dict.ContainsKey(keyName))
                    {
                        if (keyName.Contains("category"))
                        {
                            prvValue = dict[keyName];
                            value = prvValue + "/" + value;
                            dict.Remove(keyName);
                        }
                        else if (keyName.Contains("display_name"))
                        {
                            prvValue = dict[keyName];
                            value = prvValue + "/" + value;
                            dict.Remove(keyName);
                        }
                        else
                            keyName = key++ + "_" + childNode.Name;
                    }

                    if ((value != null || value != "") && keyName != "#text")
                    {
                        //formattedValue = Utilities.removeSpecialCharachters(childNode.InnerText.Trim());
                        dict.Add(keyName, value);
                        Console.WriteLine("Key = " + keyName + ", Value = " + value);
                    }

                    if (childNode.Attributes != null)
                    {
                        #region
                        /*int key = 0;
                        foreach (XmlAttribute attribute in childNode.Attributes)
                        {
                            string value = attribute.Value;
                            string attrName = attribute.Name;
                            while (dict.ContainsKey(attrName))
                            {
                                attrName = key++ + "_" + attribute.Name.ToString();
                            }

                            if (value != null || value == "")
                            {
                                dict.Add(attrName, value);
                                Console.WriteLine("Key = {0}, Value = {1}", attrName, value);
                            }                            
                        }*/
                        #endregion
                        getNodeAttributeData(childNode);
                    }
                    if (childNode.HasChildNodes)
                    {
                        getChildNodeData(childNode);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception Occurred : " + e);
                Utilities.sendEmail("Exception Occurred : " + e);
            }
            
        }

        /// <summary>
        /// Get node attribute data
        /// </summary>
        /// <param name="node"></param>
        private static void getNodeAttributeData(XmlNode node)
        {
            string value = "";
            string attrName = "";
            try
            {
                foreach (XmlAttribute attribute in node.Attributes)
                {
                    attrName = attribute.Name;
                    if (attrName.Contains("-"))
                    {
                        attrName = attrName.Replace('-', '_');
                    }
                    value = attribute.Value.Trim();
                    while (dict.ContainsKey(attrName))
                    {
                        attrName = key++ + "_" + attribute.Name.ToString();
                    }
                    if (attribute.Value != null || attribute.Value != "")
                    {
                        //formattedVal = Utilities.removeSpecialCharachters(value);
                        dict.Add(attrName, value);
                        Console.WriteLine("Key = " + attrName + ", Value = " + value);
                    }
                }
            }
            catch (Exception e)
            {
                Utilities.sendEmail("Exception Occurred : " + e);
            }           
        }


    }
}
