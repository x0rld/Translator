using System;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace CliTranslator
{
    // ReSharper disable once ClassNeverInstantiated.Global
    // ReSharper disable once ArrangeTypeModifiers
    class Program
    {
        private string _text;
        private string _targetLang;
        private string _apiToken;
        private readonly HttpClient _client = new HttpClient();
        
        public static async Task Main()
        {
            var program = new Program
            {
                _apiToken = Config.DeeplApi
            };
            program.AskToUser();
            await program.Request();
        }

        private static void DisplayTranslate(HttpResponseMessage responseMessage)
        {
            var responseContent = responseMessage.Content;
            var responseString = responseContent.ReadAsStringAsync().Result;
            var json = JObject.Parse(responseString);
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
                    Environment.Exit(2);
                }
            } while (_text.Trim() == string.Empty);

            Console.WriteLine("veuillez saisir la langue de traduction");
            _targetLang = Console.ReadLine();
            if (_targetLang == null)
            {
                Console.WriteLine(" Error cannot read line");
                Environment.Exit(2);
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