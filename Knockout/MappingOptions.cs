using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace KnockoutApi
{
    [Imported, Serializable]
    public class MappingOptions
    {
        public List<string> Include, Ignore, Copy, Observe;
    }
}