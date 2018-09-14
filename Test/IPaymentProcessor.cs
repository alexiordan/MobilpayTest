using MobilpayEncryptDecrypt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Test.Payment;

namespace Test
{
    public interface IPaymentProcessor
    {
        Task RequestInvoicePaymentAsync(int invoiceId);
        MobilpayEncrypt CreatePaymentForNetopia(int invoiceId);
        PaymentResult ConfirmPayment(string textxml, string env_key);
    }
}
