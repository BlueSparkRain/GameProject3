using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTerrainTag : MonoBehaviour
{
    [Header("是否已经被配置过")]
    public bool isEdited = false;
    public E_HexTerrainType  hexTerrainType=E_HexTerrainType.Obstacle__Ocean;

    public void SetTag(E_HexTerrainType _hexTerrainType) { 
        hexTerrainType= _hexTerrainType;
        isEdited=true;
    }
}
