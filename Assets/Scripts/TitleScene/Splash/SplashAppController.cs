using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class SplashAppController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private Animator _contentAnimator;

    [Header("타이밍 세팅")]
    [SerializeField] private float _initialDelay = 2.0f;
    [SerializeField] private float _delayBetweenLogos = 1.5f;
    [SerializeField] private float _finalDelay = 1.0f;

    [Header("타이틀 씬")]
    [SerializeField] private string _titleSceneName = "TitleScene";

    private bool _isSystemInitialized = false;

    private void Start()
    {
        StartCoroutine(InitializeSystems());

        StartCoroutine(PlaySplashSequence());
    }

    private IEnumerator InitializeSystems()
    {
        DataRepository repo = DataRepository.Instance;
        if (repo != null) Debug.Log("[Splash] DataRepository 로드 완료");

        SaveService.PeekSaveData();

        // 여기에 DataRepository 로딩 등 추가 가능
        yield return new WaitForSeconds(1.5f); // 최소 로딩 시간

        _isSystemInitialized = true;
    }

    private IEnumerator PlaySplashSequence()
    {
        yield return new WaitForSeconds(_initialDelay);

        _contentAnimator.SetTrigger("PlayJungle");
        yield return StartCoroutine(WaitUntilAnimationEnd("JungleLogo"));

        yield return new WaitForSeconds(_delayBetweenLogos);

        _contentAnimator.SetTrigger("PlayTeam");
        yield return StartCoroutine(WaitUntilAnimationEnd("TeamLogo"));

        yield return new WaitForSeconds(_finalDelay);

        while (!_isSystemInitialized)
        {
            yield return null;
        }

        Debug.Log("[Splash] 모든 시퀀스 종료. 타이틀로 이동합니다.");
        ServiceLocator.Instance.Scene.Load(_titleSceneName);
    }

    private IEnumerator WaitUntilAnimationEnd(string stateName)
    {
        while (!_contentAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null;
        }
        AnimatorStateInfo stateInfo = _contentAnimator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.IsName(stateName) && stateInfo.normalizedTime < 0.99f) 
        {
            stateInfo = _contentAnimator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        }
        Debug.Log($"[Splash] {stateName} 애니메이션 완료");
    }

}