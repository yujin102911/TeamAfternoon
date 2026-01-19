using UnityEngine;
using TMPro;

public class BuildInfoToggle : MonoBehaviour
{
    private static BuildInfoToggle _instance;

    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI text;

    private void Awake()
    {
        // 🔒 중복 방지
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (panel != null)
            panel.SetActive(false);

        if (text != null)
            text.text = GetBuildInfo();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ServiceLocator.Instance.SetGameClear();
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            ServiceLocator.Instance.SetGameInit();
        }

    }

    string GetBuildInfo()
    {
        return
            $"Version: {Application.version}\n" +
            $"Unity: {Application.unityVersion}\n" +
            $"Platform: {Application.platform}\n";
    }
}
