using UnityEngine;
using TMPro;
using System.Collections;

public class Credit : MonoBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI creditText;      // 개발자 이름 출력용
    public TextMeshProUGUI shutdownText;    // '이제 컴퓨터를 끄셔도 됩니다' 문구용

    [Header("Settings")]
    [TextArea(10, 20)]
    public string fullCredits;              // 여기에 전체 개발자 명단을 작성하세요.
    public float typingSpeed = 0.05f;       // 한 글자당 출력 속도
    public float cursorBlinkSpeed = 0.5f;   // 커서 깜빡임 속도
    public float waitBeforeShutdown = 2.0f; // 크레딧 종료 후 종료 문구까지 대기 시간

    void Start()
    {
        // 초기화
        creditText.text = "";
        shutdownText.gameObject.SetActive(false);

        // 크레딧 시작
        StartCoroutine(PlayCredits());
    }

    IEnumerator PlayCredits()
    {
        string currentText = "";

        // 1. 타이핑 효과 (커서가 항상 뒤에 붙음)
        foreach (char letter in fullCredits.ToCharArray())
        {
            currentText += letter;
            creditText.text = currentText + "_";
            yield return new WaitForSeconds(typingSpeed);
        }

        // 2. 타이핑 종료 후 커서 깜빡임 연출 (설정한 대기 시간 동안)
        float timer = 0;
        bool cursorVisible = true;
        while (timer < waitBeforeShutdown)
        {
            creditText.text = currentText + (cursorVisible ? "_" : "");
            cursorVisible = !cursorVisible;
            yield return new WaitForSeconds(cursorBlinkSpeed);
            timer += cursorBlinkSpeed;
        }

        // 3. 크레딧 창을 끄고 시스템 종료 문구 출력
        creditText.gameObject.SetActive(false);
        shutdownText.text = "It's now safe to turn off your game.";
        shutdownText.color = new Color(0.8f, 0.4f, 0f); // 윈도우 98 특유의 주황색
        shutdownText.gameObject.SetActive(true);
    }
}