using UnityEngine;
using UnityEngine.UI;

public class StageScene_Manager : MonoBehaviour
{
    [Header("전투 씬 이름")]
    public string BattleScene_Name = "BattleScene";
    [Header("덱빌딩 씬 이름")]
    public string DeckScene_Name = "";

    [Header("스테이지 책 버튼 배열")]
    [SerializeField]
    private Button[] _stageBookBtns;    // 스테이지 버튼 배열
    [Header("스테이지 버튼")]
    [SerializeField]
    private Button[] _toStageBtns;    // 스테이지 입장 버튼
    [Header("덱 빌딩 버튼")]
    [SerializeField]
    private Button _deckBtn;    // 덱 버튼

    private int selected_stage_id;

    void Start()
    {
        SetStage_btn();
        _deckBtn.onClick.AddListener(OnClick_DeckBtn);
        if (_toStageBtns != null)
        {
            foreach (Button btn in _toStageBtns)
            {
                if (btn != null)
                {
                    btn.onClick.AddListener(OnClick_StageBtn);
                }
            }
        }
    }

    private void SetStage_btn()
    {
        for(int i = 0; i < _stageBookBtns.Length; i++)
        {
            int index = i + 1; // 로컬 복사본 생성
            if (_stageBookBtns[i] != null)
            {
                _stageBookBtns[i].onClick.AddListener(() => Select_stage(index));

                var bookBtn = _stageBookBtns[i].gameObject.GetComponent<Stage_BookBtn>();
                if (bookBtn != null) bookBtn.Stage_ID = index;

                if (StageScene_UIManager.Instance != null)
                    _stageBookBtns[i].onClick.AddListener(() => StageScene_UIManager.Instance.BookClicked(index));

                if (DeckBuildingManager.Instance != null)
                    _stageBookBtns[i].onClick.AddListener(DeckBuildingManager.Instance.Off_Tags);
            }
        }
    }

    public void OnClick_StageBtn()
    {
        Debug.Log($"StageScene_Manager: OnClick_StageBtn - Stage {selected_stage_id}");
        if (selected_stage_id == 0)
        {
            Debug.LogWarning("선택된 스테이지가 없습니다.");
            return;
        }
        GameManager.SelectedStageID = selected_stage_id; // (스테이지 ID는 1부터 시작하므로 +1)
        // 씬 전환
        ServiceLocator.Instance.Scene.Load(BattleScene_Name);
    }

    public void OnClick_DeckBtn()
    {
        Debug.Log("StageScene_Manager: OnClick_DeckBtn - 덱 버튼 클릭됨");
        // 씬 전환
        ServiceLocator.Instance.Scene.Load(DeckScene_Name);
    }

    public void Select_stage(int id)
    {
        selected_stage_id = id;
    }
}
