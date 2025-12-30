using UnityEngine;
using Sirenix.OdinInspector;

public enum ActionType
{
    None,
    Attack,
    Move,        // 상하좌우 이동
    Jump,        // 대각선 4방향 이동
    Cure,
    Guard,       // 공격 막기 (이 행동 실행 때 공격 있으면 데미지 0)
    Bow_start,
    Bow_middle,
    Bow_end,
    Bow_single,
}

public enum MoveDirection
{
    None,
    Left,      // 좌로 이동(-열)
    Right,     // 우로 이동(+열)
    Front,     // 앞으로 이동(+1)
    Back,      // 뒤로 이동(-1)
    DiagonalLu, // 대각선 왼쪽 위
    DiagonalRu, // 대각선 오른쪽 위
    DiagonalLd, // 대각선 왼쪽 아래
    DiagonalRd, // 대각선 오른쪽 아래
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


    [HideInInspector, ContextMenu("Auto Setup Directions")]
    public void Init()
    {
        if (moveDirections == null || moveDirections.Length != actionTypes.Length)
            moveDirections = new MoveDirection[actionTypes.Length];

        for (int i = 0; i < actionTypes.Length; i++)
        {
            ActionType action = actionTypes[i];
            if (action == ActionType.Move)
                moveDirections[i] = (moveDirections[i] == MoveDirection.None) ? MoveDirection.Front : moveDirections[i];
            else if (action == ActionType.Jump)
                moveDirections[i] = (moveDirections[i] == MoveDirection.None) ? MoveDirection.DiagonalLu : moveDirections[i];
            else
                moveDirections[i] = MoveDirection.None;

        }
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
        Debug.Log($"{blockName} : 이동 방향 자동 설정 완료!");
    }
    #endregion
}
