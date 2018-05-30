using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using VimyyApi;

namespace vDisk_Informer_1._2
{
    public partial class Form1 : Form
    {
        string vurl;
        string vhost;
        string vuser;
        string vpassword = "password";
        bool vhttps = true;
        int vport = 443;
        bool isVC = true;
        long bytes = 1073741824;
        string gb = " GB";
        int vThreshold = 50;
        bool isTWCancelled = false;
        int vOffset = 64;
        string domuser;
        bool ftopenSO = true;
        bool ftopenVI = true;
        string dompass = "password";
        string domdom;
        string moref;
        string name;
        string vfilename;
        ServiceContent sic = new ServiceContent();

        public Form1()
        {
            InitializeComponent();
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            //this.Hide();
            AboutBox1 about = new AboutBox1();
            about.ShowDialog();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            //Program.vmwarelogin.Show();
            this.Hide();
            VMwarelogin vmwarelogin = new VMwarelogin(vpassword, vhttps, vport, ftopenVI );
            vmwarelogin.ShowDialog();
            ftopenVI = false;

        }

        public void ScanVMware(string host, string username, string password, bool https, int port)
        {
            treeView1.Nodes.Clear();
            treeView2.Nodes.Clear();
            ftopenVI = false;
            //MessageBox.Show(host);
            vhost = host;
            vport = port;
            vhttps = https;
            if (https)
            {
                if (port != 443)
                {
                    vurl = "https://" + host + ":" + port.ToString() + "/sdk";
                }
                else
                {
                    vurl = "https://" + host + "/sdk";
                }
            }
            else
            {
                if (port != 80)
                {
                    vurl = "http://" + host + ":" + port.ToString() + "/sdk";
                }
                else
                {
                    vurl = "http://" + host + "/sdk";
                }
            }


            //MessageBox.Show(username);
            vuser = username;
            //MessageBox.Show(password);
            vpassword = password;
            //MessageBox.Show(vpassword);
            //vhttps = https.ToString();
            //MessageBox.Show(port.ToString());
            //vport = port.ToString();
            //----------------------------now to activate treeWorker---------------------

            pictureBox1.Visible = true;
            toolStripButton1.Enabled = false;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = false;
            toolStripButton7.Enabled = false;
            treeWorker.RunWorkerAsync();
            // code to build the tree should go in the treeWorker_DoWork event.
            //----------------------------whenfinished------------------------------------
            //
            // when finished we should send treeWorker.RunWorkerCompleted tell worker finished.

            try
            {
                XmlTextWriter textwriter = new XmlTextWriter("Vconnection.xml", null);
                textwriter.WriteStartDocument();
                textwriter.WriteStartElement("jobs");
                textwriter.WriteEndElement();
                textwriter.WriteEndDocument();
                textwriter.Close();

                XmlDocument np = new XmlDocument();

                np.Load("Vconnection.xml");
                XmlElement el = np.CreateElement("Policy");
                string Policy = "<host>" + vhost + "</host>" +
                        "<user>" + vuser + "</user>";
                el.InnerXml = Policy;
                np.DocumentElement.AppendChild(el);
                np.Save("Vconnection.xml");
            }
            catch
            {
                MessageBox.Show("Cannot write vSphere connection file", "Error with file");
                
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            this.Hide();
            ScanOptions scanoptions = new ScanOptions(gb, vThreshold, vOffset, dompass, ftopenSO);
            scanoptions.ShowDialog();
            
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
            
            try
            {
                XmlTextWriter textwriter = new XmlTextWriter("Dconnection.xml", null);
                textwriter.WriteStartDocument();
                textwriter.WriteStartElement("jobs");
                textwriter.WriteEndElement();
                textwriter.WriteEndDocument();
                textwriter.Close();

                XmlDocument np = new XmlDocument();

                np.Load("Dconnection.xml");
                XmlElement el = np.CreateElement("Policy");
                string Policy = "<domain>" + domdom + "</domain>" +
                        "<user>" + domuser + "</user>";
                el.InnerXml = Policy;
                np.DocumentElement.AppendChild(el);
                np.Save("Dconnection.xml");
            }
            catch
            {
                MessageBox.Show("Cannot write Domain connection file", "Error with file");
            }
            ftopenSO = false;
        }
        public void GetScanOptions(int offset, bool GB, int threshold, string domain, string username, string password)
        {
            //MessageBox.Show(domain);
            domdom = domain;
            //MessageBox.Show(username);
            domuser = username;
            //MessageBox.Show(password);
            dompass = password;
            //MessageBox.Show(offset.ToString());
            //MessageBox.Show(GB.ToString());
            //MessageBox.Show(threshold.ToString());
            ftopenSO = false;
            if (GB)
            {
            }
            else
            {
                gb = " MB";
                bytes = 1048576;
            }
            vThreshold = threshold;

            if (offset != 64)
            {
                vOffset = 32;
            }
            else
            {
                vOffset = 64;
            }

        }

        private void treeWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                APIversion API = new APIversion();
                API.CreateServiceRef("ServiceInstance");
                sic = (ServiceContent)API.Collect(vurl, vuser, vpassword);
                isTWCancelled = false;


                if (sic.about.apiType == "VirtualCenter")
                {
                    isVC = true;
                    treeWorker.ReportProgress(10);
                    //##################################Collect Datacenter Objects########################################
                    DCcollector DC = new DCcollector();
                    ArrayList ObjectListBox = new ArrayList();
                    string DCType = "Datacenter";
                    DC.CreateServiceRef("ServiceInstance");
                    ObjectListBox = (ArrayList)DC.Collect("vcenter", vurl, vuser, vpassword);

                    //####################################################################################################

                    treeWorker.ReportProgress(10);

                    //##################################Collect Folder Objects########################################

                    FolderCollecter FD = new FolderCollecter();
                    //ArrayList ObjectListBox = new ArrayList();
                    string FDType = "Folder";
                    FD.CreateServiceRef("ServiceInstance");
                    ObjectListBox = (ArrayList)FD.Collect(vurl, vuser, vpassword, FDType);

                    //####################################################################################################

                    treeWorker.ReportProgress(10);

                    //##################################Collect Cluster Objects########################################

                    CScollector CS = new CScollector();
                    //ArrayList ObjectListBox = new ArrayList();
                    string CSType = "ComputeResource";
                    CS.CreateServiceRef("ServiceInstance");
                    ObjectListBox = (ArrayList)CS.Collect(vurl, vuser, vpassword, CSType);

                    //####################################################################################################

                    treeWorker.ReportProgress(10);

                    //##################################Collect Host Objects########################################

                    Hostcollector HT = new Hostcollector();
                    //ArrayList ObjectListBox = new ArrayList();
                    string HTType = "HostSystem";
                    HT.CreateServiceRef("ServiceInstance");
                    ObjectListBox = (ArrayList)HT.Collect(vurl, vuser, vpassword, HTType);

                    //####################################################################################################

                    treeWorker.ReportProgress(10);

                    //##################################Collect VM Objects########################################

                    VMCollector VM = new VMCollector();
                    //ArrayList ObjectListBox = new ArrayList();
                    string VMType = "VirtualMachine";
                    VM.CreateServiceRef("ServiceInstance");

                    ObjectListBox = (ArrayList)VM.Collect(vurl, vuser, vpassword, VMType);

                    //####################################################################################################

                }
                else
                {
                    treeWorker.ReportProgress(50);
                    isVC = false;
                    //##################################Collect EX Objects########################################
                    ESXcollector EX = new ESXcollector();
                    ArrayList ObjectListBox2 = new ArrayList();
                    string EXType = "VirtualMachine";
                    EX.CreateServiceRef("ServiceInstance");
                    ObjectListBox2 = (ArrayList)EX.Collect(vurl, vuser, vpassword, EXType, "esx");

                    //######################################################################################################
                    treeWorker.ReportProgress(50);
                }
            }
            catch
            {
                //MessageBox.Show("Check Connection Settings", "Error");
                isTWCancelled = true;
            }



        }

        private void treeWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (isTWCancelled)
            {
                MessageBox.Show("Check Connection Settings", "Error");
                pictureBox1.Visible = false;
                toolStripButton1.Enabled = true;
                treeWorker.CancelAsync();

            }
            else
            {
                pictureBox1.Visible = false;
                toolStripButton1.Enabled = true;
                toolStripButton7.Enabled = true;
                treeView1.Enabled = true;
                treeView1.Visible = true;
                splitContainer1.IsSplitterFixed = false;

                if (isVC)
                {
                    //populate VC tree

                    treeView1.Nodes.Clear();
                    try
                    {

                        treeView1.Nodes.Clear();
                        treeView1.ImageIndex = 0;

                        treeView1.ImageList = imageList1;
                        treeView1.SelectedImageIndex = 5;
                    }
                    catch
                    {
                        MessageBox.Show("Cannot load image deck", "Error VI tree");
                    }

                    // SECTION 1. Create a DOM Document and load the XML data into it.
                    XmlDocument dom = new XmlDocument();
                    try
                    {

                        dom.Load("Inventory.xml");
                    }
                    catch
                    {
                        MessageBox.Show("Cannot load inventory", "Error VI tree");
                    }
                    // SECTION 2. Initialize the TreeView control.

                    treeView1.Nodes.Add("root", vhost);
                    treeView1.HideSelection = false;

                    XmlNodeList xmlnode3 = dom.SelectNodes("//Datacenter[@moref]/DCname");
                    int nc = 0;
                    foreach (XmlNode node in xmlnode3)
                    {

                        //MessageBox.Show(node.Attributes.Item(0).Value);
                        TreeNode nod = new TreeNode();
                        //nod.Name = "Datacenter";
                        nod.Name = node.ParentNode.Attributes.Item(0).Value;
                        nod.Text = node.Attributes.Item(0).Value;
                        nod.ImageIndex = 1;

                        treeView1.Nodes[0].Nodes.Add(node.ParentNode.Attributes.Item(0).Value, node.Attributes.Item(0).Value, 1).EnsureVisible();
                        nc++;
                        XmlNode xmlnode4 = dom.SelectSingleNode("//Datacenter[@moref='" + nod.Name + "']/Folder");
                        int cluscount = 0;

                        if (xmlnode4.ChildNodes.Count != 0)
                        {
                            for (int i = 0; i <= xmlnode4.ChildNodes.Count - 1; )
                            {

                                //MessageBox.Show(xmlnode4.ChildNodes.Item(i).Name);
                                if (xmlnode4.ChildNodes.Item(i).Name == "Cluster")
                                {
                                    int hostcount = 0;
                                    //xmlnode4.ChildNodes)).container).ChildNodes)))).Items[0]))).Attributes)).Nodes)).Items[0]))).Value
                                    string clusmoref = xmlnode4.ChildNodes.Item(i).Attributes.Item(0).Value;
                                    XmlNode xmlnode5 = dom.SelectSingleNode("//Cluster[@moref='" + clusmoref + "']/ClusterName");
                                    //MessageBox.Show(xmlnode5.InnerText);
                                    treeView1.Nodes[0].Nodes[nc - 1].Nodes.Add(clusmoref, xmlnode5.InnerText, 2);

                                    XmlNode xmlnode6 = dom.SelectSingleNode("//Cluster[@moref='" + clusmoref + "']");
                                    for (int cn = 0; cn <= xmlnode6.ChildNodes.Count - 1; )
                                    {

                                        if (xmlnode6.ChildNodes.Item(cn).Name == "Host")
                                        {

                                            string hostmoref = xmlnode6.ChildNodes.Item(cn).Attributes.Item(0).Value;
                                            XmlNode xmlnode7 = dom.SelectSingleNode("//Host[@moref='" + hostmoref + "']/HostName");



                                            treeView1.Nodes[0].Nodes[nc - 1].Nodes[cluscount].Nodes.Add(hostmoref, xmlnode7.InnerText, 3);

                                            XmlNode xmlnode8 = dom.SelectSingleNode("//Host[@moref='" + hostmoref + "']");

                                            for (int vc = 0; vc <= xmlnode8.ChildNodes.Count - 1; )
                                            {
                                                if (xmlnode8.ChildNodes.Item(vc).Name == "VM")
                                                {
                                                    string vmmoref = xmlnode8.ChildNodes.Item(vc).Attributes.Item(0).Value;
                                                    XmlNode xmlnode9 = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");
                                                    if (xmlnode9 != null)
                                                    {

                                                        XmlNode xmlvmON = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/GuestState");
                                                        if (xmlvmON != null)
                                                        {
                                                            if (xmlvmON.InnerText == "toolsOk")
                                                            {
                                                                treeView1.Nodes[0].Nodes[nc - 1].Nodes[cluscount].Nodes[hostcount].Nodes.Add(vmmoref, xmlnode9.InnerText, 6);
                                                            }
                                                            else
                                                            {
                                                                treeView1.Nodes[0].Nodes[nc - 1].Nodes[cluscount].Nodes[hostcount].Nodes.Add(vmmoref, xmlnode9.InnerText, 4);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            treeView1.Nodes[0].Nodes[nc - 1].Nodes[cluscount].Nodes[hostcount].Nodes.Add(vmmoref, xmlnode9.InnerText, 4);
                                                        }
                                                    }
                                                }
                                                vc++;
                                            }
                                            hostcount++;
                                        }
                                        cn++;
                                    }

                                    cluscount++;


                                }
                                if (xmlnode4.ChildNodes.Item(i).Name == "HostFolder")
                                {
                                    //xmlnode4.ChildNodes)).container).ChildNodes)))).Items[0]))).Attributes)).Nodes)).Items[0]))).Value
                                    string hostfoldermoref = xmlnode4.ChildNodes.Item(i).Attributes.Item(0).Value;
                                    XmlNode xmlnode5 = dom.SelectSingleNode("//HostFolder[@moref='" + hostfoldermoref + "']");
                                    for (int c = 0; c <= xmlnode5.ChildNodes.Count - 1; )
                                    {
                                        string hostmoref = xmlnode5.ChildNodes.Item(c).Attributes.Item(0).Value;
                                        XmlNode xmlnode6 = dom.SelectSingleNode("//Host[@moref='" + hostmoref + "']/HostName");

                                        treeView1.Nodes[0].Nodes[nc - 1].Nodes.Add(hostmoref, xmlnode6.InnerText, 3);

                                        XmlNode xmlnode8 = dom.SelectSingleNode("//Host[@moref='" + hostmoref + "']");

                                        for (int vc = 0; vc <= xmlnode8.ChildNodes.Count - 1; )
                                        {
                                            if (xmlnode8.ChildNodes.Item(vc).Name == "VM")
                                            {
                                                string vmmoref = xmlnode8.ChildNodes.Item(vc).Attributes.Item(0).Value;
                                                XmlNode xmlnode9 = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");
                                                if (xmlnode9 != null)
                                                {
                                                    treeView1.Nodes[0].Nodes[nc - 1].Nodes[i].Nodes.Add(vmmoref, xmlnode9.InnerText, 4);
                                                }
                                            }
                                            vc++;
                                        }

                                        c++;
                                    }

                                }
                                i++;
                            }

                        }



                    }




                }

                else
                {

                    //populate tree with standalone ESX
                    treeView1.Nodes.Clear();
                    try
                    {

                        try
                        {

                            treeView1.Nodes.Clear();
                            treeView1.ImageIndex = 0;

                            treeView1.ImageList = imageList1;
                            treeView1.SelectedImageIndex = 5;
                        }
                        catch
                        {
                            MessageBox.Show("Cannot load image deck", "Error VI tree");
                        }

                    }
                    catch
                    {
                        MessageBox.Show("Cannot load image deck", "Error VI tree");
                    }
                    // SECTION 1. Create a DOM Document and load the XML data into it.
                    XmlDocument dom = new XmlDocument();
                    try
                    {
                        dom.Load("Inventory.xml");

                        // SECTION 2. Initialize the TreeView control.
                        treeView1.Nodes.Clear();
                        treeView1.ImageIndex = 0;
                        treeView1.Nodes.Add("root", vhost, 3).EnsureVisible();
                        //TreeNode tNode = new TreeNode();
                        //tNode = treeView1.Nodes[0];
                        treeView1.HideSelection = false;

                        XmlNodeList xmlnode3 = dom.SelectNodes("//VM");
                        int nc = 0;
                        foreach (XmlNode node in xmlnode3)
                        {

                            //MessageBox.Show(node.Attributes.Item(0).Value);
                            TreeNode nod = new TreeNode();

                            string vmmoref = node.Attributes.Item(0).Value;
                            XmlNode xmlvmnode = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");

                            //treeView1.Nodes[0].Nodes.Add(vmmoref,xmlvmnode.InnerText, 4).EnsureVisible();

                            XmlNode xmlvmON = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/GuestState");
                            if (xmlvmON != null)
                            {
                                if (xmlvmON.InnerText == "toolsOk")
                                {
                                    treeView1.Nodes[0].Nodes.Add(vmmoref, xmlvmnode.InnerText, 6).EnsureVisible();
                                }
                                else
                                {
                                    treeView1.Nodes[0].Nodes.Add(vmmoref, xmlvmnode.InnerText, 4).EnsureVisible();
                                }
                            }
                            else
                            {
                                treeView1.Nodes[0].Nodes.Add(vmmoref, xmlvmnode.InnerText, 4).EnsureVisible();
                            }

                            nc++;
                        }
                        treeView1.HideSelection = false;
                        treeView1.SelectedNode = treeView1.Nodes[0];
                        //treeView2.Focus();
                        treeView1.SelectedNode.EnsureVisible();
                    }
                    catch
                    {
                        MessageBox.Show("Error processing Inventory", "Error popESX");
                    }
                }
            }
            //toolStripButton2.Enabled = true;
            //toolStripButton3.Enabled = true;
            //toolStripButton4.Enabled = true;
        }

        private void toolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void treeWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
           //toolStripButton2.Enabled = true;
            //toolStripButton3.Enabled = true;

        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            try
            {
                string moref = treeView1.SelectedNode.Name;
                string name = treeView1.SelectedNode.Text;
                WastecrawlRoot(moref, name);
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
                
            }
            catch
            {
                MessageBox.Show("Make sure you configure Scan Options and select an object");
                toolStripButton2.Enabled = false;
                toolStripButton3.Enabled = false;
                toolStripButton4.Enabled = false;
            }
            vfilename = "vDiskWaste.CSV";


        }

        private void WastecrawlRoot(string moref, string name)
        {
            treeView2.Visible = true;
            treeView2.ImageList = imageList2;
            treeView2.SelectedImageIndex = 9;
            treeView2.Nodes.Clear();

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load("Inventory.xml");
                XmlNodeList xmlroot = null;

                if (moref.Substring(0, 2) == "ro")
                {
                    xmlroot = dom.SelectNodes("//VM[@moref]");
                }

                if (moref.Substring(0, 2) == "da")
                {
                    xmlroot = dom.SelectNodes("//Datacenter[@moref='" + moref + "']/*/*/*/VM");
                }
                if (moref.Substring(0, 2) == "do")
                {
                    xmlroot = dom.SelectNodes("//Datacenter/*/Cluster[@moref='" + moref + "']/*/VM");
                }

                if (moref.Substring(0, 2) == "ho")
                {
                    xmlroot = dom.SelectNodes("//Host[@moref='" + moref + "']/VM");
                }

                if (moref.Substring(0, 2) == "vm")
                {
                    xmlroot = dom.SelectNodes("//VM[@moref='" + moref + "']");
                }

                int vc = 0;
                foreach (XmlNode node in xmlroot)
                {

                    int nc = 0;
                    string vmmoref = node.Attributes.Item(0).Value;

                    XmlNode xmlvmname = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");
                    if (xmlvmname != null)
                    {
                        //MessageBox.Show(xmlvmname.InnerText);
                        treeView2.Nodes.Add(vmmoref, xmlvmname.InnerText, 0);
                        XmlNode xmlFQDN = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/FQDN");
                        if (xmlFQDN != null)
                        {
                            treeView2.Nodes[vc].Nodes.Add("FQDN", "FQDN: " + xmlFQDN.InnerText, 6);
                            nc++;
                        }
                        XmlNode xmlIP = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/IPaddress");
                        if (xmlIP != null)
                        {
                            treeView2.Nodes[vc].Nodes.Add("IP", "IPAdress: " + xmlIP.InnerText, 7);
                            nc++;
                        }

                        XmlNodeList xmldisk = dom.SelectNodes("//VM[@moref='" + vmmoref + "']/Disk");

                        foreach (XmlNode disknode in xmldisk)
                        {
                            treeView2.Nodes[vc].Nodes.Add("disk", "DiskID: " + disknode.Attributes.Item(0).Value, 2);


                            Int64 diskcap = 0;
                            Int64 diskfree = 0;
                            string diskpath = "";
                            string diskcapSTR = "";
                            string diskfreeSTR = "";
                            diskpath = disknode.Attributes.Item(1).Value;
                            diskcapSTR = disknode.Attributes.Item(2).Value;
                            diskcap = Convert.ToInt64(diskcapSTR) / bytes;
                            diskfreeSTR = disknode.Attributes.Item(3).Value;
                            diskfree = Convert.ToInt64(diskfreeSTR) / bytes;
                            double perc = vThreshold;
                            int roundperc;

                            perc = diskcap - diskfree;
                            perc = perc / diskcap;
                            perc = perc * 100;
                            roundperc = (int)perc;
                            roundperc = 100 - roundperc;

                            treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskpath", "Disk Path: " + diskpath, 2);
                            treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskcap", "Disk Capacity: " + diskcap.ToString() + gb, 4);
                            if (roundperc >= vThreshold)
                            {
                                treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskfree", "Disk Free Space: " + diskfree.ToString() + gb + " : Needs Resize", 5).EnsureVisible();
                                treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskfree", "Pecentage of Free Space: " + roundperc, 8).EnsureVisible();
                                treeView2.Nodes[vc].ImageIndex = 1;
                                treeView2.Nodes[vc].Nodes[nc].ImageIndex = 3;
                            }
                            else
                            {
                                treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskfree", "Disk Free Space: " + diskfree.ToString() + gb, 4);
                                treeView2.Nodes[vc].Nodes[nc].Nodes.Add("diskfree", "Pecentage of Free Space: " + roundperc, 8);
                            }

                            nc++;
                        }

                        vc++;
                    }


                    treeView2.HideSelection = false;
                    treeView2.SelectedNode = treeView2.Nodes[0];
                    //treeView2.Focus();
                    treeView2.SelectedNode.EnsureVisible();


                    toolStripButton4.Enabled = true;
                }
            }
            catch
            {
                MessageBox.Show("Error processing Inventory file", "Error pop");
            }
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {
            treeView2.SelectedImageIndex = treeView2.SelectedImageIndex;
        }

        private void alignWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            double percentage;
            int finishcount;

            XmlDocument xmldoc = new XmlDocument();
            XmlNode xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            xmldoc.AppendChild(xmlnode);
            //add root element
            XmlElement xmlrootnew = xmldoc.CreateElement("", "root", "");
            xmldoc.AppendChild(xmlrootnew);

            ConnectionOptions connection = new ConnectionOptions();
            connection.Username = domuser;
            connection.Password = dompass;
            connection.Authority = "ntlmdomain:" + domdom;
            connection.Timeout = new TimeSpan(0, 0, 0, 5);

            //#################################################


            XmlNodeList xmlroot = null;

            XmlDocument dom = new XmlDocument();
            dom.Load("Inventory.xml");
            if (moref.Substring(0, 2) == "ro")
            {
                xmlroot = dom.SelectNodes("//VM[@moref]");
            }

            if (moref.Substring(0, 2) == "da")
            {
                xmlroot = dom.SelectNodes("//Datacenter[@moref='" + moref + "']/*/*/*/VM");
            }
            if (moref.Substring(0, 2) == "do")
            {
                xmlroot = dom.SelectNodes("//Datacenter/*/Cluster[@moref='" + moref + "']/*/VM");
            }

            if (moref.Substring(0, 2) == "ho")
            {
                xmlroot = dom.SelectNodes("//Host[@moref='" + moref + "']/VM");
            }

            if (moref.Substring(0, 2) == "vm")
            {
                xmlroot = dom.SelectNodes("//VM[@moref='" + moref + "']");
            }
            int vc = 0;
            finishcount = 0;
            foreach (XmlNode node in xmlroot)
            {

                int nc = 0;
                string vmmoref = node.Attributes.Item(0).Value;

                XmlNode xmlvmname = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");
                int vmcount = node.ChildNodes.Count;
                percentage = 100 / xmlroot.Count;
                

                if (xmlvmname != null)
                {

                    XmlElement xmlvm = xmldoc.CreateElement(null, "VM", null);
                    XmlAttribute newAtt = xmldoc.CreateAttribute("moref");
                    newAtt.Value = vmmoref;
                    xmlvm.Attributes.Append(newAtt);
                    xmldoc.ChildNodes.Item(1).AppendChild(xmlvm);




                    XmlNode xmlnode3 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                    XmlElement xmlvmname2 = xmldoc.CreateElement(null, "VMName", null);
                    xmlvmname2.InnerText = xmlvmname.InnerText;
                    xmlnode3.AppendChild(xmlvmname2);



                    XmlNode xmlFQDN = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/FQDN");
                    if (xmlFQDN != null)
                    {
                        XmlNode xmlnode4 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                        XmlElement xmlFQDN2 = xmldoc.CreateElement(null, "FQDN", null);
                        xmlFQDN2.InnerText = xmlFQDN.InnerText;
                        xmlnode3.AppendChild(xmlFQDN2);

                        nc++;
                    }
                    XmlNode xmlIP = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/IPaddress");
                    if (xmlIP != null)
                    {
                        XmlNode xmlnode4 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                        XmlElement xmlIP2 = xmldoc.CreateElement(null, "IP", null);
                        xmlIP2.InnerText = xmlIP.InnerText;
                        xmlnode3.AppendChild(xmlIP2);
                        nc++;
                    }
                    XmlElement xmltoolOK = (XmlElement)dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/GuestState");
                    if (xmltoolOK == null)
                    {
                        
                        nc++;
                    }

                    if (xmltoolOK != null)
                    {
                        if (xmltoolOK.InnerText == "toolsOk")
                        {
                            try
                            {
                                ManagementScope scope = null;
                                ObjectQuery query = null;
                                try
                                {
                                    scope = new ManagementScope(
                                                    "\\\\" + xmlFQDN.InnerText + "\\root\\CIMV2", connection);
                                    scope.Connect();
                                    query = new ObjectQuery(
                                                    "SELECT * FROM Win32_DiskPartition");
                                }
                                catch
                                {
                                    scope = new ManagementScope(
                                                        "\\\\" + xmlIP.InnerText + "\\root\\CIMV2", connection);
                                    scope.Connect();
                                    query = new ObjectQuery(
                                                    "SELECT * FROM Win32_DiskPartition");
                                }
                                ManagementObjectSearcher searcher =
                                    new ManagementObjectSearcher(scope, query);
                                foreach (ManagementObject queryObj in searcher.Get())
                                {
                                    string startoffset = queryObj["StartingOffset"].ToString();


                                   
                                    XmlNode xmlnode4 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                    XmlElement xmlDisk2 = xmldoc.CreateElement(null, "Disk", null);
                                    string diskID = queryObj["DeviceID"].ToString();
                                    newAtt = xmldoc.CreateAttribute("DiskID");
                                    newAtt.Value = diskID;
                                    xmlDisk2.Attributes.Append(newAtt);
                                    xmlnode4.AppendChild(xmlDisk2);
                                    newAtt = xmldoc.CreateAttribute("Offset");
                                    newAtt.Value = startoffset;
                                    xmlDisk2.Attributes.Append(newAtt);
                                    xmlnode4.AppendChild(xmlDisk2);





                                }
                            }
                            catch
                            {
                                
                                XmlNode xmlnode4 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                XmlElement xmlDisk2 = xmldoc.CreateElement(null, "Disk", null);
                                string diskID = "Unable to query WMI";
                                newAtt = xmldoc.CreateAttribute("DiskID");
                                newAtt.Value = diskID;
                                xmlDisk2.Attributes.Append(newAtt);
                                xmlnode4.AppendChild(xmlDisk2);
                                newAtt = xmldoc.CreateAttribute("Offset");
                                newAtt.Value = "Unable to query WMI";
                                xmlDisk2.Attributes.Append(newAtt);
                                xmlnode4.AppendChild(xmlDisk2);

                            }

                            
                        }
                        else
                        {
                           
                            nc++;
                        }
                    }

                    vc++;
                    
                }


                finishcount = finishcount + Convert.ToInt32(percentage);
                try
                {
                    alignWorker.ReportProgress(finishcount);
                }
                catch
                {
                }


            }
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineChars = Environment.NewLine + Environment.NewLine;
            XmlWriter xmlwrite = XmlWriter.Create("align.xml", xmlWriterSettings);
            xmldoc.Save(xmlwrite);
            xmlwrite.Close();
            


        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            progressBar1.Value = 1;
            toolStripButton2.Enabled = false;
            toolStripButton3.Enabled = false;
            toolStripButton4.Enabled = false;
            vfilename = "vDiskAlignment.CSV";
            try
            {
                moref = treeView1.SelectedNode.Name;
                name = treeView1.SelectedNode.Text;
                pictureBox1.Visible = true;
                this.progressBar1.Visible = true;
                treeView2.Nodes.Clear();
                alignWorker.RunWorkerAsync();
            }
            catch
            {
                MessageBox.Show("Make sure you configure Scan Options and select an object");
                pictureBox1.Visible = false;
                this.progressBar1.Visible = false;
                toolStripButton2.Enabled = true;
                toolStripButton3.Enabled = true;
                toolStripButton4.Enabled = true;
            }
        }

        private void alignWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                treeView2.Nodes.Clear();
                treeView2.Visible = true;
                pictureBox1.Visible = false;
                XmlNodeList xmlroot = null;

                treeView2.ImageList = imageList3;
                treeView2.SelectedImageIndex = 5;

                XmlDocument dom = new XmlDocument();
                dom.Load("align.xml");
                int vc = 0;
                string moref = "ro";
                if (moref.Substring(0, 2) == "ro")
                {
                    xmlroot = dom.SelectNodes("//VM[@moref]");
                }




                foreach (XmlNode node in xmlroot)
                {

                    int nc = 0;
                    string vmmoref = node.Attributes.Item(0).Value;

                    XmlNode xmlvmname = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/VMName");
                    if (xmlvmname != null)
                    {
                        //MessageBox.Show(xmlvmname.InnerText);
                        treeView2.Nodes.Add(vmmoref, xmlvmname.InnerText, 0);
                        XmlNode xmlFQDN = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/FQDN");
                        if (xmlFQDN != null)
                        {
                            treeView2.Nodes[vc].Nodes.Add("FQDN", "FQDN: " + xmlFQDN.InnerText, 1);
                            nc++;
                        }
                        XmlNode xmlIP = dom.SelectSingleNode("//VM[@moref='" + vmmoref + "']/IP");
                        if (xmlIP != null)
                        {
                            treeView2.Nodes[vc].Nodes.Add("IP", "IPAdress: " + xmlIP.InnerText, 2);
                            nc++;
                        }

                        XmlNodeList xmldisk = dom.SelectNodes("//VM[@moref='" + vmmoref + "']/Disk");

                        foreach (XmlNode disknode in xmldisk)
                        {
                            //treeView2.Nodes[vc].Nodes.Add("disk", "DiskID: " + disknode.Attributes.Item(0).Value + ": Offset = " + disknode.Attributes.Item(1).Value, 2);
                            if (disknode.Attributes.Item(0).Value != "Unable to query WMI")
                            {
                                string startoffset = disknode.Attributes.Item(1).Value;
                                decimal stoff = decimal.Parse(startoffset);
                                stoff = stoff / 1024;
                                if (stoff <= (vOffset - 1))
                                {
                                    treeView2.Nodes[vc].Nodes.Add("disk", disknode.Attributes.Item(0).Value + ": Offset = " + stoff.ToString() + "KB - Check Alignment", 4).EnsureVisible();

                                    //treeView2.Nodes[vc].Nodes[nc].ImageIndex = 3;
                                    //treeView2.Nodes[vc].ImageIndex = 1;
                                    //treeView2.Nodes[vc].Nodes[nc].Nodes.Add("offset", "Starting Offset = " + stoff + "KB - Check Alignment", 3).EnsureVisible();
                                    nc++;
                                }
                                else
                                {
                                    stoff = stoff / 4;


                                    if (stoff % 2 == 0)
                                    {
                                        //even

                                        //treeView2.Nodes[vc].Nodes[nc].Nodes.Add("offset", "Starting Offset = " + stoff + "KB - Alignment is Good", 2);
                                        treeView2.Nodes[vc].Nodes.Add("disk", disknode.Attributes.Item(0).Value + ": Offset = " + stoff.ToString() + "KB - Alignment is good", 3);
                                        nc++;


                                    }
                                    else
                                    {
                                        //ODD
                                        //treeView2.Nodes[vc].Nodes[nc].ImageIndex = 3;
                                        //treeView2.Nodes[vc].ImageIndex = 1;
                                        //treeView2.Nodes[vc].Nodes[nc].Nodes.Add("offset", "Starting Offset = " + stoff + "KB - Check Alignment", 3).EnsureVisible();
                                        treeView2.Nodes[vc].Nodes.Add("disk", disknode.Attributes.Item(0).Value + ": Offset = " + stoff.ToString() + "KB - Check Alignment", 4).EnsureVisible();
                                        nc++;

                                    }
                                }
                            }
                            else
                            {
                                treeView2.Nodes[vc].Nodes.Add("disk", "Unable to query WMI", 4).EnsureVisible();
                            }


                            nc++;

                        }


                            vc++;
                        }


                        treeView2.HideSelection = false;
                        treeView2.SelectedNode = treeView2.Nodes[0];
                        //treeView2.Focus();
                        treeView2.SelectedNode.EnsureVisible();



                    }
                    toolStripButton4.Enabled = true;
                }
            

            catch
            {
                pictureBox1.Visible = false;
                this.progressBar1.Visible = false;
            }
            this.progressBar1.Visible = false;
            toolStripButton2.Enabled = true;
            toolStripButton3.Enabled = true;
            toolStripButton4.Enabled = true;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {

            if (vfilename == "vDiskWaste.CSV")
            {

                string csvData = "Name, FQDN, IP, Disk, Disk Path, Capacity, FreeSpace \n";
                string vmname;
                string FQDN;
                string IP;
                string diskID;
                string diskpath;
                string cap;
                string freedisk;


                foreach (TreeNode node in treeView2.Nodes)
                {
                    //csvData = csvData + node.Text + ",";
                    vmname = node.Text;
                    int i = 0;
                    foreach (TreeNode node2 in node.Nodes)
                    {
                        FQDN = node.Nodes[0].Text;
                        IP = node.Nodes[1].Text;
                        diskID = node.Nodes[2].Text;

                        string disk = node2.Text.Substring(0, 4);

                        if (disk == "Disk")
                        {

                            //csvData = csvData + node3.Text + ",";
                            diskpath = node2.Nodes[0].Text;
                            cap = node2.Nodes[1].Text;
                            freedisk = node2.Nodes[2].Text;

                            if (i != 0)
                            {
                                FQDN = " ";
                                IP = " ";
                            }

                            csvData = csvData + vmname + "," + FQDN + "," + IP + "," + node2.Text + "," + diskpath + "," + cap + "," + freedisk + "\n";
                            i++;

                        }
                    }


                }
                try
                {
                    string filename;
                    SaveFileDialog tempfile = new SaveFileDialog();
                    tempfile.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
                    tempfile.Title = "Export your vdisk waste results";
                    tempfile.FileName = "vDiskWaste.CSV";
                    tempfile.ShowDialog();
                    filename = tempfile.FileName;

                    using (StreamWriter writer = new StreamWriter(@filename))
                    {
                        writer.Write(csvData);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to write file");
                }
            }

            else
            {
                string csvData = "Name, FQDN, IP, Disk, Partition \n";
               
                
                foreach (TreeNode node in treeView2.Nodes)
                {
                    string FQDN = " ";
                    string IP = " ";
                    
                    string vname = node.Text;

                    //csvData = csvData + vname + ",";

                    try
                    {
                        FQDN = node.Nodes[0].Text;
                        IP = node.Nodes[1].Text;


                        foreach (TreeNode node2 in node.Nodes)
                        {
                            

                            string nodeValue = node2.Text;

                            if (nodeValue.Substring(0, 4) != "Disk")
                            {


                            }
                            else
                            {
                                csvData = csvData + vname + "," + FQDN + "," + IP + "," + nodeValue + "\n";
                                FQDN = " ";
                                IP = " ";
                            }
                        }
                    }
                    catch
                    {
                        csvData = csvData + vname + "\n";
                    }
                    

                    


                }
                try
                {
                    string filename;
                    SaveFileDialog tempfile = new SaveFileDialog();
                    tempfile.Filter = "CSV Files (*.csv)|*.csv|All files (*.*)|*.*";
                    tempfile.Title = "Export your vdisk waste results";
                    tempfile.FileName = "vDiskAlignment.CSV";
                    tempfile.ShowDialog();
                    filename = tempfile.FileName;

                    using (StreamWriter writer = new StreamWriter(@filename))
                    {
                        writer.Write(csvData);
                    }
                }
                catch
                {
                    MessageBox.Show("Unable to write file");
                }
            }

           
        }

        private void alignWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.progressBar1.Visible = true;
            this.progressBar1.Value = e.ProgressPercentage;
        }



    
}

    }

