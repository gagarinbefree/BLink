using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System;

namespace BLink.Models
{
    public interface IParseable
    {
        void Parse(HtmlDocument htmlDoc, PropertyInfo prop, IPreviewable preview);
    }
}
