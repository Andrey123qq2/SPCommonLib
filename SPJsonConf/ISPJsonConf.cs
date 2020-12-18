using System;
using System.Collections.Generic;
using System.Text;

namespace SPSCommon.SPJsonConf
{
    public interface ISPJsonConf<T>
    {
        T ERConf { get; }
    }
}
