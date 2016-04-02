using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace BLink.Models
{
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
}