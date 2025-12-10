//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999998)]
    public class AltdorfBank : Bank
    {
        [SaveableProperty(2)]
        public int PrestigiousAccountSolde { get; set; }

        //1 Channeler = 1 Max de vent de magie en plus
        [SaveableProperty(3)]
        public int ChannelerNumber { get; set; }

        public static int UpkeepPerChanneler = 10;
        public static int PricePerChanneler = 4000;

        public AltdorfBank(Settlement ville) : base(ville)
        {
            PrestigiousAccountSolde = 0;
            ChannelerNumber = 0;
        }



        public int CalculatePrestigiousInterests()
        {
           return (int)(PrestigiousAccountSolde * (1f/10000f));
        }

        public int CalculateChannelerCostPerDay()
        {
            //Prix par 
            return ChannelerNumber*UpkeepPerChanneler;
        }

    }
}