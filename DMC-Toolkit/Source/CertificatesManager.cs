using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NIST.DMC
{

    /// <summary>
    /// CertificatesManager provides access to the user's local store of certificates
    /// </summary>
    public class CertificatesManager
    {

        
        /// <summary>
        /// 
        /// </summary>
        private static X509Store LocalStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

        /// <summary>
        /// Retrieves the user's local certificates
        /// </summary>
        /// <returns>List of X509 certificates</returns>
        public static List<X509Certificate2> GetCertificates()
        {
            List < X509Certificate2 > ListOfCertificates = new List<X509Certificate2>();

            //Open the user's local store
            LocalStore.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 cert in LocalStore.Certificates)
            {
                ListOfCertificates.Add(cert);
            }

            LocalStore.Close();

            return ListOfCertificates;
        }


        /// <summary>
        /// Finds a certificate, in the user's local store, by its friendly name
        /// </summary>
        /// <param name="friendlyName"> Friendly name of the certificates</param>
        /// <returns>Certificate or null if not found</returns>
        public static X509Certificate2 FindByFriendlyName(string friendlyName)
        {
            return CertificatesManager.GetCertificates().Find(item => item.FriendlyName.Equals(friendlyName));
        }

        /// <summary>
        /// Adds a certificate in the user's local store
        /// </summary>
        /// <param name="filePath">Path to the file containing the certificate to add</param>
        public static void AddCertificate(string filePath)
        {
            X509Certificate2 x = new X509Certificate2(filePath);
            LocalStore.Open(OpenFlags.ReadWrite);
            LocalStore.Add(x);
            LocalStore.Close();
        }

        /// <summary>
        /// Adds a certificate in the user's local store
        /// </summary>
        /// <param name="certificate">Certificate to add to the store</param>
        public static void AddCertificate(X509Certificate2 certificate)
        {
            LocalStore.Open(OpenFlags.ReadWrite);
            LocalStore.Add(certificate);
            LocalStore.Close();
        }


    }
}
