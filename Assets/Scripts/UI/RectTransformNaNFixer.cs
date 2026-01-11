using UnityEngine;

public class RectTransformNaNFixer : MonoBehaviour
{
    void Awake()
    {
        FixAll();
    }

    [ContextMenu("Fix All RectTransforms")]
    void FixAll()
    {
        RectTransform[] rects = FindObjectsOfType<RectTransform>(true);

        foreach (var rt in rects)
        {
            Vector3 pos = rt.localPosition;

            if (float.IsNaN(pos.x) || float.IsInfinity(pos.x))
            {
                pos.x = 0f;
            }

            if (float.IsNaN(pos.y) || float.IsInfinity(pos.y))
            {
                pos.y = 0f;
            }

            rt.localPosition = pos;
        }

        Debug.Log("RectTransform NaN Fix Applied");
    }
}
