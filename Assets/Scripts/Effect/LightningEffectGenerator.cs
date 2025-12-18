using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightningEffectGenerator : MonoBehaviour
{
    public static LightningEffectGenerator Instance { get; private set; }

    [Header("Visual Settings")]
    [SerializeField] private Color lightningColor = new Color(1f, 1f, 0.3f, 1f);
    [SerializeField] private float lightningGlowIntensity = 2f;
    [SerializeField] private float lightningWidth = 0.2f;

    [Header("Generation Settings")]
    [SerializeField] private int segments = 10;        // 번개 굴곡 세그먼트 수
    [SerializeField] private float noiseScale = 0.5f;  // 번개 노이즈 강도
    [SerializeField] private float duration = 0.2f;    // 번개 유지 시간

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        // 예: 현재 위치에서 마우스 클릭 지점까지 번개 생성
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            LightningEffectGenerator.Instance.CreateLightning(transform.position, mousePos);
        }
    }

    /// <summary>
    /// 외부에서 호출 가능한 번개 생성 함수
    /// </summary>
    public void CreateLightning(Vector3 startPos, Vector3 endPos)
    {
        GameObject lightningObj = new GameObject("DynamicLightning");
        LineRenderer lr = lightningObj.AddComponent<LineRenderer>();

        SetupLineRenderer(lr);
        StartCoroutine(AnimateLightning(lr, startPos, endPos));

        // 스파크 폭발 효과 생성
        CreateSparkBurst(startPos);
        CreateSparkBurst(endPos);

        Destroy(lightningObj, duration + 0.1f);
    }

    private void SetupLineRenderer(LineRenderer lr)
    {
        lr.startWidth = lightningWidth;
        lr.endWidth = lightningWidth * 0.5f;
        lr.positionCount = segments;

        // HDR 컬러 적용
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = lightningColor * lightningGlowIntensity;
        lr.material = mat;

        // 번개 그라데이션 설정
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(lightningColor, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        lr.colorGradient = gradient;
    }

    private IEnumerator AnimateLightning(LineRenderer lr, Vector3 start, Vector3 end)
    {
        float elapsed = 0f;
        Vector3[] positions = new Vector3[segments];

        while (elapsed < duration)
        {
            if (lr == null) yield break;

            // 지그재그 경로 실시간 생성
            for (int i = 0; i < segments; i++)
            {
                float t = (float)i / (segments - 1);
                Vector3 basePos = Vector3.Lerp(start, end, t);

                if (i > 0 && i < segments - 1)
                {
                    // 수직 벡터를 이용한 노이즈 추가
                    Vector3 sideDir = Vector3.Cross(end - start, Vector3.forward).normalized;
                    basePos += sideDir * Random.Range(-noiseScale, noiseScale);
                }
                positions[i] = basePos;
            }

            lr.SetPositions(positions);

            // 투명도 페이드 아웃
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            lr.startColor = new Color(lr.startColor.r, lr.startColor.g, lr.startColor.b, alpha);

            elapsed += Time.deltaTime;
            yield return new WaitForSeconds(0.02f); // 50 FPS 업데이트
        }
    }

    private void CreateSparkBurst(Vector3 pos)
    {
        GameObject sparkObj = new GameObject("SparkBurst");
        sparkObj.transform.position = pos;

        ParticleSystem ps = sparkObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.3f; //
        main.startSpeed = 5f;      //
        main.startSize = 0.1f;     //
        main.startColor = lightningColor;

        var emission = ps.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) }); //

        var renderer = sparkObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default"));

        ps.Play();
        Destroy(sparkObj, 0.5f);
    }
}