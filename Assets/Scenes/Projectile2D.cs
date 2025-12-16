using UnityEngine;

public class Projectile2D : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 2f;

    private Vector3 dir;

    public void Fire(Vector3 from, Vector3 to)
    {
        transform.position = from;

        Vector3 d = to - from;
        d.z = 0f;

        dir = d.sqrMagnitude > 0.0001f ? d.normalized : Vector3.right;

        // 스프라이트가 오른쪽(+X)을 향하고 있다는 가정
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        CancelInvoke();
        Invoke(nameof(Despawn), lifeTime);
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 여기서 타겟 판정하면 됨
        // 예: if (other.CompareTag("Enemy")) { ... }
        Destroy(gameObject);
    }
}
