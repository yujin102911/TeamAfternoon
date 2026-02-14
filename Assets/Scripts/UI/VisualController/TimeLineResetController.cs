using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeLineResetController : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider timelineSlider;

    [Header("Belt Images")]
    [SerializeField] private Transform _container;
    [SerializeField] private RectTransform[] beltPieces;
    [SerializeField] private float beltScrollWidth = 200f;
    [SerializeField]
    private float X_offset = 0;
    [SerializeField]
    private float RightX_Pos = 0;

    [SerializeField] private float duration = 2.0f;

    private Vector2 backStartPos;
    private Vector2 silinderStartPos;

    private Vector2[] _beltStartPositions;

    private void Awake()
    {
        _beltStartPositions = new Vector2[beltPieces.Length];
        for (int i = 0; i < beltPieces.Length; i++)
        {
            _beltStartPositions[i] = beltPieces[i].anchoredPosition;
        }
    }

    [ContextMenu("TestPlay")]
    public Coroutine Play()
    {
        return StartCoroutine(PlayRoutine(duration));
    }

    public IEnumerator Play_IEnumerator()
    {
        return PlayRoutine(duration);
    }

    private IEnumerator PlayRoutine(float duration)
    {
        for(int i = 0; i < beltPieces.Length; i++)
        {
            beltPieces[i].GetComponent<Cover_controller>().ShowCover();
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            UpdateBelt(t);
            yield return null;
        }

        // 보정 (필수)
        Fixed_Pos();
    }

    private void UpdateBelt(float normalized)
    {
        float delta = Time.deltaTime * (beltScrollWidth / duration);

        //for (int i = 0; i < beltPieces.Length; i++)
        //{
        //    float x = _beltStartPositions[i].x - move;

        //    // 루프 처리 (정확!)
        //    x = Mathf.Repeat(x + beltScrollWidth, beltScrollWidth * beltPieces.Length) - beltScrollWidth;

        //    if(i == 0)
        //        Debug.Log(x);  

        //    beltPieces[i].anchoredPosition =
        //        new Vector2(x, _beltStartPositions[i].y);
        //}

        foreach (var belt in beltPieces)
        {
            belt.anchoredPosition += Vector2.left * delta;

            if (belt.anchoredPosition.x <= -beltScrollWidth - X_offset) { 
                belt.anchoredPosition += Vector2.right * beltScrollWidth * beltPieces.Length; 
                belt.anchoredPosition = new Vector2(belt.anchoredPosition.x, belt.anchoredPosition.y); 
                belt.SetAsLastSibling(); 
            }
        }
    }

    private void Fixed_Pos()
    {
        for(int i = 0; i < _container.childCount; i++)
        {
            RectTransform child = _container.GetChild(i).GetComponent<RectTransform>();
            child.anchoredPosition = _beltStartPositions[i];

            if(i == 1)
                child.GetComponent<Cover_controller>().HideCover();
        }
    }
}
