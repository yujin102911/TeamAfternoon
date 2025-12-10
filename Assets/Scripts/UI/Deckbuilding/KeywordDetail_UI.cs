using TMPro;
using UnityEngine;

public class KeywordDetail_UI : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameTxt;
    [SerializeField]
    private TextMeshProUGUI _descriptTxt;
    public void SetAndShow(string keyword, string des)
    {

        _nameTxt.text = "#" + keyword;
        _descriptTxt.text = des;
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _nameTxt.text = "";
        _descriptTxt.text = "";
        this.gameObject.SetActive(false);
    }
}
