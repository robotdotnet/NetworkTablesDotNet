using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTables.NetworkTables2.Util
{
    public class Set : List
    {
        public new void Add(object o)
        {
            if (!Contains(o))
                base.Add(o);
        }
    }
}
