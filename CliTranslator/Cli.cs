using System;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;

namespace CliTranslator
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ArrangeTypeModifiers
    class Cli
    {
        private string _text;
        private string _targetLang;
        private string _apiToken;
        private readonly HttpClient _client = new HttpClient();
        
        public static async Task Main()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Cli>().Build();
            var program = new Cli
            {
                _apiToken = config["ApiKey"]
            };
            if (program._apiToken == null)
            {
                throw new ApplicationException("no api key");
            }
            program.AskToUser();
            await program.Request();
        }

        private static void DisplayTranslate(HttpResponseMessage responseMessage)
        {
            var responseContent = responseMessage.Content;
            var responseString = responseContent.ReadAsStringAsync().Result;
            var json = JsonNode.Parse(responseString);
            var translated = json!["translations"]![0]!["text"];
            var assumeLanguage = json["translations"][0]["detected_source_language"];
            Console.WriteLine($"le texte traduit est: {translated}");
            Console.WriteLine($"La langue reconnue est {assumeLanguage}");
        }

        private void AskToUser()
        {
            do
            {
                Console.WriteLine("veuillez saisir le texte à traduire");
                _text = Console.ReadLine();
                if (_text == null)
                {
                    Console.WriteLine(" Error cannot read line");
                    throw new ApplicationException("stdin closed"); }
            } while (_text.Trim() == string.Empty);

            Console.WriteLine("veuillez saisir la langue de traduction");
            _targetLang = Console.ReadLine();
            if (_targetLang == null)
            {
                Console.WriteLine(" Error cannot read line");
                throw new ApplicationException("stdin closed"); 
            }
            _text = HttpUtility.UrlEncode(_text);
        }
        private async Task Request()
        {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = _client.GetAsync(
                $"https://api-free.deepl.com/v2/translate?auth_key={_apiToken}" +
                $"&text={_text}&target_lang={_targetLang}");
            var message = await response;
            if (message.IsSuccessStatusCode)
            {
                DisplayTranslate(message);
            }
            else
            {
                Console.WriteLine(message.StatusCode);
            }
        }
    }
}