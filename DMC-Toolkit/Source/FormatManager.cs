using System;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using System.Collections.Generic;

namespace NIST.DMC
{
    public abstract class FormatManager
    {

        /// <summary>
        /// Returns an object containing the signature for a file
        /// </summary>
        /// <param name="certificate">Certificate to use to generate the signature</param>
        /// <param name="filePath">Path to the file from which we intend to generate a signature for</param>
        /// <returns></returns>
        public abstract object EncodeCMS(X509Certificate2 certificate, string filePath);

        /// <summary>
        /// Signs a file by attaching it a digital signature
        /// </summary>
        /// <param name="filePath">Path to the file on which the method needs to attach a signature</param>
        /// <param name="digitalSignature">Digital signature attached by the method</param>
        public abstract void SignFile(string filePath, object digitalSignature);

        /// <summary>
        /// Generates a digital signature and attaches it to a file
        /// </summary>
        /// <param name="certificate">Certificate to use to generate the signature</param>
        /// <param name="filePath">Path to the file on which the signature will be attached</param>
        public abstract void EncodeAndSign(X509Certificate2 certificate, String filePath);

        /// <summary>
        /// Returns true if the signature attached to the file is valid
        /// </summary>
        /// <param name="filePath">Path the file from which the method verifies the signature</param>
        /// <returns></returns>
        public abstract Boolean VerifyFile(string filePath, ref List<KeyValuePair<X509Certificate2, bool>> verifiedCMS);

    }
}
