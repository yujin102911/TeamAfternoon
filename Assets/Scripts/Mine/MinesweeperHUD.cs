using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinesweeperHUD : MonoBehaviour
{
    [SerializeField]
    private Image _faceIcon;
    [SerializeField] private TextMeshProUGUI minesText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Icons")]
    [SerializeField] private Sprite faceSmile;
    [SerializeField] private Sprite faceWin;
    [SerializeField] private Sprite faceLose;

    private float elapsed;
    private bool running;

    public void ResetHUD(int totalMines)
    {
        elapsed = 0f;
        running = false;
        SetFaceSmile();
        SetMinesRemaining(totalMines);
        UpdateTimerText();
    }

    public void SetMinesRemaining(int remaining)
    {
        remaining = Mathf.Clamp(remaining, -99, 999);

        if (remaining >= 0)
            minesText.text = remaining.ToString("000");
        else
            minesText.text = "-" + Mathf.Abs(remaining).ToString("00");
    }

    public void SetFaceSmile() => _faceIcon.sprite = faceSmile;
    public void SetFaceWin() => _faceIcon.sprite = faceWin;
    public void SetFaceLose() => _faceIcon.sprite = faceLose;

    public void StartTimer() => running = true;
    public void StopTimer() => running = false;

    private void Update()
    {
        if (!running) return;

        elapsed += Time.deltaTime;
        UpdateTimerText();
    }

    private void UpdateTimerText()
    {
        // 초 단위 정수로 표시(지뢰찾기 감성)
        int sec = Mathf.FloorToInt(elapsed);
        timerText.text = sec.ToString("000");
    }
}
