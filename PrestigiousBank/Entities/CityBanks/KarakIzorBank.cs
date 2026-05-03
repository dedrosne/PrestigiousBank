//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.LinQuick;
using TaleWorlds.SaveSystem;
using TOR_Core.AbilitySystem.Spells;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

namespace PrestigiousBank
{
    public class KarakIzorBank : Bank
    {

        [SaveableProperty(1)]
        public int OathgoldAccountSolde { get; set; }

        [SaveableProperty(2)]
        public int MaximumStaminaBought { get; set; }

        [SaveableProperty(3)]
        public int RegenStaminaBought {  set; get; }

        [SaveableProperty(4)]
        public int ResearchPartFactorBought { get; set; }

        public static float ResearchFactorGainPerPurchaseBought = 0.02f;
        public static int MaximumStaminaGainPerPurchaseBought = 5;
        public static float RegenStaminaPerHourPerPurchaseBought = 1;
        public static int PriceMaximumStaminaUpgrade = 5000;
        public static int PriceRegenStaminaUpgrade = 5000;
        public static int PriceResearchPartFactorUpgrade = 5000;



        public KarakIzorBank(Settlement ville) : base(ville)
        {
        }

        protected override void InitMercenariesUnits()
        {
            InitMercenariesUnitFromListString(new List<string>
            {
                "tor_dw_ironbreaker",
                "tor_dw_irondrake",
                "tor_dw_ironbeard",
                "tor_dw_oldguard",
                "tor_dw_sharpshooter",
                "tor_dw_irondrake_trollhammer",
                "tor_dw_hammerer",
                "tor_dw_gatekeeper",
                "tor_dw_troll_slayer",
                "tor_dw_artillery_crew"
            });

            SortAndCleanMercenaryUnitList();
        }

        public int CalculateOathGoldGain()
        {
            return (int)(OathgoldAccountSolde * (1f / 10000f));
        }

    }
}