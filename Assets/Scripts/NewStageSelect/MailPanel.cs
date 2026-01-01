using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// OutLook에 있는 온갖 버튼들을 관리
/// </summary>
public class MailPanel : MonoBehaviour
{
    public static event Action OnMailStatusChanged;

    [Header("패널 제어")]
    [SerializeField] private Button _closeButton;

    [Header("메일 목록 설정")]
    [SerializeField] private GameObject _mailButtonPrefab;
    [SerializeField] private Transform _contentArea;

    [Header("메일 본문 UI")]
    [SerializeField] private TextMeshProUGUI _senderText;
    [SerializeField] private TextMeshProUGUI _receiverText;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _mailContextText;
    [SerializeField] private ScrollRect _bodyScrollRect;

    private void Awake()
    {
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        ClearDisplay();
        GenerateStageList();
    }

    private void ClosePanel()
    {
        Debug.Log("닫기 버튼이 눌렸습니다.");
        gameObject.SetActive(false);
        OnMailStatusChanged?.Invoke();
    }

    private void ClearDisplay()
    {
        if (_senderText != null) _senderText.text = "";
        if (_receiverText != null) _receiverText.text = "";
        if (_titleText != null) _titleText.text = "";
        if (_mailContextText != null) _mailContextText.text = "";
        if (_bodyScrollRect != null)
            _bodyScrollRect.verticalNormalizedPosition = 1f;
    }
    public void GenerateStageList()
    {
        foreach (Transform child in _contentArea) Destroy(child.gameObject);

        List<StageData> sortedStages = DataRepository.Instance.stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();

        foreach (StageData stage in sortedStages)
        {
            bool isAvailable = false;
            int index = sortedStages.IndexOf(stage);

            if (index == 0)
            {
                isAvailable = true;
            }
            else
            {
                if (sortedStages[index - 1].IsCleared)
                {
                    isAvailable = true;
                }
            }
            if (isAvailable)
            {
                GameObject go = Instantiate(_mailButtonPrefab, _contentArea);
                StageButton mailBtn = go.GetComponent<StageButton>();
                mailBtn.Setup(stage, DisplayLetterContent);
            }
            else
            {
                break;
            }
        }
    }
    private void DisplayLetterContent(StageData data)
    {
        OnMailStatusChanged?.Invoke();
        if (_senderText != null) _senderText.text = data.Sender;
        if (_receiverText != null) _receiverText.text = data.Receiver;
        if (_titleText != null) _titleText.text = data.StageName;

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
