using UnityEngine;
using UnityEngine.UI;

public class CreditPanel : MonoBehaviour
{
    // 무슨 기능이 있을진 모르겠지만 다 있으니까 일단 Credit Panel 스크립트도 만들어놨어용.
    // 필요없으면 걍 x버튼 기능 딴데로 옮기고 삭제 ㄱㄱ
    [SerializeField] private Button _xButton;


    [SerializeField] private GameObject _generalOb;
    [SerializeField] private Button _general_specialBtn;
    [SerializeField] private Button _general_thirdBtn;

    [SerializeField] private GameObject _specialOb;
    [SerializeField] private Button _special_generalBtn;
    [SerializeField] private Button _special_thridBtn;

    [SerializeField] private GameObject _thirdOb;
    [SerializeField] private Button _third_generalBtn;
    [SerializeField] private Button _third_specialBtn;

    private void Awake()
    {
        _xButton.onClick.AddListener(CloseCredit);

        _special_generalBtn.onClick.AddListener(OnGeneralBtnClick);
        _third_generalBtn.onClick.AddListener(OnGeneralBtnClick);

        _general_specialBtn.onClick.AddListener(OnSpecialBtnClick);
        _third_specialBtn.onClick.AddListener(OnSpecialBtnClick);

        _general_thirdBtn.onClick.AddListener(OnThirdBtnClick);
        _special_thridBtn.onClick.AddListener(OnThirdBtnClick);
    }

    private void CloseCredit()
    {
        gameObject.SetActive(false);
    }

    private void OnGeneralBtnClick()
    {
        _generalOb.transform.SetAsLastSibling();
    }
    private void OnSpecialBtnClick()
    {
        _specialOb.transform.SetAsLastSibling();
    }

    private void OnThirdBtnClick()
    {
        _thirdOb.transform.SetAsLastSibling();
    }

}
