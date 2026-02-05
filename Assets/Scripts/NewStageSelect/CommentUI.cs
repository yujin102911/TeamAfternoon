using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using System.Text;

public class CommentUI : MonoBehaviour
{
    public Image authorProfile;
    public TextMeshProUGUI contentText;
    public Transform cocomentContainer;

    [Header("기본 설정")]
    public Sprite defaultProfileSprite;

    [Header("치환 설정")]
    [SerializeField] private string titleTag = "[ENTER_TITLE]";

    public void SetData(TwitCommentData data, GameObject prefab, string videoTitle)
    {
        FillUI(data.AuthorProfilePath, data.LocalizedContent, videoTitle);
        HandleChildren(data.Cocoments, prefab, videoTitle);
    }

    public void SetData(TwitReplyData data, GameObject prefab, string videoTitle)
    {
        FillUI(data.AuthorProfilePath, data.LocalizedContent, videoTitle);
        HandleChildren(data.Cococoments, prefab, videoTitle);
    }

    public void SetData(TwitFinalReplyData data, GameObject prefab, string videoTitle)
    {
        FillUI(data.AuthorProfilePath, data.LocalizedContent, videoTitle);
        if (cocomentContainer != null) cocomentContainer.gameObject.SetActive(false);
    }

    private void FillUI(string profilePath, LocalizedString localizedContent, string videoTitle)
    {
        string processedContent = localizedContent.GetLocalizedString();
        
        if (!string.IsNullOrEmpty(videoTitle) && !string.IsNullOrEmpty(processedContent))
        {
            processedContent = processedContent.Replace(titleTag, videoTitle);
        }

        processedContent = ReplaceEmojiToSpriteTag(processedContent);

        contentText.text = processedContent;

        Sprite loadedSprite = string.IsNullOrEmpty(profilePath) ? null : Resources.Load<Sprite>(profilePath);
        authorProfile.sprite = (loadedSprite != null) ? loadedSprite : defaultProfileSprite;
    }

    private void HandleChildren<T>(List<T> children, GameObject prefab, string videoTitle)
    {
        if (cocomentContainer == null) return;

        if (children != null && children.Count > 0)
        {
            cocomentContainer.gameObject.SetActive(true);
            foreach (Transform child in cocomentContainer) Destroy(child.gameObject);

            foreach (var childData in children)
            {
                GameObject childObj = Instantiate(prefab, cocomentContainer);
                var ui = childObj.GetComponent<CommentUI>();

                // 타입에 따라 다른 SetData 호출
                if (childData is TwitReplyData rData) ui.SetData(rData, prefab, videoTitle);
                else if (childData is TwitFinalReplyData fData) ui.SetData(fData, prefab, videoTitle);
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(cocomentContainer.GetComponent<RectTransform>());
        }
        else
        {
            cocomentContainer.gameObject.SetActive(false);
        }
    }

    // 이모지 태그 변환
    private string ReplaceEmojiToSpriteTag(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length + 32);

        for (int i = 0; i < input.Length;)
        {
            int cp0 = char.ConvertToUtf32(input, i);
            int len0 = char.IsSurrogatePair(input, i) ? 2 : 1;

            // VS16 단독은 치환하지 않음 (앞 문자와 결합 대상)
            if (cp0 == 0xFE0F)
            {
                sb.Append(input, i, len0);
                i += len0;
                continue;
            }

            int nextIndex = i + len0;
            bool hasVS16 = false;

            // 다음 코드포인트가 VS16이면 결합
            if (nextIndex < input.Length)
            {
                int cp1 = char.ConvertToUtf32(input, nextIndex);
                int len1 = char.IsSurrogatePair(input, nextIndex) ? 2 : 1;

                if (cp1 == 0xFE0F)
                {
                    hasVS16 = true;
                    // VS16은 1코드유닛(1 char)이라 len1은 항상 1이지만 안전하게 유지
                }
            }

            if (IsEmojiBase(cp0))
            {
                if (hasVS16)
                {
                    // "xxxx-fe0f"
                    sb.Append("<sprite=\"real_emojis\" name=\"");
                    sb.Append(cp0.ToString("x"));
                    sb.Append("-fe0f");
                    sb.Append("\">");

                    i = nextIndex + 1; // VS16은 char 1개
                }
                else
                {
                    sb.Append("<sprite=\"real_emojis\" name=\"");
                    sb.Append(cp0.ToString("x"));
                    sb.Append("\">");

                    i = nextIndex;
                }
            }
            else
            {
                // 일반 문자 그대로
                sb.Append(input, i, len0);
                i += len0;
            }
        }

        return sb.ToString();
    }

    private bool IsEmojiBase(int cp)
    {
        return
            (cp >= 0x1F300 && cp <= 0x1FAFF) || // 대부분 이모지
            (cp >= 0x2600 && cp <= 0x27BF) || // 기호 이모지(⚔, ♥ 등 포함)
            cp == 0x2764; // ❤
    }
}
