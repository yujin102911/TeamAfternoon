using Sirenix.OdinInspector;
using Steamworks;
using System.Collections.Generic;
using UnityEngine;


public enum Difficulty { Easy, Hard }

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }
    public SceneService Scene { get; private set; }
    public CursorService Cursor { get; private set; }

    public UserGameData CurrentUser { get; private set; }
    public DataRepository CurrentRepository { get; private set; }
    public GlobalSaveDTO GlobalData { get; private set; }
    public TwitData CurrentTwitData { get; private set; }

    public System.Action OnGlobalDataChanged;

    [Header("난이도별 DB")]
    [SerializeField] private DataRepository easyRepository;
    [SerializeField] private DataRepository hardRepository;

    [Header("시작 유저 데이터(난이도 별)")]
    [SerializeField] private UserGameData easyModeTemplate;
    [SerializeField] private UserGameData hardModeTemplate;

    [Header("시작 트윗 데이터")]
    [SerializeField] private TwitData twitTemplate;

    [Header("테스트용 유저 데이터")]
    [SerializeField] private bool isTestMode = false;
    [SerializeField] private bool isHard = false;
    [SerializeField] private UserGameData testUserData;
    [SerializeField] private TwitData testTwitData;

    [Header("커서 애니메이션 설정")]
    [SerializeField] private List<CursorAnimation> cursorAnimations;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Scene = new SceneService();
        Cursor = new CursorService(this, cursorAnimations);
        GlobalData = GlobalSaveService.Load();
        if (!System.IO.File.Exists(GlobalSaveService.SavePath))
        {
            Debug.Log("[ServiceLocator] glabal.json이 없어 초기 파일을 생성합니다.");
            GlobalSaveService.Save(GlobalData);
        }
        if (testUserData != null)
        {
            if (isTestMode)
            {
                CurrentUser = Instantiate(testUserData);
                CurrentTwitData = Instantiate(testTwitData);
            }
        }
        if (easyRepository != null && hardRepository != null)
        {
            if (isTestMode)
            {
                if (isHard)
                {
                    CurrentRepository = hardRepository;
                }
                else
                {
                    CurrentRepository = easyRepository;
                }
            }
        }
    }


    /// <summary>
    /// 튜토리얼 전용 유저데이터를 생성하는 함수 (저장 X)
    /// </summary>
    public void CreateNewTutorial(Difficulty mode)
    {
        UserGameData template = (mode == Difficulty.Easy) ? easyModeTemplate : hardModeTemplate;
        CurrentUser = Instantiate(template);
        CurrentUser.Difficulty = mode;

        CurrentTwitData = Instantiate(this.twitTemplate);
        if (mode == Difficulty.Hard)
        {
            if (TwitSaveService.HasSaveData())
            {
                TwitSaveService.Load(CurrentTwitData);
                Debug.Log("[ServiceLocator] 하드 모드: 기존 트윗 데이터를 로드했습니다.");
            }
            else
            {
                Debug.Log("[ServiceLocator] 하드 모드: 세이브 데이터가 없어 기본 템플릿으로 시작합니다.");
            }
            TwitSaveService.SetVisibleForHardInit(CurrentTwitData);
            TwitSaveService.InitializeVideoTitles(CurrentTwitData);
            SetTutorialClear(true);

        }
        if (mode == Difficulty.Easy)
        {
            TwitSaveService.SetVisibleForEasyInit(CurrentTwitData);
            TwitSaveService.SetRandomReactionsAll(CurrentTwitData);
        }

    }

    public void SetDifficulty(Difficulty mode)
    {
        if (mode == Difficulty.Easy)
        {
            CurrentRepository = easyRepository;
            Debug.Log("이지 선택됨");
        }
        else
        {
            CurrentRepository = hardRepository;
            Debug.Log("하드 선택됨");
        }
    }
    
    /// <summary>
    /// 튜토리얼 클리어 여부를 저장하는 함수 (솔직히 굳이 필요없긴함)
    /// </summary>
    public void SetTutorialClear(bool clear)
    {
        CurrentUser.Is_Tutorial_Cleared = clear;
    }

    /// <summary>
    /// 현재의 UserData를 저장하는 함수
    /// </summary>
    public void SaveNowUserData()
    {
        SaveService.Save(CurrentUser);
    }

    public bool LoadGame()
    {
        if (!SaveService.CanContinue()) return false;

        CurrentUser = Instantiate(hardModeTemplate);
        CurrentTwitData = Instantiate(twitTemplate);
        SaveService.Load(CurrentUser);
        TwitSaveService.Load(CurrentTwitData);
        if (CurrentUser.Difficulty == Difficulty.Easy)
        {
            CurrentRepository = easyRepository;
        }
        else
        {
            CurrentRepository = hardRepository;
        }
        return true;
    }

    #region Reset Logic
    public void ResetDataToDefault(Difficulty mode)
    {
        try
        {
            SaveService.DeleteSave();
            if (mode == Difficulty.Easy) // 이지 - 하드의 트윗 데이터는 이어져야함
            {
                TwitSaveService.DeleteSave();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[ServiceLocator] 세이브 파일 삭제 실패 (무시하고 진행): {e.Message}");
        }
        SetDifficulty(mode);     // 데이터 레포지토리
        CreateNewTutorial(mode); // 유저데이터 + 트윗데이터 생성
        SaveNowUserData();       // 저장
        TwitSaveService.Save(CurrentTwitData);
    }

    [Button("게임 클리어 처리 버튼", ButtonSizes.Medium)]
    public void SetGameClear()
    {
        Debug.Log("[ServiceLocator] 게임 클리어 처리");
        GlobalData.IsGameCleared = true;
        GlobalSaveService.Save(GlobalData);

        OnGlobalDataChanged?.Invoke();
    }

    [Button("게임 클리어 여부 초기화 버튼", ButtonSizes.Medium)]
    public void SetGameInit()
    {
        Debug.Log("[ServiceLocator] 게임 클리어 여부 초기화");
        GlobalData.IsGameCleared = false;
        GlobalSaveService.Save(GlobalData);

        OnGlobalDataChanged?.Invoke();
    }
    #endregion

}
