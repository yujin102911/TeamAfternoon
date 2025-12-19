using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageInfo_Panel : MonoBehaviour
{
    [Header("UI 그룹")]
    [SerializeField] private GameObject _titlePageGroup; // 타이틀 페이지 레이아웃
    [SerializeField] private GameObject _storyPageGroup; // 스토리 페이지 레이아웃

    [Header("외부 UI")]
    [SerializeField] private GameObject _letterButton;

    [Header("타이틀 페이지 UI 요소")]
    [SerializeField] private TextMeshProUGUI _tpTitleLeft;
    [SerializeField] private TextMeshProUGUI _tpCredits;
    [SerializeField] private TextMeshProUGUI _tpTitleRight;

    [Header("타이틀 페이지 버튼")]
    [SerializeField] private Button _titleEnterBtn;
    [SerializeField] private Button _readContentBtn;

    [Header("스토리 페이지 UI 요소 - 왼쪽")]
    [SerializeField] private TextMeshProUGUI _leftChapter;
    [SerializeField] private TextMeshProUGUI _leftBodyTop;
    [SerializeField] private TextMeshProUGUI _leftBodyMiddle;

    [Header("스토리 페이지 UI 요소 - 오른쪽")]
    [SerializeField] private TextMeshProUGUI _rightChpater;
    [SerializeField] private TextMeshProUGUI _rightBodyTop;
    [SerializeField] private TextMeshProUGUI _rightBodyMiddle;

    [Header("스토리 페이지 버튼")]
    [SerializeField] private Button _storyEnterBtn;
    [SerializeField] private Button _prevBtn;
    [SerializeField] private Button _nextBtn;

    [Header("네비게이션")]
    [SerializeField] private Button _closeBtn;

    [Header("연출(오염효과)")]
    [SerializeField] private List<GameObject> _pollutionObjects;

    private StageData _currentData;
    private int _currentPageIndex = -1;

    void Start()
    {
        _prevBtn.onClick.AddListener(OnClick_Prev);
        _nextBtn.onClick.AddListener(OnClick_Next);
        _closeBtn.onClick.AddListener(Hide);
        _readContentBtn.onClick.AddListener(OnClick_Next);
    }

    public void Show(int id)
    {
        _currentData = DataRepository.Instance.GetStage(id);

        _currentPageIndex = -1;
        UpdatePageUI();

        this.gameObject.SetActive(true);
        _letterButton.SetActive(false);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        _letterButton.SetActive(true );
    }

    private void UpdatePageUI()
    {
        int maxIndex = _currentData.BookPages.Count;

        if (_currentPageIndex == -1)
        {
            _titlePageGroup.SetActive(true) ;
            _storyPageGroup.SetActive(false) ;

            _tpTitleLeft.text = _currentData.StageName;
            _tpCredits.text = $"{_currentData.BookCredit}";
            _tpTitleRight.text = _currentData.StageName;

            _readContentBtn.gameObject.SetActive(true) ;
            _titleEnterBtn.gameObject.SetActive(true) ;

            _prevBtn.gameObject.SetActive(false) ;
            _nextBtn.gameObject.SetActive(false);
            _storyEnterBtn.gameObject.SetActive(false) ;
        }
        else
        {
            _titlePageGroup.SetActive(false ) ;
            _storyPageGroup.SetActive(true);

            if (_currentPageIndex < _currentData.BookPages.Count)
            {
                SetPageContent(_currentData.BookPages[_currentPageIndex], _leftChapter, _leftBodyTop, _leftBodyMiddle);
            }
            else
            {
                ClearPage(_leftChapter, _leftBodyTop, _leftBodyMiddle);
            }
            if (_currentPageIndex + 1 < _currentData.BookPages.Count)
            {
                SetPageContent(_currentData.BookPages[_currentPageIndex + 1], _rightChpater, _rightBodyTop, _rightBodyMiddle);
            }
            else
            {
                ClearPage(_rightChpater, _rightBodyTop, _rightBodyMiddle);
            }

            _readContentBtn.gameObject .SetActive(false) ;
            _titleEnterBtn.gameObject.SetActive(false) ;

            _storyEnterBtn.gameObject.SetActive(true) ;
            _prevBtn.gameObject.SetActive(true) ;

            bool hasNext = (_currentPageIndex + 2) < maxIndex ;
            _nextBtn.gameObject.SetActive(hasNext) ;
        }
        UpdatePollution();
    }

    private void UpdatePollution()
    {
        foreach (GameObject obj in _pollutionObjects) 
            if (obj != null) obj.SetActive(false);

        if (_currentData.IsCleared) return;
        int targetPollutionID = -1;

        if (_currentPageIndex == -1)
            targetPollutionID = _currentData.TitlePollutionID;
        else
        {
            int spreadIndex = _currentPageIndex / 2;
            if (spreadIndex < _currentData.SpreadPollutionIDs.Count)
                targetPollutionID = _currentData.SpreadPollutionIDs[spreadIndex];
        }
        if (targetPollutionID >= 0 && targetPollutionID < _pollutionObjects.Count)
        {
            if (_pollutionObjects[targetPollutionID] != null)
                _pollutionObjects[targetPollutionID].SetActive(true);
        }
    }
    private void OnClick_Next()
    {
        if (_currentPageIndex == -1)
            _currentPageIndex = 0;
        else
            _currentPageIndex += 2;
        UpdatePageUI();
    }
    private void OnClick_Prev()
    {
        if (_currentPageIndex == 0)
            _currentPageIndex = -1;
        else
            _currentPageIndex -= 2;
        UpdatePageUI();
    }
    private void SetPageContent(BookPage page, TextMeshProUGUI chapterTxt, TextMeshProUGUI bodyTop, TextMeshProUGUI bodyMiddle)
    {
        if (page.IsChapterStart)
        {
            chapterTxt.gameObject.SetActive(true);
            chapterTxt.text = page.ChapterTitle;

            bodyTop.gameObject.SetActive(false);
            bodyMiddle.gameObject.SetActive(true);
            bodyMiddle.text = page.BodyText;
        }
        else
        {
            chapterTxt.gameObject.SetActive(false);

            bodyTop.gameObject.SetActive(true);
            bodyMiddle.gameObject.SetActive(false);
            bodyTop.text = page.BodyText;
        }
    }
    private void ClearPage(TextMeshProUGUI chapterTxt, TextMeshProUGUI bodyTop, TextMeshProUGUI bodyMiddle)
    {
        chapterTxt.text = "";
        bodyTop.text = "";
        bodyMiddle.text = "";
        bodyTop.gameObject.SetActive(false);
        bodyMiddle.gameObject.SetActive(false);
    }
}
