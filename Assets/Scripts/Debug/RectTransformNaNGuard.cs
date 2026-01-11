using UnityEngine;

[ExecuteAlways]
public class RectTransformNaNGuard : MonoBehaviour
{
    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (rt == null) return;

        if (HasNaN(rt))
        {
            Debug.LogError(
                $"[NaN DETECTED] {name}\n{GetComponentPath(transform)}",
                this
            );

            Debug.Break(); // 에디터 일시정지
        }
    }

    bool HasNaN(RectTransform r)
    {
        return float.IsNaN(r.anchoredPosition.x)
            || float.IsNaN(r.anchoredPosition.y)
            || float.IsNaN(r.sizeDelta.x)
            || float.IsNaN(r.sizeDelta.y)
            || float.IsNaN(r.localScale.x)
            || float.IsNaN(r.localScale.y);
    }

    string GetComponentPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
