using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType
{
    None,
    Critical,
    Duble_Dash,
    Damage_Up,
    Sturn
}


[Serializable]
public class Additional_Effect
{
    public string effectName;
    public string effectDescription;
    public EffectType effectType;
    public Color effectColor;
    public int cost;
    public ActionType[] Apply_actionTypes;
}

[CreateAssetMenu(fileName = "New EffectData", menuName = "Data/Effect")]
public class EffectData : ScriptableObject
{
    [Header("특수효과 정보(순서대로 출력함)")]
    [SerializeField]
    private List<Additional_Effect> _effectDB = new List<Additional_Effect>();

    public List<Additional_Effect> Effect_DB => _effectDB;
}
