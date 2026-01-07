using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageEnemySetup
{
    [Tooltip("배치할 적 데이터 원본")]
    public EnemyData enemyData;
    [Tooltip("피격 판정 섹터(플레이어가 여기서 공격하면 맞음)")]
    public List<int> hitSectors;
}

/// <summary>
/// 메일용 구조체
/// </summary>
[System.Serializable]
public class MailContent
{
    public string subject;
    public string sender;
    public string receiver;
    [TextArea(5, 20)]
    public string body;
    public bool isRead;

    public enum MailUnlockCondition { Always, AfterClear }
    public MailUnlockCondition unlockCondition;
}

[System.Serializable]
public class BoardEntry
{
    public string title;
    [TextArea(10, 20)]
    public string content;
    public Sprite illustration;
}

[CreateAssetMenu(fileName = "New StageData", menuName = "Data/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 설정")]
    [SerializeField]
    private int _stageNumber;
    [SerializeField]
    [TextArea(3, 20)]
    private string _stageName; //스테이지 이름

    [Header("제한 설정")]
    [SerializeField] private int _limitRound = 8;
    [SerializeField] private int _limitEffect = 1;

    [Header("메일 내용")]
    [SerializeField] private List<MailContent> _mails = new List<MailContent>();

    [Header("게시판 설정")]
    [SerializeField] private List<BoardEntry> _boardEntries = new List<BoardEntry>();

    [Header("맵 정보")]
    [SerializeField]
    private MapSize _mapSize = MapSize.Grid_3x3;
    [SerializeField]
    private List<Vector3> _sectorTransform = new List<Vector3>(); // 만약 _mapSize가 Custom인 경우에는 위치 정보가 담긴 리스트가 채워져있어야함

    [Header("적 정보")]
    [SerializeField]
    private List<StageEnemySetup> _enemySpawns = new List<StageEnemySetup>();

    [Header("체력 설정")]
    [SerializeField]
    private int _playerMaxHP = 20;

    [Header("스테이지 진행도")]
    [SerializeField]
    private bool _isCleared = false;

    public int StageNumber => _stageNumber;
    public string StageName => _stageName;
    public List<MailContent> Mails => _mails;
    public List<BoardEntry > BoardEntries => _boardEntries;
    public List<StageEnemySetup> EnemySpawns => _enemySpawns;
    public MapSize MapSize => _mapSize;
    public List<Vector3> SectorPoints => _sectorTransform;
    public int PlayerMaxHP => _playerMaxHP;
    public bool IsCleared { get => _isCleared; set => _isCleared = value; }
    public int LimitRound => _limitRound;

    public int LimitEffect => _limitEffect;
}
