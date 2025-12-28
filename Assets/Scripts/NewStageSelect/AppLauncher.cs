using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class AppLauncher : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private List<GameObject> _progressBlocks = new List<GameObject>();
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("설정")]
    [SerializeField] private float _minLoadingTime = 2.5f;
    [SerializeField] private AnimationCurve _speedCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Flash 연출")]
    [SerializeField] private Image _flashImage;
    [SerializeField] private float _flashDuration = 0.1f;

    private string[] _loadingMessages =
    {
        "Initializing Kerner...",
        "Loading System Files...",
        "Checking Registry...",
        "Allocating Memory Assets...",
        "Verifying Security Token...",
        "Establishing Connection...",
        "Syncing Database...",
        "Loading Pixel Shaders...",
        "Running Startup Scripts...",
    };

    public void LaunchBattle(string sceneName)
    {
        _loadingPanel.SetActive(true);
        foreach (var block in _progressBlocks) block.SetActive(false);

        AsyncOperation op = ServiceLocator.Instance.Scene.LoadAsync(sceneName);

        StartCoroutine(UpdateLoadingBlocks(op));
    }

    private IEnumerator UpdateLoadingBlocks(AsyncOperation op)
    {
        op.allowSceneActivation = false;

        float elapsed = 0f;
        float messageTimer = 0f;
        int totalBlocks = _progressBlocks.Count;

        while (elapsed < _minLoadingTime || op.progress < 0.9f)
        {
            elapsed += Time.deltaTime;
            messageTimer += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / _minLoadingTime);
            float progress = _speedCurve.Evaluate(t);

            float combinedProgress = Mathf.Min(progress, op.progress / 0.9f);
            int currentBlockIndex = Mathf.FloorToInt(combinedProgress * totalBlocks);

            for (int i = 0; i <= currentBlockIndex && i < totalBlocks; i++)
            {
                if (!_progressBlocks[i].activeSelf)
                    _progressBlocks[i].SetActive(true);
            }

            if (messageTimer >= 0.4f)
            {
                _statusText.text = _loadingMessages[Random.Range(0, _loadingMessages.Length)];
                messageTimer = 0f;
            }

            yield return null;

        }
        foreach (var block in _progressBlocks) block.SetActive(true);
        _statusText.text = "Starting Application...";
        yield return new WaitForSeconds(0.3f);

        if (_flashImage != null)
        {
            _flashImage.gameObject.SetActive(true);
            float flashTimer = 0f;
            Color c = _flashImage.color;

            while (flashTimer < _flashDuration)
            {
                flashTimer += Time.deltaTime;
                c.a = Mathf.Lerp(0, 1, flashTimer / _flashDuration);
                _flashImage.color = c;
                yield return null;
            }
        }

        op.allowSceneActivation = true;
    }

}
