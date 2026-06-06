using UnityEngine;

/// <summary>
/// HexFace 独立预制件的标识组件，记录所在 row/col 并暴露 Renderer
/// </summary>
public class HexFaceTag : MonoBehaviour{
    public int row;
    public int col;
    public MeshRenderer faceRenderer;
    public void Init(int r, int c){
        row = r;
        col = c;
        faceRenderer = GetComponent<MeshRenderer>();
    }
}
