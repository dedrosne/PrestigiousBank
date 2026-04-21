//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class MiddenheimBank : Bank
    {
        [SaveableProperty(12)]
        public int PartyHelperCount { get; set; }

        public static int PriceHirePartyHelper = 2000;
        public static int InitialPartyHelperUpkeep = 3;


        public MiddenheimBank(Settlement ville) : base(ville)
        {
            PartyHelperCount = 0;
        }


        //Each Helper cost 1 more to upkeep
        public int CalculatePartyHelperUpkeep() {
            return PartyHelperCount * (PartyHelperCount + 1) / 2 + (InitialPartyHelperUpkeep-1)*PartyHelperCount;
        }

        protected override void InitMercenariesUnits()
        {
            InitMercenariesUnitFromListString(new List<string> {
                "tor_empire_whitewolf_knight",
                "tor_empire_warrior_ulric",
                "tor_empire_teutogen_guard",
                "tor_ror_ostland_blackguard",
                "tor_ror_horned_hunter",
                "tor_ror_hergig_jaegerkorps",
                "tor_ror_griffon_demi_preceptor_knight",
                "tor_ror_griffon_innercircle_knight"
                 });
            SortAndCleanMercenaryUnitList();
        }

    }
}