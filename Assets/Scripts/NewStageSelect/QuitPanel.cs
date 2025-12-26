using UnityEngine;
using UnityEngine.UI;

public class QuitPanel : MonoBehaviour
{
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button xButton;

    private void Awake()
    {
        yesButton.onClick.AddListener(QuitGame);
        noButton.onClick.AddListener(AllOff);
        xButton.onClick.AddListener(AllOff);
    }

    private void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    private void AllOff()
    {
        gameObject.SetActive(false);
    }

}
