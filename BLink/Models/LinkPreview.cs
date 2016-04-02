using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BLink.Models
{
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
}