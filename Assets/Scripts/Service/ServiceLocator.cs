using System.Collections.Generic;
using UnityEngine;

public enum Difficulty { Easy, Hard }

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }
    public SceneService Scene { get; private set; }
    public UserGameData CurrentUser { get; private set; }
    public CursorService Cursor { get; private set; }


    [Header("시작 유저 데이터(난이도 별)")]
    [SerializeField] private UserGameData easyModeTemplate;
    [SerializeField] private UserGameData hardModeTemplate;

    [Header("테스트용 유저 데이터")]
    [SerializeField] private bool isTestMode = false;
    [SerializeField] private UserGameData testUserData;

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
        if (isTestMode)
        {
            CurrentUser = Instantiate(testUserData);
        }
    }


    /// <summary>
    /// 튜토리얼 전용 유저데이터를 생성하는 함수 (저장 X)
    /// </summary>
    public void CreateNewTutorial(Difficulty mode)
    {
        UserGameData template = (mode == Difficulty.Easy) ? easyModeTemplate : hardModeTemplate;
        CurrentUser = Instantiate(template);
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
        if (!SaveService.HasSaveData()) return false;

        CurrentUser = Instantiate(hardModeTemplate);
        SaveService.Load(CurrentUser);
        return true;
    }


}
