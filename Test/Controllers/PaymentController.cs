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
                    string textxml = "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";//HttpContext.Request.Form["data"];
                    string env_key = "RJ3e219Plfb29/XKmT8uvjdlJLltb6OLfJnedROxFcdUl2+e0JfKn+e3kZUuZSIsLjP6QURjS/p23p60iCn+BxZ0IMOLuKqFMFcz2HULiIIYXo9m08Vhk9rTus84yPMYC5XFg4BYocU1cjeARpuMy/Fc5CmZDuP3cTyciWWMfGc=";//HttpContext.Request.Form["env_key"];
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
