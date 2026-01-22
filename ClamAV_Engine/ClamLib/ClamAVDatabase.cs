using System;
using System.Collections.Generic;

namespace ClamAV_Engine.ClamLib
{
    public class ClamAVDatabase
    {
        public Dictionary<string, ClamAVSignature> HdbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> HsbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> MdbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> NdbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> LdbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> LduSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> FpSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ClamAVSignature> CdbSignatures { get; } = new Dictionary<string, ClamAVSignature>(StringComparer.OrdinalIgnoreCase);

        public int TotalSignatures =>
            HdbSignatures.Count +
            HsbSignatures.Count +
            MdbSignatures.Count +
            NdbSignatures.Count +
            LdbSignatures.Count +
            LduSignatures.Count +
            FpSignatures.Count +
            CdbSignatures.Count;

        public void Clear()
        {
            HdbSignatures.Clear();
            HsbSignatures.Clear();
            MdbSignatures.Clear();
            NdbSignatures.Clear();
            LdbSignatures.Clear();
            LduSignatures.Clear();
            FpSignatures.Clear();
            CdbSignatures.Clear();
        }
    }
}