using System;
using System.Linq;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DeeplWrapper;
using Microsoft.Extensions.Configuration;

namespace CliTranslator
{
    internal class Cli
    {
        private readonly DeeplClient _deepl;

        private Cli(string apikey)
        {
            _deepl = new DeeplClient(apikey);
        }

        public static async Task Main()
        {
            var config = new ConfigurationBuilder().AddUserSecrets<Cli>().Build();
            var program = new Cli(config["ApiKey"] ?? Environment.GetEnvironmentVariable("ApiKey"));
            await program.AskToUser();
        }

        private static void DisplayTranslate(Translation translations)
        {
            var (assumeLanguage, translatedSentence) = translations;
            Console.WriteLine($"le texte traduit est: {translatedSentence}");
            Console.WriteLine($"La langue reconnue est {assumeLanguage}");
        }

        private async Task AskToUser()
        {
            string text;
            do
            {
                Console.WriteLine("veuillez saisir le texte à traduire");
                text = Console.ReadLine();
                if (text != null) continue;
                throw new ApplicationException("error reading input");
            } while (string.IsNullOrWhiteSpace(text));

            Console.WriteLine("veuillez saisir la langue de traduction");
            var targetLang = Console.ReadLine();
            if (targetLang == null)
            {
                Console.WriteLine(" Error cannot read line");
                throw new ApplicationException("stdin closed"); 
            }
            text = HttpUtility.UrlEncode(text);
            await Request(text,targetLang);

        }
        private async Task Request(string text, string targetLang)
        {
            var translations = await _deepl.Translate(text,targetLang);
             DisplayTranslate(translations);
        }
    }
}