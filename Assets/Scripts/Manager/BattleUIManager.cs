using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 시작버튼을 관리하는 스크립트,,,, 이거에 넣을게 정말 시작 버튼밖에 없단말인가
/// </summary>
public class BattleUIManager : MonoBehaviour
{
    public static BattleUIManager Instance;

    [Header("스테이지 UI")]
    [SerializeField] private Button _startButton;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;

            battle.OnBattleInitialized += HandleBattleInitialized;
            //battle.UpdateCureGauage += HandleCureChanged;

            GameManager.Instance.OnGameStateChanged += RefreshStartButtonState;
            GameManager.Instance.OnBattleEnded += RefreshStartButtonState;
        }
        RefreshStartButtonState();
        //RefreshSectorSelectionPanel();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
        {
            BattleSystem battle = GameManager.Instance.BattleSystem;
            GameManager.Instance.OnGameStateChanged -= RefreshStartButtonState;
            battle.OnBattleInitialized -= HandleBattleInitialized;
            //battle.UpdateCureGauage -= HandleCureChanged;
        }
    }


    private void HandleBattleInitialized()
    {
        if (GameManager.Instance != null && GameManager.Instance.BattleSystem != null)
            InitializeUI(GameManager.Instance.BattleSystem);
    }

    public void InitializeUI(BattleSystem battle)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsExecutingRound)
        {
            Hide_startBtn();
        }
        else
        {
            RefreshStartButtonState();
        }
    }

    public void RefreshStartButtonState()
    {
        if (_startButton == null || GameManager.Instance == null) return;
        bool isRoundRunning = GameManager.Instance.IsExecutingRound;
        bool isSequencePlaying = GameManager.Instance.IsSequencePlaying;
        bool isSectorSelected = GameManager.Instance.IsSectorSelected;
        bool isGameOver = GameManager.Instance.IsBattleEnded;

        bool interactable = !isRoundRunning && isSectorSelected && !isGameOver && !isSequencePlaying;

        _startButton.interactable = interactable;
        if (GameManager.Instance.MapSystem != null)
        {
            GameManager.Instance.MapSystem.SetAllSectorsVisibility(interactable);
        }
    }

    private void RefreshStartButtonState(bool isVictory, int a, int b, int c, int d)
    {
        _startButton.interactable = false;
        //Hide_startBtn();
    }


    public void Show_startBtn()
    {
        _startButton.gameObject.SetActive(true);
    }

    public void Hide_startBtn()
    {
        _startButton.gameObject.SetActive(false);
    }
}
