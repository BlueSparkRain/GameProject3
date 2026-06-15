using UnityEngine;
/// <summary>
/// ���ڿɱ��ر���Ϊjson�ļ�
/// </summary>
public class SkillData
{
    //��¼һ�ݼ��ܵ�ǰ�Ļ������ԣ�MapIcon����ͨ����ȡһ��skillData��������Ϣ
    [Header("����ID")]
    public int skill_ID;
    [Header("[��ǰ]����ͼ��")]
    public Sprite skill_Sprite;
    [Header("��������")]
    public string skill_Name;
    [Header("��������")]
    [Multiline]
    public string skill_Description;
    [Header("[��ǰ]������ȴ")]
    public float skill_CoolDown;
    [Header("[��ǰ]���ܷ�������")]
    public float skill_sp_cost;
    [Header("[��ǰ]����ATB����")]
    public int skill_atb_cost;
    [Header("[��ǰ]����ŭ������")]
    public float skill_ang_grow;
    [Header("[��ǰ]���ܵ�Ŀ������")]
    public E_SkillTargetType_Auto skill_targetType;
    public E_SkillTargetType_ATB skill_ATBTargetType;
    public SkillDeliveryType skill_DeliveryType;

    public SkillData(SkillPropertySO sodata) { 
        skill_ID=sodata.skill_ID;
        skill_Sprite=sodata.skill_Sprite;
        skill_Name=sodata.skill_Name;
        skill_Description=sodata.skill_Description;
        skill_CoolDown=sodata.skill_CoolDown_Auto;
        skill_sp_cost=sodata.skill_sp_cost;
        skill_atb_cost=sodata.skill_AtbCost_ATB;
        skill_ang_grow=sodata.skill_ang_grow;
        skill_targetType=sodata.skill_targetType_Auto;
        skill_ATBTargetType=sodata.skill_targetType_ATB;
        skill_DeliveryType = sodata.skill_DeliveryType;
    }
}
