using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MobilpayEncryptDecrypt;

namespace Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IHostingEnvironment m_HostingEnvironment;
        private readonly PaymentConfiguration m_PaymentConfiguration;
        private readonly IPaymentProcessor m_PaymentProcessor;
        private readonly ILogger<PaymentController> m_Logger;


        public PaymentController(IHostingEnvironment hostingEnvironment, IOptions<PaymentConfiguration> paymentConfiguration, IPaymentProcessor paymentProcessor, ILogger<PaymentController> logger)
        {
            m_HostingEnvironment = hostingEnvironment;
            m_PaymentConfiguration = paymentConfiguration.Value;
            m_PaymentProcessor = paymentProcessor;
            m_Logger = logger;
        }

       
        public async Task Get()
        {
            await m_PaymentProcessor.RequestInvoicePaymentAsync(0);
        }

        [HttpGet("return")]
        public async Task<IActionResult> Return(int orderId)
        {
            m_Logger.LogDebug("Return url called");
            return Ok("Mulțumim");

        }

        // POST api/values
        [HttpPost("confirmation")]
        public async Task Post([FromForm] string value)
        {
            m_Logger.LogDebug("Confirmation url called");
            var keypath = Path.Combine(m_HostingEnvironment.WebRootPath, m_PaymentConfiguration.PathToPrivateKey);
            string errorCode = "0";
            string errorMessage = "";
            string errorType = "";
            string message;

            

            //if (((HttpContext.Request.Form.ContainsKey("data") == false || HttpContext.Request.Form["data"] == "")) & (((HttpContext.Request.Form.ContainsKey("env_key") == false || HttpContext.Request.Form["env_key"] == ""))))
            //{

            //    errorType = "0x02";
            //    errorCode = "0x300000f5";
            //    errorMessage = "mobilpay.ro posted invalid parameters";
            //}
            //else
            {
                try
                {
                    string textxml = "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";
                    string env_key = "NYbWLy3GrnTxJ/9qaBPQz4/Z2k++aqK8fzKf3KA2aI1BIO8PbQ3AlNQj9/aYMBABcd2sWAiqsTO7ZYPsaC7F8RcUUgMFHA6weclAwamI7zYDMK20DwAA/IPrHR9EcBUPRhT+KLEEdMz6WWY4dXCYV7UV3pxzcKwOlYP9bejpaIQ=";
                    m_Logger.LogDebug($"textxml: {textxml}");
                    m_Logger.LogDebug($"env_key: {env_key}");
                    var result = m_PaymentProcessor.ConfirmPayment(textxml, env_key);

                    errorType = result.ErrorType;
                    errorCode = result.ErrorCode;
                    errorMessage = result.ErrorMessage;

                }
                catch (Exception ex)
                {
                    errorType = "0x01";
                    errorCode = "1032";
                    errorMessage = ex.Message;
                }
            }


            Response.ContentType = "text/xml";
            message = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n";
            if (errorCode == "0")
            {
                message = message + "<crc>" + errorMessage + "</crc>";
            }
            else
            {
                message = message + "<crc error_type=\"" + errorType + "\" error_code=\"" + errorCode + "\"> " + errorMessage + "</crc>";
            }

            m_Logger.LogDebug($"Confirmation return: {message}");

            await Response.WriteAsync(message);
        }

        
    }
}
