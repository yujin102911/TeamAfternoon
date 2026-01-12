using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

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

        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return;

        List<StageData> sortedStages = DataRepository.Instance.stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();

        foreach (StageData stage in sortedStages)
        {
            int index = sortedStages.IndexOf(stage);
            bool isStageAvailable = (index == 0) || currentUser.IsStageCleared(sortedStages[index - 1].StageNumber);

            if (isStageAvailable)
            {
                for (int m = 0; m < stage.Mails.Count; m++)
                {
                    MailContent mail = stage.Mails[m];
                    bool canShowMail = false;

                    if (mail.unlockCondition == MailContent.MailUnlockCondition.Always)
                        canShowMail = true;
                    else if (mail.unlockCondition == MailContent.MailUnlockCondition.AfterClear && currentUser.IsStageCleared(stage.StageNumber))
                        canShowMail = true;

                    if (canShowMail)
                    {
                        GameObject go = Instantiate(_mailButtonPrefab, _contentArea);
                        go.transform.SetAsFirstSibling();

                        go.name = $"Mail_Stage{stage.StageNumber}_Index{m}";

                        StageButton mailBtn = go.GetComponent<StageButton>();
                        mailBtn.Setup(stage, m, mail, DisplayLetterContent);
                    }
                }
            }
            else break;
        }
    }
    private void DisplayLetterContent(StageData data, int mailIndex, MailContent mail)
    {
        ServiceLocator.Instance.CurrentUser.SetMailRead(data.StageNumber, mailIndex);
        OnMailStatusChanged?.Invoke();

        string GetSafeString(LocalizedString locStr)
        {
            if (locStr == null || locStr.IsEmpty) return "";

            try
            {
                return locStr.GetLocalizedString();
            }
            catch
            {
                return ""; // 예기치 못한 에러 발생 시 빈 문자열 반환
            }
        }

        if (_senderText != null) _senderText.text = mail.sender.GetLocalizedString(); 
        if (_receiverText != null) _receiverText.text = mail.receiver.GetLocalizedString();
        if (_titleText != null) _titleText.text = mail.subject.GetLocalizedString();

        string attachedFileName = GetSafeString(mail.attached);

        if (string.IsNullOrEmpty(attachedFileName))
        {
            attachedFileName = $"Dungeon_{data.StageNumber}_Day.mp4";
        }

        string linkTag = $"<color=#5865F2><u><link=\"stage_enter:{data.StageNumber}\">";
        string bodyText = GetSafeString(mail.body);

        string formattedText = bodyText.Replace("[ENTER_LINK]", $"{linkTag}{attachedFileName}</link></u></color>");

        _mailContextText.text = formattedText;
        if (_bodyScrollRect != null)
        {
            _bodyScrollRect.verticalNormalizedPosition = 1f; // 1은 맨 위, 0은 맨 아래
        }
        if (NailController.Instance != null)
        {
            string signal = $"Mail_Read_{data.StageNumber}_{mailIndex}";
            NailController.Instance.CompleteStepBySignal(signal);
        }
    }
}
