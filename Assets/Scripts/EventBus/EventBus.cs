using System;
using System.Collections.Generic;

public static class EventBus
{
    private static Dictionary<Type, List<Delegate>> _listeners
        = new Dictionary<Type, List<Delegate>>();

    public static void Subscribe<T>(Action<T> callback)
    {
        Type t = typeof(T);

        if (_listeners.ContainsKey(t) == false)
            _listeners[t] = new List<Delegate>();

        _listeners[t].Add(callback);
    }

    public static void Unsubscribe<T>(Action<T> callback)
    {
        Type t = typeof(T);

        if (_listeners.TryGetValue(t, out var list))
        {
            list.Remove(callback);

            // 리스너가 비었으면 타입 키 자체 제거 (선택사항이지만 추천)
            if (list.Count == 0)
                _listeners.Remove(t);
        }
    }


    public static void Publish<T>(T eventData)
    {
        Type t = typeof(T);

        if (_listeners.TryGetValue(t, out var list))
        {
            foreach (var callback in list)
                (callback as Action<T>)?.Invoke(eventData);
        }
    }
}
