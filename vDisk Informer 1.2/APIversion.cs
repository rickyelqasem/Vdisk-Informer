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
    class APIversion
    {
        protected VimService _service;
        protected ServiceContent _sic;
        protected ManagedObjectReference _svcRef;
        protected ManagedObjectReference _propCol;
        protected ManagedObjectReference _rootFolder;
        string folder;
        string name;
        string moref;


        public object Collect(string url, string username, string password)
        {

            if (_service != null)
            {
                Disconnect();
            }

            

            

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

            string apiversion = _sic.about.apiType;

            return _sic;

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
