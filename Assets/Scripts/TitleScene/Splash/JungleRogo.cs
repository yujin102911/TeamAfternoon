using UnityEngine;

public class JungleRogo : MonoBehaviour
{
    [SerializeField]
    private AudioSource _start;
    [SerializeField]
    private AudioSource _end;

    public void Play_Click_start()
    {
        _start.Play();
    }

    public void Play_Click_end()
    {
        _end.Play();
    }
}
