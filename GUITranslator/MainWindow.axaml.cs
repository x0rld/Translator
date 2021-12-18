using System.Net.Http;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Newtonsoft.Json.Linq;

namespace GUITranslator
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            ApiToken = Config.DeeplApi;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
          
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

        }
    

        private void OnTranslateClick(object? sender, RoutedEventArgs routedEventArgs)
        {
            NameScope thisWindowNameScope = (NameScope)this.FindNameScope();
            var inputBox = (TextBox)thisWindowNameScope.Find("InputBox");
            switch (Request(inputBox))
             {
                 case 1:
                     
                     break;
                 case 2:
                     System.Environment.Exit(2);
                     break;
             }
             

        }
        private int Request(TextBox text)
        {
            if (text.Text == null || SelectedLang == null )
            {
                return 1;
            }
            var client = new HttpClient();
            var response = client.GetAsync(
                $"https://api-free.deepl.com/v2/translate?auth_key={ApiToken}" +
                $"&text={text.Text}&target_lang={SelectedLang}").Result;

            if (!response.IsSuccessStatusCode) return 2;
            DisplayTranslate(response);
            return 0;

        }

        private void DisplayTranslate(HttpResponseMessage response)
        {
            NameScope thisWindowNameScope = (NameScope)this.FindNameScope();
            var responseBlock = (TextBlock)thisWindowNameScope.Find("ResponseBlock");
            var langBlock = (TextBlock)thisWindowNameScope.Find("LangBlock");

            var responseString = response.Content.ReadAsStringAsync().Result;
            var json = JObject.Parse(responseString); 
            var translated =json["translations"]![0]!["text"];
            var assumeLanguage = json["translations"]![0]!["detected_source_language"];
            responseBlock.Text = translated!.ToString();
            langBlock.Text = "Lang: " + assumeLanguage;
        }

        private  string? ApiToken { get;}
        private string? SelectedLang { get; set; }

        private void TargetLang_OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            var box = (ComboBox?) sender;
            var item = (ComboBoxItem?) box?.SelectedItem;
             SelectedLang = item?.Tag?.ToString();
        }
    }
}