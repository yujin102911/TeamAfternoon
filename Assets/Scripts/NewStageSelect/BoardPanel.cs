using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class BoardPanel : MonoBehaviour
{
    [Header("패널 제어")]
    [SerializeField] private Button _closeButton;

    [Header("사이드 바 설정")]
    [SerializeField] private GameObject _dayButtonPrefab;
    [SerializeField] private Transform _sidebarArea;

    [Header("게시물 목록 생성")]
    [SerializeField] private Transform _scrollContent;
    [SerializeField] private GameObject _boardItemPrefab;

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
        Debug.Log("게시판 닫기");
        gameObject.SetActive(false);
    }

    public void RefreshSidebar()
    {
        foreach (Transform child in _sidebarArea) Destroy(child.gameObject);

        List<StageData> sortedStages = DataRepository.Instance.stageDatas.Values
            .OrderBy(s => s.StageNumber)
            .ToList();

        StageData lastAvailableStage = null;

        foreach (StageData stage in sortedStages)
        {
            int index = sortedStages.IndexOf(stage);
            bool isUnlocked = (index == 0) || sortedStages[index - 1].IsCleared;
            if (isUnlocked)
            {
                GameObject go = Instantiate(_dayButtonPrefab, _sidebarArea);
                go.GetComponent<BoardButton>().Setup(stage, this);
                lastAvailableStage = stage;
            }
            else break;

        }
        if (lastAvailableStage != null) SelectDay(lastAvailableStage);

    }

    public void SelectDay(StageData stage)
    {
        foreach (Transform child in _scrollContent) Destroy(child.gameObject);

        foreach (BoardEntry entry in stage.BoardEntries)
        {
            GameObject go = Instantiate(_boardItemPrefab, _scrollContent);
            go.GetComponent<BoardItem_UI>().Setup(entry);
        }
    }
    

}
