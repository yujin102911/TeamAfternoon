using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Localization;
using Sirenix.OdinInspector;


[CreateAssetMenu(fileName = "New Twit Data", menuName = "Data/TwitData")]
public class TwitData : ScriptableObject
{
    [Header("트윗 정보")]
    [ListDrawerSettings(ListElementLabelName = "TwitID")]
    [Searchable]
    public List<TwitItem> TwitDatas = new List<TwitItem>();
}

[System.Serializable]
public class TwitItem
{
    [Header("트윗 고유 아이디(아이디가 작은 것부터 아래에 나옴)")]
    public int TwitID;

    [Header("작성자 정보")]
    public LocalizedString LocalizedAuthorName;
    public string AuthorID;
    public string AuthorProfilePath;

    [Header("게시물 정보")]
    public Difficulty Difficulty;
    public int UploadDay;
    public LocalizedString LocalizedContent;
    public bool IsVisible;
    public bool hasImage;
    public string ImagePath;
    public LocalizedString LocalizedVideoTitle;
    public string videoTitle;

    [Header("댓글")]
    public List<TwitCommentData> Comments = new List<TwitCommentData>();

    [Header("게시물 반응")]
    public int ViewCount;
    public int RetweetCount;
    public int LikeCount;

    [Header("최대 최소")]
    public int MaxRetweet;
    public int MinRetweet;
    public int MaxLike;
    public int MinLike;

    [Header("게시물 반응 상태")]
    public bool IsLiked;
    public bool IsRetweeted;

}

[System.Serializable]
public class TwitCommentData
{
    [Header("댓글 작성자 정보")]
    public string AuthorProfilePath;

    [Header("댓글 내용")]
    public LocalizedString LocalizedContent;

    [Header("대댓글 내용")]
    public List<TwitReplyData> Cocoments = new List<TwitReplyData>();

}

[System.Serializable]
public class TwitReplyData
{
    public string AuthorProfilePath;
    public LocalizedString LocalizedContent;
    [Header("대대댓글 내용")]
    public List<TwitFinalReplyData> Cococoments = new List<TwitFinalReplyData>();
}

[System.Serializable]
public class TwitFinalReplyData
{
    public string AuthorProfilePath;
    public LocalizedString LocalizedContent;
}