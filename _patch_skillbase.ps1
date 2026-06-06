$enc = [System.Text.Encoding]::GetEncoding(936)
$path = 'D:/Projects/Unity/GameProject3/Assets/Scripts/Skill/Skill_Logic/SkillBase.cs'
$text = [System.IO.File]::ReadAllText($path, $enc)

# Normalize to LF for matching (ReadAllText on Windows PS keeps CRLF)
$text = $text -replace "`r`n", "`n"

# === 1. Add using System.Diagnostics ===
$old = 'using System.Collections.Generic;'
$new = 'using System.Collections.Generic;' + "`n" + 'using System.Diagnostics;'
$text = $text.Replace($old, $new)

# === 2. Replace GetTargets to use BuffHandler ===
$old = '    void GetTargets()
    {
        targets = BattleTargetSelector.GetValidTargets(self, skillTargetType);
    }'
$new = '    void GetTargets()
    {
        var targetType = BuffHandler.GetModifiedTargetType(skillTargetType, IsMagicType);
        targets = BattleTargetSelector.GetValidTargets(self, targetType);
    }'
$text = $text.Replace($old, $new)

# === 3. Add properties after skillTargetType ===
$old = '    public E_SkillTargetType skillTargetType { get; set; }

    public SkillBase(E_SkillTargetType _skillTargetType)'
$new = '    public E_SkillTargetType skillTargetType { get; set; }

    public int AtbCost { get; set; }
    public float AngGrow { get; set; }
    protected Battle_Controller Controller => self?.battleDamageHandler?.BattleController;
    protected BattleBuffHandler BuffHandler => self.battleDamageHandler.BuffHandler;
    public virtual bool IsMagicType => false;

    public SkillBase(E_SkillTargetType _skillTargetType)'
$text = $text.Replace($old, $new)

# === 4. Add ATB check at start of SkillExcute ===
# Use Unicode escapes for Chinese enum values to avoid .ps1 encoding issues
$jiShi = [char]0x57FA + [char]0x7840 + [char]0x7248 + [char]0x672C  # 基础版本
$jQian = [char]0x52A0 + [char]0x5F3A + [char]0x7248 + [char]0x672C  # 加强版本

$old = '    public void SkillExcute(E_SkillLevel skillLevel, int henceTime = 0)
    {
        GetTargets();'
$new = '    public void SkillExcute(E_SkillLevel skillLevel, int henceTime = 0)
    {
        if (skillLevel == E_SkillLevel.' + $jQian + ' && AtbCost > 0)
        {
            float currentATB = Controller?.GetCharacterModelValue(E_BattleModelType.ATBPoints) ?? 0;
            if (currentATB < AtbCost)
            {
                UnityEngine.Debug.LogWarning($"[SkillBase] ATB不足，无法释放加强技能 (需要{AtbCost}, 当前{currentATB})");
                return;
            }
            Controller.AdjustCharacterModelValue(E_BattleModelType.ATBPoints, -AtbCost);
            UnityEngine.Debug.Log($"[SkillBase] 加强技能消耗ATB:{AtbCost}, 剩余:{currentATB - AtbCost}");
        }

        GetTargets();'
$text = $text.Replace($old, $new)

# === 5. Add AG growth + IsMagicType after the switch/for block ===
$old = '        for (int i = 0; i < targets.Count; i++)
        {
            switch (skillLevel)
            {
                case E_SkillLevel.' + $jiShi + ': SkillEffect_Base(targets[i]); break;
                case E_SkillLevel.' + $jQian + ': SkillEffect_Enhence(targets[i], henceTime); break;
            }
        }
    }'
$new = '        for (int i = 0; i < targets.Count; i++)
        {
            switch (skillLevel)
            {
                case E_SkillLevel.' + $jiShi + ': SkillEffect_Base(targets[i]); break;
                case E_SkillLevel.' + $jQian + ': SkillEffect_Enhence(targets[i], henceTime); break;
            }
        }

        if (AngGrow > 0) Controller?.AdjustCharacterModelValue(E_BattleModelType.AG, AngGrow);

        if (IsMagicType)
        {
            UnityEngine.Debug.Log("魔法类型技能，触发BUFF重铸");
            EventCenter.EventTrigger(E_EventType.Do_MagAttack, BuffHandler, this, skillLevel, henceTime);
        }
        else
        {
            UnityEngine.Debug.Log("非魔法类型技能");
        }
    }'
$text = $text.Replace($old, $new)

# Restore CRLF for Windows before writing
$text = $text -replace "`n", "`r`n"
[System.IO.File]::WriteAllText($path, $text, $enc)
Write-Output ('OK: SkillBase.cs patched, length=' + $text.Length + ' chars')

# Also save as UTF-8 for verification
$utf8 = [System.Text.Encoding]::UTF8
[System.IO.File]::WriteAllText('_check3.txt', $text, $utf8)
Write-Output 'Verification file: _check3.txt'
