using Core;
using UnityEngine;

/// <summary>
/// 测试脚本——按O打开装备配置面板
/// </summary>
public class EquipmentPanelTestOpener : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameRoot.GetManager<UIManager>().OpenPanel<EquipmentPanel>(E_UIPanelType.EquipmentPanel);
        }
    }
}
