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

        // 튜토리얼 연동을 위한 플래그
        bool isFirstMailInTutorial = true;
        DialogueManager dm = FindObjectOfType<DialogueManager>();

        foreach (StageData stage in sortedStages)
        {
            int index = sortedStages.IndexOf(stage);
            bool isStageAvailable = (index == 0) || sortedStages[index - 1].IsCleared;

            if (isStageAvailable)
            {
                foreach (MailContent mail in stage.Mails)
                {
                    bool canShowMail = false;
                    if (mail.unlockCondition == MailContent.MailUnlockCondition.Always)
                    {
                        canShowMail = true;
                    }
                    else if (mail.unlockCondition == MailContent.MailUnlockCondition.AfterClear && stage.IsCleared)
                    {
                        canShowMail = true;
                    }

                    if (canShowMail)
                    {
                        GameObject go = Instantiate(_mailButtonPrefab, _contentArea);
                        StageButton mailBtn = go.GetComponent<StageButton>();
                        mailBtn.Setup(stage, mail, DisplayLetterContent);

                        // [튜토리얼 연동] 
                        // 현재 튜토리얼 매니저가 활성화되어 있고, 이번이 첫 메일 생성이라면
                        if (dm != null && dm.bubbleObject.activeSelf && isFirstMailInTutorial)
                        {
                            // 1. 생성된 버튼을 하이라이트 (블로커 위로 올림)
                            dm.SetHighlight(go);

                            // 2. 이 버튼을 누르면 대사도 같이 넘어가도록 이벤트 추가
                            Button btn = go.GetComponent<Button>();
                            if (btn != null)
                            {
                                btn.onClick.AddListener(() => dm.DisplayNextStep());
                            }

                            // 3. 한 번만 등록하기 위해 플래그 해제
                            isFirstMailInTutorial = false;
                        }
                    }
                }
            }
            else break;
        }
    }

    private void DisplayLetterContent(StageData data, MailContent mail)
    {
        mail.isRead = true;
        OnMailStatusChanged?.Invoke();

        if (_senderText != null) _senderText.text = mail.sender;
        if (_receiverText != null) _receiverText.text = mail.receiver;
        if (_titleText != null) _titleText.text = mail.subject;

        string rawText = mail.body;

        string linkTag = $"<color=#5865F2><u><link=\"stage_enter:{data.StageNumber}\">";
        string formattedText = rawText.Replace($"[ENTER_LINK]", linkTag + $"던전{data.StageNumber}일차.mp4</link></u></color>");

        _mailContextText.text = formattedText;
        if (_bodyScrollRect != null)
        {
            _bodyScrollRect.verticalNormalizedPosition = 1f; // 1은 맨 위, 0은 맨 아래
        }
    }
}