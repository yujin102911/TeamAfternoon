using System.Collections;
using UnityEngine;

public class BeamLaser : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private LineRenderer line;

    [Header("Test Input (Optional)")]
    [SerializeField] private Transform testOrigin;
    [SerializeField] private Transform testTarget;
    [SerializeField] private KeyCode fireKey = KeyCode.Space;

    [Header("Timing")]
    [SerializeField] private float duration = 0.25f;

    [Header("Look")]
    [SerializeField] private float baseWidth = 0.09f;

    [Header("Sprite Swap (Optional)")]
    [SerializeField] private SpriteRenderer casterSprite;
    [SerializeField] private Sprite castSprite;
    [SerializeField] private float preDelay = 0.0f;

    [Header("VFX (Optional)")]
    [SerializeField] private ParticleSystem muzzleVfx;     // 발사점 반짝(선택)
    [SerializeField] private Transform muzzleAnchor;       // 발사 위치(선택, 비우면 originTransform 사용)
    [SerializeField] private ParticleSystem impactVfx;     // 맞을 때 임팩트
    [SerializeField] private Transform impactAnchor;       // 임팩트 위치(선택, 비우면 targetTransform 사용)

    [Header("VFX Options")]
    [SerializeField] private bool followImpactDuringBeam = true; // 빔 쏘는 동안 타겟이 움직이면 임팩트도 따라갈지
    [SerializeField] private bool stopVfxOnEnd = true;           // 끝날 때 VFX 강제 정지할지

    private Coroutine runningRoutine;
    private Sprite originalSprite;

    private void Update()
    {
        if (Input.GetKeyDown(fireKey))
        {
            if (testOrigin == null || testTarget == null)
            {
                Debug.LogWarning("BeamLaser: testOrigin/testTarget not assigned.");
                return;
            }

            PlayOnce(testOrigin, testTarget);
        }
    }

    public void PlayOnce(Transform originTransform, Transform targetTransform)
    {
        if (originTransform == null || targetTransform == null)
        {
            Debug.LogWarning("BeamLaser: originTransform/targetTransform is null.");
            return;
        }

        if (runningRoutine != null)
            StopCoroutine(runningRoutine);

        runningRoutine = StartCoroutine(PlayRoutine(originTransform, targetTransform));
    }

    private IEnumerator PlayRoutine(Transform originTransform, Transform targetTransform)
    {
        // 스프라이트 백업 & 변경
        if (casterSprite != null && castSprite != null)
        {
            originalSprite = casterSprite.sprite;
            casterSprite.sprite = castSprite;
        }

        if (preDelay > 0f)
            yield return new WaitForSeconds(preDelay);

        // 빔 켜기
        if (line != null)
        {
            line.enabled = true;
            line.positionCount = 2;
            line.startWidth = baseWidth;
            line.endWidth = baseWidth;
        }

        // VFX 시작 (발사점)
        Transform muzzleT = muzzleAnchor != null ? muzzleAnchor : originTransform;
        if (muzzleVfx != null)
        {
            muzzleVfx.transform.position = muzzleT.position;
            muzzleVfx.Play();
        }

        // VFX 시작 (임팩트)
        Transform impactT = impactAnchor != null ? impactAnchor : targetTransform;
        if (impactVfx != null)
        {
            impactVfx.transform.position = impactT.position;
            impactVfx.Play();
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;

            // 라인 업데이트
            if (line != null)
            {
                line.SetPosition(0, originTransform.position);
                line.SetPosition(1, targetTransform.position);
            }

            // VFX 위치 업데이트 (선택)
            if (followImpactDuringBeam && impactVfx != null)
            {
                Transform followImpactT = impactAnchor != null ? impactAnchor : targetTransform;
                impactVfx.transform.position = followImpactT.position;
            }

            if (muzzleVfx != null)
            {
                Transform followMuzzleT = muzzleAnchor != null ? muzzleAnchor : originTransform;
                muzzleVfx.transform.position = followMuzzleT.position;
            }

            yield return null;
        }

        // 빔 끄기
        if (line != null) line.enabled = false;

        // VFX 종료 처리(선택)
        if (stopVfxOnEnd)
        {
            if (muzzleVfx != null)
                muzzleVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            if (impactVfx != null)
                impactVfx.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        // 스프라이트 원복
        if (casterSprite != null && castSprite != null)
            casterSprite.sprite = originalSprite;

        runningRoutine = null;
    }
}
