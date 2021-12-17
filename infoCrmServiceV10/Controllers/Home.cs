using infoCrmServiceV10.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http.Formatting;
using System.Text.Json;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML.Messaging;
using Twilio.Types;

namespace infoCrmServiceV10.Controllers
{
    //ngrok usage tips: [port] : is equal to sslPort
    //path: Properties => launchSettings.json => iisSettings => iisExpress => sslPort
    //ngrok http https://localhost:[port] -host-header="localhost:[port]"
    [Route("api/[controller]")]
    [ApiController]
    public class Home : ControllerBase
    {
        IConfiguration _iConfig;

        public Home(IConfiguration iConfig)
        {
            _iConfig = iConfig;
        }

        //send sms method use phone number (turkey exp: 905XXXXXXXXXX)
        //textsms:(must not be empty)  
        [HttpGet("sendsms")]
        public IActionResult sendSms(string telephoneNumber, string textSms)
        {
            if (!IsPhoneNumber(telephoneNumber))
            {
                if (String.IsNullOrEmpty(textSms))
                    return BadRequest(Messages.ErrorEmptyText);

                return BadRequest(Messages.ErrorValidTelephoneNumber);
            }
            else
            {
                //accountSid from twilio
                string accountSid = _iConfig.GetSection("Twilio").GetSection("accountSid").Value;
                //authToken from twilio
                string authToken = _iConfig.GetSection("Twilio").GetSection("authToken").Value;
                //twilio phone number taken by twilio
                string twilionumber = _iConfig.GetSection("Twilio").GetSection("phoneNumber").Value;
                //initialize client twilio
                TwilioClient.Init(accountSid, authToken);
                var messageOptions = new CreateMessageOptions(
                   new PhoneNumber("whatsapp:+" + telephoneNumber));
                messageOptions.From = new PhoneNumber("whatsapp:" + twilionumber);              
                messageOptions.Body = textSms;
                //send sms
                var message = MessageResource.Create(messageOptions);
                //save message
                if (message != null && message.Sid != null)
                {
                    //check if directory exist if not create
                    string Path =@"C:\message\Send\";
                    if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string jsonString = JsonSerializer.Serialize(message, options);
                    string filePath = Path + message.Sid + ".json";

                    FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                    StreamWriter sw = new StreamWriter(fs);

                    sw.WriteLine(jsonString);
                    sw.Flush();

                    sw.Close();
                    fs.Close();
                }
               
                return Ok(message);
            }
        }

        //filter get messages
        //to(which number you send format sample for turkey:905XXXXXXXXXX)
        //afterTime : 22-10-2021,beforeTime : 23-10-2021
        //limit must be greater than 0 or empty
        [HttpGet("getsms")]
        public IActionResult getSms(string? to, string? afterTime, string? beforeTime,int limit)
        {
            DateTime after;
            DateTime before;
 
            string accountSid = _iConfig.GetSection("Twilio").GetSection("accountSid").Value;
            string authToken = _iConfig.GetSection("Twilio").GetSection("authToken").Value;
            TwilioClient.Init(accountSid, authToken);
            var messages = MessageResource.Read(
                dateSentBefore: DateTime.TryParse(beforeTime, out before) ? before : null,
                dateSentAfter: DateTime.TryParse(afterTime, out after) ? after : null,
                to: IsPhoneNumber(to) ? "whatsapp:+" + to :null,
                limit: (limit==0) ? 100 : limit
            );

            return Ok(messages);
        }

        //check phone number is empty or valid
        public static bool IsPhoneNumber(string number)
        {
            if (String.IsNullOrEmpty(number))
                return false;

            return Regex.IsMatch(number, @"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}") || Regex.IsMatch(number, @"^[0-9]+$");
        }

        //Get Sended Whatsapp messages to twilio number(+14155238886) and Save C:\message path
        [HttpPost("Send")]
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult Send([FromForm] ResponseMessage responseMessage)
        {
            if(responseMessage != null)
            {
                //check response body
                if (responseMessage.Body == null) responseMessage.Body = "";

                //check if directory exist if not create
                string Path = @"C:\message\Receive\";
                if (!Directory.Exists(Path)) Directory.CreateDirectory(Path);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonString = JsonSerializer.Serialize(responseMessage, options);
                string filePath = Path + responseMessage.SmsMessageSid + ".json";

                FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(jsonString);
                sw.Flush();

                sw.Close();
                fs.Close();

                var testContent = $@"<Response><Message>your number:{responseMessage.From} and your message: '{responseMessage.Body}'</Message></Response>";
                return Content(testContent, "text/xml");
            }

            return BadRequest(Messages.ErrorReceivedMessage);
        }

        //other usage comsume request params
        //[HttpPost("Post")]
        //[Consumes("application/x-www-form-urlencoded")]
        //public IActionResult Post([FromForm] IFormCollection value)
        //{
        //}
    }
}
