using UnityEngine;

[ExecuteAlways]
public class CameraRectSanitizer : MonoBehaviour
{
    void OnEnable()
    {
        var cam = GetComponent<Camera>();
        if (cam == null) return;

        if (float.IsNaN(cam.rect.x) ||
            float.IsNaN(cam.rect.y) ||
            float.IsNaN(cam.rect.width) ||
            float.IsNaN(cam.rect.height))
        {
            Debug.LogError("[CameraRectSanitizer] NaN detected. Resetting Camera.rect", this);
            cam.rect = new Rect(0, 0, 1, 1);
        }
    }
}
