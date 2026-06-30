using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
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
    public class PrestigiousPartyHealingModel : PartyHealingModel
    {
        private PartyHealingModel _previousModel;

        public PrestigiousPartyHealingModel(PartyHealingModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultPartyHealingModel();
        }

        public override ExplainedNumber GetBattleEndHealingAmount(PartyBase partyBase, Hero hero)
        {
            return _previousModel.GetBattleEndHealingAmount(partyBase, hero);
        }

        public override ExplainedNumber GetDailyHealingForRegulars(PartyBase partyBase, bool isPrisoner, bool includeDescriptions = false)
        {
            return _previousModel.GetDailyHealingForRegulars(partyBase, isPrisoner, includeDescriptions);
        }

        public override ExplainedNumber GetDailyHealingHpForHeroes(PartyBase partyBase, bool isPrisoners, bool includeDescriptions = false)
        {
            return _previousModel.GetDailyHealingHpForHeroes(partyBase, isPrisoners, includeDescriptions);
        }

        public override int GetHeroesEffectedHealingAmount(Hero hero, float healingRate)
        {
            return _previousModel.GetHeroesEffectedHealingAmount(hero, healingRate);
        }

        public override float GetSiegeBombardmentHitSurgeryChance(PartyBase party)
        {
            return _previousModel.GetSiegeBombardmentHitSurgeryChance(party);
        }

        public override int GetSkillXpFromHealingTroop(PartyBase party)
        {
            return _previousModel.GetSkillXpFromHealingTroop(party);
        }

        public override float GetSurgeryChance(PartyBase party)
        {
            return _previousModel.GetSurgeryChance(party);
        }

        public override float GetSurvivalChance(PartyBase party, CharacterObject character, DamageTypes damageType, bool canDamageKillEvenIfBlunt, PartyBase enemyParty = null)
        {
            var Result = _previousModel.GetSurvivalChance(party, character, damageType, canDamageKillEvenIfBlunt, enemyParty);
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