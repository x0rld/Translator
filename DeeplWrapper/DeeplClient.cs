using System.Net.Http.Headers;
using System.Text.Json;

namespace DeeplWrapper;
public record Translation(string DetectedSourceLanguage,string TranslatedText);
public class DeeplClient
{
    private readonly string _apikey;
    private static readonly HttpClient Client = new HttpClient();
    public DeeplClient(string apikey)
    {
        _apikey = apikey;
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Translation> Translate(string text,string targetLang)
    {
        var response =  await Client.GetAsync(
                $"https://api-free.deepl.com/v2/translate?auth_key={_apikey}" +
                $"&text={text}&target_lang={targetLang}");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("error in target language");
        }

        return JsonSerializer.Deserialize<Translation>(await response.Content.ReadAsStringAsync()) ??
               throw new InvalidOperationException("invalid json from api");
    }
}