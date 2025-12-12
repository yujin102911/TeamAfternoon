using UnityEngine;
using UnityEngine.UI;

public class Index_Panel : MonoBehaviour
{
    [SerializeField]
    private Image _image;
    [Header("패널 이미지")]
    [SerializeField]
    private Sprite[] _panels;
    [Header("인덱스 버튼")]
    [SerializeField]
    private Button[] _buttons;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for (int i = 0; i < _buttons.Length; i++) 
        {
            if(_buttons[i] != null)
            {
                int index = i;
                _buttons[i].onClick.RemoveAllListeners();
                _buttons[i].onClick.AddListener(() => Switch_Panel(index));
            }
                

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Switch_Panel(int index)
    {
        foreach (var btn in _buttons) 
        {
            btn.gameObject.SetActive(true);
        }

        _image.sprite = _panels[index];
        _buttons[index].gameObject.SetActive(false);
    }
}
