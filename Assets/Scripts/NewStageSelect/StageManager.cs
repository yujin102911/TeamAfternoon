using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageManager : MonoBehaviour
{
    [SerializeField] private GameObject _mailButtonPrefab;
    [SerializeField] private Transform _contentArea;
    [SerializeField] private TextMeshProUGUI _mailContextText;
    [SerializeField] private ScrollRect _bodyScrollRect;

    private void Start()
    {
        GenerateStageList();
    }

    public void GenerateStageList()
    {
        foreach (Transform child in _contentArea) Destroy(child.gameObject);
        var allStages = DataRepository.Instance.stageDatas;
        foreach (var stage in allStages.Values)
        {
            GameObject go = Instantiate(_mailButtonPrefab, _contentArea);
            StageButton mailBtn = go.GetComponent<StageButton>();

            mailBtn.Setup(stage, DisplayLetterContent);
        }
    }

    private void DisplayLetterContent(StageData data)
    {
        string rawText = data.RequestLetter;

        string linkTag = $"<color=#5865F2><u><link=\"stage_enter:{data.StageNumber}\">";
        string formattedText = rawText.Replace($"[ENTER_LINK]", linkTag + $"던전{data.StageNumber}일차.mp4</link></u></color>");

        _mailContextText.text = formattedText;
        if (_bodyScrollRect != null)
        {
            _bodyScrollRect.verticalNormalizedPosition = 1f; // 1은 맨 위, 0은 맨 아래
        }
    }
    

}
