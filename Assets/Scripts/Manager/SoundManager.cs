using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

/// <summary>
/// 사운드 설정
/// 언어 설정
/// 해상도 설정
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mainMixer;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [SerializeField] private List<SoundData> soundDatas;

    private const string MasterKey = "MasterVol";
    private const string BGMKey = "BGMVol";
    private const string SFXKey = "SFXVol";

    private Dictionary<SoundID, SoundData> _soundMap;

    private AudioSource _bgmSource;
    private List<AudioSource> _sfxPool = new();

    #region 볼륨 조절
    public void SetMasterVolume(float value) => SetMixerVolume(MasterKey, value);
    public void SetBGMVolume(float value) => SetMixerVolume(BGMKey, value);
    public void SetSFXVolume(float value) => SetMixerVolume(SFXKey, value);
    #endregion


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.outputAudioMixerGroup = bgmMixerGroup;

        _soundMap = new Dictionary<SoundID, SoundData>();
        foreach (var data in soundDatas)
            _soundMap[data.id] = data;
        LoadAndApplySettings();
    }

    private void Update()
    {
        if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Play(SoundID.UI_Click);
        }
        else if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            Play(SoundID.UI_Click2);
        }
    }

    private void LoadAndApplySettings()
    {
        // 저장된 값이 없으면 기본값 0.8f(80%) 사용
        SetMasterVolume(PlayerPrefs.GetFloat(MasterKey, 0.8f));
        SetBGMVolume(PlayerPrefs.GetFloat(BGMKey, 0.8f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFXKey, 0.8f));
    }

    public void Play(SoundID id)
    {
        if (!_soundMap.TryGetValue(id, out SoundData data))
            return;

        switch (data.type)
        {
            case SoundType.BGM:
                PlayBGM(data);
                break;
            case SoundType.SFX:
            case SoundType.UI:
                PlaySFX(data);
                break;
        }
    }

    void PlayBGM(SoundData data)
    {
        if (_bgmSource.clip == data.clip)
            return;

        _bgmSource.clip = data.clip;
        _bgmSource.volume = data.volume;
        _bgmSource.loop = true;
        _bgmSource.Play();
    }

    void PlaySFX(SoundData data)
    {
        AudioSource source = GetAvailableSource();
        source.ignoreListenerPause = (data.type == SoundType.UI);

        source.clip = data.clip;
        source.volume = data.volume;
        source.pitch = Random.Range(
            data.pitch - 0.05f,
            data.pitch + 0.05f
        );

        source.loop = false;
        source.Play();
    }

    public void StopSFX()
    {
        foreach (var src in _sfxPool)
        {
            if (src.isPlaying)
                src.Stop();
        }
    }

    AudioSource GetAvailableSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying)
                return src;

        var newSource = gameObject.AddComponent<AudioSource>();
        newSource.outputAudioMixerGroup = sfxMixerGroup;
        _sfxPool.Add(newSource);
        return newSource;
    }

    //public void SetBGMVolume(float value)
    //{
    //    _bgmSource.volume = value;
    //}

    //public void SetSFXVolume(float value)
    //{
    //    foreach (var src in _sfxPool)
    //        src.volume = value;
    //}

    public void SetMixerVolume(string key, float value)
    {
        float volume = Mathf.Log10(Mathf.Max(0.0001f, value)) * 20;
        mainMixer.SetFloat(key, volume);

        PlayerPrefs.SetFloat(key, value);
    }

    public float GetVolume(string key) => PlayerPrefs.GetFloat(key, 0.8f);
}
