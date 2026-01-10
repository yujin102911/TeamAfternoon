using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FixedAspectCamera : MonoBehaviour
{
    public float targetAspect = 16f / 9f;

    private Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
        UpdateCameraRect();
    }

    void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        UpdateCameraRect();
    }

    void Update()
    {
        // 창 크기 바뀌는 에디터 / 런타임 대응
        UpdateCameraRect();
    }

    void UpdateCameraRect()
    {
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // 위아래 검은 여백 (Letterbox)
            cam.rect = new Rect(
                0f,
                (1f - scaleHeight) / 2f,
                1f,
                scaleHeight
            );
        }
        else
        {
            // 좌우 검은 여백 (Pillarbox)
            float scaleWidth = 1f / scaleHeight;

            cam.rect = new Rect(
                (1f - scaleWidth) / 2f,
                0f,
                scaleWidth,
                1f
            );
        }
    }
}
