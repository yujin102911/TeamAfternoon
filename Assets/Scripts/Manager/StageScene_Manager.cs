using UnityEngine;
using UnityEngine.UI;

public class StageScene_Manager : MonoBehaviour
{
    [Header("전투 씬 이름")]
    public string BattleScene_Name = "BattleScene";
    [Header("덱빌딩 씬 이름")]
    public string DeckScene_Name = "";

    [Header("스테이지 버튼 배열")]
    [SerializeField]
    private Button[] _stageBtns;    // 스테이지 버튼 배열
    [Header("덱 빌딩 버튼")]
    [SerializeField]
    private Button _deckBtn;    // 덱 버튼

    void Start()
    {
        SetStage_btn();
        _deckBtn.onClick.AddListener(OnClick_DeckBtn);
    }

    private void SetStage_btn()
    {
        for(int i = 0; i < _stageBtns.Length; i++)
        {
            int index = i; // 로컬 복사본 생성
            _stageBtns[i].onClick.AddListener(() => OnClick_StageBtn(index));
        }
    }

    public void OnClick_StageBtn(int stageIndex)
    {
        Debug.Log($"StageScene_Manager: OnClick_StageBtn - Stage {stageIndex} 버튼 클릭됨");
        // 씬 전환
        ServiceLocator.Instance.Scene.Load(BattleScene_Name);
    }

    public void OnClick_DeckBtn()
    {
        Debug.Log("StageScene_Manager: OnClick_DeckBtn - 덱 버튼 클릭됨");
        // 씬 전환
        ServiceLocator.Instance.Scene.Load(DeckScene_Name);
    }
}
