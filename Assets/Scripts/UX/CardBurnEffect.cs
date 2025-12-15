using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 카드 불태우기 효과를 제어하는 컴포넌트
/// 
/// 주요 책임:
/// - UI Image에 적용된 Burn Shader의 Dissolve 효과 제어
/// - 불타는 애니메이션 재생 및 완료 콜백 처리
/// - Material Instance를 사용한 효율적인 Material 관리
/// 
/// 동작 흐름:
/// 1. Initialize() - Burn Material 설정 및 초기화
/// 2. PlayBurnEffect() - 불타는 애니메이션 시작
/// 3. Dissolve 값을 0에서 1로 증가시키며 카드가 불타는 효과 연출
/// 4. 완료 시 콜백 실행
/// 
/// 실무 권장사항:
/// - Material Instance 사용으로 각 카드 독립적 제어
/// - Coroutine 대신 Update 사용 고려 (대량의 카드 처리 시)
/// - Object Pooling과 함께 사용 권장
/// </summary>
public class CardBurnEffect : MonoBehaviour
{
    #region Inspector Fields

    [Header("References")]
    [SerializeField]
    [Tooltip("효과를 적용할 UI Image 컴포넌트")]
    private Image targetImage;

    [Header("Burn Material")]
    [SerializeField]
    [Tooltip("Burn 효과가 적용된 Material (UI-Burn.shader 사용)")]
    private Material burnMaterial;

    [Header("Burn Effect Settings")]
    [SerializeField]
    [Tooltip("불타는 애니메이션 지속 시간 (초)")]
    [Range(0.3f, 3f)]
    private float burnDuration = 1.0f;

    [SerializeField]
    [Tooltip("Dissolve 시작 값 (0 = 정상 상태)")]
    [Range(0f, 0.3f)]
    private float dissolveStartValue = 0f;

    [SerializeField]
    [Tooltip("Dissolve 종료 값 (1 = 완전히 사라짐)")]
    [Range(0.7f, 1.2f)]
    private float dissolveEndValue = 1.1f;

    [SerializeField]
    [Tooltip("불타는 가장자리 두께")]
    [Range(0.01f, 0.3f)]
    private float burnEdgeWidth = 0.1f;

    [SerializeField]
    [Tooltip("애니메이션 곡선 (Ease 효과)")]
    private AnimationCurve burnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Advanced Settings")]
    [SerializeField]
    [Tooltip("효과 시작 전 대기 시간 (초)")]
    [Range(0f, 1f)]
    private float startDelay = 0f;

    [SerializeField]
    [Tooltip("효과 완료 후 오브젝트 자동 비활성화")]
    private bool autoDeactivateOnComplete = true;

    [SerializeField]
    [Tooltip("디버그 로그 출력")]
    private bool enableDebugLog = false;

    #endregion

    #region Private Fields

    // Material 인스턴스 (각 카드마다 독립적인 Material)
    private Material materialInstance;

    // Shader Property IDs (성능 최적화를 위한 캐싱)
    private static readonly int DissolveAmountID = Shader.PropertyToID("_DissolveAmount");
    private static readonly int EdgeWidthID = Shader.PropertyToID("_EdgeWidth");

    // 현재 실행 중인 코루틴 참조
    private Coroutine currentBurnCoroutine;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// 컴포넌트가 활성화될 때 호출됩니다.
    /// 필요한 경우 자동으로 초기화를 진행합니다.
    /// </summary>
    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// 오브젝트가 파괴될 때 Material Instance를 정리합니다.
    /// 메모리 누수 방지를 위해 반드시 필요합니다.
    /// 
    /// 실무 팁:
    /// - Material Instance는 명시적으로 Destroy해야 메모리에서 해제됨
    /// - 안 하면 Scene을 여러 번 로드할 때 메모리 누수 발생 가능
    /// </summary>
    private void OnDestroy()
    {
        if (materialInstance != null)
        {
            Destroy(materialInstance);
            materialInstance = null;
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Burn Material을 외부에서 설정합니다.
    /// CardTrashAnimator에서 런타임에 Material을 할당할 때 사용합니다.
    /// </summary>
    public void SetBurnMaterial(Material material)
    {
        if (material == null)
        {
            Debug.LogWarning("[CardBurnEffect] SetBurnMaterial: Material이 null입니다!", this);
            return;
        }

        burnMaterial = material;

        if (enableDebugLog)
        {
            Debug.Log($"[CardBurnEffect] Burn Material 설정 완료: {material.name}");
        }
    }

    /// <summary>
    /// Burn Duration을 외부에서 설정합니다.
    /// CardTrashAnimator에서 불타는 시간을 동적으로 설정할 때 사용합니다.
    /// </summary>
    public void SetBurnDuration(float duration)
    {
        burnDuration = Mathf.Clamp(duration, 0.3f, 3f);

        if (enableDebugLog)
        {
            Debug.Log($"[CardBurnEffect] Burn Duration 설정: {burnDuration}초");
        }
    }

    /// <summary>
    /// Burn 효과를 초기화합니다.
    /// 반드시 PlayBurnEffect() 호출 전에 한 번 실행해야 합니다.
    /// 
    /// 동작:
    /// 1. targetImage에 Burn Material Instance를 생성하여 할당
    /// 2. 초기 Dissolve 값을 0으로 설정 (정상 상태)
    /// 
    /// Material Instance 사용 이유:
    /// - 여러 카드가 같은 Material을 공유하면 모두 동시에 불탐
    /// - Instance를 만들면 각 카드가 독립적으로 효과 진행 가능
    /// </summary>
    public void Initialize()
    {
        


        if (targetImage == null)
        {
            Debug.LogError("[CardBurnEffect] targetImage가 설정되지 않았습니다!", this);
            return;
        }

        if (burnMaterial == null)
        {
            Debug.LogError("[CardBurnEffect] burnMaterial이 설정되지 않았습니다!", this);
            return;
        }

        // Material Instance 생성 (각 카드마다 독립적)
        materialInstance = new Material(burnMaterial);
        targetImage.material = materialInstance;

        // 초기 상태로 설정 (불타지 않은 상태)
        UpdateMaterialProperties(dissolveStartValue);

        if (enableDebugLog)
        {
            Debug.Log($"[CardBurnEffect] 초기화 완료: {gameObject.name}");
        }

        // Noise 텍스처 확인
        if (materialInstance.GetTexture("_NoiseTex") == null)
        {
            Debug.LogWarning("[CardBurnEffect] Noise Texture가 설정되지 않았습니다!");
        }
    }

    /// <summary>
    /// 불타는 효과를 재생합니다.
    /// 
    /// 동작:
    /// 1. 이미 실행 중이면 중지하고 새로 시작
    /// 2. Coroutine으로 Dissolve 값을 0 -> 1로 증가
    /// 3. 완료 시 onComplete 콜백 실행
    /// 
    /// 매개변수:
    /// onComplete - 효과가 끝났을 때 실행할 콜백 함수 (옵션)
    /// 
    /// 실무 사용 예시:
    /// burnEffect.PlayBurnEffect(() => {
    ///     Destroy(cardObject);
    ///     DrawNewCard();
    /// });
    /// </summary>
    public void PlayBurnEffect(Action onComplete = null)
    {
        if (materialInstance == null)
        {
            Debug.LogWarning("[CardBurnEffect] Initialize()를 먼저 호출해야 합니다!", this);
            Initialize();
        }

        // 이미 실행 중이면 중지
        if (currentBurnCoroutine != null)
        {
            StopCoroutine(currentBurnCoroutine);
        }

        currentBurnCoroutine = StartCoroutine(BurnAnimationCoroutine(onComplete));
    }

    /// <summary>
    /// 진행 중인 효과를 즉시 중단합니다.
    /// 카드를 재사용하거나 효과를 취소해야 할 때 사용합니다.
    /// </summary>
    public void StopEffect()
    {
        if (currentBurnCoroutine != null)
        {
            StopCoroutine(currentBurnCoroutine);
            currentBurnCoroutine = null;
        }

        ResetEffect();
    }

    /// <summary>
    /// 효과를 초기 상태로 되돌립니다.
    /// Object Pooling 사용 시 카드를 재사용하기 위해 호출합니다.
    /// </summary>
    public void ResetEffect()
    {
        UpdateMaterialProperties(dissolveStartValue);
    }

    /// <summary>
    /// Dissolve 값을 수동으로 설정합니다.
    /// 테스트나 특수한 경우에 사용합니다.
    /// </summary>
    /// <param name="value">Dissolve 값 (0 = 정상, 1 = 완전히 불탐)</param>
    public void SetDissolveValue(float value)
    {
        UpdateMaterialProperties(Mathf.Clamp01(value));
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// 불타는 애니메이션을 실행하는 코루틴입니다.
    /// Dissolve 값을 시간에 따라 증가시켜 카드가 불타는 효과를 만듭니다.
    /// </summary>
    private IEnumerator BurnAnimationCoroutine(Action onComplete)
    {
        if (enableDebugLog)
        {
            Debug.Log($"[CardBurnEffect] 불태우기 시작: {gameObject.name}");
        }

        // 시작 대기
        if (startDelay > 0f)
        {
            yield return new WaitForSeconds(startDelay);
        }

        float elapsed = 0f;

        // Dissolve 애니메이션 진행
        while (elapsed < burnDuration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsed / burnDuration);

            // AnimationCurve를 사용한 자연스러운 진행
            float curveValue = burnCurve.Evaluate(normalizedTime);

            // Dissolve 값 계산
            float currentDissolve = Mathf.Lerp(dissolveStartValue, dissolveEndValue, curveValue);

            // Material Property 업데이트
            UpdateMaterialProperties(currentDissolve);

            yield return null;
        }

        // 최종 값으로 보정
        UpdateMaterialProperties(dissolveEndValue);

        if (enableDebugLog)
        {
            Debug.Log($"[CardBurnEffect] 불태우기 완료: {gameObject.name}");
        }

        // 자동 비활성화
        if (autoDeactivateOnComplete)
        {
            gameObject.SetActive(false);
        }

        // 완료 콜백 실행
        onComplete?.Invoke();

        currentBurnCoroutine = null;
    }

    /// <summary>
    /// Material Instance의 Shader 프로퍼티를 업데이트합니다.
    /// 
    /// Material Instance 사용:
    /// - 각 카드가 독립적인 Material을 가지므로 개별 제어 가능
    /// - SetFloat 등으로 직접 프로퍼티 수정
    /// - 다른 카드에 영향을 주지 않음
    /// 
    /// 성능 고려사항:
    /// - Material Instance는 메모리를 사용하지만 UI 카드는 수가 적어서 괜찮음
    /// - 실시간 업데이트가 필요한 경우 이 방식이 가장 안정적
    /// </summary>
    private void UpdateMaterialProperties(float dissolveAmount)
    {
        if (materialInstance == null) return;

        // Dissolve 기본 프로퍼티만 설정 (색상 관련 제거됨)
        materialInstance.SetFloat(DissolveAmountID, dissolveAmount);
        materialInstance.SetFloat(EdgeWidthID, burnEdgeWidth);
    }

    #endregion

    #region Debug Methods

    /// <summary>
    /// Inspector에서 효과를 테스트할 수 있는 메서드입니다.
    /// 우클릭 > Test Burn Effect로 실행 가능
    /// </summary>
    [ContextMenu("Test Burn Effect")]
    private void TestBurnEffect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[CardBurnEffect] 플레이 모드에서만 테스트할 수 있습니다.");
            return;
        }

        PlayBurnEffect(() => Debug.Log("[CardBurnEffect] 테스트 완료!"));
    }

    /// <summary>
    /// Inspector에서 효과를 리셋하는 메서드입니다.
    /// 우클릭 > Reset Effect로 실행 가능
    /// </summary>
    [ContextMenu("Reset Effect")]
    private void DebugResetEffect()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[CardBurnEffect] 플레이 모드에서만 실행할 수 있습니다.");
            return;
        }

        StopEffect();
        Debug.Log("[CardBurnEffect] 효과 리셋 완료");
    }

    /// <summary>
    /// Inspector에서 Dissolve를 50%로 설정하는 메서드입니다.
    /// 우클릭 > Set Dissolve 0.5로 실행 가능
    /// </summary>
    [ContextMenu("Set Dissolve 0.5")]
    private void DebugSetHalfDissolve()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[CardBurnEffect] 플레이 모드에서만 실행할 수 있습니다.");
            return;
        }

        SetDissolveValue(0.5f);
        Debug.Log("[CardBurnEffect] Dissolve = 0.5 설정");
    }

    #endregion
}