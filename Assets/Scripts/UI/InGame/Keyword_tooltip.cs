using TMPro;
using UnityEngine;

public class Keyword_tooltip : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _nameTxt;
    [SerializeField]
    private TextMeshProUGUI _descriptionTxt;

    public void Show(string name, string descript)
    {
        _nameTxt.text = name;
        _descriptionTxt.text = descript;

        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _nameTxt.text = "";
        _descriptionTxt.text = "";

        this.gameObject.SetActive(false);
    }
}
