using UnityEngine;

public class FloatObject : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("얼마나 높이 움직일건지")]
    public float floatStrength = 0.5f;
    [Tooltip("얼마나 빨리 움직일건지")]
    public float floatSpeed = 1.0f;

    private Vector3 startPos;

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        float newY = startPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatStrength);

        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }

}
