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

            return number;
        }
    }
}