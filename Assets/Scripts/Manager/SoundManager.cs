using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Mixer Groups")]
    [SerializeField] private AudioMixerGroup bgmMixerGroup;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    [SerializeField] private List<SoundData> soundDatas;

    private Dictionary<SoundID, SoundData> _soundMap;

    private AudioSource _bgmSource;
    private List<AudioSource> _sfxPool = new();


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
        source.clip = data.clip;
        source.volume = data.volume;
        source.pitch = Random.Range(
            data.pitch - 0.05f,
            data.pitch + 0.05f
        );

        source.loop = false;
        source.Play();
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

    public void SetBGMVolume(float value)
    {
        _bgmSource.volume = value;
    }

    public void SetSFXVolume(float value)
    {
        foreach (var src in _sfxPool)
            src.volume = value;
    }
}
