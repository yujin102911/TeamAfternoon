using UnityEngine;

[CreateAssetMenu(menuName = "Data/Keywords/Vulnerable", fileName = "VulnerableKeyword")]
public class VulnerableKeyword : KeywordData
{
    [Header("상세 설정")]
    [Tooltip("부여할 취약 턴 수")]
    [SerializeField] private int turnAmount = 2;

    public override void OnBlockStart(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        if (battleSystem != null)
        {
            int currentTurns = battleSystem.GetBuffValue("Vulnerable", false);
            battleSystem.SetBuff("Vulnerable", currentTurns + turnAmount, false);
            Debug.Log($"[키워드] 취약: 적의 취약이 {turnAmount}턴 부여됐습니다!");
        }
    }

}
