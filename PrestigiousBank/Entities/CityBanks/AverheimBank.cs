//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank
{
    public class AverheimBank : Bank
    {
        //  1  Blessing = 1HP
        [SaveableProperty(12)]
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

        
        protected override void InitMercenariesUnits()
        {
            InitMercenariesUnitFromListString(new List<string>
            {
                "tor_empire_ironsider",
                "tor_empire_master_engineer",
                "tor_empire_veteran_artillery_crew",
                "tor_empire_novice_engineer",
                "tor_ror_kragsburg_house_guard",
                "tor_ror_kragsburg_personal_guard",
                "tor_ror_blazing_sun_innercircle",
                "tor_ror_blazing_sun_demigryph_innercircle"
            });

            SortAndCleanMercenaryUnitList();
        }

    }
}