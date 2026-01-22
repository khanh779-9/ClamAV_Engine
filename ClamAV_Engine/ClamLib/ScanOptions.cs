using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClamAV_Engine.ClamLib
{
    public class ScanOptions
    {

        public bool ScanArchive { get; set; } = false;
        public bool BlockEncrypted { get; set; } = false;
        public int MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB
        public int MaxScanSize { get; set; } = 100 * 1024 * 1024;
        public int MaxFiles { get; set; } = 10000;
        public int MaxRecursion { get; set; } = 16;

        public bool DetectPUA { get; set; } = false; // Potentially Unwanted Applications
        public bool HeuristicScan { get; set; } = false;
        public bool ScanPE { get; set; } = false;
        public bool ScanELF { get; set; } = false;
        public bool ScanOLE2 { get; set; } = false;
        public bool ScanPDF { get; set; } = false;
        public bool ScanHTML { get; set; } = false;
        public bool ScanMail { get; set; } = false;

        public bool UseMultithreading { get; set; } = true;
        public int MaxThreads { get; set; } = Environment.ProcessorCount;

        public bool Verbose { get; set; } = false;
        public bool ShowProgress { get; set; } = true;
        public bool InfectedOnly { get; set; } = false;

        public bool Recursive { get; set; } = true;
        public bool FollowSymlinks { get; set; } = false;
        public List<string> ExcludeDirectories { get; set; } = new List<string>();
        public List<string> ExcludeExtensions { get; set; } = new List<string>();

        public static ScanOptions Default => new ScanOptions();
    }
}
