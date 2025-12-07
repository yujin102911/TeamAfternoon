using UnityEngine;

/// <summary>
/// 키워드 베이스 클래스 - 상속받아서 구체적인 효과 구현
/// </summary>
public class KeywordData : ScriptableObject
{
    [Header("키워드 아이디")]
    [SerializeField]
    private int _keywordID;

    [Header("키워드 이름")]
    [SerializeField]
    private string _keywordName;

    [Header("키워드 설명")]
    [TextArea(3, 10)]
    [SerializeField]
    private string _keywordDescription;

    public int KeywordID => _keywordID;
    public string KeywordName => _keywordName;
    public string KeywordDescription => _keywordDescription;


    ///// <summary>
    ///// 카드가 타임라인에 배치될 때 호출
    ///// </summary>
    //public virtual void OnCardPlaced(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 카드가 타임라인에서 제거될 때 호출
    ///// </summary>
    //public virtual void OnCardRemoved(GameManager gm, DeckManager dm, PlacedCard placed) { }

    /// <summary>
    /// 블록이 활성화된 상태에서 매 틱마다 호출
    /// </summary>
    public virtual void OnTick(PlacedBlock placed, int currentTick, ActionType actionThisBlock) { }

    /// <summary>
    /// 이 카드가 타임라인에 배치되어 블록 내 첫틱이 시작될 때 호출
    /// </summary>
    public virtual void OnBlockStart(PlacedBlock placed, int currentTick) { }

    /// <summary>
    /// 이 카드가 타임라인에 배치되어있고 블록이 끝날 때 호출
    /// </summary>
    public virtual void OnBlockEnded(PlacedBlock placed, int currentTick) { }

    /// <summary>
    /// 이 카드가 타임라인에 배치되어있고 1틱 시작할 때
    /// </summary>
    public virtual void OnRoundStart(PlacedBlock placed) { }

    /// <summary>
    /// 이 카드가 타임라인에 배치되어있고 마지막틱 끝나고
    /// </summary>
    public virtual void OnRoundEnded(PlacedBlock placed) { }

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어 있고 블록이 시작할 때 호출(코루틴 전용 Virtual)
    ///// </summary>
    //public virtual IEnumerator OnBlockStartCoroutine(GameManager gm, DeckManager dm, PlacedCard placed)
    //{
    //    yield return null;
    //}

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어 있고 블록이 끝날 때 호출(코루틴 전용 Virtual)
    ///// </summary>
    //public virtual IEnumerator OnBlockEndedCoroutine(GameManager gm, DeckManager dm, PlacedCard placed)
    //{
    //    yield return null;
    //}
}
