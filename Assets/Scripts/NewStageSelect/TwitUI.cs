using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Localization;

public class TwitUI : MonoBehaviour
{
    [System.Serializable]
    public class SpecialTextConfig
    {
        public int targetTwitID;
        public GameObject textBoxObject;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI subTitleText;
        public LocalizedString localizedSubTitle;
        public int dayValue;
    }

    [Header("제목 텍스트 설정")]
    [SerializeField] private List<SpecialTextConfig> specialTextConfigs;

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
        string authorName = data.LocalizedAuthorName.GetLocalizedString();
        if (data.Difficulty == Difficulty.Easy)
        {
            authorNameText.text = $"{authorName}<size=20><color=#8e8e8e>•Day{data.UploadDay}</size></color>\n<size=22><color=#5a5a5a>{data.AuthorID}</size></color>";
        }
        else
        {
            authorNameText.text = $"{authorName}<size=20><color=#8e8e8e>•Week{data.UploadDay}</size></color>\n<size=22><color=#5a5a5a>{data.AuthorID}</size></color>";
        }

        string rawContent = data.LocalizedContent.GetLocalizedString();
        if (!string.IsNullOrEmpty(data.videoTitle))
        {
            rawContent = rawContent.Replace(titleTag, data.videoTitle);
        }
        contentText.text = rawContent;

        UpdateSpecialTextBox(data.videoTitle);

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

    private void UpdateSpecialTextBox(string videoTitle)
    {
        if (specialTextConfigs == null) return;

        foreach (var config in specialTextConfigs)
        {
            if (config.textBoxObject == null) continue;

            bool isTarget = (currentTwitID == config.targetTwitID);
            config.textBoxObject.SetActive(isTarget);

            if (isTarget)
            {
                if (config.titleText != null)
                    config.titleText.text = videoTitle;

                if (config.subTitleText != null && !config.localizedSubTitle.IsEmpty)
                {
                    config.subTitleText.text = config.localizedSubTitle.GetLocalizedString(config.dayValue);
                }
            }
        }
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
