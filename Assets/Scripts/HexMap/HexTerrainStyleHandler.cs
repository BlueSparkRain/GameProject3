using Core;
using System;
using UnityEngine;

public class HexTerrainStyleHandler : MonoBehaviour
{
    [Header("是否已经被配置过")]
    public bool isEdited = false;
    E_HexTerrainType hexTerrainType = E_HexTerrainType.Obstacle_Ocean;
    public E_HexTerrainType HexTerrainType => hexTerrainType;

    [Header("地形精灵组件")]
    public SpriteRenderer  terrainSpriteRenderer;

    public bool useTerrainSprite=false;

    [Header("房间纸牌精灵")]
    public SpriteRenderer modelSpriteRenderer;
    //地形Data
    TerrainSOData terrainSOData;
    //模型Data
    RoomModelSOData modelSOData;
    public void InitTerrainStyle(E_HexTerrainType _hexTerrainType,HexRoomTag roomTag){
        hexTerrainType = _hexTerrainType;
        isEdited = true;
        GetComponent<HexRoomStyleHandler>().SetRoomType(GetRoomType(),roomTag);
        SetRoomSprite();
        SetTerrainSprite();
    }
    public void InitTerrainStyle(E_HexTerrainType _hexTerrainType)
    {
        hexTerrainType = _hexTerrainType;
        isEdited = true;
        SetRoomSprite();
        SetTerrainSprite();
    }

    /// <summary>
    /// 设置标识地块类型的精灵
    /// </summary>
    void SetTerrainSprite() {
        if (terrainSpriteRenderer == null) return;
        if (!useTerrainSprite){ 
            terrainSpriteRenderer.enabled = false; return; }

        terrainSOData = ResourcesLoader.FindTerrainData(hexTerrainType);
        Sprite sprite = terrainSOData.sprites.GetRandomElement();
        terrainSpriteRenderer.sprite =sprite;
    }

    /// <summary>
    /// 设置标识房间类型的精灵
    /// </summary>
    void SetRoomSprite(){
        if (modelSpriteRenderer == null) return;
        if (modelSOData != null) {
            modelSpriteRenderer.sprite=modelSOData.roomSprites.GetRandomElement();
        }
    }
    E_HexRoomType GetRoomType(){
        switch (hexTerrainType){
            case E_HexTerrainType.Obstacle_Ocean: {
            return E_HexRoomType.None; }
            case E_HexTerrainType.Walkable_EmptyLand:{
                    //加载模型数据
                    return E_HexRoomType.None; }
            case E_HexTerrainType.Obstacle_Tree:{
                    modelSOData = ResourcesLoader.FindRoomModelSO(E_RoomModelType.树木);
                    return E_HexRoomType.None; }
            case E_HexTerrainType.Obstacle_Stone:{ 
                    modelSOData = ResourcesLoader.FindRoomModelSO(E_RoomModelType.石头);
                    return E_HexRoomType.None; }
            case E_HexTerrainType.Obstacle_Mountain: {
                    modelSOData = ResourcesLoader.FindRoomModelSO(E_RoomModelType.石头);
                    return E_HexRoomType.None; }
            case E_HexTerrainType.Walkable_LowLevel_BattleRoom:{
                    return E_HexRoomType.Battle_LowLevel; }
            case E_HexTerrainType.Walkable_MidLevel_BattleRoom: {
                    return E_HexRoomType.Battle_MidLevel; }
            case E_HexTerrainType.Walkable_HighLevel_BattleRoom: {
                    
                    return E_HexRoomType.Battle_HighLevel; }
            case E_HexTerrainType.Walkable_UnknownEventRoom: {
                    
                    return E_HexRoomType.UnknownEvent; }
            case E_HexTerrainType.Walkable_RewardRoom: {
                    
                    return E_HexRoomType.Reward; }
            case E_HexTerrainType.Walkable_CityShopRoom:{ 
                    modelSOData = ResourcesLoader.FindRoomModelSO(E_RoomModelType.城镇);
                    return E_HexRoomType.CityShop; }
            default: return E_HexRoomType.None;
        }
    }
}

public enum E_RoomModelType { 
    城镇,石头,树木,
}