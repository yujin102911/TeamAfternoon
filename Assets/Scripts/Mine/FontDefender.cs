using TMPro;
using UnityEngine;

public class FontDefender : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI targetText;
    [SerializeField]
    private TMP_FontAsset defaultFont;


    private void OnEnable()
    {
        if(defaultFont != null &&
            targetText != null &&
            targetText.font != defaultFont)
        {
            targetText.font = defaultFont;
        }
    }
}
