using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DeeplWrapper;

public record RootObject(
    [property: JsonPropertyName("translations")]
    Translation[] Translations
);

public record Translation(
    [property: JsonPropertyName("detected_source_language")]
    string DetectedSourceLanguage, 
    [property: JsonPropertyName("text")]
    string Text);

[JsonSerializable(typeof(RootObject))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}

public class DeeplClient
{
    private readonly string _apikey;
    private static readonly HttpClient Client = new HttpClient();

    public DeeplClient(string apikey)
    {
        _apikey = apikey ?? throw new ArgumentException("api key invalid", apikey);
        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<Translation> Translate(string text, string targetLang)
    {
        var response = await Client.GetAsync(
            $"https://api-free.deepl.com/v2/translate?auth_key={_apikey}" +
            $"&text={text}&target_lang={targetLang}");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("error in target language");
        }

       
        var result = JsonSerializer.Deserialize<RootObject>(
                         await response.Content.ReadAsStreamAsync(),
                         SourceGenerationContext.Default.RootObject) ??
                     throw new InvalidOperationException("invalid json from api");
        return result.Translations.First();
    }
}