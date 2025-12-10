using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Keyword_UI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _keywordTxt;

    public void SetAndShow(string keyword)
    {
        
        _keywordTxt.text = "#" + keyword;
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _keywordTxt.text = "";
        this.gameObject.SetActive(false);
    }

}
