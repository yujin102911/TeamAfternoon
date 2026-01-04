using System.IO;
using UnityEngine;

public class BattleRecorder : MonoBehaviour
{
    private ReplayData _replayData;
    private RoundSnapshot _currentRound;

    public void StartNewRecording(int stageNum)
    {
        _replayData = new ReplayData { stageNumber = stageNum };
    }

    public void StartNewRound(int roundNum, string patternName)
    {
        _currentRound = new RoundSnapshot { roundNumber = roundNum, enemyPatternName = patternName };
        _replayData.rounds.Add(_currentRound);
    }

    public void CaptureTick(int tick, BattleSystem bs, RuntimeEnemy enemy)
    {
        if (_currentRound == null) return;

        _currentRound.ticks.Add(new TickSnapshot
        {
            tick = tick,
            playerHP = bs.PlayerHP,
            playerSector = bs.PlayerCurrentSector,
            playerAction = bs.LastAction,
            playerMoveDir = bs.LastMoveDir,
            damageToEnemy = bs.LastDealtDamage,
            isCrit = bs.LastWasCrit,
            isInterrupted = bs.LastInterrupted,
            enemyHP = bs.EnemyHP,
            isLeft = enemy.IsLeft,
            enemyAttackSectors = bs.CurrentAttackSectors,
            damageToPlayer = bs.LastTakenDamage,
            stonePositions = bs.GetStoneSectors()
        });

        // 데이터 수집 후 초기화
        bs.LastAction = ActionType.None;
        bs.LastDealtDamage = 0;
        bs.LastTakenDamage = 0;
    }

    public void SaveToJson()
    {
        string json = JsonUtility.ToJson(_replayData, true);
        string path = Path.Combine(Application.persistentDataPath, $"Replay_{_replayData.stageNumber}_{Time.time}.json");
        File.WriteAllText(path, json);
        Debug.Log($"[Recorder] 리플레이 저장 완료: {path}");
    }
}