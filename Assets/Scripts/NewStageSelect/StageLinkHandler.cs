using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class StageLinkHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string battleSceneName = "battleScene";

    private TextMeshProUGUI _tmpText;

    private void Awake() => _tmpText = GetComponent<TextMeshProUGUI>();

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(_tmpText, eventData.position, null);
        if (linkIndex != -1)
        {
            string linkID = _tmpText.textInfo.linkInfo[linkIndex].GetLinkID();

            if (linkID.StartsWith("stage_enter:"))
            {
                string stageNumStr = linkID.Split(':')[1];
                if (int.TryParse(stageNumStr, out int stageNum) )
                {
                    EnterStage(stageNum);
                }
            }
        }

    }
    private void EnterStage(int id)
    {
        Debug.Log($"[LinkHandler] 스테이지 {id}로 이동합니다.");
        GameManager.SelectedStageID = id;
        if (ServiceLocator.Instance != null && ServiceLocator.Instance.Scene != null)
        {
            ServiceLocator.Instance.Scene.Load(battleSceneName);
        }
    }

}
