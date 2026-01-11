using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NailController : MonoBehaviour
{
    public static NailController Instance { get; private set; }
    public string currentRequiredSignal = "";

    private void Awake()
    {
        Instance = this;
        Btn1_2.onClick.AddListener(OneToTwo);
        Btn2_3.onClick.AddListener(TwoToThree);
        Btn4_5.onClick.AddListener(FourToFive);
        Btn5_6.onClick.AddListener(FiveToSix);
        Btn6_7.onClick.AddListener(SixToSeven);
        Btn9_10.onClick.AddListener(NineToTen);
    }

    private void Start()
    {
        Init();
    }
    #region SerializeField
    [Header("버튼 연결")]
    [SerializeField] private Button Btn1_2;
    [SerializeField] private Button Btn2_3;
    [SerializeField] private Button Btn4_5;
    [SerializeField] private Button Btn5_6;
    [SerializeField] private Button Btn6_7;
    [SerializeField] private Button Btn9_10;

    [Header("말풍선 패널 연결")]
    [SerializeField] private List<GameObject> nails;

    [Header("Nail오브젝트 연결 (2->3)")]
    [SerializeField] private GameObject nailObject1;
    [SerializeField] private GameObject nailObject2;

    [Header("특정 행동 시그널")]
    [SerializeField] private string signal3_4 = "MSN";
    [SerializeField] private string signal7_8 = "OWL";
    [SerializeField] private string signal11_12 = "END";
    #endregion
    public void CompleteStepBySignal(string signal)
    {
        if (signal != currentRequiredSignal) return;
        if (signal ==  signal3_4) ThreeToFour();
        else if (signal == signal7_8) SevenToEight();
        else if (signal == "Mail_Read_1_1") EightToNine();
        else if (signal == "Mail_Read_1_0") TenToEleven();
        else if (signal == signal11_12) ElevenToEnd();

    }

    private void Init()
    {
        foreach (GameObject nail in nails)
        {
            nail.gameObject.SetActive(false);
        }
        nailObject1.SetActive(true);
        nailObject2.SetActive(false);
        nails[0].SetActive(true);
    }

    private void OneToTwo() // 버튼
    {
        nails[0].SetActive(false);
        nails[1].SetActive(true);
    }

    private void TwoToThree() // 버튼
    {
        nails[1].SetActive(false);
        nails[2].SetActive(true);
        nailObject1.SetActive(false);
        nailObject2.SetActive(true);
        currentRequiredSignal = signal3_4;
    }
    private void ThreeToFour() // 트리거
    {
        nails[2].SetActive(false);
        nails[3].SetActive(true);
        currentRequiredSignal = "";
    }
    private void FourToFive() // 버튼
    {
        nails[3].SetActive(false);
        nails[4].SetActive(true);
    }
    private void FiveToSix() // x버튼
    {
        nails[4].SetActive(false);
        nails[5].SetActive(true);
    }
    private void SixToSeven() // 버튼
    {
        nails[5].SetActive(false);
        nails[6].SetActive(true);
        currentRequiredSignal = signal7_8;
    }
    private void SevenToEight() // 트리거
    {
        nails[6].SetActive (false);
        nails[7].SetActive(true);
        currentRequiredSignal = "Mail_Read_1_1";
    }
    private void EightToNine() // 메시지 클릭?? 어케 구현
    {
        nails[7].SetActive (false);
        nails[8].SetActive(true);
        currentRequiredSignal = "";
    }
    private void NineToTen() // 버튼
    {
        nails[8].SetActive (false);
        nails[9].SetActive(true);
        currentRequiredSignal = "Mail_Read_1_0";
    }
    private void TenToEleven() // 메시지 클릭
    {
        nails[9].SetActive (false);
        nails[10].SetActive(true);
        currentRequiredSignal = signal11_12;
    }
    private void ElevenToEnd() // 첨부파일 클릭
    {
        nails[10].SetActive(false);
    }

}
