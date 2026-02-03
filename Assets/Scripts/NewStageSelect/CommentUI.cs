using UnityEngine;
using TMPro;
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
        string processedContent = data.Content;
        if (!string.IsNullOrEmpty(videoTitle) && !string.IsNullOrEmpty(processedContent))
        {
            processedContent = processedContent.Replace(titleTag, videoTitle);
        }
        contentText.text = processedContent;

        Sprite loadedSprite = null;
        if (!string.IsNullOrEmpty(data.AuthorProfilePath))
        {
            loadedSprite = Resources.Load<Sprite>(data.AuthorProfilePath);
        }
        authorProfile.sprite = (loadedSprite != null) ? loadedSprite : defaultProfileSprite;

        if (cocomentContainer != null)
        {
            if (data.Cocoments != null && data.Cocoments.Count > 0)
            {
                cocomentContainer.gameObject.SetActive(true);

                foreach (Transform child in cocomentContainer)
                {
                    if (Application.isPlaying) Destroy(child.gameObject);
                }

                foreach (var cocomentData in data.Cocoments)
                {
                    GameObject childObj = Instantiate(prefab, cocomentContainer);
                    childObj.GetComponent<CommentUI>().SetData(cocomentData, prefab, videoTitle);
                }
                LayoutRebuilder.ForceRebuildLayoutImmediate(cocomentContainer.GetComponent<RectTransform>());
            }
            else
            {
                cocomentContainer.gameObject.SetActive(false);
            }
        }
    }

}
