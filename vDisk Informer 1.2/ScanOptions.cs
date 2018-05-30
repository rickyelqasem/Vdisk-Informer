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
    public partial class ScanOptions : Form
    {
        bool firsttime = true;
        public delegate void delPassScan(int offset, bool GB, int threshold, string domain, string username, string password);
        public ScanOptions(string GB, int thresh, int off, string dompass, bool ftSO)
        {
            InitializeComponent();
            this.numericUpDown1.Value = thresh;
            this.trackBar1.Value = thresh;
            this.textBox2.Text = dompass;
            this.textBox1.Text = "Domain User";
            this.comboBox1.Text = "Domain";
            firsttime = ftSO;
            if (ftSO)
            {
                textBox2.Font = new Font(textBox2.Font, FontStyle.Italic);
                textBox2.ForeColor = Color.Gray;
               
            }
            else
            {
                this.textBox2.PasswordChar = '*';
               
            }
            if (GB == " GB")
            {
                this.radioButton3.Checked = false;
                this.radioButton4.Checked = true;
            }
            else
            {
                this.radioButton4.Checked = false;
                this.radioButton3.Checked = true;
            }

            if (off == 64)
            {
                this.radioButton1.Checked = false;
                this.radioButton2.Checked = true;
            }
            else
            {
                this.radioButton2.Checked = false;
                this.radioButton1.Checked = true;
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            Program.MyForm1.Show();
            this.Hide();
        }

        private void ScanOptions_FormClosed(object sender, FormClosedEventArgs e)
        {
            Program.MyForm1.Show();
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int offset = 64;
            if (radioButton1.Checked)
            {
                offset = 32;
            }
            bool GB = true;
            if (radioButton3.Checked)
            {
                GB = false;
            }
            int threshold = trackBar1.Value;
            delPassScan del = new delPassScan(Program.MyForm1.GetScanOptions);
            del(offset, GB, threshold, comboBox1.Text, textBox1.Text, textBox2.Text);
            Program.MyForm1.Show();
            this.Hide();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            numericUpDown1.Value = trackBar1.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            trackBar1.Value = (int)numericUpDown1.Value;
        }

        private void ScanOptions_Load(object sender, EventArgs e)
        {
            toolTip1.SetToolTip(trackBar1, "The ratio of free disk space to class as wasted. Example: 25 = report if free is more than 25% of disk capacity.");
            toolTip1.SetToolTip(numericUpDown1, "The ratio of free disk space to class as wasted. Example: 25 = report if free is more than 25% of disk capacity.");
            toolTip1.SetToolTip(radioButton1, "Choose a starting offset depending on the advice of your storage vendor");
            toolTip1.SetToolTip(radioButton2, "Choose a starting offset depending on the advice of your storage vendor");
            toolTip1.SetToolTip(radioButton3, "Report Disk space in MB");
            toolTip1.SetToolTip(radioButton4, "Report Disk space in GB");
            toolTip1.SetToolTip(textBox1, "Domain User used for WMI");
            toolTip1.SetToolTip(textBox2, "Domain user password");
            toolTip1.SetToolTip(comboBox1, "Input or select a Windows Domain");

            comboBox1.Items.Add(Environment.UserDomainName.ToString());

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load("Dconnection.xml");
                XmlNode root1 = xmlDoc.SelectSingleNode(@"jobs/Policy/domain");
                if (root1.InnerText != "")
                {
                    comboBox1.Text = root1.InnerText;
                }
                XmlNode root2 = xmlDoc.SelectSingleNode(@"jobs/Policy/user");
                if (root2.InnerText != "")
                {
                    textBox1.Text = root2.InnerText;
                }
            }
            catch
            {
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
            textBox2.Font = new Font(textBox1.Font, FontStyle.Regular);
            textBox2.ForeColor = Color.Black;
            textBox2.PasswordChar = '*';
        }

        private void comboBox1_Click(object sender, EventArgs e)
        {
            
            comboBox1.Font = new Font(comboBox1.Font, FontStyle.Regular);
            comboBox1.ForeColor = Color.Black;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            textBox1.Font = new Font(textBox1.Font, FontStyle.Regular);
            textBox1.ForeColor = Color.Black;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (firsttime)
            {
                textBox2.Font = new Font(textBox2.Font, FontStyle.Italic);
                textBox2.ForeColor = Color.Gray;
                firsttime = false;
            }
            else
            {
                textBox2.Font = new Font(textBox2.Font, FontStyle.Regular);
                textBox2.ForeColor = Color.Black;
                textBox2.PasswordChar = '*';
            }

        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            comboBox1.Font = new Font(comboBox1.Font, FontStyle.Regular);
            comboBox1.ForeColor = Color.Black;
        }
    }
}
