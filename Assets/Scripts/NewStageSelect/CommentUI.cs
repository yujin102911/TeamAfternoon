using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        FillUI(data.AuthorProfilePath, data.Content, videoTitle);
        HandleChildren(data.Cocoments, prefab, videoTitle);
    }

    public void SetData(TwitReplyData data, GameObject prefab, string videoTitle)
    {
        FillUI(data.AuthorProfilePath, data.Content, videoTitle);
        HandleChildren(data.Cococoments, prefab, videoTitle);
    }

    public void SetData(TwitFinalReplyData data, GameObject prefab, string videoTitle)
    {
        FillUI(data.AuthorProfilePath, data.Content, videoTitle);
        if (cocomentContainer != null) cocomentContainer.gameObject.SetActive(false);
    }

    private void FillUI(string profilePath, string content, string videoTitle)
    {
        string processedContent = content;
        if (!string.IsNullOrEmpty(videoTitle) && !string.IsNullOrEmpty(processedContent))
        {
            processedContent = processedContent.Replace(titleTag, videoTitle);
        }
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

}
