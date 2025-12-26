using UnityEngine;
using UnityEngine.UI;

public class CreditPanel : MonoBehaviour
{
    // 무슨 기능이 있을진 모르겠지만 다 있으니까 일단 Credit Panel 스크립트도 만들어놨어용.
    // 필요없으면 걍 x버튼 기능 딴데로 옮기고 삭제 ㄱㄱ
    [SerializeField] private Button _xButton;

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseCredit);
    }

    private void CloseCredit()
    {
        gameObject.SetActive(false);
    }

}
