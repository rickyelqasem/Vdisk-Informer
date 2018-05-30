using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace vDisk_Informer_1._2
{
    public partial class VMwarelogin : Form
    {
        bool firsttime;

        public delegate void delPassData(string url, string username, string password, bool https, int port);
        public VMwarelogin(string vpassword, bool https, int port, bool ftopenVI)
        {
            firsttime = ftopenVI;
            InitializeComponent();
            this.textBox3.Text = vpassword;
            if (https)
            {
                this.radioButton1.Checked = true;
                this.radioButton2.Checked = false;
            }
            else
            {
                this.radioButton2.Checked = true;
                this.radioButton1.Checked = false;
            }
            this.numericUpDown1.Value = port;

            if (ftopenVI)
            {
                textBox3.Font = new Font(textBox3.Font, FontStyle.Italic);
                textBox3.ForeColor = Color.Gray;
                
            }
            else
            {
                textBox3.PasswordChar = '*';
                
            }
        }

        private void VMwarelogin_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(radioButton1, "Use HTTPS when communicating with VI SDK");
            toolTip1.SetToolTip(radioButton2, "Use HTTP when communicating with VI SDK");
            toolTip1.SetToolTip(numericUpDown1, "The TCP port used when communicating with VI SDK");
            toolTip1.SetToolTip(textBox1, "Hostname or IP Address of your ESX or vCenter server");
            toolTip1.SetToolTip(textBox2, "vSphere User");
            toolTip1.SetToolTip(textBox3, "vSphere User Password");

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Vconnection.xml");
                XmlNode root1 = xmlDoc.SelectSingleNode(@"jobs/Policy/host");
                textBox1.Text = root1.InnerText;
                XmlNode root2 = xmlDoc.SelectSingleNode(@"jobs/Policy/user");
                textBox2.Text = root2.InnerText;
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            Program.MyForm1.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool https = true;
            if (radioButton2.Checked)
            {
                https = false;
            }
            int port = (int)numericUpDown1.Value;
            delPassData del = new delPassData(Program.MyForm1.ScanVMware);
            del(textBox1.Text, textBox2.Text, textBox3.Text, https, port);
            Program.MyForm1.Show();
            this.Hide();
        }

        private void VMwarelogin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MyForm1.Show();
            this.Hide();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                numericUpDown1.Value = 443;
            }
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked)
            {
                numericUpDown1.Value = 80;
            }
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.Font = new Font(textBox1.Font, FontStyle.Regular);
            textBox1.ForeColor = Color.Black;
        }

        private void textBox2_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            textBox2.Font = new Font(textBox2.Font, FontStyle.Regular);
            textBox2.ForeColor = Color.Black;
        }

        private void textBox3_Click(object sender, EventArgs e)
        {
            textBox3.Text = "";
            textBox3.Font = new Font(textBox3.Font, FontStyle.Regular);
            textBox3.ForeColor = Color.Black;
            textBox3.PasswordChar = '*';
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Font = new Font(textBox1.Font, FontStyle.Regular);
            textBox1.ForeColor = Color.Black;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            textBox2.Font = new Font(textBox2.Font, FontStyle.Regular);
            textBox2.ForeColor = Color.Black;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            

            if (firsttime)
            {
                textBox3.Font = new Font(textBox3.Font, FontStyle.Italic);
                textBox3.ForeColor = Color.Gray;
                firsttime = false;
            }
            else
            {
                textBox3.Font = new Font(textBox3.Font, FontStyle.Regular);
                textBox3.ForeColor = Color.Black;
                textBox3.PasswordChar = '*';
            }
            
        }

        


        


    }
}
