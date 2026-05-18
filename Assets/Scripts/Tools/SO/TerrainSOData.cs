using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="TerrainSOData",menuName ="SOData/TerrainData")]
public class TerrainSOData : ScriptableObject
{
    [Header("地块样式-随机选择")]
    public List<Sprite> sprites = new List<Sprite>();


   
}
