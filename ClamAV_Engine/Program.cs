using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ClamAV_Engine
{

    /*
     
        Thành phần	Nặng / nhẹ	Lý do
        ExtractCvd	Nhẹ	Chỉ tách tar.gz
        Giải nén tar.gz	Nhẹ	Không load vào RAM nhiều
        LoadNdb	Nhẹ	Hex pattern
        LoadHdb	Rất nhẹ	Hash
        LoadMdb	Nhẹ	Metadata
        LoadLdb	NẶNG	Regex phức tạp
        Build AC matcher	NẶNG NHẤT	Tốn RAM & CPU
      
     */


    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
