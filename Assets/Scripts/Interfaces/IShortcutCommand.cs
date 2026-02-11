using UnityEngine;

public interface IShortcutCommand
{
    string Id { get; }
    bool CanExecute();
    void Execute(float dt); // Once면 dt 무시하면 됨
}
