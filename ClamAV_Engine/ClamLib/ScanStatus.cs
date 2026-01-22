using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamAV_Engine.ClamLib
{
    public enum ScanStatus
    {
        Clean,
        Infected,
        Error,
        Whitelisted,
        Encrypted,
        Skipped
    }
}
