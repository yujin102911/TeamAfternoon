using UnityEngine;

[CreateAssetMenu(fileName = "New RelicData", menuName = "Data/RelicData")]
public class RelicData : ScriptableObject
{
    [Header("유물 ID")]
    [SerializeField]
    private int _relicID;

    [Header("유물 이름")]
    [SerializeField]
    private string _relicName;

    [Header("유물 설명")]
    [SerializeField]
    private string _relicDescription;

    //유물효과 추가 필요

    public int Relic_ID => _relicID;
    public string Relic_Name => _relicName;
    public string Relic_Description => _relicDescription;
}
