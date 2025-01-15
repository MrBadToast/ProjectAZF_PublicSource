using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundMaterial
{
    // 사운드 재질의 종류
    Default,
    Sand,
    Water,
    Grass,
    Wood
}

public class SoundMaterialBehavior : MonoBehaviour
{
    //============================================
    //
    // 현재 땅의 재질을 나타내기 위한 클래스 입니다.
    // 사운드를 내는 오브젝트가 SoundMaterial 정보를 필요로 할 때, GetSoundMaterial을 통해 어떤 사운드 재질 정보를 얻어야 하는지 결정합니다.
    // 이는 Terrain 같은 다중 SoundMaterial을 필요로 하는 오브젝트 같은 곳에도 필요합니다. (TerrainSoundMaterialBehavior.cs 참고)
    //
    //============================================

    [SerializeField] private SoundMaterial soundmat;

    public virtual SoundMaterial GetSoundMaterial() 
    // 현재 soundMat반환
    {
        return soundmat;
    }

    public virtual SoundMaterial GetSoundMaterial(Vector3 position) 
    // position : 소리나는곳의 위치, 오버라이드 되지 않을 시 현재 soundmat 그대로 반환
    {
        return soundmat;
    }
}
