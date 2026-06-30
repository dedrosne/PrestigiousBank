using PrestigiousBank;
using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousPartySpeedModel : PartySpeedModel
    {
        private PartySpeedModel _previousModel;

        public PrestigiousPartySpeedModel(PartySpeedModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultPartySpeedCalculatingModel();
        }

        public override float BaseSpeed => _previousModel.BaseSpeed;

        public override float MinimumSpeed => _previousModel.MinimumSpeed;

        public override ExplainedNumber CalculateBaseSpeed(MobileParty mobileParty, bool includeDescriptions = false, int additionalTroopOnFootCount = 0, int additionalTroopOnHorseCount = 0)
        {
            ExplainedNumber number = _previousModel.CalculateBaseSpeed(mobileParty, true);

            if (mobileParty.IsMainParty)
            {
                int pegaseBought = ParravonBankCampaignBehavior.BankInstance.PegaseBought;
                if (pegaseBought != 0)
                {
                    number.Add(0.01f * pegaseBought, new TextObject("Pégases de Parravon"));
                }
            }

            return number;
        }

        public override ExplainedNumber CalculateFinalSpeed(MobileParty mobileParty, ExplainedNumber finalSpeed)
        {
            return _previousModel.CalculateFinalSpeed(mobileParty, finalSpeed);
        }
    }
}