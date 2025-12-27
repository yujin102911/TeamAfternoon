using TMPro;
using UnityEngine;

public class StageScene_UIManager : MonoBehaviour
{
    public static StageScene_UIManager Instance;

    [Header("빛")]
    [SerializeField]
    private GameObject _light;

    [Header("책 제목")]
    [SerializeField]
    private TextMeshProUGUI _bookTitleTxt;

    [Header("책 정보 패널")]
    [SerializeField]
    private StageInfo_Panel _stageInfoPanel;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hide_Txt();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // 책이 클릭 될 때 작동
    public void BookClicked(int id)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.Play(SoundID.UI_Click2);
        //_stageInfoPanel.Show(id);
    }

    // 책 위에 마우스 호버
    public void BookHoverEnter(int id)
    {
        StageData stageData = DataRepository.Instance.GetStage(id);

        Light_On();
        SetandShow_Txt(stageData.StageName);
    }

    // 책 위에서 마우스 벗어날 때
    public void BookHoverExit()
    {
        Light_Off();
        Hide_Txt();
    }

    private void Light_On()
    {
        _light.SetActive(true);
    }

    private void Light_Off()
    {
        _light.SetActive(false);
    }

    private void SetandShow_Txt(string message)
    {
        _bookTitleTxt.text = message;
    }

    private void Hide_Txt() 
    {
        _bookTitleTxt.text = "";
    }

    public void Exit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
