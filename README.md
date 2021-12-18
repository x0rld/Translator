#Deepl Translator

This is just a translator using deepl API to do a project in C#

To use the app you have to create a Config.cs in CliTranslator or GUITranslator directory  
Put your own API KEY and associate with the right namespace
```c#
namespace CliTranslator // this can change 
{
    public static class Config
    {
    internal const string DeeplApi = "YOURAPIKEY";
    }
}
```
