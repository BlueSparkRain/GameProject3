using UnityEditor;
using UnityEngine;

/// <summary>
/// 六边形噪声数据编辑器预览
/// </summary>
[CustomEditor(typeof(HexMapNoiseData))]
public class HexMapNoiseDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        HexMapNoiseData data = (HexMapNoiseData)target;

        // 生成按钮
        if (GUILayout.Button("生成地形噪声")){
            data.terrainMap = NoiseGenerator.GenerateHexTerrain(data);
            data.previewTexture = NoiseGenerator.GeneratePreviewTexture(data.terrainMap);
            EditorUtility.SetDirty(data);
        }

        // 预览纹理
        if (data.previewTexture != null){
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("地形预览（白色=凸起可行走，黑色=平坦不可走）", EditorStyles.boldLabel);
            Rect rect = GUILayoutUtility.GetRect(200, 200);
            EditorGUI.DrawPreviewTexture(rect, data.previewTexture);
        }
    }
}