using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StageEnemySetup
{
    [Tooltip("배치할 적 데이터 원본")]
    public EnemyData enemyData;
    [Tooltip("타임라인 점유 범위")]
    public Vector2Int tickRange;
    [Tooltip("피격 판정 섹터(플레이어가 여기서 공격하면 맞음)")]
    public List<int> hitSectors;
}

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
    private List<StageEnemySetup> _enemySpawns = new List<StageEnemySetup>();

    [Header("체력 설정")]
    [SerializeField]
    private int _playerMaxHP = 20;                //추후 유저 데이터에서 받아오기

    [Header("스테이지 진행도")]
    [SerializeField]
    private bool _isCleared = false;

    public int StageNumber => _stageNumber;
    public string StageName => _stageName;
    public string StageDescription => _stageDescription;
    public List<StageEnemySetup> EnemySpawns => _enemySpawns;
    public int PlayerMaxHP => _playerMaxHP;
    public bool IsCleared { get => _isCleared; set => _isCleared = value; }

}
