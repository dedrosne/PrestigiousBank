using PrestigiousBank;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousHideoutModel : HideoutModel
    {
        HideoutModel _previousModel;

        //public override int CanAttackHideoutStartTime => CampaignTime.SunSet + 1;
        public override int CanAttackHideoutStartTime => 0;

        public override int CanAttackHideoutEndTime => 24;

        public PrestigiousHideoutModel(HideoutModel previousModel)
        {
            _previousModel = previousModel;
            if(previousModel == null) _previousModel = new DefaultHideoutModel();

        }

        public override CampaignTime HideoutHiddenDuration => _previousModel.HideoutHiddenDuration;

        public override float GetRogueryXpGainOnHideoutMissionEnd(bool isSucceeded)
        {
            return _previousModel.GetRogueryXpGainOnHideoutMissionEnd(isSucceeded);
        }


    }
}