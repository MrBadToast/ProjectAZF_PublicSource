using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainSoundMaterialBehavior : SoundMaterialBehavior
{
    //============================================
    //
    // Terrain은 플레이어의 위치에 따라 다른 소리를 내야할 필요가 있습니다.
    // 이 스크립트는 Terrain의 스플랫맵을 읽어 대상의 위치가 Terrain의 어느 텍스쳐 위에 올라와있고 거기에 맞는 SoundMaterial을 가질 수 있게 되어있습니다.
    //
    //============================================

    [SerializeField] private SoundMaterial[] soundPerLayer;     // Terrain의 LayerPalette에 1대1로 대응하는 SoundMaterial 리스트

    private Terrain terrain;

    TerrainData terrainData;

    float[,,] splatmap;
    int textureCount;

    private void Awake()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        splatmap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
        textureCount = terrainData.alphamapLayers;

    }

    private Vector3 ConvertToSplatMapCoordinate(Vector3 worldPosition)
    {
        Vector3 splatPosition = new Vector3Int();
        Vector3 terPosition = terrain.transform.position;
        splatPosition.x = (worldPosition.x - terPosition.x) / terrain.terrainData.size.x * terrain.terrainData.alphamapWidth;
        splatPosition.z = (worldPosition.z - terPosition.z) / terrain.terrainData.size.z * terrain.terrainData.alphamapHeight;
        return splatPosition;
    }

    /// <summary>
    /// 월드좌표의 position값이 Terrain에서 어떤 SoundMaterial을 가지는지 확인합니다.
    /// </summary>
    /// <param name="position"> 기준 위치 </param>
    /// <returns></returns>
    public override SoundMaterial GetSoundMaterial(Vector3 position)
    {
        Vector3 terrain_coord = ConvertToSplatMapCoordinate(position);
        Vector3Int integer_coord = new Vector3Int((int)terrain_coord.x,(int)terrain_coord.y,(int)terrain_coord.z);
        int retTex = 0;

        for(int i= 0; i < textureCount; i++)
        {
            retTex = splatmap[integer_coord.z, integer_coord.x, i] > splatmap[integer_coord.z, integer_coord.x, retTex] ? i : retTex ;
        }

        return soundPerLayer[retTex];
    }
}
