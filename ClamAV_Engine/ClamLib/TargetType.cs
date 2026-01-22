using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamAV_Engine.ClamLib
{
    public enum TargetType
    {
        Any = 0,
        PE = 1,
        OLE2 = 2,
        HTML = 3,
        Mail = 4,
        Graphics = 5,
        ELF = 6,
        ASCII = 7,
        Unused = 8,
        MachO = 9,
        PDF = 10,
        Flash = 11,
        Java = 12
    }
}
