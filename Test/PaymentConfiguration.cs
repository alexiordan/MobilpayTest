using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Test
{
    public class PaymentConfiguration
    {
        public string MobilPayUrl { get; set; }
        public string Signature { get; set; }
        public string ConfirmUrl { get; set; }
        public string ReturnUrl { get; set; }
        public string PathToPrivateKey { get; set; }
        public string PathToCertificate { get; set; }

    }
}
