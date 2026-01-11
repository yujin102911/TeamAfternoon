using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class Action_info
{
    public ActionType ActionType;

    public string Name_key;
    public string Desc_Key;
    
}

[CreateAssetMenu(fileName = "New ActionData", menuName = "Data/Action")]
public class ActionData : ScriptableObject
{
    [Header("액션 정보")]
    [SerializeField]
    private List<Action_info> _actionDB = new List<Action_info>();

    public List<Action_info> Action_DB => _actionDB;

    public Action_info GetAction_Info(ActionType actionType) 
    {
        Action_info info = _actionDB.Find(x => x.ActionType == actionType);

        if (info == null)
        {
            Debug.LogWarning($"[ActionData] ActionType {actionType}에 대한 정보가 없습니다.");
        }

        return info;
    }
}
