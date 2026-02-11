using TMPro;
using UnityEngine;

public class BuildInfo : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    void Start()
    {
        if (text != null)
            text.text = GetBuildInfo();
    }

    string GetBuildInfo()
    {
        return
            $"Version: {Application.version}\n" +
            $"Unity: {Application.unityVersion}\n" +
            $"Platform: {Application.platform}\n";
    }
}
