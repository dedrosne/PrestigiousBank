using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Extensions;
using TOR_Core.Models;
using TOR_Core.Utilities;


namespace PrestigiousBank
{
    public class PrestigiousPartyHealingModel : TORPartyHealingModel
    {

        public override float GetSurvivalChance(PartyBase party, CharacterObject character, DamageTypes damageType, bool canDamageKillEvenIfBlunt, PartyBase enemyParty = null)
        {
            var Result = base.GetSurvivalChance(party, character, damageType, canDamageKillEvenIfBlunt, enemyParty);
            YnEdrylKoiranBankCampaignBehavior ynEdrylKoiranBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<YnEdrylKoiranBankCampaignBehavior>();

            if (party != null && party.LeaderHero != null && party.LeaderHero == Hero.MainHero
                && enemyParty != null
                && enemyParty.EstimatedStrength * 3 < party.EstimatedStrength)
                Result = 1;
            else if (ynEdrylKoiranBankCampaignBehavior != null && party != null && party.LeaderHero != null && party.LeaderHero == Hero.MainHero && !character.IsUndead())
            {
                Result = 1 - ((1 - Result) * (1 - (YnEdrylKoiranBankCampaignBehavior.BankInstance.BlessingAmount / 100f)));
            }

            return Result;
        }
    }
}