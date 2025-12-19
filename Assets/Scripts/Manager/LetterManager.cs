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

    [Tooltip("편지 내용이 표시될 텍스트")]
    [SerializeField] private TextMeshProUGUI letterContentText;

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
                letterContentText.text = _stageData.ThankLetter;
            }
            else
            {
                // 클리어 전이라면 의뢰 편지
                letterContentText.text = _stageData.RequestLetter;
            }
        }
        else
        {
            Debug.LogWarning("LetterManager에 StageData가 할당되지 않았습니다.");
            letterContentText.text = "편지 내용을 불러올 수 없습니다.";
        }
        letterPanel.SetActive(true);
        notification.SetActive(false);
        letterButton.SetActive(false);

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.UI_Click);
    }

    public void PressXButton()
    {
        letterPanel.SetActive(false);
        bookButton.SetActive(true);
        letterButton.SetActive(true);

        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.UI_Click);
    }

}
