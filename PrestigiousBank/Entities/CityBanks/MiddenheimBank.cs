//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999994)]
    public class MiddenheimBank : Bank
    {
        [SaveableProperty(2)]
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


    }
}