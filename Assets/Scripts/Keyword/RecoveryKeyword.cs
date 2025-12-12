using UnityEngine;

[CreateAssetMenu(menuName = "Data/Keywords/Recovery", fileName = "RecoveryKeyword")]
public class RecoveryKeyword : KeywordData
{
    [Header("상세 설정")]
    [Tooltip("체크 시 (전체체력 * 변수) 만큼의 체력 회복")]
    [SerializeField] private bool isPercentage = false;
    [Tooltip("체크 안하면 그냥 이 변수만큼의 체력 회복")]
    [SerializeField] private float amount;

    public override void OnBlockStart(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        int finalHeal;
        if (isPercentage)
        {
            finalHeal = Mathf.FloorToInt(battleSystem.PlayerMaxHP * amount);
        }
        else
        {
            finalHeal = Mathf.FloorToInt(amount);
        }
        battleSystem.HealPlayer(finalHeal);
    }
}
