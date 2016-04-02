using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLink.Models
{
    public interface IPreviewable
    {
        string Url { set; get; }
        string Host { set; get; }
    }
}
