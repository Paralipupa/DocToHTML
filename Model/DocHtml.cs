using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocToHTML.Properties;
using Mammoth;
using System.Xml;
using System.Text.RegularExpressions;

namespace DocToHTML.Model
{
    class DocHtml : ObservableObject
    {
        public List<string> Warnings { get; set; } = new List<string>();
        public string HTML { get; set; } = string.Empty;


        public bool IsClearNodeA
        {
            get { return bool.Parse(Settings.Default.isClearNodeA); }
            set
            {
                Settings.Default.isClearNodeA = value.ToString();
                OnPropertyChanged("IsClearNodeA");
                Settings.Default.Save();
            }
        }

        public bool IsBorderTable
        {
            get { return bool.Parse(Settings.Default.isBorderTable); }
            set
            {
                Settings.Default.isBorderTable = value.ToString();
                OnPropertyChanged("IsBorderTable");
                Settings.Default.Save();
            }
        }


        public bool ConvertDocToHtml(string fileName)
        {
            bool bRet = false;
            HTML = "";
            try
            {
                Warnings.Clear();


                var converter = new DocumentConverter();

                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "options.txt");
                if (File.Exists(path))
                {
                    string[] NewFile = File.ReadAllLines(path);
                    foreach (string item in NewFile)
                    {
                        converter.AddStyleMap(item);
                    }

                }

                converter.PreserveEmptyParagraphs();
                var result = converter.ConvertToHtml(fileName);


                HTML = result.Value;

                foreach (string item in result.Warnings)
                {
                    Warnings.Add(item);
                }

                bRet = true;

            }
            catch (Exception ex)
            {
                Log.Instance.Write(ex);
                Warnings.Add(ex.Message);
            }

            return bRet;
        }


        /// <summary>
        /// Предварительная обработка HTML
        /// </summary>
        public bool ProcessingHTML()
        {

            if (string.IsNullOrEmpty(HTML))
            {
                return false;
            }

            bool bRet = false;
            try
            {
                HTML = $"<div>{HTML}</div>";

                Encoding enc = Encoding.UTF8;
                Stream stream = new MemoryStream(enc.GetBytes(HTML));

                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(stream);
                XmlElement xRoot = xDoc.DocumentElement;

                #region Обработка XML
                if (IsClearNodeA) //убрать ненужные ссылки
                {
                    Analize_Reference(ref xRoot);
                }

                Analize_Table(ref xRoot);
                Analize_Paragraph(ref xRoot);

                HTML = xRoot.InnerXml;

                if (IsBorderTable)
                {
                    HTML = HTML.Replace("<table>", "<table border=1 cellspacing=0 cellpadding=0>");
                }

                HTML = HTML.Replace("<br />", "<br />\r\n").Replace("> <p>", ">\r\n<p>").Replace("><p>", ">\r\n<p>").Replace("><h", ">\r\n<h").Replace("><tr>", ">\r\n<tr>");

                #endregion

                bRet = true;
            }
            catch (Exception ex)
            {
                Log.Instance.Write(ex);
            }

            return bRet;
        }

        private void Analize_Paragraph(ref XmlElement xRoot)
        {
            try
            {
                //

                List<XmlNode> listNodes = new List<XmlNode>();
                XmlNodeList childnodes = xRoot.ChildNodes;
                foreach (XmlNode n in childnodes)
                {

                    if (n.OuterXml.Contains("{{") && n.OuterXml.Contains("}}"))
                    {

                        listNodes.Add(n);
                    }
                }

                //Заменить ссылки <a> на <span> 
                foreach (XmlNode node in listNodes)
                {
                    XmlNode parent = node.ParentNode;

                    XmlElement e = node.OwnerDocument.CreateElement(node.Name);
                    XmlAttribute attribute = xRoot.OwnerDocument.CreateAttribute("style");

                    string str = node.InnerText;
                    while (str.Contains("{{"))
                    {
                        int beg = str.IndexOf("{{");
                        int end = str.IndexOf("}}");
                        string attr = str.Substring(beg + 2, end - beg - 2);
                        str = (beg > 0 ? str.Substring(0, beg) : "") + str.Substring(end + 2);

                        if (attr.Contains(":"))
                        {
                            attribute.Value += $"{attr}; ";
                        }
                        else
                        {
                            attribute.Value += $"color:{attr}; ";
                        }
                    }

                    e.Attributes.Append(attribute);
                    e.InnerText = str;

                    parent.ReplaceChild(e, node);

                }

            }
            catch (Exception ex)
            {
                Log.Instance.Write(ex);
            }
}

private void Analize_Reference(ref XmlElement xRoot)
{
    try
    {
        List<XmlNode> listNodes = new List<XmlNode>();
        XmlNodeList childnodes = xRoot.SelectNodes("//a");
        foreach (XmlNode n in childnodes)
        {
            if (n.OuterXml.Contains("Pril") == false)
            {

                foreach (XmlAttribute item in n.Attributes)
                {
                    if (item.Name == "href")
                    {
                        listNodes.Add(n);
                    }
                }
            }
        }

        //Заменить ссылки <a> на <span> 
        foreach (XmlNode node in listNodes)
        {
            XmlNode parent = node.ParentNode;

            XmlElement e = node.OwnerDocument.CreateElement("span");
            e.InnerText = node.InnerText;

            parent.ReplaceChild(e, node);

        }

    }
    catch (Exception ex)
    {
        Log.Instance.Write(ex);
    }
}

private void Analize_Table(ref XmlElement xRoot)
{
    try
    {
        List<XmlNode> listNodes = new List<XmlNode>();
        XmlNodeList childnodes = xRoot.SelectNodes("//table");
        foreach (XmlNode nTable in childnodes)
        {
            XmlNodeList childTableNodes = nTable.SelectNodes("tr");
            if (childTableNodes.Count != 0)
            {
                XmlNodeList childTRNodes = childTableNodes[0].SelectNodes("td");
                foreach (XmlNode nTD in childTRNodes)
                {
                    int index = nTD.InnerText.IndexOf("~"); //размер поля
                    if (index != -1)
                    {
                        int nW = 0;
                        try
                        {
                            nW = Int16.Parse(nTD.InnerText.Substring(index + 1).Trim());
                        }
                        catch
                        {
                            nW = 0;
                        }

                        if (nW != 0)
                        {

                            XmlElement e = nTD.OwnerDocument.CreateElement("td");

                            foreach (XmlAttribute item in nTD.Attributes)
                            {
                                e.Attributes.Append(item);
                            }
                            e.InnerText = nTD.InnerText.Substring(0, index);

                            XmlAttribute attribute = xRoot.OwnerDocument.CreateAttribute("width");

                            attribute.Value = nW.ToString();
                            e.Attributes.Append(attribute);

                            XmlNode parent = nTD.ParentNode;
                            parent.ReplaceChild(e, nTD);
                        }


                    }

                }
            }

        }
    }
    catch (Exception ex)
    {
        Log.Instance.Write(ex);
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="fileName"></param>
/// <param name="str"></param>
public void WriteToFile(string fileName)
{
    using (StreamWriter sw = new StreamWriter(fileName, false, System.Text.Encoding.Default))
    {
        sw.WriteLine(HTML);
    }
}
    }
}
