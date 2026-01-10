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
    BGM_Stage,
    BGM_Battle,
    BGM_Boss,

    // UI
    UI_Click,
    UI_Click2,
    UI_Click3,
    UI_Click4,
    UI_Closing_Book,

    // SFX
    SFX_Lightning,
    SFX_Cure,
    SFX_Spell_Write,
    SFX_Spell_Cancle,
    SFX_Execute,

    // Player
    Player_hit,
    Player_Sword,
    Player_Sword2,
    Player_Bow,
    Player_Bow2,
    Player_Bow3,

    // Enemy
    EnemyHurt_0,
    EnemyHurt_1,
    EnemyHurt_2,
    EnemyHurt_3,
    EnemyHurt_4,
    EnemyHurt_5,

    EnemyEffect_0,
    EnemyEffect_1,
    EnemyEffect_2,
    EnemyEffect_3,
    EnemyEffect_4,
    EnemyEffect_5

}
