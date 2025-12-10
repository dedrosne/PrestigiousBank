//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999996)]
    public class CouronneBank : Bank
    {
        [SaveableProperty(2)]
        public int ChivalryAccountSolde { get; set; }


        //Increased Companion Limit
        [SaveableProperty(3)]
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