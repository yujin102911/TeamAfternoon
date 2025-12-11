using UnityEngine;

public class Auto_Size : MonoBehaviour
{
    [SerializeField]
    private RectTransform block;
    [SerializeField]
    private RectTransform myRect;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        block = transform.Find("Block").GetComponent<RectTransform>();
        myRect = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        myRect.sizeDelta = new Vector2(block.rect.width, myRect.sizeDelta.y);
    }

}
