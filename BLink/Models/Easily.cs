using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using HtmlAgilityPack;
using System;

namespace BLink.Models
{
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
}