using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;

public class LanguageInitializer : MonoBehaviour
{
    private const string LANGUAGE_KEY = "LANGUAGE_CODE";

    IEnumerator Start()
    {
        // 🔴 이거 없으면 빌드에서 100% 문제 난다
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;

        // 1️⃣ 저장된 언어가 있으면 우선
        if (PlayerPrefs.HasKey(LANGUAGE_KEY))
        {
            string savedCode = PlayerPrefs.GetString(LANGUAGE_KEY);
            Locale savedLocale = locales.Find(l => l.Identifier.Code == savedCode);

            if (savedLocale != null)
            {
                LocalizationSettings.SelectedLocale = savedLocale;
                yield break;
            }
        }

        // 2️⃣ OS 언어와 가장 가까운 Locale 찾기
        SystemLanguage systemLang = Application.systemLanguage;
        Locale matched = locales.Find(l =>
            l.Identifier.CultureInfo.EnglishName.Contains(systemLang.ToString())
        );

        if (matched != null)
        {
            LocalizationSettings.SelectedLocale = matched;
            yield break;
        }

        // 3️⃣ 그래도 없으면 첫 번째(Locale Table에서 맨 위)
        LocalizationSettings.SelectedLocale = locales[0];
    }
}
