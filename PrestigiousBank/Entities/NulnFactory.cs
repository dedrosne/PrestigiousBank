using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank.Entities
{
    [SaveableRootClass(99999992)]
    public class NulnFactory
    {
        public static int InitialFactoryPrice = 50_000;
        public static int InitialRessourcePrice = 25_000;

        //Level=0 => Not yet bought  || Max level = 5
        [SaveableProperty(1)]
        public int FactoryLevel {  get; set; }

        [SaveableProperty(2)]
        public int WoodLevel {get;set;}

        [SaveableProperty(3)]
        public int CharcoalLevel {get;set;}

        [SaveableProperty(4)]
        public int IronLevel {get;set;}

        [SaveableProperty(5)]
        public int ClayLevel {get;set;}

        [SaveableProperty(6)]
        public int SilverLevel {get;set;}

        [SaveableProperty(7)]
        public int Benefits {get;set; }

        [SaveableProperty(8)]
        public int PreviousDayBenefits { get; set; }

        [SaveableProperty(9)]
        private ItemRoster ItemStash { get; set; }

        [SaveableProperty(10)]
        public int WorkStrenght { get; set; }

        [SaveableProperty(11)]
        public int SelectedFactoryLevel {  get; set; }

        [SaveableProperty(12)]
        public int SelectedWoodLevel {get;set;}

        [SaveableProperty(13)]
        public int SelectedCharcoalLevel {get;set;}

        [SaveableProperty(14)]
        public int SelectedIronLevel {get;set;}

        [SaveableProperty(15)]
        public int SelectedClayLevel {get;set;}

        [SaveableProperty(16)]
        public int SelectedSilverLevel {get;set;}

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

        //{SelectedWoodLevel, (WorkStrenght, Production)}
        public static Dictionary<int, (int WorkStrenght,int Production)> WoodProductionAndWorkStrenghtPerLevel 
        = new Dictionary<int, (int WorkStrenght,int Production)>
        {
            { 1 ,(1,1) },
            { 2 ,(4,5) },
            { 3 ,(8,10) },
            { 4 ,(12,20) },
            { 5 ,(25,50) }
        };

        //{SelectedCharcoalLevel, (WoodConsumption, WorkStrenght, Production)}
        public static Dictionary<int, (int WoodConsumption, int WorkStrenght,int Production)> CharcoalProductionWoodAndWorkStrenghtPerLevel 
        = new Dictionary<int, (int WoodConsumption,int WorkStrenght,int Production)>
        {
            { 1 ,(2,1,1) },
            { 2 ,(9,4,5) },
            { 3 ,(16,8,10) },
            { 4 ,(20,12,15) },
            { 5 ,(25,25,25) }
        };

        //{SelectedIronLevel, (WorkStrenght, Production)}
        public static Dictionary<int, (int WorkStrenght,int Production)> IronProductionAndWorkStrenghtPerLevel 
        = new Dictionary<int, (int WorkStrenght,int Production)>
        {
            { 1 ,(5,1) },
            { 2 ,(22,5) },
            { 3 ,(40,10) },
            { 4 ,(50,15) },
            { 5 ,(75,25) }
        };

        public enum PossibleProduction { MachiningPart, Weapon, ConstructionMaterials }

        public PossibleProduction chosenProduction;

        public NulnFactory(Settlement ville) 
        {
            FactoryLevel = 0;  
            Ville = ville;
            chosenProduction = PossibleProduction.Weapon;
            Benefits = 0;
            PreviousDayBenefits = 0;
            ItemStash = new ItemRoster();
            WorkStrenght = 0;
        }

        public int GetDailySkillXP()
        {
            return 500*FactoryLevel+100*WoodLevel+100*CharcoalLevel+100*IronLevel+100*ClayLevel*100*SilverLevel;
        }

        public int CalculatePriceToLevelUpFactory()
        {
            return InitialFactoryPrice * (FactoryLevel + 1);
        }

        public int CalculatePriceToLevelUpRessource(int CurrentLevel)
        {
            return (CurrentLevel+1)*InitialRessourcePrice;
        }

        public ItemRoster GetItemStash()
        {
            if (ItemStash == null) ItemStash = new ItemRoster();
            return ItemStash;
        }

        public ItemObject GetMinimumItemNumberInStash(List<ItemObject> list)
        {
            ItemObject minNumberItem = list[0];//TODO
            int minNumber = 9999;
            foreach (ItemObject item in list)
            {
                if (GetItemStash().GetItemNumber(item) < minNumber)
                {
                    minNumberItem = item;
                    minNumber = GetItemStash().GetItemNumber(item);
                }
            }

            return minNumberItem;
        }

        public void TryToProduceWood()
        {
            int ToProduce = WoodProductionAndWorkStrenghtPerLevel[SelectedWoodLevel].Production;
            int RequiredWorkStrenght = WoodProductionAndWorkStrenghtPerLevel[SelectedWoodLevel].WorkStrenght;

            if (WorkStrenght < RequiredWorkStrenght) return;
            else
            {
                WorkStrenght-=RequiredWorkStrenght;
                GetItemStash().Add(new ItemRosterElement(DefaultItems.HardWood, ToProduce));
                PrestigiousBank.LogMessage("Bois produit :" + ToProduce);
            }
        }

        public void TryToProduceCharcoal()
        {
            int ToProduce = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].Production;
            int RequiredWorkStrenght = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].WorkStrenght;
            int RequiredWood = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].WoodConsumption;


            if (WorkStrenght < RequiredWorkStrenght || GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.HardWood)) < RequiredWood)   return;
            else
            {
                WorkStrenght-=RequiredWorkStrenght;
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.HardWood, RequiredWood));
                GetItemStash().Add(new ItemRosterElement(DefaultItems.Charcoal, ToProduce));
            }
        }

        public void TryToProduceIron()
        {
            int ToProduce = IronProductionAndWorkStrenghtPerLevel[SelectedIronLevel].Production;
            int RequiredWorkStrenght = IronProductionAndWorkStrenghtPerLevel[SelectedIronLevel].WorkStrenght;

            if (WorkStrenght < RequiredWorkStrenght) return;
            else
            {
                WorkStrenght-=RequiredWorkStrenght;
                GetItemStash().Add(new ItemRosterElement(DefaultItems.IronOre, ToProduce));
            }
        }

    }
}
