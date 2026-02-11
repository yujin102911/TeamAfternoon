using UnityEngine;
using UnityEngine.UI;

public class ScrollMoveCommand : IShortcutCommand
{
    public string Id { get; }
    private readonly System.Func<ScrollRect> _getScroll;
    private readonly float _speed;

    public ScrollMoveCommand(string id,
                              System.Func<ScrollRect> getScroll,
                              float speed)
    {
        Id = id;
        _getScroll = getScroll;
        _speed = speed;
    }

    public bool CanExecute()
    {
        var scroll = _getScroll();
        return scroll != null && scroll.gameObject.activeInHierarchy;
    }

    public void Execute(float dt)
    {
        var scroll = _getScroll();
        if (scroll == null) return;

        var content = scroll.content;
        var viewport = scroll.viewport;

        if (content == null || viewport == null)
            return;

        float contentHeight = content.rect.height;
        float viewHeight = viewport.rect.height;

        float scrollable = contentHeight - viewHeight;
        if (scrollable <= 0f)
            return;

        float pixelMove = _speed * dt;
        float normalizedMove = pixelMove / scrollable;

        float v = scroll.verticalNormalizedPosition;
        v += normalizedMove;

        scroll.verticalNormalizedPosition = Mathf.Clamp01(v);
    }
}
