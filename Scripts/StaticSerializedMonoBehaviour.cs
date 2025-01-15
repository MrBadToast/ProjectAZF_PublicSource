using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticSerializedMonoBehaviour<T> : SerializedMonoBehaviour 
    where T : SerializedMonoBehaviour
{
    // =====================================

    //  !!매우 중요!!
    //  이 클래스를 상속받으면 SerializedMonoBehaviour를 사용하는 싱글턴 클래스가 됩니다.
    //  이 클래스는 DontDestroyOnLoad에 포함되지 않습니다.
    //  반드시 이 함수를 상속받고 Awake()를 사용할 때 아래 Awake()를 오버라이딩하고 base.Awake()를 사용하세요!
    //
    //  예시)
    //  protected override void Awake()
    //  {
    //      base.Awake();
    //
    //      input = new MainPlayerInputActions();
    //  }
    //
    // =====================================


    [ShowInInspector, ReadOnly,LabelText("INSTANCE OBJECT"),InfoBox("THIS OBJECT IS SINGLETON")]
    private string debug_static_objcect;

    static private T instance;
    static public T Instance { get { return instance; } }                           // 싱글턴 인스턴스를 받아옵니다.
    static public bool IsInstanceValid { get { return instance != null; } }         // 현재 인스턴스가 정상적으로 존재하는지 확인합니다.

    protected virtual void Awake()
    {
        if(instance == null) { instance = this as T; debug_static_objcect = gameObject.name; }
        else { Debug.LogWarning(typeof(T).Name + " : Duplicated SingletonObject, "+ gameObject.name + " : This Object Will be Destroyed."); Destroy(gameObject); }
    }

}
