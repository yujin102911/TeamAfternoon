using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
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
        string authorName = "Unknown";
        if (!data.LocalizedAuthorName.IsEmpty &&
        data.LocalizedAuthorName.TableReference.ReferenceType != UnityEngine.Localization.Tables.TableReference.Type.Empty)
        {
            authorName = data.LocalizedAuthorName.GetLocalizedString();
        }
        else
        {
            Debug.LogWarning($"[TwitUI] ID {data.TwitID}의 LocalizedAuthorName 설정이 누락되었습니다.");
        }
        if (data.Difficulty == Difficulty.Easy)
        {
            authorNameText.text = $"{authorName}<size=20><color=#8e8e8e>•Day{data.UploadDay}</size></color>\n<size=22><color=#5a5a5a>{data.AuthorID}</size></color>";
        }
        else
        {
            authorNameText.text = $"{authorName}<size=20><color=#8e8e8e>•Week{data.UploadDay}</size></color>\n<size=22><color=#5a5a5a>{data.AuthorID}</size></color>";
        }

        string rawContent = data.LocalizedContent.GetLocalizedString();
        if (!data.LocalizedContent.IsEmpty &&
        data.LocalizedContent.TableReference.ReferenceType != UnityEngine.Localization.Tables.TableReference.Type.Empty)
        {
            rawContent = data.LocalizedContent.GetLocalizedString();
        }
        else
        {
            Debug.LogWarning($"[TwitUI] ID {data.TwitID}의 LocalizedContent 설정이 누락되었습니다.");
        }

        rawContent = ReplaceEmojiToSpriteTag(rawContent);

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
