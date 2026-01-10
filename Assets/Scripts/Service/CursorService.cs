using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class CursorAnimation
{
    public string Name;
    public Texture2D[] Frames;
    public float FPS = 10f;
    public Vector2 Hotspot = Vector2.zero;
}

public class CursorService 
{
    private readonly MonoBehaviour _owner;
    private readonly List<CursorAnimation> _animations;
    private Coroutine _activeCoroutine;

    public CursorService(MonoBehaviour owner, List<CursorAnimation> animations)
    {
        _owner = owner;
        _animations = animations;
    }

    public void StartAnimation(string animName)
    {
        CursorAnimation anim = _animations.Find(a => a.Name == animName);
        if (anim == null) return;

        StopAnimation();
        _activeCoroutine = _owner.StartCoroutine(PlayAnimation(anim));
    }

    public void StopAnimation()
    {
        if (_activeCoroutine != null)
        {
            _owner.StopCoroutine(_activeCoroutine);
            _activeCoroutine = null;
        }
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private IEnumerator PlayAnimation(CursorAnimation anim)
    {
        int frame = 0;
        while (true)
        {
            Cursor.SetCursor(anim.Frames[frame], anim.Hotspot, CursorMode.Auto);
            frame = (frame + 1) % anim.Frames.Length;
            yield return new WaitForSecondsRealtime(1f / anim.FPS);
        }
    }

    public void SetCursorConfined(bool isConfined)
    {
        Cursor.lockState = isConfined ? CursorLockMode.Confined : CursorLockMode.None;
    }

}
