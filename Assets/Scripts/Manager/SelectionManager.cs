using System.Collections.Generic;
using UnityEngine;

public class SelectionManager<T>
{
    public HashSet<T> Selected = new();
    public T Focused { get; private set; }

    public void SelectSingle(T item)
    {
        Selected.Clear();
        Selected.Add(item);
        Focused = item;
    }

    public void Toggle(T item)
    {
        if (Selected.Contains(item))
            Selected.Remove(item);
        else
            Selected.Add(item);

        Focused = item; // 마지막 클릭은 항상 포커스
    }

    public void Clear()
    {
        Selected.Clear();
        Focused = default;
    }
}

