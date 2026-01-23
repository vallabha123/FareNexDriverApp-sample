using System.Collections.Generic;

namespace FareNexDriverApp.Core.Interfaces.Core.Localization
{
    public interface ILocalizationService
    {
        string GetLocalizedValue(string key);
        void SetLanuage(string language);
        Dictionary <string ,string> GetSupportedLanuageList();
    }
}