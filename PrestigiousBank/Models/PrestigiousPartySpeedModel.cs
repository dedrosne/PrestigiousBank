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
    public class PrestigiousPartySpeedModel : TORPartySpeedCalculatingModel
    {

        public override ExplainedNumber CalculateBaseSpeed(MobileParty mobileParty, bool includeDescriptions = false, int additionalTroopOnFootCount = 0, int additionalTroopOnHorseCount = 0)
        {
            ExplainedNumber number = base.CalculateBaseSpeed(mobileParty, true);

            if (mobileParty.IsMainParty)
            {
                int pegaseBought = ParravonBankCampaignBehavior.ParravonBank.PegaseBought;
                if (pegaseBought != 0)
                {
                    number.Add(0.01f * pegaseBought, new TextObject("Pégases de Parravon"));
                }
            }

            return number;
        }
    }
}