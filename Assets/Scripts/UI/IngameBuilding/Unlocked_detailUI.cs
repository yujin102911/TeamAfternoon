using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Unlocked_detailUI : MonoBehaviour
{
    [SerializeField]
    private Draggable_Block _tickInfo; // 틱 정보 출력 담당
    [SerializeField]
    private TextMeshProUGUI _nameTxt; // 이름 텍스트 UI
    [SerializeField]
    private TextMeshProUGUI _damageTxt;

    private Image _image;
    private Color _highlightColor = new Color(1f, 1f, 0f, 0.5f); // 강조 색상 (노란색 반투명)
    private Color _originalColor;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (this.gameObject.activeSelf)
            this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show(BlockData block)
    {
        this.gameObject.SetActive(true);

        // 틱 정보 출력
        _tickInfo.Show(block);

        // 이름 텍스트 설정
        _nameTxt.text = block.blockName;

        _damageTxt.text = "위력: " + block.attackDamage.ToString();
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
