using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 게임 한 판 끝나고 뜨는 패널 관리용
/// </summary>
public class GameResultUI : MonoBehaviour
{
    [Header("패널")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private GameObject _defeatPanel;

    [Header("버튼")]
    [SerializeField] private Button _goToMainBtn;
    [SerializeField] private string _librarySceneName;

    private void Start()
    {
        _victoryPanel.SetActive(false);
        _defeatPanel.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBattleEnded += HandleBattleEnded;
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnBattleEnded -= HandleBattleEnded;
        }
    }

    private void HandleBattleEnded(bool isVictory)
    {
        if (isVictory)
        {
            DealingWithVictory();
        }
        else
        {
            DealingWithDefeat();
        }
    }

    private void DealingWithVictory()
    {
        Debug.Log("승리 패널 활성화");
        _victoryPanel.SetActive(true);
        // TODO: 승리 처리
    }

    private void DealingWithDefeat()
    {
        Debug.Log("패배 패널 활성화");
        _defeatPanel.SetActive(true);
        // TODO: 패배 처리
    }

    public void GoToLibrary()
    {
        Debug.Log("고투 라이브러리");
        ServiceLocator.Instance.Scene.Load(_librarySceneName);
    }

}
