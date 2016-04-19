using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using HtmlAgilityPack;
using System.IO;
using System.Drawing;

namespace BLink.Models
{
    public class Imagy : IParseable
    {
        public void Parse(HtmlDocument htmlDoc, PropertyInfo prop, IPreviewable preview)
        {
            ImageSelectorAttribute tagSelector = prop.GetCustomAttribute<ImageSelectorAttribute>();
            if (tagSelector == null)
                return;

            object propValue = prop.GetValue(preview);
            HashSet<string> inners = new HashSet<string>();
            foreach (string path in tagSelector.Paths)
            {
                HtmlNodeCollection nodes = htmlDoc.DocumentNode.SelectNodes(path);
                if (nodes != null)
                {                    
                    foreach (HtmlNode node in nodes)
                    {
                        string imageUrl = "";
                        if (node.Name.ToLower() == "meta")
                        {
                            HtmlAttribute attr = node.Attributes.FirstOrDefault(f => f.Name.ToLower() == "content");
                            if (attr != default(HtmlAttribute))
                                imageUrl = _getImageLink(_fullyQualifiedImage(attr.Value, preview.Url));
                        }
                        else
                        {
                            if (node.Attributes["href"] != null)
                                imageUrl = _getImageLink(_fullyQualifiedImage(node.Attributes["href"].Value, preview.Url));

                            if (node.Attributes["src"] != null)
                                imageUrl = _getImageLink(_fullyQualifiedImage(node.Attributes["src"].Value, preview.Url));
                        }

                        if (!String.IsNullOrWhiteSpace(imageUrl))
                            inners.Add(imageUrl);

                        if (inners.Count() > 4)
                            break;                        
                    }
                }
            }

            if (inners.Count() > 0)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(preview, inners.FirstOrDefault());

                if (prop.PropertyType == typeof(IList<string>))
                    prop.SetValue(preview, inners.ToList());
            }
        }


        private string _getImageLink(string url)
        {
            try
            {
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    byte[] imageData = webClient.DownloadData(url);
                    MemoryStream stream = new MemoryStream(imageData);
                    Image img = Image.FromStream(stream);
                    stream.Close();

                    if ((img.Width > 100 && img.Height > 50) || (img.Width > 50 && img.Height > 100))
                        return url;
                }
            }
            catch { }

            return "";
        }

        private string _fullyQualifiedImage(string imageUrl, string siteUrl)
        {
            if (imageUrl.Contains("http:") || imageUrl.Contains("https:"))
                return imageUrl;

            if (imageUrl.IndexOf("//") == 0)
                return "http:" + imageUrl;

            try
            {
                string baseurl = siteUrl.Replace("http://", string.Empty).Replace("https://", string.Empty);
                baseurl = baseurl.Split('/')[0];
                return string.Format("http://{0}{1}", baseurl, imageUrl);

            }
            catch { }

            return imageUrl;
        }
    }      
}