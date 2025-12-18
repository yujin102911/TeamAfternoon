using System.Collections.Generic;
using UnityEngine;

public enum PhaseConditionType
{
    None,            // 조건 없음
    HpThreshold,     // 적 전체 체력 n% 이하
    EnemyCount,      // 남은 적 n마리 이하
}

[System.Serializable]
public class PhaseTransitionData
{
    public string PhaseName; // "1->2페이즈 조건"
    public PhaseConditionType ConditionType;
    public float ConditionValue; // 0.5면 50%, 1이면 1마리 등
}

[System.Serializable]
public class BookPage
{
    public string ChapterTitle;
    public bool IsChapterStart;
    [TextArea(5, 20)]
    public string BodyText;
}

[System.Serializable]
public struct StageEnemySetup
{
    [Tooltip("배치할 적 데이터 원본")]
    public EnemyData enemyData;
    [Tooltip("피격 판정 섹터(플레이어가 여기서 공격하면 맞음)")]
    public List<int> hitSectors;
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

    [Header("편지 내용")]
    [Tooltip("클리어 전 보일 의뢰 편지")]
    [SerializeField]
    [TextArea(5, 20)]
    private string _requestLetter;

    [Tooltip("클리어 후 보일 감사 편지")]
    [SerializeField]
    [TextArea(5, 20)]
    private string _thankLetter;

    [Header("책 표지/타이틀 정보")]
    [TextArea(4,20)]
    public string BookCredit = "2025 by Team Afternoon";

    [Header("타이틀 페이지 오염 ID")]
    public int TitlePollutionID = 0;

    [Header("펼침면 별 오염 ID 리스트")]
    [Tooltip("순서대로 1번째 펼침면 (0,1).,..")]
    [SerializeField] private List<int> _spreadPollutionIDs = new List<int>();

    [Header("책 본문 내용(인덱스 순서대로 2페이지씩 배치)")]
    [SerializeField]
    private List<BookPage> _bookPages = new List<BookPage>();

    [Header("맵 정보")]
    [SerializeField]
    private MapSize _mapSize = MapSize.Sectors_8;
    [SerializeField]
    private List<Vector3> _sectorTransform = new List<Vector3>(); // 만약 _mapSize가 Custom인 경우에는 위치 정보가 담긴 리스트가 채워져있어야함

    [Header("적 정보")]
    [SerializeField]
    private List<StageEnemySetup> _enemySpawns = new List<StageEnemySetup>();

    [Header("페이즈 전환 조건( Index0: 1->2 전환 조건)")]
    [SerializeField]
    private List <PhaseTransitionData> _phaseConditions = new List<PhaseTransitionData>();

    [Header("체력 설정")]
    [SerializeField]
    private int _playerMaxHP = 20;                //추후 유저 데이터에서 받아오기

    [Header("스테이지 진행도")]
    [SerializeField]
    private bool _isCleared = false;

    public int StageNumber => _stageNumber;
    public string StageName => _stageName;
    public List<BookPage > BookPages => _bookPages;
    public List<int> SpreadPollutionIDs => _spreadPollutionIDs;
    public List<StageEnemySetup> EnemySpawns => _enemySpawns;
    public List<PhaseTransitionData> PhaseConditions => _phaseConditions;
    public MapSize MapSize => _mapSize;
    public List<Vector3> SectorPoints => _sectorTransform;
    public int PlayerMaxHP => _playerMaxHP;
    public bool IsCleared { get => _isCleared; set => _isCleared = value; }
    public string RequestLetter => _requestLetter;
    public string ThankLetter => _thankLetter;

}
