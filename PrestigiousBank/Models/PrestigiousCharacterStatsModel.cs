using System;
using PrestigiousBank;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TOR_Core.Models;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.CampaignSystem.ComponentInterfaces;


namespace PrestigiousBank
{
    public class PrestigiousCharacterStatsModel: CharacterStatsModel
    {
        CharacterStatsModel _previousModel;

        public override int MaxCharacterTier => _previousModel.MaxCharacterTier;

        public PrestigiousCharacterStatsModel(CharacterStatsModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultCharacterStatsModel();
        }

        public override int GetTier(CharacterObject character)
        {
            return _previousModel.GetTier(character);
        }

        public override ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false)
        {
            
            var number = _previousModel.MaxHitpoints(character, includeDescriptions);

            if (character.IsHero && character.HeroObject == Hero.MainHero)
            {
                //Middenheim
                AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();
                if (AverheimBankCampaignBehavior == null || AverheimBankCampaignBehavior.BankInstance == null) return number;


                number.Add(AverheimBankCampaignBehavior.BankInstance.BlessingAmount, new TextObject("Bénédiction de Sigmar"));
            }
            return number;
        }

        public override int WoundedHitPointLimit(Hero hero)
        {
            return _previousModel.WoundedHitPointLimit(hero);
        }
    }
}