using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;
using Steamworks;

[System.Serializable]
public class ScenarioStep
{
    public string dialogueKey;
    public Image background;

    [Tooltip("체크하면 페이드 효과 없이 바로 이미지가 바뀝니다.")]
    public bool instantChange;

    [Header("스크롤 설정")]
    [Tooltip("체크하면 이 장면이 시작될 때 스크롤이 위로 올라갑니다.")]
    public bool triggerScroll;
    public float scrollAmount = 300f;

    [Header("하단바 설정")]
    [Tooltip("체크하면 하단바가 토글됩니다.")]
    public bool toggleBottomBar;
}

public class OutroManager : MonoBehaviour
{
    [Header("종료 버튼")]
    public Button quitButton;
    public Button goToTitle;

    [Header("씬 설정")]
    public string titleScene = "TitleScene";

    [Header("Localization")]
    [SerializeField] private LocalizedString _dialogueLocalizedString;

    [Header("External Systems")]
    public AutoScroll scrollController;
    public BottomBarController bottomBarController;

    [Header("Credit Speed Control")]
    public float fastScrollMultiplier = 2.0f;

    [Header("Ending Credits Settings")]
    public GameObject endingCreditPanel;   // 엔딩 크레딧 전체 패널
    public RectTransform creditContent;    // 움직일 텍스트 내용물
    public GameObject endGamePanel;        // 크레딧 끝나면 뜰 최종 종료 패널

    [Space(10)]
    public float creditScrollSpeed = 50f;  // 올라가는 속도
    public float creditStartDelay = 5.0f;  // 시작 전 대기 시간

    [Header("Credit Positions")]
    public float creditStartY = -500f;     // 크레딧 시작 Y 위치
    public float creditEndY = 1500f;       // 크레딧 끝나는 Y 위치

    [Header("UI Components")]
    public TextMeshProUGUI textDisplay;

    [Header("Settings")]
    public float typeSpeed = 0.05f;
    public float fadeDuration = 1.0f;

    [Header("Scenario Data")]
    public List<ScenarioStep> scenarioList;

    private int currentIndex = -1;
    private bool isTyping = false;
    private string currentFullText = "";

    private Image currentActiveImage = null;
    private Coroutine transitionCoroutine = null;

    void Start()
    {
        textDisplay.text = "";

        // 시작할 때 크레딧 관련 패널들은 다 꺼두기
        if (endingCreditPanel != null) endingCreditPanel.SetActive(false);
        if (endGamePanel != null) endGamePanel.SetActive(false);

        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
        if (goToTitle != null) goToTitle.onClick.AddListener(GoToTitle);

        //BGM 변경: 배경 화면 씬
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.Play(SoundID.BGM_Title);
            SoundManager.Instance.Set_BGM_Volume(0.4f);
        }
            
        
            //SoundManager.Instance.Play(SoundID.BGM_Title);

        NextStep();
    }

    void Update()
    {
        // 엔딩 크레딧이나 종료 패널이 떠있으면 클릭 무시
        if ((endingCreditPanel != null && endingCreditPanel.activeSelf) ||
            (endGamePanel != null && endGamePanel.activeSelf))
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (isTyping)
            {
                StopCoroutine("TypewriterEffect");
                textDisplay.text = currentFullText;
                textDisplay.maxVisibleCharacters = currentFullText.Length;
                isTyping = false;
            }
            else
            {
                NextStep();
            }
        }
    }

    private void OnEnable()
    {
        _dialogueLocalizedString.StringChanged += OnDialogueChanged;
    }

    private void OnDisable()
    {
        _dialogueLocalizedString.StringChanged -= OnDialogueChanged;
    }

    #region Localization Methods
    private void OnDialogueChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        currentFullText = value;
        StopCoroutine("TypewriterEffect");
        StartCoroutine("TypewriterEffect");
    }
    #endregion

    void NextStep()
    {
        currentIndex++;

        if (currentIndex >= scenarioList.Count)
        {
            Debug.Log("시나리오 종료 -> 엔딩 크레딧 재생");
            StartCoroutine(PlayEndingCredits());
            return;
        }

        ScenarioStep step = scenarioList[currentIndex];

        // 1. 스크롤 로직
        if (step.triggerScroll && scrollController != null)
        {
            scrollController.MoveNext(step.scrollAmount);
        }

        // 2. 하단바 토글 로직
        if (step.toggleBottomBar && bottomBarController != null)
        {
            bottomBarController.ToggleBar();
        }

        // 3. 텍스트 처리
        if (!string.IsNullOrEmpty(step.dialogueKey))
        {
            _dialogueLocalizedString.TableEntryReference = step.dialogueKey;
        }
        else
        {
            textDisplay.text = "";
            isTyping = false;
        }

        // 4. 이미지 처리 (★ 수정된 부분)
        if (step.background != null && step.background != currentActiveImage)
        {
            if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

            // currentIndex == 0 (맨 처음)이거나, instantChange가 켜져 있으면 -> 즉시 전환
            if (currentIndex == 0 || step.instantChange)
            {
                ChangeImageInstantly(currentActiveImage, step.background);
            }
            else
            {
                // 그 외에는 부드럽게 페이드 전환
                transitionCoroutine = StartCoroutine(TransitionImages(currentActiveImage, step.background));
            }

            currentActiveImage = step.background;
        }
    }

    IEnumerator PlayEndingCredits()
    {
        CheckEndingAchievement();
        CheckDeathAchievement();
        SetGameClear();

        //BGM 변경: 타이틀 씬
        if (SoundManager.Instance != null)
            SoundManager.Instance.FadeBGMVolume(1f,0.2f);
        //SoundManager.Instance.Play(SoundID.BGM_Title);

        if (endingCreditPanel != null)
        {
            endingCreditPanel.SetActive(true);
        }

        if (creditContent != null)
        {
            Vector2 startPos = creditContent.anchoredPosition;
            startPos.y = creditStartY;
            creditContent.anchoredPosition = startPos;
        }

        yield return new WaitForSeconds(creditStartDelay);

        if (creditContent != null)
        {
            while (creditContent.anchoredPosition.y < creditEndY)
            {
                float speedMultiplier = 1f;

                // 좌클릭 유지 시 가속
                if (Input.GetMouseButton(0))
                    speedMultiplier = fastScrollMultiplier;

                creditContent.anchoredPosition +=
                    Vector2.up * creditScrollSpeed * speedMultiplier * Time.deltaTime;

                yield return null;
            }
        }



        Debug.Log("크레딧 종료 -> 최종 패널 표시");
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(true);

            if (SoundManager.Instance != null)
                SoundManager.Instance.Play(SoundID.UI_Popup);
        }
    }

    // ... (이하 기존 함수들) ...

    IEnumerator TypewriterEffect()
    {
        isTyping = true;
        textDisplay.text = currentFullText;
        textDisplay.maxVisibleCharacters = 0;
        int totalLength = currentFullText.Length;
        for (int i = 0; i <= totalLength; i++)
        {
            textDisplay.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typeSpeed);
        }
        isTyping = false;
    }

    void ChangeImageInstantly(Image outgoing, Image incoming)
    {
        if (outgoing != null) outgoing.gameObject.SetActive(false);
        if (incoming != null)
        {
            incoming.gameObject.SetActive(true);
            Color c = incoming.color;
            c.a = 1f; // 알파값 1 (완전 불투명)
            incoming.color = c;
        }
    }

    IEnumerator TransitionImages(Image outgoing, Image incoming)
    {
        float timer = 0f;
        if (incoming != null)
        {
            incoming.gameObject.SetActive(true);
            Color inColor = incoming.color;
            inColor.a = 0f;
            incoming.color = inColor;
        }
        Color outColor = Color.white;
        if (outgoing != null) outColor = outgoing.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            if (outgoing != null)
            {
                outColor.a = Mathf.Lerp(1f, 0f, progress);
                outgoing.color = outColor;
            }
            if (incoming != null)
            {
                Color inColor = incoming.color;
                inColor.a = Mathf.Lerp(0f, 1f, progress);
                incoming.color = inColor;
            }
            yield return null;
        }

        if (outgoing != null) outgoing.gameObject.SetActive(false);
        if (incoming != null)
        {
            Color c = incoming.color;
            c.a = 1f;
            incoming.color = c;
        }
    }
    private void QuitGame()
    {
        SaveService.DeleteSave();
        Debug.Log("모든 세이브 데이터가 삭제되었습니다.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void GoToTitle()
    {
        Time.timeScale = 1f;
        ServiceLocator.Instance.Scene.Load(titleScene);
    }

    private void SetGameClear()
    {
        Debug.Log("[OutroManager] 게임 클리어 처리");
        ServiceLocator.Instance.GlobalData.IsGameCleared = true;
        GlobalSaveService.Save(ServiceLocator.Instance.GlobalData);
        SaveService.DeleteSave();
    }

    #region Achievement Methods
    private void CheckDeathAchievement()
    {
        UserGameData userData = ServiceLocator.Instance.CurrentUser;
        if (userData == null) return;

        if (userData.Difficulty == Difficulty.Easy && userData.GameOverCount == 0)
        {
            string achievementKey = "NEW_ACHIEVEMENT_9_0";
            SteamAchievementManager.Unlock(achievementKey);
        }
    }

    private void CheckEndingAchievement()
    {
        UserGameData userData = ServiceLocator.Instance.CurrentUser;
        if (userData == null) return;
        string achievementKey = "NEW_ACHIEVEMENT_7_0";
        if (userData.Difficulty == Difficulty.Hard)
        {
            achievementKey = "NEW_ACHIEVEMENT_8_0";
        }
        SteamAchievementManager.Unlock(achievementKey);
    }

    #endregion
}