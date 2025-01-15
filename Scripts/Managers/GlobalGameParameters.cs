using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalGameParameters : StaticSerializedMonoBehaviour<GlobalGameParameters>
{
    //====================================
    //
    // [싱글턴 오브젝트]
    // 글로벌 패러미터는 게임에서 각종 변수가 생겼을 때, 다른 곳에서도 값을 참조할 수 있도록 문자열 값으로 저장하여 전역으로 저장하는 데이터들입니다.
    // 새로운 글로벌 패러미터를 추가하고자 한다면, Asset/ScriptableObjects에 있는 GlobalParameterSettings 스크립터블 오브젝트에 추가하고
    // 아래 노션 리스트에 패러미터 관련 정보를 적어두세요.
    // https://www.notion.so/badtoast/662cb4e0b4154db5a40b76c0af61c4e7?pvs=4
    //
    //====================================

    [SerializeField] private GlobalParameterSettings settingsAsset;
    [SerializeField, ReadOnly] private Dictionary<string, int> data;

    /// <summary>
    /// 글로벌 패러미터의 데이터들입니다.
    /// </summary>
    public Dictionary<string, int> Data { get { return data; } }

    protected override void Awake()
    {
        base.Awake();
        if (settingsAsset != null)
        {
            data = new Dictionary<string, int>();
            foreach (KeyValuePair<string, int> kvp in settingsAsset.Settings)
            {
                data.Add(kvp.Key, kvp.Value);
            }
        }
    }

    /// <summary>
    /// 글로벌 패러미터를 설정합니다.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool SetGlobalParameter(string key, int value)
    {
        if (data.ContainsKey(key))
        {
            Debug.Log("글로벌 패러미터 설정됨 | " + key + " = " + value);
            data[key] = value;
            return true;
        }
        else
            return false;
    }
}
