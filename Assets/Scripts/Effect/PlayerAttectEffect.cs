using UnityEngine;

public class PlayerAttectEffect : MonoBehaviour
{
    [SerializeField] private GameObject swordAttackFX;
    [SerializeField] private GameObject bowAttackFX;
    [SerializeField] private GameObject bowChargingFX;

    [SerializeField] private Vector3 attackOffset;
    [SerializeField] private Vector3 chargingOffset;


    public void SpawnSwordAttackEffect()
    {
        Vector3 spawnPos = attackOffset;
        Instantiate(swordAttackFX, spawnPos, Quaternion.identity);
    }

    public void SpawnBowAttackEffect()
    {
        Vector3 spawnPos = attackOffset;
        Instantiate(bowAttackFX, spawnPos, Quaternion.identity);
    }

    public void SpawnBowChargingEffect()
    {
        Vector3 spawnPos = chargingOffset;
        Instantiate(bowChargingFX, spawnPos, Quaternion.identity);
    }
}
