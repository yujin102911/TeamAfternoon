using System;
using UnityEngine;

public class QuestOptionState
{
    public bool IsHit { get; private set; }
    public bool IsEight { get; private set; }
    public bool IsTwice { get; private set; }

    public int Attack_Count = 0;

    public event Action<QuestOptionState> OnChanged;

    public void Initialize()
    {
        IsHit = false;
        IsEight = false;
        IsTwice = false;
        Attack_Count = 0;
        OnChanged?.Invoke(this);
    }

    // 값 변경 메서드
    public void SetHit(bool value)
    {
        if (IsHit == value) return;
        IsHit = value;
        OnChanged?.Invoke(this);
    }

    public void SetEight(bool value)
    {
        if (IsEight == value) return;
        IsEight = value;
        OnChanged?.Invoke(this);
    }

    public void SetTwice(bool value)
    {
        if (IsTwice == value) return;
        IsTwice = value;
        OnChanged?.Invoke(this);
    }

    public void IncreaseCount()
    {
        Attack_Count++;

        if (Attack_Count >= 2)
        {
            SetTwice(true);
        }
        else
        {
            SetTwice(false);
        }
    }
}
