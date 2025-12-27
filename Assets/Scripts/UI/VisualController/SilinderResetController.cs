using System.Collections;
using UnityEngine;

public class SilinderResetController : MonoBehaviour
{
    [Header("Belt Images")]
    [SerializeField] private RectTransform[] beltPieces;
    [SerializeField] private float beltScrollWidth = 200f;
    [SerializeField] private float duration = 2.0f;

    private Vector2[] _beltStartPositions;

    private void Awake()
    {
        _beltStartPositions = new Vector2[beltPieces.Length];
        for (int i = 0; i < beltPieces.Length; i++)
        {
            _beltStartPositions[i] = beltPieces[i].anchoredPosition;
        }
    }

    public void Play()
    {
        //StopAllCoroutines();
        StartCoroutine(PlayRoutine(duration));
    }


    private IEnumerator PlayRoutine(float duration)
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            UpdateBelt(t);
            yield return null;
        }

        // 보정 (필수)
        //UpdateBelt(1f);
    }

    private void UpdateBelt(float normalized)
    {
        float delta = Time.deltaTime * (beltScrollWidth / duration);

        Debug.Log(delta);

        foreach (var belt in beltPieces)
        {
            belt.anchoredPosition += Vector2.left * delta;

            if (belt.anchoredPosition.x <= -beltScrollWidth)
            {
                belt.anchoredPosition += Vector2.right * beltScrollWidth * beltPieces.Length;
                belt.anchoredPosition = new Vector2(belt.anchoredPosition.x, belt.anchoredPosition.y);
                belt.SetAsLastSibling();
            }
        }
    }
}
