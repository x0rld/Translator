using System;
using System.Net.Http;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Material.Styles;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace GUITranslator
{
    public class MainWindow : Window
    {
        private  string? ApiToken { get;}
        private string? SelectedLang { get; set; }
        public MainWindow()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<MainWindow>().Build();
            ApiToken = config["ApiKey"];
            if (ApiToken == null)
            {
                throw new ApplicationException("no api key");
            }
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
          
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }
    

        private async void OnTranslateClick(object? sender, RoutedEventArgs routedEventArgs)
        {
            void DisplayError(NameScope nameScope, ArgumentException e, TextBox textBox, object card)
            {
                if (nameScope.Find("ErrorInput") is TextBlock errorBlock)
                {
                    errorBlock.Text = e.Message;
                    textBox.Text = string.Empty;
                }

                if (card is Card c)
                {
                    c.IsVisible = true;
                }
            }

            var thisWindowNameScope = (NameScope)this.FindNameScope();
            var errorInputCard = thisWindowNameScope.Find("ErrorInputCard");
            if (errorInputCard is Card card )
            {
                card.IsVisible = false;    
            }
            var inputBox = thisWindowNameScope.Find<TextBox>("InputBox");
            try
            {
                await Request(inputBox);
            }
            catch (ArgumentException e)
            {
                DisplayError(thisWindowNameScope, e, inputBox, errorInputCard);
            }
            catch (Exception e)
            {
                if (thisWindowNameScope.Find("ErrorInput") is TextBlock errorBlock)
                {
                    errorBlock.Text = e.Message;
                }
                
                if (errorInputCard is Card c)
                {
                    c.IsVisible = true;
                }
            }
        }
        private async Task Request(TextBox text)
        {
            if (string.IsNullOrWhiteSpace(text.Text) || SelectedLang == null )
            {
                throw new ArgumentException("text is empty or whitespace");
            }
            var client = new HttpClient();
            var response = await client.GetAsync(
                $"https://api-free.deepl.com/v2/translate?auth_key={ApiToken}" +
                $"&text={text.Text}&target_lang={SelectedLang}");

            if (!response.IsSuccessStatusCode) throw new Exception("error with api");
            await DisplayTranslate(response);
        }

        private async Task DisplayTranslate(HttpResponseMessage response)
        {
            var thisWindowNameScope = (NameScope)this.FindNameScope();
            var responseBlock = (TextBlock)thisWindowNameScope.Find("ResponseBlock");
            var langBlock = (TextBlock)thisWindowNameScope.Find("LangBlock");

            var responseString = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(responseString); 
            var translated =json["translations"]![0]!["text"];
            var assumeLanguage = json["translations"]![0]!["detected_source_language"];
            responseBlock.Text = translated!.ToString();
            langBlock.Text = "Lang: " + assumeLanguage;
        }


        private void TargetLang_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var box = (ComboBox?) sender;
            var item = (ComboBoxItem?) box?.SelectedItem;
             SelectedLang = item?.Tag?.ToString();
        }
    }
}