using UnityEngine;

/// <summary>
/// Perlin Noise 텍스처 생성기
/// 
/// 주요 책임:
/// - Dissolve 효과에 사용할 Noise 텍스처를 런타임 또는 에디터에서 생성
/// - Perlin Noise 알고리즘을 사용한 자연스러운 패턴 생성
/// - Inspector에서 파라미터 조정 가능
/// 
/// 사용 방법:
/// 1. 빈 GameObject에 이 스크립트 추가
/// 2. Inspector에서 파라미터 조정
/// 3. [Generate Noise Texture] 버튼 클릭 (Context Menu)
/// 4. 생성된 텍스처를 Material의 Noise Texture 슬롯에 할당
/// 
/// 실무 팁:
/// - 게임 빌드 시 매번 생성하지 말고, 에디터에서 생성 후 파일로 저장 권장
/// - 텍스처 해상도는 256x256이면 충분 (메모리 절약)
/// - 다양한 Scale 값으로 테스트해서 마음에 드는 패턴 찾기
/// </summary>
public class NoiseTextureGenerator : MonoBehaviour
{
    [Header("Texture Settings")]
    [SerializeField]
    [Tooltip("생성할 텍스처 크기 (정사각형)")]
    [Range(64, 512)]
    private int textureSize = 256;

    [SerializeField]
    [Tooltip("Noise 스케일 (값이 클수록 패턴이 커짐)")]
    [Range(1f, 50f)]
    private float noiseScale = 20f;

    [SerializeField]
    [Tooltip("Octave 수 (디테일 레벨, 값이 클수록 복잡함)")]
    [Range(1, 8)]
    private int octaves = 4;

    [SerializeField]
    [Tooltip("Persistence (각 Octave의 영향력 감소율)")]
    [Range(0.1f, 1f)]
    private float persistence = 0.5f;

    [SerializeField]
    [Tooltip("Lacunarity (각 Octave의 주파수 증가율)")]
    [Range(1f, 4f)]
    private float lacunarity = 2f;

    [SerializeField]
    [Tooltip("랜덤 시드 (다른 패턴을 원할 때 변경)")]
    private int seed = 0;

    [Header("Output")]
    [SerializeField]
    [Tooltip("생성된 Noise 텍스처 (자동 할당됨)")]
    private Texture2D generatedTexture;

    [Header("Advanced Settings")]
    [SerializeField]
    [Tooltip("대비 강화 (0 = 원본, 1 = 최대 대비)")]
    [Range(0f, 1f)]
    private float contrast = 0.3f;

    [SerializeField]
    [Tooltip("밝기 조정 (-1 = 어둡게, 0 = 원본, 1 = 밝게)")]
    [Range(-1f, 1f)]
    private float brightness = 0f;

    [SerializeField]
    [Tooltip("생성 시 자동으로 Material에 적용 (선택사항)")]
    private Material targetMaterial;

    #region Public Methods

    /// <summary>
    /// Noise 텍스처를 생성합니다.
    /// Inspector의 Context Menu 또는 코드에서 호출 가능합니다.
    /// </summary>
    [ContextMenu("Generate Noise Texture")]
    public void GenerateNoiseTexture()
    {
        // 기존 텍스처 정리
        if (generatedTexture != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedTexture);
            }
            else
            {
                DestroyImmediate(generatedTexture);
            }
        }

        // 새 텍스처 생성
        generatedTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        generatedTexture.name = "GeneratedNoiseTexture";

        // Noise 데이터 생성 및 텍스처에 적용
        GenerateNoiseData();

        // 텍스처 설정
        // Unity 6에서는 FilterMode.Linear 사용 (Bilinear는 deprecated)
        generatedTexture.filterMode = UnityEngine.FilterMode.Bilinear;
        generatedTexture.wrapMode = TextureWrapMode.Repeat;
        generatedTexture.Apply();

        // Material에 자동 적용 (설정된 경우)
        if (targetMaterial != null)
        {
            targetMaterial.SetTexture("_NoiseTex", generatedTexture);
            Debug.Log($"[NoiseTextureGenerator] Material에 텍스처 자동 적용: {targetMaterial.name}");
        }

        Debug.Log($"[NoiseTextureGenerator] Noise 텍스처 생성 완료: {textureSize}x{textureSize}");
    }

    /// <summary>
    /// 생성된 텍스처를 반환합니다.
    /// </summary>
    public Texture2D GetGeneratedTexture()
    {
        return generatedTexture;
    }

    /// <summary>
    /// 특정 Material에 생성된 텍스처를 적용합니다.
    /// </summary>
    public void ApplyToMaterial(Material material, string propertyName = "_NoiseTex")
    {
        if (generatedTexture == null)
        {
            Debug.LogWarning("[NoiseTextureGenerator] 텍스처가 생성되지 않았습니다. GenerateNoiseTexture()를 먼저 호출하세요.");
            return;
        }

        if (material == null)
        {
            Debug.LogError("[NoiseTextureGenerator] Material이 null입니다.");
            return;
        }

        material.SetTexture(propertyName, generatedTexture);
        Debug.Log($"[NoiseTextureGenerator] Material에 적용 완료: {material.name}");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Perlin Noise 알고리즘을 사용하여 텍스처 데이터를 생성합니다.
    /// 
    /// 알고리즘:
    /// - Multi-Octave Perlin Noise (Fractal Brownian Motion)
    /// - 여러 주파수의 Noise를 합성하여 자연스러운 패턴 생성
    /// - Octave가 높을수록 디테일이 풍부해짐
    /// 
    /// 성능:
    /// - 256x256 텍스처 생성에 약 10~30ms 소요
    /// - 런타임보다는 에디터에서 미리 생성 권장
    /// </summary>
    private void GenerateNoiseData()
    {
        // 랜덤 오프셋 생성 (시드 기반)
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000);
            float offsetY = prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // 텍스처의 각 픽셀에 대해 Noise 값 계산
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float noiseValue = CalculatePerlinNoise(x, y, octaveOffsets);

                // 대비 및 밝기 조정
                noiseValue = AdjustContrast(noiseValue, contrast);
                noiseValue = Mathf.Clamp01(noiseValue + brightness);

                // 그레이스케일 색상 적용
                Color pixelColor = new Color(noiseValue, noiseValue, noiseValue, 1f);
                generatedTexture.SetPixel(x, y, pixelColor);
            }
        }
    }

    /// <summary>
    /// Multi-Octave Perlin Noise 값을 계산합니다.
    /// 
    /// Fractal Brownian Motion (FBM):
    /// - 여러 주파수(Octave)의 Noise를 합성
    /// - 각 Octave는 진폭(Amplitude)과 주파수(Frequency)를 가짐
    /// - Persistence: 진폭 감소율
    /// - Lacunarity: 주파수 증가율
    /// </summary>
    private float CalculatePerlinNoise(int x, int y, Vector2[] octaveOffsets)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        float maxValue = 0f; // 정규화를 위한 최대값

        for (int i = 0; i < octaves; i++)
        {
            // 샘플링 좌표 계산
            float sampleX = (x / (float)textureSize * noiseScale * frequency) + octaveOffsets[i].x;
            float sampleY = (y / (float)textureSize * noiseScale * frequency) + octaveOffsets[i].y;

            // Perlin Noise 값 계산 (-1 ~ 1 범위)
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;

            // 가중치 적용하여 합산
            noiseHeight += perlinValue * amplitude;

            // 다음 Octave를 위한 진폭 및 주파수 조정
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // 0 ~ 1 범위로 정규화
        noiseHeight = (noiseHeight / maxValue + 1f) * 0.5f;

        return Mathf.Clamp01(noiseHeight);
    }

    /// <summary>
    /// 대비를 조정합니다.
    /// 
    /// 대비 공식:
    /// result = (value - 0.5) * (1 + contrast) + 0.5
    /// 
    /// - contrast = 0: 원본
    /// - contrast > 0: 대비 증가 (밝은 부분은 더 밝게, 어두운 부분은 더 어둡게)
    /// </summary>
    private float AdjustContrast(float value, float contrastAmount)
    {
        return Mathf.Clamp01((value - 0.5f) * (1f + contrastAmount) + 0.5f);
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// 다양한 프리셋으로 빠르게 테스트할 수 있는 메서드들
    /// </summary>

    [ContextMenu("Preset: Fine Detail (세밀한 패턴)")]
    private void PresetFineDetail()
    {
        noiseScale = 30f;
        octaves = 6;
        persistence = 0.5f;
        lacunarity = 2f;
        contrast = 0.3f;
        GenerateNoiseTexture();
        Debug.Log("[NoiseTextureGenerator] Preset 적용: Fine Detail");
    }

    [ContextMenu("Preset: Large Clouds (큰 구름)")]
    private void PresetLargeClouds()
    {
        noiseScale = 15f;
        octaves = 3;
        persistence = 0.6f;
        lacunarity = 2f;
        contrast = 0.4f;
        GenerateNoiseTexture();
        Debug.Log("[NoiseTextureGenerator] Preset 적용: Large Clouds");
    }

    [ContextMenu("Preset: Sharp Edges (날카로운 가장자리)")]
    private void PresetSharpEdges()
    {
        noiseScale = 25f;
        octaves = 4;
        persistence = 0.4f;
        lacunarity = 2.5f;
        contrast = 0.7f;
        GenerateNoiseTexture();
        Debug.Log("[NoiseTextureGenerator] Preset 적용: Sharp Edges");
    }

    [ContextMenu("Preset: Smooth Gradient (부드러운 그라디언트)")]
    private void PresetSmoothGradient()
    {
        noiseScale = 10f;
        octaves = 2;
        persistence = 0.7f;
        lacunarity = 1.5f;
        contrast = 0.2f;
        GenerateNoiseTexture();
        Debug.Log("[NoiseTextureGenerator] Preset 적용: Smooth Gradient");
    }

    [ContextMenu("Random Seed (새 패턴)")]
    private void RandomizeSeed()
    {
        seed = Random.Range(0, 10000);
        GenerateNoiseTexture();
        Debug.Log($"[NoiseTextureGenerator] 새 시드 적용: {seed}");
    }

    #endregion

    #region Unity Lifecycle

    private void OnValidate()
    {
        // Inspector에서 값 변경 시 자동 재생성 (선택사항)
        // 주석 해제하면 값 변경할 때마다 자동 생성됨
        // if (generatedTexture != null && Application.isPlaying)
        // {
        //     GenerateNoiseTexture();
        // }
    }

    private void OnDestroy()
    {
        // 텍스처 메모리 정리
        if (generatedTexture != null)
        {
            if (Application.isPlaying)
            {
                Destroy(generatedTexture);
            }
            else
            {
                DestroyImmediate(generatedTexture);
            }
        }
    }

    #endregion
}