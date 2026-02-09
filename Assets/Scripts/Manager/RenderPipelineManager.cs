using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class RenderPipelineManager : MonoBehaviour
{
    public static RenderPipelineManager Instance;

    // ✅ URP Pipeline Asset 2개
    [SerializeField] private UniversalRenderPipelineAsset lowPipeline;
    [SerializeField] private UniversalRenderPipelineAsset highPipeline;

    private const string RP_KEY = "RP_MODE"; // 0=low, 1=high

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
        ApplyFromPrefs();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplyFromPrefs();
    }

    private void ApplyFromPrefs()
    {
        bool high = PlayerPrefs.GetInt(RP_KEY, 1) == 1;
        ApplyPipelineAsset(high);
    }

    public static void ApplyPipelineAsset(bool high)
    {
        var mgr = Instance;
        if (mgr == null) return;

        var asset = high ? mgr.highPipeline : mgr.lowPipeline;
        if (asset == null) return;

        // ✅ 1) 기본 파이프라인(모든 Quality 공통)
        GraphicsSettings.defaultRenderPipeline = asset;

        // ✅ 2) 현재 Quality 레벨 파이프라인도 같이 맞춰주는 게 안전
        // (프로젝트가 Quality마다 파이프라인을 따로 쓰는 경우를 대비)
        QualitySettings.renderPipeline = asset;

        // 선택: 즉시 반영을 강제하고 싶다면 한 프레임 대기 후 카메라 리셋 같은 걸 할 수도 있음.
        // 일반적으로는 위 두 줄이면 충분.
    }
}

