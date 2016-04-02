using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using HtmlAgilityPack;

namespace BLink.Models
{
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
}