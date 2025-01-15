using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BindFromSequences : StaticSerializedMonoBehaviour<BindFromSequences>
{
    public Dictionary<string, UnityEvent> eventBindings;

    public void Invoke(string Key)
    {
        if(eventBindings != null)
        {
            if (!eventBindings.ContainsKey(Key)) { Debug.LogWarning("BindFromSequences : " + Key + " 를 찾을 수 없었습니다."); return; } 
            eventBindings[Key].Invoke();
        }
    }
}
