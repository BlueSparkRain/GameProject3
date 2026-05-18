public static class LevelCalculator
{

    //基础经验 + (当前等级 - 1) × 每级增量（基础：200；增量：100）

    //根据角色类型获取属性成长值
    public static void GetGrowthData(int currentLevel)
    {
    }



    public static float GetLevelUP_EXPGoal(int currentLevel)
    {
        return  (200 + (currentLevel - 1) * 100);


    }

    public static void GrowProperty(E_CharacterType e_CharacterType,CharacterData data) { 
        
    
    }

}
