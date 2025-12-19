using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace PrestigiousBank.Entities
{
    [SaveableRootClass(99999992)]
    public class NulnFactory
    {
        public static int InitialFactoryPrice = 50_000;
        public static int InitialRessourcePrice = 25_000;

        public static int NbDaysDelayBetweenProductionChange = 4;

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

        [SaveableProperty(17)]
        public int NbDaysLeftBetweenProductionChange {get; set;}

        //If not enough ressources to run at selectedLevel, runLevel is lower until ressource requirement is meeted
        [SaveableProperty(18)]
        public int RunFactoryLevel {get;set;}

        public Settlement _ville;

        public Settlement Ville
        {
            get { return _ville; }
            set
            {
                _ville = value;
            }
        }

        public static Dictionary<int, int> ValueGainedPerRecruitTier = new Dictionary<int, int>
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
            {0, (0,0)},
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
            {0, (0,0,0)},
            { 1 ,(2,1,1) },
            { 2 ,(9,4,5) },
            { 3 ,(16,8,10) },
            { 4 ,(20,12,15) },
            { 5 ,(25,18,25) }
        };

        //{SelectedIronLevel, (WorkStrenght, Production)}
        public static Dictionary<int, (int WorkStrenght,int Production)> IronProductionAndWorkStrenghtPerLevel 
        = new Dictionary<int, (int WorkStrenght,int Production)>
        {
            {0, (0,0)},
            { 1 ,(5,1) },
            { 2 ,(10,5) },
            { 3 ,(15,10) },
            { 4 ,(20,15) },
            { 5 ,(30,25) }
        };

        //<selectedLevel, (int WoodConsumption, int CharcoalConsumption, int IronConsumption)>
        public static Dictionary <int, (int WoodConsumption, int CharcoalConsumption, int IronConsumption)> WeaponProductionRessourceConsumption
        = new Dictionary<int, (int WoodConsumption, int CharcoalConsumption, int IronConsumption)>
        {
            {1,(0,0,0)},
            {2,(1,3,2)},
            {3,(2,6,4)},
            {4,(3,7,5)},
            {5,(3,8,6)}
        };

        public static Dictionary <int, (int WoodConsumption, int CharcoalConsumption, int IronConsumption)> MachiningPartProductionRessourceConsumption
        = new Dictionary<int, (int WoodConsumption, int CharcoalConsumption, int IronConsumption)>
        {
            {1,(0,0,0)},
            {2,(1,3,2)},
            {3,(2,6,4)},
            {4,(3,7,5)},
            {5,(3,8,6)}
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

        public int CalculateTotalWorkStrenght()
        {
            int result = 0;
            result += WoodProductionAndWorkStrenghtPerLevel[SelectedWoodLevel].WorkStrenght;
            result += CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].WorkStrenght;
            result += IronProductionAndWorkStrenghtPerLevel[SelectedIronLevel].WorkStrenght;

            return result;
        }

        public PossibleProduction TryChangeProduction(PossibleProduction selectedProduction)
        {
            if (selectedProduction != chosenProduction)
            {
                chosenProduction = selectedProduction;
                NbDaysLeftBetweenProductionChange = NbDaysDelayBetweenProductionChange;
                PrestigiousBank.LogMessage("Production demandée : "+selectedProduction.ToString()+
                    "\nDélai de changement deproduction : "+NbDaysDelayBetweenProductionChange+" jours");
            }
            return selectedProduction;
        }

        public ItemObject GetMinimumItemNumberInStash(List<ItemObject> list)
        {
            ItemObject minNumberItem = list[0];
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
            if (SelectedWoodLevel == 0) return;
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
            if (SelectedCharcoalLevel == 0) return;
            int ToProduce = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].Production;
            int RequiredWorkStrenght = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].WorkStrenght;
            int RequiredWood = CharcoalProductionWoodAndWorkStrenghtPerLevel[SelectedCharcoalLevel].WoodConsumption;

            if (WorkStrenght < RequiredWorkStrenght || GetItemStash().GetItemNumber(item: DefaultItems.HardWood) < RequiredWood)   return;
            else
            {
                WorkStrenght-=RequiredWorkStrenght;
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.HardWood, RequiredWood));
                GetItemStash().Add(new ItemRosterElement(DefaultItems.Charcoal, ToProduce));
            }
        }

        public void TryToProduceIron()
        {
            if (SelectedIronLevel == 0) return;
            int ToProduce = IronProductionAndWorkStrenghtPerLevel[SelectedIronLevel].Production;
            int RequiredWorkStrenght = IronProductionAndWorkStrenghtPerLevel[SelectedIronLevel].WorkStrenght;

            if (WorkStrenght < RequiredWorkStrenght) return;
            else
            {
                WorkStrenght-=RequiredWorkStrenght;
                GetItemStash().Add(new ItemRosterElement(DefaultItems.IronOre, ToProduce));
            }
        }

        public void TryToProduceRessources()
        {
            List<ItemObject> listItems = new List<ItemObject> {
                new ItemObject(DefaultItems.HardWood),
                new ItemObject(DefaultItems.Charcoal),
                new ItemObject(DefaultItems.IronOre)
                };
            
            while (listItems.Count != 0) 
            {
                ItemObject minItem = GetMinimumItemNumberInStash(listItems);
                if (minItem.StringId == "hardwood") TryToProduceWood();//TODO
                if (minItem.StringId == "charcoal") TryToProduceCharcoal();//TODO
                if (minItem.StringId == "iron") TryToProduceIron();//TODO
                listItems.Remove(minItem);
            }
        }

        private bool CheckRessourceAvailabilitiesForFactory(int level)
        {
            if (chosenProduction==PossibleProduction.Weapon)
            {
                if (WeaponProductionRessourceConsumption[level].WoodConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.HardWood))
                    && WeaponProductionRessourceConsumption[level].CharcoalConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.Charcoal))
                    && WeaponProductionRessourceConsumption[level].IronConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.IronOre)))
                {
                    return true;
                }
                    
            }
            if (chosenProduction == PossibleProduction.MachiningPart)
            {
                if (MachiningPartProductionRessourceConsumption[level].WoodConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.HardWood))
                    && MachiningPartProductionRessourceConsumption[level].CharcoalConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.Charcoal))
                    && MachiningPartProductionRessourceConsumption[level].IronConsumption >= GetItemStash().GetItemNumber(item: new ItemObject(DefaultItems.IronOre)))
                {
                    return true;
                }
            }
            return false;
        }

        private void ConsumeRessource(int level)
        {
            if (chosenProduction==PossibleProduction.Weapon)
            {
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.HardWood, WeaponProductionRessourceConsumption[level].WoodConsumption));
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.Charcoal, WeaponProductionRessourceConsumption[level].CharcoalConsumption));
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.IronOre, WeaponProductionRessourceConsumption[level].IronConsumption));
            }
            if (chosenProduction==PossibleProduction.MachiningPart)
            {
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.HardWood, MachiningPartProductionRessourceConsumption[level].WoodConsumption));
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.Charcoal, MachiningPartProductionRessourceConsumption[level].CharcoalConsumption));
                GetItemStash().Remove(new ItemRosterElement(DefaultItems.IronOre, MachiningPartProductionRessourceConsumption[level].IronConsumption));
            }
        }

        //Factory tries to consume ressources at best level, starting from selectedFactoryLevel
        public void ConsumeRessourcesToRun()
        {
            for (int level = SelectedFactoryLevel; level>=1; level--)
            {
                //Check for ressources availability depending on level
                if (CheckRessourceAvailabilitiesForFactory(level))
                {
                    ConsumeRessource(level);
                    RunFactoryLevel = level;
                    break;
                }
            }
        }

        public void ApplyWorkshopGains(Workshop workshop, float factorClanAgency = 1f)
        {
            //Chiffre d'affaire = Profit+Expense
            int Turnover = workshop.ProfitMade + workshop.Expense;

            //Production gain = Chiffre d'affaire * level*0.05 * ClanAgencyLevelFactor
            int ProductionGain = (int)(Turnover * (RunFactoryLevel * 0.20)* factorClanAgency);
            //Increase Gold as new Machining part increase production
            workshop.ChangeGold(ProductionGain);

            //Price of Machining parts
            if (workshop.Owner != Hero.MainHero)
            {
                workshop.ChangeGold((int)(-ProductionGain * 0.8));
                this.Benefits += (int)(ProductionGain * 0.8);

            }
        }

        public string GetToolTipFactoryPerLevel(int level)
        {
            string result = "";


            //Consommation de bois
            result += "Consommation de bois :";
            if (chosenProduction == PossibleProduction.Weapon) result += WeaponProductionRessourceConsumption[level].WoodConsumption;
            if (chosenProduction == PossibleProduction.MachiningPart) result += MachiningPartProductionRessourceConsumption[level].WoodConsumption;

            //Consommation de charbon
            result += "\nConsommation de charbon :";
            if (chosenProduction == PossibleProduction.Weapon) result += WeaponProductionRessourceConsumption[level].CharcoalConsumption;
            if (chosenProduction == PossibleProduction.MachiningPart) result += MachiningPartProductionRessourceConsumption[level].CharcoalConsumption;

            //Consommation de fer
            result += "\nConsommation de fer :";
            if (chosenProduction == PossibleProduction.Weapon) result += WeaponProductionRessourceConsumption[level].IronConsumption;
            if (chosenProduction == PossibleProduction.MachiningPart) result += MachiningPartProductionRessourceConsumption[level].IronConsumption;

            return result;
        }

    }
}
