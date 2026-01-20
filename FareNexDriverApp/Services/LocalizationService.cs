using AndroidX.AppCompat.Content.Res;
using FareNexDriverApp.Core.Interfaces.Core.Localizations;
using FareNexDriverApp.Core.Interfaces.Core.Utilities;
using FareNexDriverApp.Core.Utilities;
using FareNexDriverApp.Resources.Strings;
using System.Globalization;
using System.Security.Cryptography.X509Certificates;

namespace FareNexDriverApp.Services
{
    public class LocalizationService : ILocalizationService
    {
        public string GetLocalizationValue(string key)
        {
            try
            {
                return AppResources.ResourceManager.GetString(key, CultureInfo.CurrentCulture) ?? key;
            }
            catch (Exception)
            {
                return key;
            }
            
        }
        public Dictionary<string , string> GetSupportedLanguagesList()
        {
            return new Dictionary<string, string>
            {
                {"US English",AppConstants.LangEnUs },
            };
        }
        public void SetLanuage(string languageCode)
        {
            CultureInfo.CurrentCulture = new  CultureInfo (languageCode);
        }
    }
}
