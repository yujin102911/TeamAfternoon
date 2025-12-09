using UnityEngine;

[CreateAssetMenu(menuName = "Data/Keywords/Power", fileName = "Power")]
public class PowerKeyword : KeywordData
{
    [Header("강화 수치")]
    [Tooltip("증가시킬 공격력(0.3 = 30% 증가)")]
    [SerializeField] private float powerAmount = 0.3f;

    [Header("발동 조건")]
    [Tooltip("체크 시: 블록 내에 Move 액션이 하나라도 있어야 발동")]
    [SerializeField] private bool condition_HasMove = false;

    [Tooltip("체크 시: 블록의 마지막 틱이 공격일 때, 그 마지막 공격에만 발동")]
    [SerializeField] private bool condition_LastAttackOnly = false;

    private int CalculateBuffAmount(PlacedBlock placed)
    {
        int baseDamage = placed.GetBlockData().AttackDamage;
        return Mathf.CeilToInt(baseDamage * powerAmount);
    }
    
    private bool CheckCondition(PlacedBlock placed)
    {
        if (condition_HasMove)
        {
            bool foundMove = false;
            BlockData data = placed.GetBlockData();

            foreach(var action in data.actionTypes)
            {
                if (action == ActionType.Move)
                {
                    foundMove = true;
                    break;
                }
            }
            if (!foundMove) return false;
        }
        return true;
    }

    public override void OnBlockStart(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        if (condition_LastAttackOnly) return;
        if (CheckCondition(placed))
        {
            int amount = CalculateBuffAmount(placed);
            AddPower(battleSystem, amount);
            Debug.Log($"[조건부강화] 조건 만족! Power + {powerAmount}");
        }
    }

    public override void OnTick(PlacedBlock placed, int currentTick, ActionType action, BattleSystem battleSystem)
    {
        // "마지막 공격만" 옵션이 꺼져있으면, 이미 OnBlockStart에서 했으므로 패스
        if (!condition_LastAttackOnly) return;

        // 조건 체크 (이동 포함 여부 등)가 틀리면 패스
        if (!CheckCondition(placed)) return;

        // 현재가 마지막 틱인지 확인
        BlockData data = placed.GetBlockData();
        int lastIndex = data.blockLength - 1;
        int myIndex = placed.GetCardTickIndex(currentTick);

        // 마지막 틱이면서 && 행동이 공격일 때만
        if (myIndex == lastIndex && action == ActionType.Attack)
        {
            int amount = CalculateBuffAmount(placed);
            AddPower(battleSystem, amount);
            Debug.Log($"[조건부 강화] 마지막 일격! Power +{powerAmount}");
        }
    }

    public override void OnBlockEnded(PlacedBlock placed, int currentTick, BattleSystem battleSystem)
    {
        // 버프를 줬었는지 조건을 다시 확인해서 판단 (SO는 상태 저장을 못하므로)
        bool conditionMet = CheckCondition(placed);

        if (!conditionMet) return; // 조건을 만족 안 했으면 줬던 버프도 없으니 패스

        int amount = CalculateBuffAmount(placed);
        // A. "마지막 공격만" 모드였던 경우
        if (condition_LastAttackOnly)
        {
            // 마지막 틱 액션이 공격이었어야 버프를 줬을 것임
            BlockData data = placed.GetBlockData();
            int lastIndex = data.blockLength - 1;

            if (data.GetEffectAt(lastIndex) == ActionType.Attack)
            {
                RemovePower(battleSystem, amount);
            }
        }
        // B. "블록 전체" 모드였던 경우
        else
        {
            RemovePower(battleSystem, amount);
        }
    }

    #region 중복 코드
    private void AddPower(BattleSystem battle, int amount)
    {
        if (battle == null) return;
        int current = battle.GetBuffValue("Power", true);
        battle.SetBuff("Power", current + amount, true);
    }

    private void RemovePower(BattleSystem battle, int amount)
    {
        if (battle == null) return;
        int current = battle.GetBuffValue("Power", true);
        int result = Mathf.Max(0, current - amount);
        battle.SetBuff("Power", result, true);
    }
    #endregion

}
