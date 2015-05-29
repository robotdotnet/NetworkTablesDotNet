using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkTablesDotNet.NetworkTables2.Util
{
    public abstract class StringCache
    {
        private readonly Dictionary<string, string> cache = new Dictionary<string, string>();

        public string Get(string input)
        {
            string cachedValue;
            if (!cache.TryGetValue(input, out cachedValue))
            {
                cache.Add(input, cachedValue = Calc(input));
            }
            return cachedValue;
        }

        public abstract string Calc(string input);
    }
}
