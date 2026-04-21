using PrestigiousBank;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousHideoutModel : DefaultHideoutModel
    {

        //public override int CanAttackHideoutStartTime => CampaignTime.SunSet + 1;
        public override int CanAttackHideoutStartTime => 0;

        public override int CanAttackHideoutEndTime => 24;
    }
}