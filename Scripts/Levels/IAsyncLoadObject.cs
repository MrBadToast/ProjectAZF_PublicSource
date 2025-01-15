using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 월드에서 유동적으로 로드/언로드 되는 오브젝트들을 관리하기 위한 인터페이스 입니다.
/// </summary>
public interface IAsyncLoadObject
{
    /// <summary>
    /// 오브젝트 로드 시
    /// </summary>
    public void OnObjectLoaded();

    /// <summary>
    /// 오브젝트 언로드 시
    /// </summary>
    public void OnObjectUnloaded();
}
