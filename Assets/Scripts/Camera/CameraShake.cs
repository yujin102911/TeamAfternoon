using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private Vector3 originalPos;
    private Coroutine hitStopCoroutine;
    private Coroutine shakeCoroutine;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        originalPos = transform.localPosition;
    }

    public void Play_Hitstop(float duration = 0.15f)
    {
        if (hitStopCoroutine != null)
            StopCoroutine(hitStopCoroutine);

        hitStopCoroutine = StartCoroutine(HitStop(duration));
    }

    [Button("카메라 흔들기")]
    public void Shake(float duration = 0.15f, float strength = 0.15f)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, strength));
    }

    private IEnumerator ShakeCoroutine(float duration, float strength)
    {
        float time = 0f;

        while (time < duration)
        {
            Vector2 offset = Random.insideUnitCircle * strength;
            transform.localPosition = originalPos + (Vector3)offset;

            time += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }

    public void RePosition(Vector3 pos)
    {
        originalPos = pos;
    }

    //히트스탑
    IEnumerator HitStop(float duration)
    {
        float C_scale = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = C_scale;
    }
}
