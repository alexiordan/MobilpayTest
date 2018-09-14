using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MobilpayEncryptDecrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;


namespace Test.Payment
{
    public class PaymentProcessor : IPaymentProcessor
    {
        //public string service = "service";
        public string yourtransactionId = "114";
        public string yourtransactiontype = "card";
        public decimal youramount = 10;
        public string yourcurrency = "RON";
        public string yourdetails = "details";


        private readonly PaymentConfiguration m_PaymentConfiguration;
        private readonly HttpClient m_HttpClient;
        private readonly IHostingEnvironment m_HostingEnvironment;
        private readonly ILogger<PaymentProcessor> m_Logger;

        private string m_UserToken;

        public PaymentProcessor(HttpClient client, IOptions<PaymentConfiguration> paymentConfiguration, IHostingEnvironment hostingEnvironment, ILogger<PaymentProcessor> logger)
        {
            m_PaymentConfiguration = paymentConfiguration.Value;
            m_HttpClient = client;
            m_HostingEnvironment = hostingEnvironment;
            m_Logger = logger;
        }


        public MobilpayEncrypt CreatePaymentForNetopia(int invoiceId)
        {
            MobilpayEncrypt encrypt = new MobilpayEncrypt();

            Mobilpay_Payment_Request_Card card = new Mobilpay_Payment_Request_Card();
            Mobilpay_Payment_Invoice invoice = new Mobilpay_Payment_Invoice();
            Mobilpay_Payment_Address billing = new Mobilpay_Payment_Address();
            Mobilpay_Payment_Address shipping = new Mobilpay_Payment_Address();
            Mobilpay_Payment_Invoice_Item itmm = new Mobilpay_Payment_Invoice_Item();
            Mobilpay_Payment_Invoice_Item itmm1 = new Mobilpay_Payment_Invoice_Item();
            Mobilpay_Payment_ItemCollection itmColl = new Mobilpay_Payment_ItemCollection();
            Mobilpay_Payment_Exchange_RateCollection exColl = new Mobilpay_Payment_Exchange_RateCollection();
            Mobilpay_Payment_Exchange_Rate ex = new Mobilpay_Payment_Exchange_Rate();
            Mobilpay_Payment_Request_Contact_Info ctinfo = new Mobilpay_Payment_Request_Contact_Info();
            Mobilpay_Payment_Confirm conf = new Mobilpay_Payment_Confirm();
            Mobilpay_Payment_Request_Url url = new Mobilpay_Payment_Request_Url();



            MobilpayEncryptDecrypt.MobilpayEncryptDecrypt encdecr = new MobilpayEncryptDecrypt.MobilpayEncryptDecrypt();
            card.OrderId = new Random().Next().ToString();
            card.Type = "card";
            card.Signature = m_PaymentConfiguration.Signature;
            url.ConfirmUrl = m_PaymentConfiguration.ConfirmUrl;
            url.ReturnUrl = m_PaymentConfiguration.ReturnUrl;
            //card.Service = service;
            card.Url = url;
            card.TimeStamp = DateTime.Now.ToString("yyyyMMddhhmmss");
            invoice.Amount = youramount;
            invoice.Currency = yourcurrency;
            
            invoice.Details = yourdetails;
            invoice.Items = new Mobilpay_Payment_ItemCollection();
            billing.FirstName = "ceva";
            billing.LastName = "ceva";
            billing.IdentityNumber = "ceva";
            billing.FiscalNumber = "ceva";
            billing.MobilPhone = "ceva";
            billing.Type = "person";
            billing.ZipCode = "ceva";
            billing.Iban = "ceva";
            billing.Address = "ceva";
            billing.Bank = "ceva";
            billing.City = "ceva";
            billing.Country = "ceva";
            billing.County = "ceva";
            billing.Email = "ceva@gmail.com";

            shipping.Sameasbilling = "1";


            ctinfo.Billing = billing;
            ctinfo.Shipping = shipping;


            invoice.ContactInfo = ctinfo;

 
            card.Invoice = invoice;
            if (String.IsNullOrWhiteSpace(m_UserToken) == false)
            {
                card.Invoice.TokenId = m_UserToken;
            }

            encrypt.Data = encdecr.GetXmlText(card);
            encrypt.X509CertificateFilePath = GetPathToCertificate();
            m_Logger.LogInformation($"X509CertificateFilePath= [{encrypt.X509CertificateFilePath}]");
            encdecr.Encrypt(encrypt);
            //Apelul urmator nu arunca exceptie
            //encdecr.EncryptWithCng(encrypt);
           

            return encrypt;
        }

        private string GetPathToCertificate()
        {
            var contentRootPath = m_HostingEnvironment.ContentRootPath.AddBackslash();
            var pathToCertificate = Path.GetFullPath(Path.Combine(contentRootPath, m_PaymentConfiguration.PathToCertificate));
            return pathToCertificate;
        }

        public async Task RequestInvoicePaymentAsync(int invoiceId)
        {
            MobilpayEncrypt encrypt = CreatePaymentForNetopia(invoiceId);            

            HttpContent content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("data", encrypt.EncryptedData),
                    new KeyValuePair<string, string>("env_key", encrypt.EnvelopeKey)
                });

            await m_HttpClient.PostAsync(m_PaymentConfiguration.MobilPayUrl, content);
        }

        public PaymentResult ConfirmPayment(string textxml, string env_key)
        {
            var contentRootPath = m_HostingEnvironment.ContentRootPath.AddBackslash();
            var keypath = Path.GetFullPath(Path.Combine(contentRootPath, m_PaymentConfiguration.PathToPrivateKey));

            PaymentResult result = new PaymentResult();
        
            MobilpayEncryptDecrypt.MobilpayEncryptDecrypt encdecrypt = new MobilpayEncryptDecrypt.MobilpayEncryptDecrypt();
            MobilpayDecrypt decrypt = new MobilpayDecrypt();
            decrypt.Data = textxml;
            decrypt.EnvelopeKey =  env_key;
            decrypt.PrivateKeyFilePath = keypath;
            encdecrypt.Decrypt(decrypt);
            Mobilpay_Payment_Request_Card card = new Mobilpay_Payment_Request_Card();
            card = encdecrypt.GetCard(decrypt.DecryptedData);

            var panMasked = card.Confirm.PanMasked;
            m_UserToken = card.Confirm.TokenId;
            m_Logger.LogInformation($"User token retrieved: {String.IsNullOrEmpty(card.Confirm.TokenId)}");
            var tokenExpirationDate = card.Confirm.TokenExpirationDate;

            m_Logger.LogInformation($"Rezultatul tranzactiei este: {card.Confirm.Action}");

            switch (card.Confirm.Action)
            {
                case "confirmed"://plata efectuata
                case "paid": //bani blocati 
                    {
                        decimal paidAmount = card.Confirm.Original_Amount;
                        result.ErrorMessage = card.Confirm.Crc;
                        if (card.Confirm.Action == "confirmed" && card.Confirm.Error.Code == "0")
                        {
                            //var invoice = m_Repository.All<Invoice>().Where(i => i.Id == nMessage.InvoiceId).First();
                            //if (!invoice.IsPaid)
                            //{
                            //    m_NetopiaSystem.RecordInvoicePayment(invoice.Id, paidAmount);
                            //}

                        }
                        break;
                    }
                default:
                    {
                        result.ErrorType = "0x02";
                        result.ErrorCode = "0x300000f6";
                        result.ErrorMessage = "mobilpay_refference_action paramaters is invalid";
                        break;
                    }
            }

            return result;
        }
    }
}

