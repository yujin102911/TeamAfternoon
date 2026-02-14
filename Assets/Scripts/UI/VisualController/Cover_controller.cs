using UnityEngine;

public class Cover_controller : MonoBehaviour
{
    [SerializeField]
    private GameObject coverObject;

    public void ShowCover()
    {
        if (coverObject != null)
        {
            coverObject.SetActive(true);
        }
    }

    public void HideCover()
    {
        if (coverObject != null)
        {
            coverObject.SetActive(false);
        }
    }
}
