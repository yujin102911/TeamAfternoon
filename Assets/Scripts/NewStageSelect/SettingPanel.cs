using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SettingPanel : MonoBehaviour
{
    // 게임 설정 패널에 관련된 기능 여따가 넣으면 될듯
    [SerializeField] private Button _xButton;

    [Header("볼륨 슬라이더")]
    [SerializeField] private Slider _masterSlider;
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("해상도 설정")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;

    [Header("적용 버튼")]
    [SerializeField] private Button _activeBtn;

    [Header("적용 확인 패널")]
    [SerializeField] private GameObject confirmPopup;
    [SerializeField] private TMP_Text confirmText;

    [Header("언어 설정")]
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Header("CRT 토글")]
    [SerializeField] private Toggle pipelineToggle;

    private const string RP_KEY = "RP_MODE";

    private const string LanguageKey = "LANGUAGE"; // 예: "en", "ko-KR"

    private List<Locale> _availableLocales = new List<Locale>();
    private bool _isInitializingLanguageUI = false;

    private Resolution[] resolutions;
    private List<string> options = new List<string>();

    // 이전 해상도 저장용
    private int prevWidth;
    private int prevHeight;
    private bool prevFullscreen;

    // 타이머
    private Coroutine confirmCoroutine;
    private const float CONFIRM_TIME = 15f;

    private bool _isInitializing = false;

    // 언어 딕셔너리
    static readonly Dictionary<string, string> AutonymMap = new()
{
    { "en", "English" },
    { "ko-KR", "한국어" },
    { "ja", "日本語" },
    { "zh-Hans", "简体中文" },
    { "zh-TW", "繁體中文" },
    { "ru", "Русский" },
};

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseSetting);

        _masterSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetMasterVolume(val));
        _bgmSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetBGMVolume(val));
        _sfxSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetSFXVolume(val));

        //_activeBtn.onClick.AddListener(OnSetting_change);

        // ✅ 즉시 적용 리스너 연결
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownChanged_Immediate);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged_Immediate);

        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenToggleChanged_Immediate);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggleChanged_Immediate);

        pipelineToggle.onValueChanged.AddListener(OnPipelineToggleChanged);
    }

    private IEnumerator Start()
    {
        _isInitializing = true;

        InitResolutionDropdown();

        // 초기값 세팅 시 이벤트 발동 방지
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
        pipelineToggle.isOn = PlayerPrefs.GetInt(RP_KEY, 1) == 1;

        yield return LocalizationSettings.InitializationOperation;

        InitLanguageDropdown();

        _isInitializing = false;
    }

    private void OnEnable()
    {
        if (SoundManager.Instance != null)
        {
            _masterSlider.value = SoundManager.Instance.GetVolume("MasterVol");
            _bgmSlider.value = SoundManager.Instance.GetVolume("BGMVol");
            _sfxSlider.value = SoundManager.Instance.GetVolume("SFXVol");
        }
    }
    #region ClosePanel Methods
    private void CloseSetting()
    {
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }
    #endregion

    //드롭다운 초기화
    void InitResolutionDropdown()
    {
        //resolutions = Screen.resolutions;

        //resolutionDropdown.ClearOptions();
        //options.Clear();

        //int currentIndex = 0;

        //for (int i = 0; i < resolutions.Length; i++)
        //{
        //    string option = $"{resolutions[i].width} x {resolutions[i].height}";
        //    options.Add(option);

        //    if (resolutions[i].width == Screen.width &&
        //        resolutions[i].height == Screen.height)
        //    {
        //        currentIndex = i;
        //    }
        //}

        //resolutionDropdown.AddOptions(options);
        //resolutionDropdown.value = currentIndex;
        //resolutionDropdown.RefreshShownValue();

        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();
        options.Clear();

        List<Resolution> filtered = new List<Resolution>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution r = resolutions[i];

            // 🔹 16:9 비율만 허용
            float aspect = (float)r.width / r.height;
            if (Mathf.Abs(aspect - (16f / 9f)) > 0.01f)
                continue;

            // 🔹 중복 해상도 제거
            bool exists = filtered.Exists(x =>
                x.width == r.width && x.height == r.height);

            if (exists)
                continue;

            filtered.Add(r);

            string option = $"{r.width} x {r.height}";
            options.Add(option);

            if (r.width == Screen.width && r.height == Screen.height)
                currentIndex = filtered.Count - 1;
        }

        resolutions = filtered.ToArray();

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();

        if (fullscreenToggle != null)
            fullscreenToggle.isOn = Screen.fullScreen;
    }

    private void OnResolutionDropdownChanged_Immediate(int index)
    {
        if (_isInitializing) return;

        ApplyDisplaySettingsAndAskConfirm();
    }

    private void OnFullscreenToggleChanged_Immediate(bool isFullscreen)
    {
        if (_isInitializing) return;

        ApplyDisplaySettingsAndAskConfirm();
    }

    private void ApplyDisplaySettingsAndAskConfirm()
    {
        // 이미 확인 팝업 떠있는 상태에서 또 바꾸면
        // "이전값"을 덮어쓰면 롤백이 꼬여서,
        // 팝업이 떠있으면 먼저 롤백값을 업데이트하지 않는 편이 안전함.
        // 여기선 "팝업이 꺼져있을 때만 prev 저장" 방식으로 처리.
        if (confirmPopup != null && !confirmPopup.activeSelf)
        {
            prevWidth = Screen.width;
            prevHeight = Screen.height;
            prevFullscreen = Screen.fullScreen;
        }

        // ✅ 즉시 적용
        ApplyResolution(resolutionDropdown.value, fullscreenToggle.isOn);
    }

    private void ApplyResolution(int index, bool fullscreen)
    {
        if (resolutions == null || resolutions.Length == 0) return;
        index = Mathf.Clamp(index, 0, resolutions.Length - 1);

        Resolution selected = resolutions[index];

        Screen.SetResolution(selected.width, selected.height, fullscreen);
    }


    public void OnResolutionChanged(int index)
    {
        Resolution selected = resolutions[index];

        Screen.SetResolution(
            selected.width,
            selected.height,
            fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen
        );
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    public void OnSetting_change()
    {
        // 이전 상태 저장
        prevWidth = Screen.width;
        prevHeight = Screen.height;
        prevFullscreen = Screen.fullScreen;

        // 해상도 즉시 적용
        OnResolutionChanged(resolutionDropdown.value);
        OnFullscreenToggle(fullscreenToggle.isOn);

        // 확인 팝업 표시
        ShowConfirmPopup();
    }

    void ShowConfirmPopup()
    {
        confirmPopup.SetActive(true);

        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        confirmCoroutine = StartCoroutine(ConfirmTimer());
    }

    IEnumerator ConfirmTimer()
    {
        float time = CONFIRM_TIME;

        while (time > 0f)
        {
            confirmText.text = $"이 설정을 유지하시겠습니까? ({Mathf.CeilToInt(time)})";
            time -= Time.unscaledDeltaTime;
            yield return null;
        }

        // 시간 초과 → 롤백
        RollbackResolution();
    }

    public void OnConfirmYes()
    {
        confirmPopup.SetActive(false);

        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        PlayerPrefs.Save(); // 확정 저장
    }

    public void OnConfirmNo()
    {
        RollbackResolution();
    }

    void RollbackResolution()
    {
        if (confirmCoroutine != null)
            StopCoroutine(confirmCoroutine);

        Screen.SetResolution(prevWidth, prevHeight, prevFullscreen);
        fullscreenToggle.isOn = prevFullscreen;

        confirmPopup.SetActive(false);
    }

    string GetDisplayName(Locale locale)
    {
        var code = locale.Identifier.Code; // "ko-KR"
        if (AutonymMap.TryGetValue(code, out var name)) return name;

        // fallback: "Korean (South Korea)" 같은 기본 이름
        return locale.LocaleName;
    }

    private void InitLanguageDropdown()
    {
        if (languageDropdown == null) return;

        _isInitializingLanguageUI = true;

        _availableLocales.Clear();
        _availableLocales.AddRange(LocalizationSettings.AvailableLocales.Locales);

        languageDropdown.ClearOptions();

        // 표시 이름은 LocaleName(예: English, Korean (South Korea)) 사용
        List<string> options = new List<string>(_availableLocales.Count);
        for (int i = 0; i < _availableLocales.Count; i++)
        {
            string name = GetDisplayName(_availableLocales[i]);
            options.Add(name);
        }
        languageDropdown.AddOptions(options);

        // 저장된 언어가 있으면 그걸로 선택, 없으면 현재 SelectedLocale
        string savedCode = PlayerPrefs.GetString(LanguageKey, "");
        int selectedIndex = GetLocaleIndexByCode(savedCode);

        if (selectedIndex < 0)
        {
            var current = LocalizationSettings.SelectedLocale;
            selectedIndex = _availableLocales.IndexOf(current);
            if (selectedIndex < 0) selectedIndex = 0;
        }

        languageDropdown.SetValueWithoutNotify(selectedIndex);
        languageDropdown.RefreshShownValue();

        // 이벤트 연결 (중복 방지)
        languageDropdown.onValueChanged.RemoveListener(OnLanguageDropdownChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

        _isInitializingLanguageUI = false;
    }

    private int GetLocaleIndexByCode(string code)
    {
        if (string.IsNullOrEmpty(code)) return -1;

        for (int i = 0; i < _availableLocales.Count; i++)
        {
            // Locale.Identifier.Code 예: "en", "ko-KR"
            if (_availableLocales[i].Identifier.Code == code)
                return i;
        }
        return -1;
    }

    private void OnLanguageDropdownChanged(int index)
    {
        if (_isInitializingLanguageUI) return;
        if (index < 0 || index >= _availableLocales.Count) return;

        Locale selected = _availableLocales[index];

        // 즉시 적용
        LocalizationSettings.SelectedLocale = selected;

        // 저장(다음 실행 때 유지)
        PlayerPrefs.SetString(LanguageKey, selected.Identifier.Code);
        PlayerPrefs.Save();
    }

    void OnPipelineToggleChanged(bool high)
    {
        PlayerPrefs.SetInt(RP_KEY, high ? 1 : 0);
        PlayerPrefs.Save();

        RenderPipelineManager.ApplyPipelineAsset(high);
    }

}
