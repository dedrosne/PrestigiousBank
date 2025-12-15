using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank.Entities
{
    [SaveableRootClass(99999992)]
    public class NulnFactory
    {
        public static int InitialPrice = 50_000;

        //Level=0 => Not yet bought  || Max level = 5
        [SaveableProperty(1)]
        public int Level {  get; set; }

        [SaveableProperty(2)]
        public int Benefits {get;set; }

        [SaveableProperty(3)]
        public int PreviousDayBenefits { get; set; }

        [SaveableProperty(4)]
        private ItemRoster ItemStash { get; set; }

        [SaveableProperty(5)]
        public int WorkStrenght { get; set; }

        public Settlement _ville;

        public Settlement Ville
        {
            get { return _ville; }
            set
            {
                _ville = value;
            }
        }

        public static Dictionary<int, int> ValuePerTiers = new Dictionary<int, int>
        {
            { 1 ,1 },
            { 2 ,5 },
            { 3 ,15 },
            { 4 ,50 },
            { 5 ,100 },
            { 6 ,300 },
            { 7 ,500 },
            { 8 ,1000 },
            { 9 ,5000 }
        };

        public enum PossibleProduction { MachiningPart, Weapon, ConstructionMaterials }

        public PossibleProduction chosenProduction;

        public NulnFactory(Settlement ville) 
        {
            Level = 0;  
            Ville = ville;
            chosenProduction = PossibleProduction.Weapon;
            Benefits = 0;
            PreviousDayBenefits = 0;
            ItemStash = new ItemRoster();
            WorkStrenght = 0;
        }

        public int GetDailySkillXP()
        {
            return 1;
        }

        public int CalculatePriceToLevelUp()
        {
            return InitialPrice * (Level + 1);
        }

        public ItemRoster GetItemStash()
        {
            if (ItemStash == null) ItemStash = new ItemRoster();
            return ItemStash;
        }

    }
}
