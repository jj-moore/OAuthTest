using JJM_ImageSystems.Data;
using JJM_ImageSystems.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Serialization;

namespace JJM_ImageSystems.Controllers
{
    public class School
    {
        public int id { get; set; }
        public string name { get; set; }
    }
    public class PsUser
    {
        public int Dcid { get; set; }
        public string FirstName { get; set; }
        public string UserName { get; set; }
    }
    public class RegistrationCredentials
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
    }

    public class RegistrationRequest
    {
        public RegistrationCredentials credentials { get; set; }
        public string verify_url { get; set; }
    }

    public class Resource
    {
        public string time { get; set; }
        public string timestamp { get; set; }
    }

    public class RegistrationResponse
    {
        public int Callback_result { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }
    public class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string expires_in { get; set; }
    }

    public class RegistrationController : Controller
    {
        private readonly IHostingEnvironment _environment;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApplicationDbContext _context;

        public RegistrationController(IHostingEnvironment environment, IHttpClientFactory clientFactory, ApplicationDbContext context)
        {
            _environment = environment;
            _clientFactory = clientFactory;
            _context = context;
        }

        public async Task<IActionResult> Index([FromBody]RegistrationRequest request)
        {
            // LOG INCOMING REQUEST
            var json = new JavaScriptSerializer().Serialize(request);
            CreateFile("Request.txt", json);

            // PERSIST CREDENTIALS
            var powerSchoolToAdd = new PowerSchool
            {
                ClientId = request.credentials.client_id,
                ClientSecret = request.credentials.client_secret
            };
            _context.PowerSchool.Add(powerSchoolToAdd);
            await _context.SaveChangesAsync();

            // GET TIME FROM POWERSCHOOL AS A TEST
            var client = _clientFactory.CreateClient();
            HttpResponseMessage timeResponse = await client.GetAsync("https://DevPowerSchool.ceoimage.com/ws/v1/time");
            if (timeResponse.IsSuccessStatusCode)
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Resource), new XmlRootAttribute("resource"));
                using (Stream stream = timeResponse.Content.ReadAsStreamAsync().Result)
                {
                    Resource resource = (Resource)serializer.Deserialize(stream);
                    string timeJson = new JavaScriptSerializer().Serialize(resource);
                    CreateFile("Time.txt", timeJson);
                }
            }
            else
            {
                CreateFile("Time.txt", timeResponse.StatusCode.ToString());
            }

            // GENERATE RESPONSE TO POWERSCHOOL
            var response = new RegistrationResponse
            {
                Callback_result = 200,
                Message = "myMessage",
                Time = DateTime.UtcNow
            };

            return Json(response);
        }

        public async Task<IActionResult> GetToken()
        {
            // GET CLIENT ID AND SECRET FROM DATABASE AND WRITE TO FILE
            var credentials = _context.PowerSchool.LastOrDefault();
            if (credentials == null)
            {
                return Ok();
            }
            CreateFile("ClientId.txt", credentials.ClientId);

            // ENCODE CLIENT ID AND SECRET AND WRITE TO FILE
            string myBase64 = EncodeTo64(credentials.ClientId + ":" + credentials.ClientSecret);
            CreateFile("Base64.txt", myBase64);

            // CREATE POST REQUEST FOR ACCESS TOKEN AND WRITE TO FILE
            Uri uri = new Uri("https://devpowerschool.ceoimage.com/oauth/access_token/");
            List<KeyValuePair<string, string>> content = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };
            var tokenRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new FormUrlEncodedContent(content)
            };
            tokenRequest.Headers.Add("Authorization", "Basic " + myBase64);
            CreateFile("TokenRequest.txt", tokenRequest.ToString());

            // SEND POST REQUEST AND WRITE RESPONSE TO FILE
            var client = _clientFactory.CreateClient();
            HttpResponseMessage tokenResponse = await client.SendAsync(tokenRequest);
            if (tokenResponse.IsSuccessStatusCode)
            {
                string response = await tokenResponse.Content.ReadAsStringAsync();
                CreateFile("TokenResponse.txt", response);
                TokenResponse myTokenResponse = JsonConvert.DeserializeObject<TokenResponse>(response);
                CreateFile("AccessToken.txt", myTokenResponse.access_token);
                credentials.AccessToken = myTokenResponse.access_token;
                await _context.SaveChangesAsync();
            }
            else
            {
                CreateFile("TokenResponse.txt", tokenResponse.StatusCode.ToString());
            }

            return Json(credentials.AccessToken);
        }

        private void CreateFile(string fileName, string text)
        {
            string directory = Path.Combine(_environment.WebRootPath, "Tests");
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            string path = Path.Combine(_environment.WebRootPath, directory, fileName);
            using (FileStream stream = System.IO.File.Create(path))
            {
                AddText(stream, text);
            }
        }

        public async Task<IActionResult> GetSchoolList()
        {
            PowerSchool credentials = _context.PowerSchool.LastOrDefault();
            if (credentials == null)
            {
                return BadRequest();
            }
            string token = credentials.AccessToken;

            Uri uri = new Uri("https://devpowerschool.ceoimage.com/ws/v1/district/school/");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Headers.Add("Accept", "application/json");
            
            HttpClient client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();

                JObject json = JObject.Parse(body);
                List<JToken> results = json["schools"]["school"].Children().ToList();
                List<School> schools = new List<School>();
                foreach (JToken result in results)
                {
                    School school = result.ToObject<School>();
                    schools.Add(school);
                }
                return Json(schools);
            }
            else
            {
                return BadRequest();
            }
        }

        public async Task<IActionResult> GetPsUser()
        {
            PowerSchool credentials = _context.PowerSchool.LastOrDefault();
            if (credentials == null)
            {
                return BadRequest();
            }
            string token = credentials.AccessToken;
            CreateFile("UserRequestToken.txt", token);

            Uri uri = new Uri("https://devpowerschool.ceoimage.com/ws/xte/user/teacher/me");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Headers.Add("Accept", "application/json");
            CreateFile("UserRequest.txt", request.ToString());

            HttpClient client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                PsUser user = response.Content.ReadAsAsync<PsUser>().Result;
                return Json(user);
            }
            else
            {
                return BadRequest();
            }
        }

        private void AddText(FileStream fs, string value)
        {
            byte[] info = new System.Text.UTF8Encoding(true).GetBytes(value);
            fs.Write(info, 0, info.Length);
        }

        static public string EncodeTo64(string toEncode)
        {
            byte[] toEncodeAsBytes = System.Text.Encoding.ASCII.GetBytes(toEncode);
            string returnValue = Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }
    }
}