using System.Collections;
using TMPro;
using UnityEngine;

public class FontDefender : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI targetText;
    [SerializeField]
    private TMP_FontAsset defaultFont;


    Coroutine _co;

    private void OnEnable()
    {
        // 기본적으로 enable 시에도 한 번 잠금
        ScheduleLock();
    }

    public void ScheduleLock()
    {
        if (!isActiveAndEnabled) return;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(LockNextFrame());
    }

    private IEnumerator LockNextFrame()
    {
        yield return null; // 1프레임 뒤 (Localization/Binding이 끝난 시점)

        if (targetText == null) yield break;

        var fontToLock = defaultFont != null ? defaultFont : TMP_Settings.defaultFontAsset;
        if (fontToLock == null) yield break;

        // 그 프레임에 누가 바꿨든, 최종적으로 다시 고정
        if (targetText.font != fontToLock)
            targetText.font = fontToLock;
    }
}
