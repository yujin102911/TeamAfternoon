using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RenderPipelineManager : MonoBehaviour
{
    public static RenderPipelineManager Instance;

    [SerializeField] private UniversalRendererData lowRenderer;
    [SerializeField] private UniversalRendererData highRenderer;

    private const string RP_KEY = "RP_MODE"; // 0=low, 1=high
    private int _currentRendererIndex = 0;

    public int GetCurrentRendererIndex() => _currentRendererIndex;
    public bool IsHigh() => _currentRendererIndex == 1;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        bool high = PlayerPrefs.GetInt(RP_KEY, 0) == 1;
        ApplyPipeline(high);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool high = PlayerPrefs.GetInt(RP_KEY, 0) == 1;
        ApplyPipeline(high);
    }

    public static void ApplyPipeline(bool high)
    {
        var manager = Instance;
        if (manager == null) return;

        var camera = Camera.main;
        if (camera == null) return;

        var camData = camera.GetComponent<UniversalAdditionalCameraData>();
        if (camData == null) return;

        int index = high ? 1 : 0;

        camData.SetRenderer(index);

        manager._currentRendererIndex = index;
        PlayerPrefs.SetInt(RP_KEY, index == 1 ? 1 : 0);
    }

    public int GetCurrentRender()
    {
        // 1) 캐시값 우선 (가장 확실)
        return _currentRendererIndex;
    }
}
