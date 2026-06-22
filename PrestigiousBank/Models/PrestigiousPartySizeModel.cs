using PrestigiousBank;
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

                if (MiddenheimBankCampaignBehavior == null || MiddenheimBankCampaignBehavior.MiddenheimBank is null) return number;


                number.Add(MiddenheimBankCampaignBehavior.MiddenheimBank.PartyHelperCount, new TextObject("Aides de camp de Middenheim"));
                
            }

            //Last Stand : Infinite number of troops
            else if (Campaign.Current?.GetCampaignBehavior<LastStandCampaignBehavior>() != null 
                && LastStandCampaignBehavior.LastStands.LastStandPerCulture[party.Culture.StringId].isConsumed) 
            {
                number.Add(9999, new TextObject("Dernier rempart de " + party.Culture.StringId));
            }


            return number;
        }
    }
}