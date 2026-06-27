using PrestigiousBank;
using PrestigiousBank.Entities;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;
using static TaleWorlds.CampaignSystem.CampaignBehaviors.LordConversationsCampaignBehavior;


namespace PrestigiousBank
{
    public class PrestigiousPartySizeModel : TORPartySizeModel
    {

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber number = base.GetPartyMemberSizeLimit(party, includeDescriptions);
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
    }
}