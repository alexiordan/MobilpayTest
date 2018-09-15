using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.OpenSsl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    public class PrivateKeyExtractor
    {
        public string GetPrivateKey(string pathToPrivateKey)
        {
            //var bytesToDecrypt = Convert.FromBase64String(base64String); // string to decrypt, base64 encoded

            AsymmetricCipherKeyPair keyPair = null;

            using (var reader = File.OpenText(pathToPrivateKey))
            {// file containing RSA PKCS1 private key
                var ceva1 = new PemReader(reader).ReadObject();
            }

            var ceva = keyPair.Private;
            return ceva.ToString();

            var decryptEngine = new Pkcs1Encoding(new RsaEngine());
           // decryptEngine.Init(false, keyPair.Private);

            //var decrypted = Encoding.UTF8.GetString(decryptEngine.ProcessBlock(bytesToDecrypt, 0, bytesToDecrypt.Length));

            //return decrypted;
        }
    }
}
