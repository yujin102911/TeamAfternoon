using UnityEngine;

public class TutorialObjectTrigger : MonoBehaviour
{
    [SerializeField] private string keyword = "action";
    private void OnEnable()
    {
        if (NailController.Instance != null)
        {
            NailController.Instance.OnGetSignal(keyword);
        }
    }

}
