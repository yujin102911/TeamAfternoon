using UnityEngine;

public enum Difficulty { Easy, Hard }

public class ServiceLocator : MonoBehaviour
{
    public static ServiceLocator Instance { get; private set; }
    public SceneService Scene { get; private set; }
    public UserGameData CurrentUser { get; private set; }

    [Header("시작 유저 데이터(난이도 별)")]
    [SerializeField] private UserGameData easyModeTemplate;
    [SerializeField] private UserGameData hardModeTemplate;

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
    }

    public void CreateNewGame(Difficulty mode)
    {
        UserGameData template = (mode == Difficulty.Easy) ? easyModeTemplate : hardModeTemplate;
        CurrentUser = Instantiate(template);

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
