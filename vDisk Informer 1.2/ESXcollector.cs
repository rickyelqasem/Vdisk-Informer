using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Net;
using VimyyApi;
using System.Xml;

namespace vDisk_Informer_1._2
{
    class ESXcollector
    {
        protected VimService _service;
        protected ServiceContent _sic;
        protected ManagedObjectReference _svcRef;
        protected ManagedObjectReference _propCol;
        protected ManagedObjectReference _rootFolder;

        public object Collect(string url, string username, string password, string EntityType, string ESXname)
        {

            if (_service != null)
            {
                Disconnect();
            }

            ArrayList ObjectList = new ArrayList();

            //###################setup xml file################

            XmlDocument xmldoc = new XmlDocument();
            XmlNode xmlnode = xmldoc.CreateNode(XmlNodeType.XmlDeclaration, "", "");
            xmldoc.AppendChild(xmlnode);
            //add root element
            XmlElement xmlroot = xmldoc.CreateElement("", ESXname, "");
            xmldoc.AppendChild(xmlroot);

            //###################################################

            _service = new VimService();
            _service.Url = url;
            _svcRef = new ManagedObjectReference();
            _svcRef.type = "ServiceInstance";
            _svcRef.Value = "ServiceInstance";
            _service.CookieContainer = new System.Net.CookieContainer();
            _sic = _service.RetrieveServiceContent(_svcRef);
            _propCol = _sic.propertyCollector;
            _rootFolder = _sic.rootFolder;

            if ((_sic.sessionManager != null))
            {
                _service.Login(_sic.sessionManager, username, password, null);
            }

            TraversalSpec rpToRp = new TraversalSpec();
            rpToRp.type = "ResourcePool";
            rpToRp.path = "resourcePool";
            rpToRp.skip = false;
            rpToRp.name = "rpToRp";
            rpToRp.selectSet = new SelectionSpec[] { new SelectionSpec(), new SelectionSpec() };
            rpToRp.selectSet[0].name = "rpToRp";
            rpToRp.selectSet[1].name = "rpToVm";

            TraversalSpec rpToVm = new TraversalSpec();
            rpToVm.type = "ResourcePool";
            rpToVm.path = "vm";
            rpToVm.skip = false;
            rpToVm.name = "rpToVm";
            rpToVm.selectSet = new SelectionSpec[] { };

            TraversalSpec crToRp = new TraversalSpec();
            crToRp.type = "ComputeResource";
            crToRp.path = "resourcePool";
            crToRp.skip = false;
            crToRp.name = "crToRp";
            crToRp.selectSet = new SelectionSpec[] { rpToRp, new SelectionSpec() };
            crToRp.selectSet[1].name = "rpToVm";

            TraversalSpec crToH = new TraversalSpec();
            crToH.type = "ComputeResource";
            crToH.path = "host";
            crToH.skip = false;
            crToH.name = "crToH";
            crToH.selectSet = new SelectionSpec[] { };

            TraversalSpec dcToHf = new TraversalSpec();
            dcToHf.type = "Datacenter";
            dcToHf.path = "hostFolder";
            dcToHf.skip = false;
            dcToHf.name = "dcToHf";
            dcToHf.selectSet = new SelectionSpec[] { new SelectionSpec() };
            dcToHf.selectSet[0].name = "visitFolders";

            TraversalSpec dcToVmf = new TraversalSpec();
            dcToVmf.type = "Datacenter";
            dcToVmf.path = "vmFolder";
            dcToVmf.skip = false;
            dcToVmf.name = "dcToVmf";
            dcToVmf.selectSet = new SelectionSpec[] { new SelectionSpec() };
            dcToVmf.selectSet[0].name = "visitFolders";

            TraversalSpec HToVm = new TraversalSpec();
            HToVm.type = "HostSystem";
            HToVm.path = "vm";
            HToVm.skip = false;
            HToVm.name = "HToVm";
            HToVm.selectSet = new SelectionSpec[] { new SelectionSpec() };
            HToVm.selectSet[0].name = "visitFolders";

            TraversalSpec visitFolders = new TraversalSpec();
            visitFolders.type = "Folder";
            visitFolders.path = "childEntity";
            visitFolders.skip = false;
            visitFolders.name = "visitFolders";
            visitFolders.selectSet = new SelectionSpec[] { new SelectionSpec(), dcToHf, crToH, crToRp, rpToVm };
            visitFolders.selectSet[0].name = "visitFolders";

            TraversalSpec tSpec = default(TraversalSpec);
            tSpec = visitFolders;
            PropertySpec[] propSpecArray = null;
            propSpecArray = new PropertySpec[] { new PropertySpec() };
            propSpecArray[0].type = EntityType;
            propSpecArray[0].all = true;
            propSpecArray[0].allSpecified = true;

            PropertyFilterSpec spec = new PropertyFilterSpec();
            spec.propSet = propSpecArray;
            spec.objectSet = new ObjectSpec[] { new ObjectSpec() };
            spec.objectSet[0].obj = _sic.rootFolder;
            spec.objectSet[0].skip = false;
            spec.objectSet[0].selectSet = new SelectionSpec[] { tSpec };

            ObjectContent[] ocary = _service.RetrieveProperties(_propCol, new PropertyFilterSpec[] { spec });

            if (ocary != null)
            {

                ObjectContent oc = null;
                ManagedObjectReference mor = null;
                DynamicProperty[] pcary = null;
                DynamicProperty pc = null;

                for (Int32 oci = 0; oci <= ocary.Length - 1; oci++)
                {
                    oc = ocary[oci];
                    mor = oc.obj;
                    string vmmoref = mor.Value;
                    vmmoref = "vm-" + vmmoref;
                    pcary = oc.propSet;
                    //string vmname ="";
                    for (int propi = 0; propi <= pcary.Length - 1; propi++)
                    {
                        pc = pcary[propi];


                        if ((pc.name.Equals("guest")))
                        {
                            XmlElement xmlvm = xmldoc.CreateElement(null, "VM", null);
                            //XmlNode xmlnode3 = xmldoc.SelectSingleNode("//[@moref='" + moref + "']");
                            XmlAttribute newAtt = xmldoc.CreateAttribute("moref");
                            newAtt.Value = vmmoref;
                            xmlvm.Attributes.Append(newAtt);
                            //xmldoc.AppendChild(xmlvm);
                            xmldoc.ChildNodes.Item(1).AppendChild(xmlvm);
                            

                            if (((VimyyApi.GuestInfo)(pc.val)).disk != null)
                            {
                                

                                Int32 diskcount = ((VimyyApi.GuestInfo)(pc.val)).disk.Length;
                                XmlElement xmlvdisk = xmldoc.CreateElement(null, "Vdisk", null);
                                XmlNode xmlnode3 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                newAtt = xmldoc.CreateAttribute("diskcount");
                                newAtt.Value = diskcount.ToString();
                                xmlvdisk.Attributes.Append(newAtt);
                                xmlnode3.AppendChild(xmlvdisk);

                                for (Int32 guestcount = 0; guestcount <= diskcount - 1; guestcount++)
                                {

                                    long capacity = (((VimyyApi.GuestInfo)(pc.val)).disk[guestcount].capacity);
                                    //capacity = capacity / bytes;
                                    long freespace = (((VimyyApi.GuestInfo)(pc.val)).disk[guestcount].freeSpace);
                                    //freespace = freespace / bytes;
                                    string diskpath = (((VimyyApi.GuestInfo)(pc.val)).disk[guestcount].diskPath);

                                    XmlElement xmldisk = xmldoc.CreateElement(null, "Disk", null);
                                    xmlnode3 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                    newAtt = xmldoc.CreateAttribute("ID");
                                    newAtt.Value = guestcount.ToString();
                                    xmldisk.Attributes.Append(newAtt);
                                    newAtt = xmldoc.CreateAttribute("Path");
                                    newAtt.Value = diskpath;
                                    xmldisk.Attributes.Append(newAtt);
                                    newAtt = xmldoc.CreateAttribute("Capacity");
                                    newAtt.Value = capacity.ToString();
                                    xmldisk.Attributes.Append(newAtt);
                                    newAtt = xmldoc.CreateAttribute("Freespace");
                                    newAtt.Value = freespace.ToString();
                                    xmldisk.Attributes.Append(newAtt);


                                    xmlnode3.AppendChild(xmldisk);



                                }
                                XmlNode xmlnode4 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                XmlElement xmlguestState = xmldoc.CreateElement(null, "GuestState", null);
                                xmlguestState.InnerText = ((VimyyApi.GuestInfo)(pc.val)).toolsStatus.ToString();
                                xmlnode4.AppendChild(xmlguestState);

                                XmlNode xmlnode5 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                XmlElement xmlguestFamily = xmldoc.CreateElement(null, "GuestFamily", null);
                                xmlguestFamily.InnerText = ((VimyyApi.GuestInfo)(pc.val)).guestFamily;
                                xmlnode5.AppendChild(xmlguestFamily);

                                XmlNode xmlnode6 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                XmlElement xmlhostname = xmldoc.CreateElement(null, "FQDN", null);
                                xmlhostname.InnerText = ((VimyyApi.GuestInfo)(pc.val)).hostName;
                                xmlnode6.AppendChild(xmlhostname);

                                XmlNode xmlnode7 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                                XmlElement xmlIP = xmldoc.CreateElement(null, "IPaddress", null);
                                xmlIP.InnerText = ((VimyyApi.GuestInfo)(pc.val)).ipAddress;
                                xmlnode7.AppendChild(xmlIP);

                            }
                        }
                        if ((pc.name.Equals("name")))
                        {
                            string name = (pc.val.ToString());

                            XmlNode xmlnode3 = xmldoc.SelectSingleNode("//VM[@moref='" + vmmoref + "']");
                            XmlElement xmlvmname = xmldoc.CreateElement(null, "VMName", null);
                            xmlvmname.InnerText = name;
                            xmlnode3.AppendChild(xmlvmname);

                        }
                    }
                }

            }
            else
            {
                //("No Managed Entities retrieved!"); 
            }
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            xmlWriterSettings.NewLineChars = Environment.NewLine + Environment.NewLine;
            XmlWriter xmlwrite = XmlWriter.Create("Inventory.xml", xmlWriterSettings);
            xmldoc.Save(xmlwrite);
            xmlwrite.Close();

                       

            return ObjectList;

        }
        public void Disconnect()
        {

            if (_service != null)
            {
                _service.Logout(_sic.sessionManager);
                _service.Dispose();
                _service = null;

                _sic = null;
            }
        }
        public class CertPolicy : System.Net.ICertificatePolicy
        {


            public bool CheckValidationResult(System.Net.ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, System.Net.WebRequest request, int certificateProblem)
            {


                return true;
            }

        }
        public object CreateServiceRef(string svcRefVal)
        {
            System.Net.ServicePointManager.CertificatePolicy = new CertPolicy();
            ManagedObjectReference _svcRef = new ManagedObjectReference();
            _svcRef.type = "ServiceInstance";

            _svcRef.Value = svcRefVal;
            return _svcRef;
        }
    }
}

