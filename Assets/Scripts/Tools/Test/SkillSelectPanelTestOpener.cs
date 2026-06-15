using Core;
using UnityEngine;

/// <summary>
/// 测试脚本——按J切换技能选择面板(打开/关闭)
/// </summary>
public class SkillSelectPanelTestOpener : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            var uiMgr = GameRoot.GetManager<UIManager>();
            if (uiMgr == null) return;
            var panel = uiMgr.GetPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel);
            if (panel != null && panel.IsAnimating) return;
            if (panel != null && panel.gameObject.activeSelf)
                panel.Hide();
            else
                uiMgr.OpenPanel<SkillSelectPanel>(E_UIPanelType.SkillSelectPanel);
        }
    }
}
