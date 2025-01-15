using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[CreateAssetMenu(fileName = "NewGlobalParameterSettings", menuName = "NewGlobalParameterSettings")]
public class GlobalParameterSettings : SerializedScriptableObject
{
    [SerializeField] private Dictionary<string, int> settings;
    public Dictionary<string, int> Settings { get { return settings; } }
}