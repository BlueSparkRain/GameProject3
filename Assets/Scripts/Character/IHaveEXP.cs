

/// <summary>
/// 可移动的角色时可以升级的
/// 玩家控制角色需要更新UI视图
/// AI操控角色不需要更新UI
/// </summary>
public interface IHaveEXP
{
    public void UpdateView();
    
    /// <summary>
    /// 更新当前的经验值
    /// </summary>
    public void UpdateEXPGoal();
}
