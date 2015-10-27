using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace NIST.DMC
{
    public abstract class Binary4PLOT : FormatManager
    {
        public override void EncodeAndSign(X509Certificate2 x, string filePath)
        {
            throw new NotImplementedException();
        }

        public override object EncodeCMS(X509Certificate2 x, string filePath)
        {
            throw new NotImplementedException();
        }

        public override void SignFile(string filePath, object digitalSignature)
        {
            throw new NotImplementedException();
        }

        public override bool VerifyFile(string filePath, ref List<KeyValuePair<X509Certificate2, bool>> verifiedCMS)
        {
            throw new NotImplementedException();
        }
    }
}
