using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;

namespace BLink.Models
{

    public class TextMatch
    {
        public int Count { set; get; }
        public String Text { set; get; }        
    }

    public class ImageMatch
    {
        public string Url { set; get; }
        public int Width { set; get; }
        public int Height { set; get; }
    }
    

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class HtmlSelectorAttribute : Attribute
    {

        public readonly string[] Paths;

        public HtmlSelectorAttribute(params string[] paths)
        {
            Paths = paths;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ImageSelectorAttribute : Attribute
    {
        public readonly string[] Paths;

        public ImageSelectorAttribute(params string[] paths)
        {
            Paths = paths;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MaxMatchTargerAttribute : Attribute
    {
        public readonly string TargetProperty;

        public MaxMatchTargerAttribute(string targetProperty)
        {
            TargetProperty = targetProperty;
        }
    }

    public class LinkImage
    {
        public string Url { set; get; }
    }

    public interface IPreviewable
    {
        string Url { set; get; }
        string Host { set; get; }        
    }

    public class LinkPreview : IPreviewable
    {
        public string Url { set; get; }
        public string Host { set; get; }

        [HtmlSelector("html/head/title"
            , "html/head/meta[@property=\"og:title\"]/@content")]
        public string Title { set; get; }

        [HtmlSelector("html/head/meta[@name='description']/@content" 
            , "html/head/meta[@property='og:description']/@content")]
        public string Description { set; get; }

        [MaxMatchTarger("Title")]
        [HtmlSelector("html/body//p[not(ancestor::script|ancestor::style|ancestor::noscript)]"
            , "html/body//text()[not(ancestor::script|ancestor::style|ancestor::noscript)]")]        
        public string Text { set; get; }

        [ImageSelector("html/head/meta[@property='og:image']"
            , "html/head/meta[@property='twitter:image:src']"
            , "html/body//img/@src")]
        public IList<string> Images { set; get; }
        public LinkPreview()
        {
            Images = new List<string>();
        }
    }

    public interface IParseable
    {
        void Parse(HtmlDocument htmlDoc, PropertyInfo prop, IPreviewable preview);
    }

    public class Easily : IParseable
    {
        public void Parse(HtmlDocument htmlDoc, PropertyInfo prop, IPreviewable preview)
        {
            HtmlSelectorAttribute tagSelector = prop.GetCustomAttribute<HtmlSelectorAttribute>();
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
                        if (node.Name.ToLower() == "meta")
                        {
                            HtmlAttribute attr = node.Attributes.FirstOrDefault(f => f.Name.ToLower() == "content");
                            if (attr != default(HtmlAttribute))
                                inners.Add(HttpUtility.HtmlDecode(attr.Value));
                        }
                        else
                            inners.Add(HttpUtility.HtmlDecode(node.InnerText));
                    }
                }
            }

            if (inners.Count() > 0)
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(preview, inners.OrderBy(f => f.Length).ThenBy(f => f).LastOrDefault());

                if (prop.PropertyType == typeof(IList<string>))
                    prop.SetValue(preview, inners.ToList());
            }
        }
    }

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
                                imageUrl =_getImageLink(_fullyQualifiedImage(node.Attributes["src"].Value, preview.Url)); 
                        }

                        if (!String.IsNullOrWhiteSpace(imageUrl))
                            inners.Add(imageUrl); 
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
                using(System.Net.WebClient webClient = new System.Net.WebClient())
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
            catch {}

            return imageUrl;
        }
    }      
        

    public class Heavy : IParseable
    {
        public void Parse(HtmlDocument htmlDoc, PropertyInfo prop, IPreviewable preview)
        {
            HtmlSelectorAttribute tagSelectorAttr = prop.GetCustomAttribute<HtmlSelectorAttribute>();
            MaxMatchTargerAttribute targetAttr = prop.GetCustomAttribute<MaxMatchTargerAttribute>();

            if (tagSelectorAttr == null || targetAttr == null)
                return;

            PropertyInfo targetProp = preview.GetType().GetProperty(targetAttr.TargetProperty);
            if (targetProp == null)
                return;

            string targetValue = targetProp.GetValue(preview).ToString();
            if (String.IsNullOrWhiteSpace(targetValue))
                return;
            
            IList<TextMatch> matches = new List<TextMatch>();
            foreach (string path in tagSelectorAttr.Paths)
            {
                //var nodes == e.SelectNodes("//style|//script").ForEach(n => n.Remove())

                var nodes = htmlDoc.DocumentNode.SelectNodes(path)
                    .Where(f => f.InnerText.Trim().Length > targetValue.Length
                        && !f.ParentNode.Name.ToLower().Contains("script")
                        && !f.ParentNode.Name.ToLower().Contains("style")
                        && !f.OuterHtml.ToLower().Contains("style")
                        && !f.OuterHtml.ToLower().Contains("script"));
                if (nodes != null)
                {
                    foreach (HtmlNode node in nodes)
                    {
                        string text = node.InnerText;
                        int count = _matchCount(targetValue, text);
                        if (count > 0)
                        {
                            matches.Add(new TextMatch
                            {
                                Count = count,
                                Text = text                                
                            });
                        }
                    }
                }
            }

            TextMatch top = matches.OrderBy(f => f.Count).OrderBy(f => f.Text.Length).LastOrDefault();
            if (top != default(TextMatch))
                prop.SetValue(preview, HttpUtility.HtmlDecode(top.Text));
            
        }

        private int _matchCount(string targetValue, string innerText)
        {
            int ret = 0;
            if (String.IsNullOrWhiteSpace(targetValue) || String.IsNullOrWhiteSpace(innerText))
                return ret;

            string[] targetArr = targetValue.Split(new char[] { ' ', '\t' }).Where(f => f.Length > 3).ToArray();
            foreach (string target in targetArr)
            {
                ret += Regex.Matches(innerText, target).Count;
            }

            return ret;
        }
    }

    public class StratParser
    {
        public IParseable Alg { private set; get; }        
        public HtmlDocument HtmlDoc { private set; get; }

        public StratParser(HtmlDocument htmlDoc, IParseable alg)
        {
            if (alg == null)
                throw new Exception("Algorithm is not defined");

            Alg = alg;
            HtmlDoc = htmlDoc;
        }

        public void Pasre(PropertyInfo prop, IPreviewable preview)
        {
            Alg.Parse(HtmlDoc, prop, preview);
        }
    }

    public interface ILinkParseable
    {
        IPreviewable Parse(string url);
    }

    public class LinkParser : ILinkParseable
    {
        private IPreviewable _preview;
        private HtmlDocument _doc;

        public LinkParser(IPreviewable preview)
        {
            _preview = preview;
        }               

        public IPreviewable Parse(string url)
        {
            Uri uri = null;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri) || uri == null)
                throw new Exception("Dont't create uri from url string");            

            _preview.Host = uri.Host;

            _doc = _loadHtml(url);
           
            IList<PropertyInfo> props = new List<PropertyInfo>(_preview.GetType().GetProperties());
            foreach (PropertyInfo prop in props)
            {
                _propParse(prop);
            }

            return _preview;
        }

        private void _propParse(PropertyInfo prop)
        {
            object[] attrs = prop.GetCustomAttributes(true);
            if (attrs == null || attrs.Length == 0)
                return;

            Attribute attr = attrs[0] as Attribute;
            if (attr == null)
                return;
                        
            StratParser parser = new StratParser(_doc, _getAlgorithm(attr));
            parser.Pasre(prop, _preview);            
        }

        private IParseable _getAlgorithm(Attribute attr)
        {
            if (attr is MaxMatchTargerAttribute)
                return new Heavy();

            if (attr is HtmlSelectorAttribute)
                return new Easily();

            if (attr is ImageSelectorAttribute)
                return new Imagy();

            return null;
        }

        private HtmlDocument _loadHtml(string url)
        {
            HtmlDocument ret = new HtmlDocument();
            try
            {
                byte[] download = null;
                using (System.Net.WebClient webClient = new System.Net.WebClient())
                {
                    download = webClient.DownloadData(url);
                }

                if (download == null || download.Length == 0)
                    throw new Exception("Data is empty");

                string data = Encoding.UTF8.GetString(download);

                var encod = ret.DetectEncodingHtml(data);
                if (encod == null)
                {
                    var ude = new Ude.CharsetDetector();
                    ude.Feed(download, 0, download.Length);
                    ude.DataEnd();

                    encod = !String.IsNullOrEmpty(ude.Charset) ? Encoding.GetEncoding(ude.Charset) : Encoding.UTF8;
                }

                string convstr = Encoding.Unicode.GetString(Encoding.Convert(encod, Encoding.Unicode, download));
                if (String.IsNullOrEmpty(convstr))
                    throw new Exception("Don't convert data to string");

                ret.LoadHtml(convstr);
            }
            catch (Exception ex)
            {
                throw new Exception("Don't create html from url", ex);
            }

            return ret;
        }
    }
}
