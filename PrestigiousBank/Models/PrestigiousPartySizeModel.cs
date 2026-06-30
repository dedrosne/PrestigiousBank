using PrestigiousBank;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Naval;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.LordConversationsCampaignBehavior;


namespace PrestigiousBank
{
    public class PrestigiousPartySizeModel : PartySizeLimitModel
    {
        PartySizeLimitModel _previousModel;

        public override int MinimumNumberOfVillagersAtVillagerParty => _previousModel.MinimumNumberOfVillagersAtVillagerParty;

        public PrestigiousPartySizeModel(PartySizeLimitModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultPartySizeLimitModel();
        }

        public override ExplainedNumber CalculateGarrisonPartySizeLimit(Settlement settlement, bool includeDescriptions = false)
        {
            return _previousModel.CalculateGarrisonPartySizeLimit(settlement, includeDescriptions);
        }

        public override TroopRoster FindAppropriateInitialRosterForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
        {
            return _previousModel.FindAppropriateInitialRosterForMobileParty(party, partyTemplate);
        }

        public override List<Ship> FindAppropriateInitialShipsForMobileParty(MobileParty party, PartyTemplateObject partyTemplate)
        {
            return _previousModel.FindAppropriateInitialShipsForMobileParty(party, partyTemplate);
        }

        public override int GetAssumedPartySizeForLordParty(Hero leaderHero, IFaction partyMapFaction, Clan actualClan)
        {
            return _previousModel.GetAssumedPartySizeForLordParty(leaderHero, partyMapFaction, actualClan);
        }

        public override int GetClanTierPartySizeEffectForHero(Hero hero)
        {
            return _previousModel.GetClanTierPartySizeEffectForHero(hero);
        }

        public override int GetIdealVillagerPartySize(Village village)
        {
            return _previousModel.GetIdealVillagerPartySize(village);
        }

        public override int GetNextClanTierPartySizeEffectChangeForHero(Hero hero)
        {
            return _previousModel.GetNextClanTierPartySizeEffectChangeForHero(hero);
        }

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber number = _previousModel.GetPartyMemberSizeLimit(party, includeDescriptions);
            if (party == PartyBase.MainParty)
            {
                //Middenheim
                MiddenheimBankCampaignBehavior MiddenheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<MiddenheimBankCampaignBehavior>();

                if (MiddenheimBankCampaignBehavior == null || MiddenheimBankCampaignBehavior.BankInstance is null) return number;


                number.Add(MiddenheimBankCampaignBehavior.BankInstance.PartyHelperCount, new TextObject("Aides de camp de Middenheim"));
                
            }

            //party.Culture can trigger NullReferenceException even if party is not null, so we need to catch it.
            try
            {
                           
                //Last Stand : Infinite number of troops
                if (party != PartyBase.MainParty && party != null 
                    && party.MapFaction != null //Contains the culture
                    && party.Culture != null 
                    && Campaign.Current?.GetCampaignBehavior<LastStandCampaignBehavior>() != null)
                {
                    if (LastStandCampaignBehavior.LastStands.GetLastStandForCulture(party.Culture.StringId) != null
                    && LastStandCampaignBehavior.LastStands.GetLastStandForCulture(party.Culture.StringId).IsConsumed)
                        number.Add(9999, new TextObject("Dernier rempart de " + party.Culture.StringId));
                }
            }
            catch (NullReferenceException ex)
            {
                InformationManager.DisplayMessage(new InformationMessage("PrestigiousBank: Error in PartySizeModel for lastStand: " + ex.Message));
            }

            return number;
        }

        public override ExplainedNumber GetPartyPrisonerSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            return _previousModel.GetPartyPrisonerSizeLimit(party, includeDescriptions);
        }
    }
}