using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class BoardPanel : MonoBehaviour
{
    public static event System.Action OnBoardStatusChanged;

    [Header("패널 제어")]
    [SerializeField] private Button _closeButton;

    [Header("사이드 바 설정")]
    [SerializeField] private GameObject _dayButtonPrefab;
    [SerializeField] private Transform _sidebarArea;

    [Header("게시물 목록 생성")]
    [SerializeField] private Transform _scrollContent;
    [SerializeField] private GameObject _boardItemPrefab;

    private List<BoardButton> _spawnedButtons = new List<BoardButton>();
    private int _currentSelectedStageID = -1;

    private void Awake()
    {
        _closeButton.onClick.AddListener(ClosePanel);
    }

    private void OnEnable()
    {
        RefreshSidebar();
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
        OnBoardStatusChanged?.Invoke();
    }

    public void RefreshSidebar()
    {
        foreach (Transform child in _sidebarArea) Destroy(child.gameObject);
        _spawnedButtons.Clear();

        UserGameData currentUser = ServiceLocator.Instance.CurrentUser;
        if (currentUser == null) return;

        List<StageData> sortedStages = DataRepository.Instance.stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();

        StageData lastAvailableStage = null;

        foreach (StageData stage in sortedStages)
        {
            int index = sortedStages.IndexOf(stage);
            bool isUnlocked = (index == 0) || currentUser.IsStageCleared(sortedStages[index - 1].StageNumber);
            if (isUnlocked)
            {
                GameObject go = Instantiate(_dayButtonPrefab, _sidebarArea);
                BoardButton btn = go.GetComponent<BoardButton>();

                btn.Setup(stage, this);
                _spawnedButtons.Add(btn);

                lastAvailableStage = stage;
            }
            else break;

        }
        if (lastAvailableStage != null) SelectDay(lastAvailableStage);

    }

    public void SelectDay(StageData stage)
    {
        _currentSelectedStageID = stage.StageNumber;

        ServiceLocator.Instance.CurrentUser.SetBoardRead(_currentSelectedStageID);
        OnBoardStatusChanged?.Invoke();

        foreach (BoardButton btn in _spawnedButtons)
        {
            btn.UpdateVisual(btn.GetStageNumber() == _currentSelectedStageID);
        }
        RefreshContent(stage);
    }

    private void RefreshContent(StageData stage)
    {
        foreach (Transform child in _scrollContent) Destroy(child.gameObject);
        foreach (BoardEntry entry in stage.BoardEntries)
        {
            GameObject go = Instantiate(_boardItemPrefab, _scrollContent);
            go.GetComponent<BoardItem_UI>().Setup(entry);
        }
    }


}
