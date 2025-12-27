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

[CreateAssetMenu(fileName = "New StageData", menuName = "Data/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 설정")]
    [SerializeField]
    private int _stageNumber;
    [SerializeField]
    [TextArea(3, 20)]
    private string _stageName; //스테이지 이름

    [Header("메일 내용")]
    [Tooltip("클리어 전 보일 의뢰 메일")]
    [SerializeField]
    [TextArea(5, 20)]
    private string _requestLetter;
    [Tooltip("보내는 사람")]
    [SerializeField]
    private string _sender = "@inailedit.com";
    [Tooltip("받는 사람")]
    [SerializeField]
    private string _receiver = "@clearrun.fake";

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
    [SerializeField]
    private bool _isRead = false;

    public int StageNumber => _stageNumber;
    public string StageName => _stageName;
    public string Sender => _sender;
    public string Receiver => _receiver;
    public List<StageEnemySetup> EnemySpawns => _enemySpawns;
    public MapSize MapSize => _mapSize;
    public List<Vector3> SectorPoints => _sectorTransform;
    public int PlayerMaxHP => _playerMaxHP;
    public bool IsCleared { get => _isCleared; set => _isCleared = value; }
    public bool IsRead { get => _isRead; set => _isRead = value; }
    public string RequestLetter => _requestLetter;

}
