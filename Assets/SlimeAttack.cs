using UnityEngine;

public class SlimeAttack : MonoBehaviour
{
    public GameObject attackFxPrefab;

    public void SpawnAttackEffect()
    {
        Vector3 spawnPos = transform.position + transform.right * -5f;
        Instantiate(attackFxPrefab, spawnPos, Quaternion.identity);
    }
}
