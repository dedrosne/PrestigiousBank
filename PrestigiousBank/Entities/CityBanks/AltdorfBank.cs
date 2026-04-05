//using Birke.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.LinQuick;
using TaleWorlds.SaveSystem;
using TOR_Core.AbilitySystem.Spells;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

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

        [SaveableProperty(4)]
        public bool IsTeleportUnblocked { get; set; }

        public static int UpkeepPerChanneler = 10;
        public static int PricePerChanneler = 4000;
        public static int PriceUnblockTeleport = 100_000;

        public AltdorfBank(Settlement ville) : base(ville)
        {
            PrestigiousAccountSolde = 0;
            ChannelerNumber = 0;
            IsTeleportUnblocked = false;
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

        public void LearnNewLore()
        {
            List<InquiryElement> list = new List<InquiryElement>();

            var lores = LoreObject.GetAll();

            lores = lores.WhereQ(X => !Hero.MainHero.HasKnownLore(X.ID)).ToList();

            foreach (var lore in lores)
            {
                list.Add(new InquiryElement(lore, lore.Name, null, true, "Learn new lore"));
            }

            var inquirydata = new MultiSelectionInquiryData("New Lore", "Select a new lore to learn", list, true, 1, 1, "Confirm", "Cancel", SelectLore, null, "", true);
            MBInformationManager.ShowMultiSelectionInquiry(inquirydata, true);

            void SelectLore(List<InquiryElement> inquiryElements)
            {
                var newlore = (LoreObject)inquiryElements[0].Identifier;

                Hero.MainHero.AddKnownLore(newlore.ID);
            }
        }

    }
}