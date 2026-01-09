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

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseSetting);

        _masterSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetMasterVolume(val));
        _bgmSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetBGMVolume(val));
        _sfxSlider.onValueChanged.AddListener(val => SoundManager.Instance.SetSFXVolume(val));
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

}
