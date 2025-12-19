using UnityEngine;

[CreateAssetMenu(menuName = "Sound/SoundData")]
public class SoundData : ScriptableObject
{
    public SoundID id;
    public AudioClip clip;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.5f, 2f)] public float pitch = 1f;

    public bool loop;
    public SoundType type;
}

public enum SoundType
{
    BGM,
    SFX,
    UI
}

public enum SoundID
{
    // BGM
    BGM_Title,
    BGM_Battle,

    // UI
    UI_Click,
    UI_Popup_Open,

    // SFX
    SFX_Attack,
    SFX_Hit,
}
