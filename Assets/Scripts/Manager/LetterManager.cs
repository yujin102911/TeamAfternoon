using TMPro;
using UnityEngine;

public class LetterManager : MonoBehaviour
{
    [Header(" Vertical Data Connection")]
    [SerializeField] private StageData _stageData;

    [SerializeField] private GameObject bookButton;
    [SerializeField] private GameObject notification;
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private GameObject letterButton;

    //[Tooltip("편지 내용이 표시될 텍스트")]
    //[SerializeField] private TextMeshProUGUI letterContentText;
    [Header("클리어 전 보여줄 의뢰서")]
    [SerializeField] private GameObject requestPanel;

    [Header("클리어 후 보여줄 패널")]
    [SerializeField] private GameObject thankPanel;

    public void SetStageData(StageData stageData)
    {
        _stageData = stageData;
    }

    public void PressLetterButton()
    {
        if (_stageData != null)
        {
            if (_stageData.IsCleared)
            {
                // 클리어 했다면 감사 편지
                if (thankPanel != null) thankPanel.SetActive(true);
                if (requestPanel != null) requestPanel.SetActive(false);
            }
            else
            {
                // 클리어 전이라면 의뢰 편지
                if (requestPanel != null) requestPanel.SetActive(true);
                if (thankPanel != null) thankPanel.SetActive(false);
            }
        }
        else
        {
            Debug.LogWarning("LetterManager에 StageData가 할당되지 않았습니다.");
            if (requestPanel != null) requestPanel.SetActive(true);
            if (thankPanel != null) thankPanel.SetActive(false);
        }
        letterPanel.SetActive(true);
        notification.SetActive(false);
        letterButton.SetActive(false);
    }

    public void PressXButton()
    {
        letterPanel.SetActive(false);
        bookButton.SetActive(true);
        letterButton.SetActive(true);
    }

}
