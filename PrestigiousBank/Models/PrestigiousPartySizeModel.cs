using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousPartySizeModel : TORPartySizeModel
    {

        public override ExplainedNumber GetPartyMemberSizeLimit(PartyBase party, bool includeDescriptions = false)
        {
            ExplainedNumber number = base.GetPartyMemberSizeLimit(party, includeDescriptions);

            //Middenheim
            MiddenheimBankCampaignBehavior MiddenheimBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<MiddenheimBankCampaignBehavior>();

            if (MiddenheimBankCampaignBehavior == null || MiddenheimBankCampaignBehavior.MiddenheimBank is null) return number;
            
            
            number.Add(MiddenheimBankCampaignBehavior.MiddenheimBank.PartyHelperCount, new TextObject("Aides de camp de Middenheim"));
            return number;
        }
    }
}