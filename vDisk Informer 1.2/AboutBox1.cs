using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace vDisk_Informer_1._2
{
    partial class AboutBox1 : Form
    {
        public AboutBox1()
        {
            InitializeComponent();
            
        }

       
   

        private void okButton_Click(object sender, EventArgs e)
        {
            
        }

        private void AboutBox1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Form1 form1 = new Form1();
            //form1.Show();
            this.Close();
        }


    }
}
