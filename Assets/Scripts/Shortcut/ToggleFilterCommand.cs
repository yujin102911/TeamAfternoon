using UnityEngine.UI;

public class ToggleFilterCommand : IShortcutCommand
{
    public string Id { get; }

    private readonly Toggle _toggle;

    public ToggleFilterCommand(string id, Toggle toggle)
    {
        Id = id;
        _toggle = toggle;
    }

    public bool CanExecute()
        => _toggle != null && _toggle.gameObject.activeInHierarchy;

    public void Execute(float dt)
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsGameStarted || GameManager.Instance.IsBattleEnded)
        {
            return;
        }

        if(CardTooltip.Instance != null)
        {
            CardTooltip.Instance.Hide();
        }

        _toggle.isOn = !_toggle.isOn;
    }
}
