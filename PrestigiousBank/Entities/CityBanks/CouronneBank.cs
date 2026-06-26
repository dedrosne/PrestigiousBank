//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class CouronneBank : Bank
    {
        public static int FriendshipCost=50_000;
        public static int FriendshipCostIncrease=5_000;

        [SaveableProperty(12)]
        public int ChivalryAccountSolde { get; set; }


        //Increased Companion Limit
        [SaveableProperty(13)]
        public int FriendshipQty { get; set; }

        public CouronneBank(Settlement ville) : base(ville)
        {
            ChivalryAccountSolde = 0;
            FriendshipQty = 0;
        }

        public int CalculateChivalryInterests()
        {
           return (int)(ChivalryAccountSolde * (1f/10000f));
        }


    }
}