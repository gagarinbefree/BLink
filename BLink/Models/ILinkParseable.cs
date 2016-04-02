using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLink.Models
{
    public interface ILinkParseable
    {
        IPreviewable Parse(string url);
    }
}
