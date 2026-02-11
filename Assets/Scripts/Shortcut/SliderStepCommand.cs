public class SliderStepCommand : IShortcutCommand
{
    public string Id { get; }

    private readonly StepSlider _slider;
    private readonly int _delta;

    public SliderStepCommand(string id, StepSlider slider, int delta)
    {
        Id = id;
        _slider = slider;
        _delta = delta;
    }

    public bool CanExecute()
        => _slider != null && _slider.gameObject.activeInHierarchy;

    public void Execute(float dt)
    {
        if (GameManager.Instance != null) 
        { 
            if (!GameManager.Instance.IsGameStarted || GameManager.Instance.IsExecutingRound || GameManager.Instance.IsBattleEnded) 
            {
                return;
            }
        }


        _slider.MoveStepRepeated(_delta, dt);
    }
}
