using System;
using PrestigiousBank;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TOR_Core.Models;
using TaleWorlds.Library;
using TaleWorlds.Localization;


namespace PrestigiousBank
{
    public class PrestigiousCharacterStatsModel: TORCharacterStatsModel
    {


        public override ExplainedNumber MaxHitpoints(CharacterObject character, bool includeDescriptions = false)
        {
            
            var number = base.MaxHitpoints(character, includeDescriptions);

            if (character.IsHero && character.HeroObject == Hero.MainHero)
            {
                //Middenheim
                AverheimBankCampaignBehavior AverheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<AverheimBankCampaignBehavior>();
                if (AverheimBankCampaignBehavior == null || AverheimBankCampaignBehavior.AverheimBank == null) return number;


                number.Add(AverheimBankCampaignBehavior.AverheimBank.BlessingAmount, new TextObject("Bénédiction de Sigmar"));
            }
            return number;
        }

    }
}