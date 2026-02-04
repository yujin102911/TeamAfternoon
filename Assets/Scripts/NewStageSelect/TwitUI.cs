using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TwitUI : MonoBehaviour
{
    [Header("UI 참조")]
    public TextMeshProUGUI authorNameText;
    public TextMeshProUGUI contentText;
    public Image authorProfileImage;
    public Image postImage;
    public GameObject postImageContainer;
    public Transform commentContainer;
    public TextMeshProUGUI retweetCountText;
    public TextMeshProUGUI likeCountText;

    [Header("반응 토글")]
    [SerializeField] private Toggle retweetToggle;
    [SerializeField] private Toggle likeToggle;

    [Header("기본 설정")]
    public Sprite defaultProfileSprite;

    [Header("치환 설정")]
    [SerializeField] private string titleTag = "[ENTER_TITLE]";

    private int currentTwitID;

    public void SetData(TwitItem data, GameObject commentPrefab)
    {
        currentTwitID = data.TwitID;
        authorNameText.text = $"{data.AuthorName}<size=20><color=#8e8e8e>•{data.UploadDay}일</size></color>\n<size=22><color=#5a5a5a>{data.AuthorID}</size></color>";
        
        string processedContent = data.Content;
        if (!string.IsNullOrEmpty(data.videoTitle))
        {
            processedContent = processedContent.Replace(titleTag, data.videoTitle);
        }
        contentText.text = processedContent;

        retweetToggle.onValueChanged.RemoveAllListeners();
        likeToggle.onValueChanged.RemoveAllListeners();

        retweetToggle.isOn = data.IsRetweeted;
        likeToggle.isOn = data.IsLiked;

        UpdateCounterUI(data.RetweetCount, data.LikeCount);

        if (postImageContainer != null)
        {
            bool hasPic = data.hasImage && !string.IsNullOrEmpty(data.ImagePath);
            postImageContainer.SetActive(hasPic);

            if (hasPic && postImage != null)
            {
                postImage.sprite = Resources.Load<Sprite>(data.ImagePath);
            }
        }

        if (authorProfileImage != null)
        {
            Sprite loadedSprite = null;

            if (!string.IsNullOrEmpty(data.AuthorProfilePath))
            {
                loadedSprite = Resources.Load<Sprite>(data.AuthorProfilePath);
            }

            authorProfileImage.sprite = (loadedSprite != null) ? loadedSprite : defaultProfileSprite;
        }

        // 댓글 생성
        if (commentContainer != null)
        {
            foreach (Transform child in commentContainer) Destroy(child.gameObject);

            foreach (var commentData in data.Comments)
            {
                GameObject cObj = Instantiate(commentPrefab, commentContainer);
                cObj.GetComponent<CommentUI>().SetData(commentData, commentPrefab, data.videoTitle);
            }
        }
        SetupToggleEvents();
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }

    private void UpdateCounterUI(int retweet, int like)
    {
        if (retweetCountText != null) retweetCountText.text = retweet.ToString();
        if (likeCountText != null) likeCountText.text = like.ToString();
    }

    private void SetupToggleEvents()
    {
        retweetToggle.onValueChanged.RemoveAllListeners();
        retweetToggle.onValueChanged.AddListener((isOn) =>
        {
            TwitSaveService.UpdateTwit(ServiceLocator.Instance.CurrentTwitData, currentTwitID, (twit) =>
            {
                twit.IsRetweeted = isOn;
                twit.RetweetCount += isOn ? 1 : -1;
                UpdateCounterUI(twit.RetweetCount, twit.LikeCount);
            });
        });

        likeToggle.onValueChanged.AddListener((isOn) =>
        {
            TwitSaveService.UpdateTwit(ServiceLocator.Instance.CurrentTwitData, currentTwitID, (twit) =>
            {
                twit.IsLiked = isOn;
                twit.LikeCount += isOn ? 1 : -1;
                UpdateCounterUI(twit.RetweetCount, twit.LikeCount);
                if (isOn)
                {
                    CheckAllLikedAchievement();
                }
            });
        });

    }
    private void CheckAllLikedAchievement()
    {
        ServiceLocator locator = ServiceLocator.Instance;
        if (locator.CurrentUser.Difficulty != Difficulty.Hard) return;

        bool allLiked = locator.CurrentTwitData.TwitDatas.All(tag => tag.IsLiked);
        if (allLiked)
        {
            SteamAchievementManager.Unlock("ACHIEVEMENT_ALL_LIKED");
        }
    }
}
