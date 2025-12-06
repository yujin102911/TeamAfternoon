using UnityEngine;

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

    //나중에 구현

    ///// <summary>
    ///// 카드가 타임라인에 배치될 때 호출
    ///// </summary>
    //public virtual void OnCardPlaced(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 카드가 타임라인에서 제거될 때 호출
    ///// </summary>
    //public virtual void OnCardRemoved(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 라운드 실행 중 이 카드가 현재 틱에서 활성될 때 호출
    ///// </summary>
    //public virtual void OnTick(GameManager gm, DeckManager dm, PlacedCard placed, int currentTick, EffectType effectThisTick) { }

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어 블록 내 1틱이 시작될 때 호출
    ///// </summary>
    //public virtual void OnBlockStart(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어있고 블록이 끝날 때 호출
    ///// </summary>
    //public virtual void OnBlockEnded(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어있고 1틱 시작할 때
    ///// </summary>
    //public virtual void OnRoundStart(GameManager gm, DeckManager dm, PlacedCard placed) { }

    ///// <summary>
    ///// 이 카드가 타임라인에 배치되어있고 마지막틱 끝나고
    ///// </summary>
    //public virtual void OnRoundEnded(GameManager gm, DeckManager dm, PlacedCard placed) { }

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
