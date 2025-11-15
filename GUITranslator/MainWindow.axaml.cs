using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using DeeplWrapper;
using Material.Styles;
using Microsoft.Extensions.Configuration;

namespace GUITranslator
{
    public class MainWindow : Window
    {
        private readonly DeeplClient _deeplClient;
        private string? SelectedLang { get; set; }

        public MainWindow()
        {
            var config = new ConfigurationBuilder()
                .AddUserSecrets<MainWindow>().Build();
                _deeplClient = new DeeplClient(config["ApiKey"] ?? Environment.GetEnvironmentVariable("ApiKey"));

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
            try
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

                var thisWindowNameScope = (NameScope) this.FindNameScope();
                var errorInputCard = thisWindowNameScope.Find("ErrorInputCard");
                if (errorInputCard is Card card)
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
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync(e.Message);
                await Console.Error.WriteLineAsync(e.StackTrace);
            }
        }

        private async Task Request(TextBox text)
        {
            if (string.IsNullOrWhiteSpace(text.Text) || SelectedLang == null)
            {
                throw new ArgumentException("text is empty or whitespace");
            }

            var translation = await _deeplClient.Translate(text.Text, SelectedLang);
            DisplayTranslate(translation);
        }

        private void DisplayTranslate(Translation response)
        {
            var thisWindowNameScope = (NameScope) this.FindNameScope();
            var responseBlock = (TextBlock) thisWindowNameScope.Find("ResponseBlock");
            var langBlock = (TextBlock) thisWindowNameScope.Find("LangBlock");

            var translated = response.Text;
            var assumeLanguage = response.DetectedSourceLanguage;
            responseBlock.Text = translated;
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