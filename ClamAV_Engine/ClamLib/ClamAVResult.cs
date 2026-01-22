using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Threading.Tasks;

namespace ClamAV_Engine.ClamLib
{
    public class ClamAVResult
    {
        public ScanStatus Status { get; set; }
        public string FilePath { get; set; }
        public string VirusName { get; set; }
        public SignatureType? DetectionType { get; set; }
        public long FileSize { get; set; }
        public string MD5 { get; set; }
        public string SHA256 { get; set; }
        public DateTime ScanTime { get; set; }
        public TimeSpan ScanDuration { get; set; }
        public string ErrorMessage { get; set; }
        public List<ClamAVResult> ArchiveResults { get; set; }


        public ClamAVResult()
        {
            ScanTime = DateTime.Now;
            ArchiveResults = new List<ClamAVResult>();
        }

        public override string ToString()
        {
            switch (Status)
            {
                case ScanStatus.Infected:
                    return $"[INFECTED] {FilePath}: {VirusName} ({DetectionType})";
                case ScanStatus.Clean:
                    return $"[CLEAN] {FilePath}";
                case ScanStatus.Whitelisted:
                    return $"[WHITELISTED] {FilePath}";
                case ScanStatus.Error:
                    return $"[ERROR] {FilePath}: {ErrorMessage}";
                default:
                    return $"[UNKNOWN] {FilePath}";
            }
        }

        public void PrintSummary()
        {
            Console.WriteLine($"\n----------- SCAN SUMMARY -----------");
            Console.WriteLine($"File: {FilePath}");
            Console.WriteLine($"Status: {Status}");
            if (!string.IsNullOrEmpty(VirusName))
                Console.WriteLine($"Virus: {VirusName}");
            Console.WriteLine($"Size: {FileSize:N0} bytes");
            Console.WriteLine($"MD5: {MD5}");
            Console.WriteLine($"SHA256: {SHA256}");
            Console.WriteLine($"Scan Time: {ScanDuration.TotalMilliseconds:F2}ms");
            if (ArchiveResults.Count > 0)
            {
                Console.WriteLine($"\nArchive Contents ({ArchiveResults.Count} files):");
                foreach (var ar in ArchiveResults)
                {
                    Console.WriteLine($"  {ar}");
                }
            }
            Console.WriteLine($"------------------------------------\n");
        }
    }
}
