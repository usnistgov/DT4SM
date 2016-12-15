using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace NIST.DMCLI
{
    public abstract class FileOption
    {
        [Option('f', "file", Required = true, HelpText = "Absolute path to the file to process.")]
        public string File { get; set; }

    }

    public class SignOption: FileOption
    {

        [Option('c', "cert", Required = true, HelpText = "Certificate friendly name.")]
        public string Certificate { get; set; }

        [Option("path", HelpText ="Path to the certificate.")]
        public string Path { get; set; }

        [Option("pwd", HelpText ="Password to access the certificate.")]
        public string Password { get; set; }
    }

    public class VerifyOption: FileOption 
    {

    }

    public class VerifyWithMetadata: FileOption
    {

    }
    public class SignWithMetadata: SignOption
    {
        [OptionArray("metadata", Required = true, HelpText ="Metadata to embed in the signature.")]
        public String[] Metadata { get; set; }
    }

    public class Options
    {
        [VerbOption("sign", HelpText = "Sign a file.")]
        public SignOption SignVerb { get; set; }

        [VerbOption("verify",HelpText ="Verify the signature(s) in a file.")]
        public VerifyOption VerifyVerb { get; set; }

        [VerbOption("signm", HelpText ="Sign a file and add metadata.")]
        public SignWithMetadata SignMVerb { get; set; }

        [VerbOption("verifym", HelpText = "Verify the signature(s) in a file that contains metadata.")]
        public VerifyWithMetadata VerifyMVerb { get; set; }
    }
}
