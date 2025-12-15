using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyDamageEffect : MonoBehaviour
{
    public static EnemyDamageEffect Instance;
    public GameObject textPrefab; // 생성될 프리팹

    [Header("점멸 설정")]
    public float blinkDuration = 0.3f;
    public Color EnemyHitColor = Color.red;

    [Header("텍스트 설정")]
    public float textLifetime = 1.0f;
    public float floatUpSpeed = 1.0f;

    SpriteRenderer sr;
    private Color originalColor;
    private bool isBlinking = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        if (Instance == null ) Instance = this;
        else Destroy(gameObject);
    }

    public IEnumerator ShowEnemyDamage()
    {
        int damage = 0;

        SpawnDamageText( damage );

        isBlinking = true;
        sr.color = EnemyHitColor;
        yield return new WaitForSeconds(blinkDuration);
        sr.color = originalColor;

        isBlinking = false;
    }

    private void SpawnDamageText(int damage)
    {
        if (textPrefab == null) return;
        GameObject go = Instantiate(textPrefab, transform.position, Quaternion.identity);

        TextMeshProUGUI tmp = go.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null )
        {
            tmp.text = damage.ToString();
        }
        StartCoroutine(DestroyDamageText(go));
    }

    private IEnumerator DestroyDamageText(GameObject go)
    {
        float timer = 0f;
        while (timer < textLifetime)
        {
            timer += Time.deltaTime;
            go.transform.position += Vector3.up * floatUpSpeed * Time.deltaTime;

            yield return null;
        }
        Destroy(go);
    }


}
