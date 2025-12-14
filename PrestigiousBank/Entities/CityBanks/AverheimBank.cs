//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    [SaveableRootClass(99999993)]
    public class AverheimBank : Bank
    {
        //  1  Blessing = 1HP
        [SaveableProperty(2)]
        public int BlessingAmount { get; set; }

        public static int InitialPricePerHP = 2000;
        public static int PriceIncreasePerHP = 500;


        public AverheimBank(Settlement ville) : base(ville)
        {
            BlessingAmount = 0;
        }

        public int CalculatePriceAdditionnalHP()
        {
            return InitialPricePerHP + PriceIncreasePerHP * BlessingAmount;
        }

    }
}