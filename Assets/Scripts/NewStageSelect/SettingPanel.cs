using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
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

    private Resolution[] resolutions;
    private List<string> options = new List<string>();

    // 이전 해상도 저장용
    private int prevWidth;
    private int prevHeight;
    private bool prevFullscreen;

    // 타이머
    private Coroutine confirmCoroutine;
    private const float CONFIRM_TIME = 15f;

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseSetting);

        _masterSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetMasterVolume(val));
        _bgmSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetBGMVolume(val));
        _sfxSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetSFXVolume(val));

        _activeBtn.onClick.AddListener(OnSetting_change);
    }

    void Start()
    {
        InitResolutionDropdown();

        fullscreenToggle.isOn = Screen.fullScreen;
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

}
