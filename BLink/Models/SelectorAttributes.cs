
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BLink.Models
{
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
}