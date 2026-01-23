using TMPro;
using UnityEngine;

public class Debug_Text : MonoBehaviour
{
    public static Debug_Text Instance;

    [SerializeField]
    private TextMeshProUGUI m_Text;

    private void Awake()
    {
        Instance = this;
        m_Text.text = "";
    }

    public void Add_Message(string ms)
    {
        m_Text.text += ms + "\n";
    }
}
