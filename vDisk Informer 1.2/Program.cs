using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace vDisk_Informer_1._2
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 
        static public Form1 MyForm1;  

        //static public VMwarelogin vmwarelogin;  


        [STAThread]
        static void Main()
        {
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            MyForm1 = new Form1();
            //vmwarelogin = new VMwarelogin("password", true, 443, true);
            Application.Run(new Form1());
        }
    }
}
