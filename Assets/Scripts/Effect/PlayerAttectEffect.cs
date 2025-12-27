using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [SerializeField] private GameObject swordAttackFX;
    [SerializeField] private GameObject bowAttackFX;
    [SerializeField] private Vector3 offset;

    public void SpawnSwordAttackEffect()
    {
        Vector3 spawnPos = offset;
        Instantiate(swordAttackFX, spawnPos, Quaternion.identity);
    }

    public void SpawnBowAttackEffect()
    {
        Vector3 spawnPos = offset;
        Instantiate(bowAttackFX, spawnPos, Quaternion.identity);
    }
}
