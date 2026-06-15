using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 通用图标UI组件 —— 外部传入Sprite赋值给Image。
/// 用于弱点图标等需要动态设置图标的场景。
/// </summary>
public class IconUI : MonoBehaviour
{
    [SerializeField] Image iconImage;

    /// <summary>
    /// 设置图标精灵
    /// </summary>
    public void SetIcon(Sprite sprite)
    {
        if (iconImage != null)
            iconImage.sprite = sprite;
    }
}
