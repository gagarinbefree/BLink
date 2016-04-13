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
using System.Net;

namespace BLink.Models
{         
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

            _preview.Url = url;
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
                    webClient.Headers.Add("user-agent", "Mozilla/5.0 (Windows NT 6.1; Trident/7.0; rv:11.0) like Gecko");
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
