using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamAV_Engine.ClamLib
{
    [Flags]
    public enum SignatureType
    {
        
        ANY = 0, // Any type

        HDB,    // MD5 or SHA-256 hash
        HSB,    // MD5 or SHA-256 hash
        MDB,    // MD5 or SHA-256 of PE section
        FP,     // False positive (whitelist)
        NDB,    // Hex pattern (body-based)
        LDB,    // Logical signature
        CDB ,    // Container/Archive signature
        LDU  // Logical signature update
    }
}
