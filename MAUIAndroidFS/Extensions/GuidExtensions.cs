using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAUIAndroidFS.Extensions
{
    public static class GuidExtensions
    {
        public static Guid UuidFromPartial(this Int32 @partial)
        {
            //this sometimes returns only the significant bits, e.g.
            //180d or whatever. so we need to add the full string
            string id = @partial.ToString("X").PadRight(4, '0');
            if (id.Length == 4)
            {
                id = "5402" + id + "-0200-afb1-e511-542660cd3e69";
            }
            return Guid.ParseExact(id, "d");
        }
    }
}
