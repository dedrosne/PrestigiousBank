using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;
using static TaleWorlds.MountAndBlade.Launcher.Library.NativeMessageBox;


namespace PrestigiousBank
{
    public class PrestigiousCharacterDevelopmentModel: CharacterDevelopmentModel
    {
        CharacterDevelopmentModel _previousModel;



        public PrestigiousCharacterDevelopmentModel(CharacterDevelopmentModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultCharacterDevelopmentModel();
        }

        public override int MaxAttribute => _previousModel.MaxAttribute;

        public override int MaxFocusPerSkill => _previousModel.MaxFocusPerSkill;

        public override int MaxSkillRequiredForEpicPerkBonus => _previousModel.MaxSkillRequiredForEpicPerkBonus;

        public override int MinSkillRequiredForEpicPerkBonus => _previousModel.MinSkillRequiredForEpicPerkBonus;

        public override int FocusPointsPerLevel => _previousModel.FocusPointsPerLevel;

        public override int FocusPointsAtStart => _previousModel.FocusPointsAtStart;

        public override int AttributePointsAtStart => _previousModel.AttributePointsAtStart;

        public override int LevelsPerAttributePoint => _previousModel.LevelsPerAttributePoint;

        public override ExplainedNumber CalculateLearningLimit(IReadOnlyPropertyOwner<CharacterAttribute> characterAttributes, int focusValue, SkillObject skill, bool includeDescriptions = false)
        {
            ExplainedNumber result = _previousModel.CalculateLearningLimit(characterAttributes, focusValue, skill, includeDescriptions);

            AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();
            if (AverheimBankCampaignBehavior != null)
            {
                result.Add(AverheimBankCampaignBehavior.BankInstance.MaxSkillIncreaseBought, new TextObject("Averheim Perfection"));
            }
            return result;
        }

        public override ExplainedNumber CalculateLearningRate(IReadOnlyPropertyOwner<CharacterAttribute> characterAttributes, int focusValue, int skillValue, SkillObject skill, bool includeDescriptions = false)
        {
            return _previousModel.CalculateLearningRate(characterAttributes, focusValue, skillValue, skill, includeDescriptions);
        }

        public override int GetMaxSkillPoint()
        {
            return _previousModel.GetMaxSkillPoint();
        }

        public override CharacterAttribute GetNextAttributeToUpgrade(Hero hero)
        {
            return _previousModel.GetNextAttributeToUpgrade(hero);
        }

        public override PerkObject GetNextPerkToChoose(Hero hero, PerkObject perk)
        {
            return _previousModel.GetNextPerkToChoose(hero, perk);
        }

        public override SkillObject GetNextSkillToAddFocus(Hero hero)
        {
            return _previousModel.GetNextSkillToAddFocus(hero);
        }

        public override int GetSkillLevelChange(Hero hero, SkillObject skill, float skillXp)
        {
            return _previousModel.GetSkillLevelChange(hero, skill, skillXp);
        }

        public override void GetTraitLevelForTraitXp(Hero hero, TraitObject trait, int newValue, out int traitLevel, out int traitXp)
        {
            _previousModel.GetTraitLevelForTraitXp(hero, trait, newValue, out traitLevel, out traitXp);
        }

        public override int GetTraitXpRequiredForTraitLevel(TraitObject trait, int traitLevel)
        {
            return _previousModel.GetTraitXpRequiredForTraitLevel(trait, traitLevel);
        }

        public override int GetXpAmountForSkillLevelChange(Hero hero, SkillObject skill, int skillLevelChange)
        {
            return _previousModel.GetXpAmountForSkillLevelChange(hero, skill, skillLevelChange);
        }

        public override int GetXpRequiredForSkillLevel(int skillLevel)
        {
            return _previousModel.GetXpRequiredForSkillLevel(skillLevel);
        }

        public override int SkillsRequiredForLevel(int level)
        {
            return _previousModel.SkillsRequiredForLevel(level);
        }
    }
}