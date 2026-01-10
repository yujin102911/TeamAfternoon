using System.Collections;
using UnityEngine;

public class Nail : MonoBehaviour
{
    [Header("행동변경 주기")]
    [SerializeField]
    private float _changeDuration = 2.0f;

    [SerializeField]
    private Animator animator;
    [SerializeField]
    private int _maxIdleCount;
    

    private int _currentIdle = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(ChangeIdle());
    }

    IEnumerator ChangeIdle()
    {
        while (true) 
        {
            yield return new WaitForSecondsRealtime(_changeDuration);
            _currentIdle = Random.Range(0, _maxIdleCount);

            switch (_currentIdle)
            {
                case 0:
                    ChangeAnimation("Idle_0");
                    break;
                case 1:
                    ChangeAnimation("Idle_1");
                    break;
                case 2:
                    ChangeAnimation("Idle_2");
                    break;
            }
        }

    }

    private void ChangeAnimation(string anim, float crossfade = 0.2f)
    {
        animator.CrossFade(anim, crossfade);
    }
}
