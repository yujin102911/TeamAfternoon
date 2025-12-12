using System.Collections;
using UnityEngine;

public class BookMove : MonoBehaviour
{
    [Header("Rect")]
    [SerializeField]
    private RectTransform _rect;
    [Header("시작 좌표")]
    [SerializeField]
    private Vector2 _start;
    [Header("도착 좌표")]
    [SerializeField]
    private Vector2 _end;
    [Header("시간")]
    [SerializeField]
    private float _duration;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MoveAction(bool is_open)
    {
        if (_rect == null) return;

        if (is_open)
        {
            StartCoroutine(MoveUI(_rect, _start, _end, _duration, is_open));
        }
        else
        {
            StartCoroutine(MoveUI(_rect, _end, _start, _duration, is_open));
        }
    }

    public IEnumerator MoveUI(RectTransform rect, Vector2 start, Vector2 end, float duration, bool is_open)
    {
        float time = 0f;
        rect.anchoredPosition = start;

        // 열릴때 오브젝트 켜기
        if (is_open) 
        {
            if(!this.gameObject.activeSelf)
                this.gameObject.SetActive(true);
        }

        while (time < duration)
        {
            time += Time.deltaTime;

            float t = Mathf.Clamp01(time / duration);

            // 자연스러운 ease-in-out
            t = Mathf.SmoothStep(0f, 1f, t);

            rect.anchoredPosition = Vector2.Lerp(start, end, t);

            yield return null;
        }

        rect.anchoredPosition = end;

        // 닫힐때 오브젝트 끄기
        if (!is_open && this.gameObject.activeSelf)
        {
            this.gameObject.SetActive(false);
        }

        //다 도착해서 작동
        if (DeckBuildingManager.Instance != null) 
        {
            if (is_open)
            {
                DeckBuildingManager.Instance.On_BackBtn();
                DeckBuildingManager.Instance.On_Inhance();
            }
            else
            {
                DeckBuildingManager.Instance.Off_Background();
                DeckBuildingManager.Instance.On_Tags();
            }
        }
    }

}
