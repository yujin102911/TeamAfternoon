using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RenderPipelineManager : MonoBehaviour
{
    public static RenderPipelineManager Instance;

    [SerializeField] private UniversalRendererData lowRenderer;
    [SerializeField] private UniversalRendererData highRenderer;

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
        bool high = PlayerPrefs.GetInt("RP_MODE", 0) == 1;
        ApplyPipeline(high);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool high = PlayerPrefs.GetInt("RP_MODE", 0) == 1;
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

        camData.SetRenderer(high ? 1 : 0);
    }
}
