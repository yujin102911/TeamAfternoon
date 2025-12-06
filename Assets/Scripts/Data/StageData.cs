using UnityEngine;

[CreateAssetMenu(fileName = "New StageData", menuName = "Data/Stage Data")]
public class StageData : ScriptableObject
{
    [Header("기본 설정")]
    [SerializeField]
    private int _stageNumber;
    //public MapSize mapSize = MapSize.Sectors_8; //기본적으로 8섹터 게임으로 설정
    [SerializeField]
    private string _stageName; //스테이지 이름
    [TextArea(3, 10)]
    [SerializeField]
    private string _stageDescription; //스테이지 설명

    [Header("적 정보")]
    [SerializeField]
    private EnemyData _enemyData;

    [Header("체력 설정")]
    [SerializeField]
    private int _playerMaxHP = 20;                //추후 유저 데이터에서 받아오기

    [Header("스테이지 진행도")]
    [SerializeField]
    private bool _isCleared = false;

    public int StageNumber => _stageNumber;
    public string StageName => _stageName;
    public string StageDescription => _stageDescription;
    public EnemyData Enemy_Data => _enemyData;
    public int PlayerMaxHP => _playerMaxHP;
    public bool IsCleared { get => _isCleared; set => _isCleared = value; }

}
