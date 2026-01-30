using UnityEngine;

public class JungleRogo : MonoBehaviour
{
    [SerializeField]
    private AudioSource _audioSource;

    public void Play_Click()
    {
        _audioSource.Play();
    }
}
