using UnityEngine;

public class ActionCommand : IShortcutCommand
{
    public string Id { get; }
    private readonly System.Func<bool> _can;
    private readonly System.Action _exec;

    public ActionCommand(string id, System.Action exec, System.Func<bool> can = null)
    {
        Id = id;
        _exec = exec;
        _can = can;
    }

    public bool CanExecute() => _exec != null && (_can?.Invoke() ?? true);
    public void Execute(float dt) => _exec();
}