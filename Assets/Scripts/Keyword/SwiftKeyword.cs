using UnityEngine;

[CreateAssetMenu(menuName = "Data/Keywords/Swift", fileName = "SwiftKeyword")]
public class SwiftKeyword : KeywordData
{
    [Header("상세 설정")]
    [Tooltip("증가시킬 이동 속도 (기본 1 + 키워드 적용값) 만큼 이동함")]
    [SerializeField] private int speedAmount = 1;

    public override void OnBlockStart(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        if (battleSystem != null)
        {
            int currentSpeed = battleSystem.GetBuffValue("Speed", true);
            battleSystem.SetBuff("Speed", currentSpeed + speedAmount, true);

            Debug.Log($"[키워드] 신속{speedAmount}: 이동속도가 {speedAmount} 증가했습니다.");
        }
    }

    public override void OnBlockEnded(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        if (battleSystem != null)
        {
            int currentSpeed = battleSystem.GetBuffValue("Speed", true);
            int newSpeed = Mathf.Max(0, currentSpeed - speedAmount);

            battleSystem.SetBuff("Speed", newSpeed, true);

            Debug.Log($"[키워드] 신속{speedAmount}: 신속 효과가 사라졌습니다.");
        }
    }

    public override int GetSpeedBonus()
    {
        return speedAmount;
    }

}
