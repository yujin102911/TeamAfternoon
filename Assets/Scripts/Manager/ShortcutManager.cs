using System;
using System.Collections.Generic;
using UnityEngine;

public enum TriggerType
{
    Once,   // GetKeyDown
    Hold    // GetKey
}

public class ShortcutManager : MonoBehaviour
{
    public static ShortcutManager Instance { get; private set; }

    [System.Serializable]
    public class Binding
    {
        public KeyCode key;
        public bool ctrl;
        public bool shift;
        public bool alt;
        public string commandId;
        public TriggerType trigger = TriggerType.Once;
    }

    [SerializeField] private List<Binding> bindings = new();
    private readonly Dictionary<string, IShortcutCommand> _commands = new();

    public event Action Onrelease;

    public void Register(IShortcutCommand cmd) => _commands[cmd.Id] = cmd;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        float dt = Time.unscaledDeltaTime;

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D))
        {
            Onrelease?.Invoke();
        }

        foreach (var b in bindings)
        {
            bool pressed =
                (b.trigger == TriggerType.Once) ? Input.GetKeyDown(b.key)
                                                : Input.GetKey(b.key);

            if (!pressed) continue;

            if (b.ctrl && !(Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) continue;
            if (b.shift && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) continue;
            if (b.alt && !(Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))) continue;

            if (_commands.TryGetValue(b.commandId, out var cmd) && cmd.CanExecute())
            {
                cmd.Execute(dt);
            }
        }
    }
}
