//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999988)]
    public class ParravonBank : Bank
    {
        public static int PricePerPegase = 2000;
        public static int PriceIncrease = 200;

        [SaveableProperty(1)]
        public int PegaseBought { get; set; }

        public ParravonBank(Settlement ville) : base(ville)
        {
        }

        public int CalculatePriceNewPegase()
        {
            return PricePerPegase + PriceIncrease * PegaseBought;
        }
    }
}