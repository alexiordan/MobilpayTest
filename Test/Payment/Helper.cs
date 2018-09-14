using JavaScience;
using MobilpayEncryptDecrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Test.Payment
{
    public static class Helper
    {
        public static string AddBackslash(this string path)
        {
            // They're always one character but EndsWith is shorter than
            // array style access to last path character. Change this
            // if performance are a (measured) issue.
            string separator1 = Path.DirectorySeparatorChar.ToString();
            string separator2 = Path.AltDirectorySeparatorChar.ToString();

            // Trailing white spaces are always ignored but folders may have
            // leading spaces. It's unusual but it may happen. If it's an issue
            // then just replace TrimEnd() with Trim(). Tnx Paul Groke to point this out.
            path = path.TrimEnd();

            // Argument is always a directory name then if there is one
            // of allowed separators then I have nothing to do.
            if (path.EndsWith(separator1) || path.EndsWith(separator2))
                return path;

            // If there is the "alt" separator then I add a trailing one.
            // Note that URI format (file://drive:\path\filename.ext) is
            // not supported in most .NET I/O functions then we don't support it
            // here too. If you have to then simply revert this check:
            // if (path.Contains(separator1))
            //     return path + separator1;
            //
            // return path + separator2;
            if (path.Contains(separator2))
                return path + separator2;

            // If there is not an "alt" separator I add a "normal" one.
            // It means path may be with normal one or it has not any separator
            // (for example if it's just a directory name). In this case I
            // default to normal as users expect.
            return path + separator1;
        }

        /// <summary>
        /// Inlocuitor pentru Encrypt cu RSACryptoServiceProvider 
        /// </summary>
        /// <param name="dummy"></param>
        /// <param name="mobilpayEncrypt"></param>
        /// <returns></returns>
        public static int EncryptWithCng(this MobilpayEncryptDecrypt.MobilpayEncryptDecrypt dummy, MobilpayEncrypt mobilpayEncrypt)
        {
            try
            {
                byte[] bytes = Encoding.ASCII.GetBytes(mobilpayEncrypt.Data);
                Random random = new Random();
                byte[] array = new byte[8];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = (byte)random.Next(0, 255);
                }
                RC4(ref bytes, array);
                X509Certificate2 x509Certificate = new X509Certificate2(mobilpayEncrypt.X509CertificateFilePath);
                var rSACng = x509Certificate.GetRSAPublicKey();//.PublicKey.Key as RSACng;
                rSACng.ExportParameters(false);
                byte[] inArray = rSACng.Encrypt(array, RSAEncryptionPadding.Pkcs1);
                mobilpayEncrypt.EncryptedData = Convert.ToBase64String(bytes);
                mobilpayEncrypt.EnvelopeKey = Convert.ToBase64String(inArray);
            }
            catch (CryptographicException ex)
            {
                throw ex;
            }
            return 0;
        }


        /// <summary>
        /// Asta e un loc de exersat
        /// </summary>
        /// <param name="dummy"></param>
        /// <param name="mobilpayDecrypt"></param>
        /// <returns></returns>
        public static int DecryptWithCng(this MobilpayEncryptDecrypt.MobilpayEncryptDecrypt dummy, MobilpayDecrypt mobilpayDecrypt)
        {
            try
            {
                StreamReader streamReader = File.OpenText(mobilpayDecrypt.PrivateKeyFilePath);
                string sPrivKey = streamReader.ReadToEnd().Trim();
                streamReader.Close();

                //RSACng ceva = new RSACng( CngKey.Import(streamReader.rea);
                RSACryptoServiceProvider decodedPrivKeyInfo = opensslkey.GetDecodedPrivKeyInfo(sPrivKey);
                string envelopeKey = mobilpayDecrypt.EnvelopeKey;
                byte[] rgb = Convert.FromBase64String(envelopeKey);
                byte[] bytes = Convert.FromBase64String(mobilpayDecrypt.Data);
                try
                {
                    byte[] key = decodedPrivKeyInfo.Decrypt(rgb, false);
                    RC4(ref bytes, key);
                    mobilpayDecrypt.DecryptedData = Encoding.ASCII.GetString(bytes);
                }
                catch (CryptographicException ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex2)
            {
                throw ex2;
            }
            return 0;
        }

        // MobilpayEncryptDecrypt.MobilpayEncryptDecrypt
        private static void RC4(ref byte[] bytes, byte[] key)
        {
            byte[] array = new byte[256];
            byte[] array2 = new byte[256];
            int i;
            for (i = 0; i < 256; i++)
            {
                array[i] = (byte)i;
                array2[i] = key[i % key.GetLength(0)];
            }
            int num = 0;
            for (i = 0; i < 256; i++)
            {
                num = (num + array[i] + array2[i]) % 256;
                byte b = array[i];
                array[i] = array[num];
                array[num] = b;
            }
            i = (num = 0);
            for (int j = 0; j < bytes.GetLength(0); j++)
            {
                i = (i + 1) % 256;
                num = (num + array[i]) % 256;
                byte b = array[i];
                array[i] = array[num];
                array[num] = b;
                int num2 = (array[i] + array[num]) % 256;
                bytes[j] ^= array[num2];
            }
        }
    }
}
