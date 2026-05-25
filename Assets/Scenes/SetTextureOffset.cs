// 挂载到需要设置纹理的小面片上
using UnityEngine;

public class SetTextureOffset : MonoBehaviour
{
    public Vector2 offset; // 在大图上裁剪的起始点 (0~1范围)
    public Vector2 tiling; // 裁剪区域的尺寸 (也是0~1范围)

    void Start()
    {
        // 获取材质实例，注意避免使用 sharedMaterial，否则会互相影响
        Material mat = GetComponent<Renderer>().material;
        mat.SetTextureOffset("_MainTex", offset);
        mat.SetTextureScale("_MainTex", tiling);
    }
}