using UnityEngine;
using Sirenix.OdinInspector;

public enum ActionType
{
    None,
    Attack,
    Move,
    Cure
}

public enum MoveDirection
{
    None,
    Left,
    Right,
}

[CreateAssetMenu(fileName = "New BlockData", menuName = "Data/Block Data")]
public class BlockData : ScriptableObject
{
    //───────────────────────────────
    // 기본 정보
    //───────────────────────────────
    [BoxGroup("기본 정보"), LabelText("블럭 ID"), MinValue(0)]
    public int blockID;

    [BoxGroup("기본 정보"), LabelText("블럭 이름")]
    public string blockName;

    [BoxGroup("기본 정보"), LabelText("블럭 길이"), MinValue(1)]
    public int blockLength;


    //───────────────────────────────
    // 액션 설정
    //───────────────────────────────
    [BoxGroup("액션 설정"), LabelText("액션 조합"), ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true)]
    public ActionType[] actionTypes;

    [BoxGroup("액션 설정"), LabelText("이동 방향"), ListDrawerSettings(ShowFoldout = true, ShowIndexLabels = true)]
    [InfoBox("Move가 아닌 틱은 자동으로 None 처리됨", InfoMessageType.None)]
    public MoveDirection[] moveDirections;


    //───────────────────────────────
    // 전투 설정
    //───────────────────────────────
    [BoxGroup("전투 설정"), LabelText("공격 데미지"), MinValue(0)]
    public int attackDamage;


    // ───────────────────────────────────────────────
    //  프로퍼티 (Readonly - 인스펙터 숨김)
    // ───────────────────────────────────────────────
    [HideInInspector] public int BlockID => blockID;
    [HideInInspector] public string BlockName => blockName;
    [HideInInspector] public int BlockLength => blockLength;
    [HideInInspector] public ActionType[] ActionTypes => actionTypes;
    [HideInInspector] public MoveDirection[] MoveDirections => moveDirections;
    [HideInInspector] public int AttackDamage => attackDamage;

    // 기존 함수들은 그대로 유지 (숨김 처리 가능)
    #region Runtime Methods (Hidden in Inspector)
    
    // 정화 액션의 위치에 따른 정화량 계산
    [HideInInspector]
    public int CalCulate_CurePower(int index)
    {
        int result = 0;

        if(blockLength <= 2)
        {
            result = 1;
        }
        else
        {
            int midLeft = (blockLength - 1) / 2;
            int midRight = blockLength / 2;
            //int center = (blockLength - 1) / 2;
            //int distance = Mathf.Abs(index - center);


            //result = (center - distance) + 1;

            int distance = Mathf.Min(Mathf.Abs(index - midLeft), Mathf.Abs(index - midRight));
            result = (midLeft + 1 - distance);
        }

        return result;
    }

    [HideInInspector]
    public ActionType GetEffectAt(int tickIndex)
    {
        if (tickIndex < 0 || tickIndex >= actionTypes.Length)
            return ActionType.None;
        return actionTypes[tickIndex];
    }

    [HideInInspector]
    public bool HasAttack()
    {
        for (int i = 0; i < actionTypes.Length; i++)
            if (actionTypes[i] == ActionType.Attack)
                return true;
        return false;
    }

    [HideInInspector]
    public void ToggleMovingDirection(int tickIndex)
    {
        if (tickIndex < 0 || tickIndex >= moveDirections.Length) return;
        if (actionTypes[tickIndex] != ActionType.Move) return;

        moveDirections[tickIndex] = moveDirections[tickIndex] == MoveDirection.Left ?
                                    MoveDirection.Right : MoveDirection.Left;
    }

    [HideInInspector]
    public void InitiailizeMovingDirection()
    {
        for (int i = 0; i < moveDirections.Length; i++)
        {
            if (actionTypes[i] != ActionType.Move)
                moveDirections[i] = MoveDirection.None;
            else
                moveDirections[i] = MoveDirection.Right;
        }
    }

    [HideInInspector, ContextMenu("Auto Setup Directions")]
    public void Init()
    {
        if (moveDirections == null || moveDirections.Length != actionTypes.Length)
            moveDirections = new MoveDirection[actionTypes.Length];

        for (int i = 0; i < actionTypes.Length; i++)
        {
            moveDirections[i] = actionTypes[i] == ActionType.Move ?
                                (moveDirections[i] == MoveDirection.None ? MoveDirection.Right : moveDirections[i])
                                : MoveDirection.None;
        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"{blockName} : 이동 방향 자동 설정 완료!");
    }
    #endregion
}
