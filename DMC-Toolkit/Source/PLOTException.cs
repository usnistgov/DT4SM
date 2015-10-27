using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NIST.DMC
{
    public class NoSignatureFoundException:Exception
    {
        public NoSignatureFoundException()
        {

        }

        public NoSignatureFoundException(string filePath):base(String.Format("Could not find signatures in {0}", filePath))
        {

        }
    }

    public class InvalidSignatureFoundExcetpion : Exception
    {
        public InvalidSignatureFoundExcetpion()
        {

        }

        public InvalidSignatureFoundExcetpion(string filePath): base(String.Format("Invalid signature found in {0}", filePath))
        {

        }
    }

    public class InvalidSignerInformationException : Exception
    {
        public InvalidSignerInformationException()
        {

        }

        public InvalidSignerInformationException(string signature) : base(String.Format("Invalid signer information in", signature))
        {

        }
    }
}
