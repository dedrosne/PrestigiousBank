using Messages.FromClient.ToLobbyServer;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;
using static TaleWorlds.CampaignSystem.Settlements.Workshops.WorkshopType;
using TaleWorlds.CampaignSystem.GameState;
using Helpers;
using TOR_Core.CampaignMechanics.Menagery;
using System.Runtime.Remoting.Messaging;

namespace PrestigiousBank
{
    public class NulnFactoryMenu
    {
        public string _cityName;
        public string _cityID;
        public NulnFactory _nulnFactory;
        public static int _optionBankIndex = -1;

        public virtual void RegisterFactoryMenu(CampaignGameStarter campaignGameStarter, NulnFactory NulnFactory)
        {
            _nulnFactory = NulnFactory;
            _cityID = _nulnFactory.Ville.Town.StringId;
            _cityName = _nulnFactory.Ville.Name.Value;
            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");
            MBTextManager.SetTextVariable("PRESTIGE_ICON",
                    CustomResourceManager.GetResourceObject("Prestige").GetCustomResourceIconAsText());


            

            CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Town -> Acheter l'usine
            campaignGameStarter.AddGameMenuOption("town",
                                                  String.Format("{0}_nulnFactoryBuy_menu", _cityID),
                                                  "["+NulnFactory.InitialFactoryPrice+"{GOLD_ICON}] Acheter une usine à Nuln",
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      args.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialFactoryPrice;
                                                      args.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialFactoryPrice ? null : new TextObject("Pas assez d'or");
                                                      if (Settlement.CurrentSettlement.Town.StringId != _cityID) return false;
                                                      else if (NulnFactory.FactoryLevel > 0) return false;
                                                      else return true;
                                                  },
                                                  _ => {
                                                      _nulnFactory.FactoryLevel += 1;
                                                      Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialFactoryPrice);
                                                      GameMenu.SwitchToMenu("town");
                                                      CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                                      GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUpFactory());
                                                  },
                                                  isLeave: false,
                                                  _optionBankIndex);

            //Town -> NulnFactory
            campaignGameStarter.AddGameMenuOption("town",
                                                  "nulnFactory_menu",
                                                  "Usine de Nuln",
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      if (Settlement.CurrentSettlement.Town.StringId != _cityID) return false;
                                                      else if (NulnFactory.FactoryLevel == 0) return false;
                                                      else return true;
                                                  },
                                                  _ => {
                                                      CreateOrUpdateGameMenuDesc(campaignGameStarter); 
                                                      GameMenu.SwitchToMenu("nulnFactory_menu"); },
                                                  isLeave: false,
                                                  _optionBankIndex);


            // Factory Menu -> ProductionChoice
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_productionChoice", _cityID), "Changer la production",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Manage; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID)),
                isLeave: false, index: 1);
            RegisterProductionChoiceMenuOptions(campaignGameStarter);


            //Open Stash
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_openStash", _cityID), "Ouvrir l'entrepôt",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => InventoryManager.OpenScreenAsStash(_nulnFactory.GetItemStash()),
                isLeave: false, index: 2);

            // Factory Menu -> RawMaterials Production
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_rawMaterials_menu", _cityID), "Production de matières premières",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.SneakIn; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: false, index: 3);
            RegisterRawMaterialsProductionMenuOptions(campaignGameStarter);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            RegisterLevelSelectionMenuOptions(campaignGameStarter);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //Quitter l'usine
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactory_menu_back", _cityID), "Quitter l'usine",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: -1);
        }

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {

            int factoryLevel = _nulnFactory.FactoryLevel;
            NulnFactory.PossibleProduction chosenProduction = _nulnFactory.chosenProduction;
            string chosenProductionString = chosenProduction.ToString();

            // Factory Menu
            campaignGameStarter.AddGameMenu("nulnFactory_menu",
                String.Format("Usine de Nuln\nNiveau de l'usine : {0}\nProduction Actuelle : {1}", factoryLevel, chosenProductionString),
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            //ProductionChoice
            campaignGameStarter.AddGameMenu(String.Format("{0}_productionChoice", _cityID),
                "Choix de la production"+
                "\nARMES : {GOLD_ICON} généré selon le tiers des unités recrutées dans la ville"+
                "\nPIECES D'USINAGE : Récupère un pourcentage de la valeur produite des Workshops de la ville, ou augmente la production de ses propres Workshops"+
                "\nMATERIAUX DE CONSTRUCTION : Ne fait rien pour l'instant"//TODO
                , null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            // Raw Materials
            campaignGameStarter.AddGameMenu(String.Format("{0}_rawMaterials_menu", _cityID),
                "Production de matières premières :"+
                "\nChaque jour, les ouvriers meurent à la tâche pour produire des matières premières\n"+
                "Force de travail : "+_nulnFactory.WorkStrenght+
                "\nDemande actuelle d'ouvrier/jour :"+_nulnFactory.CalculateTotalWorkStrenght(),
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            // Wood Production
            campaignGameStarter.AddGameMenu(String.Format("{0}_woodProduction", _cityID),
                "Scierie de bois :"+
                "\nProduit chaque jour du bois en fonction du niveau de production et de la force de travail disponible\n"+
                "Force de travail : "+_nulnFactory.WorkStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            // Charcoal Production
            campaignGameStarter.AddGameMenu(String.Format("{0}_charcoalProduction", _cityID),
                "Four à charbon :"+
                "\nProduit chaque jour du charbon en fonction du niveau de production, du bois disponible et de la force de travail disponible\n"+
                "Force de travail : "+_nulnFactory.WorkStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            // Iron Production
            campaignGameStarter.AddGameMenu(String.Format("{0}_ironProduction", _cityID),
                "Mine de fer :"+
                "\nProduit chaque jour du fer en fonction du niveau de production et de la force de travail disponible\n"+
                "Force de travail : "+_nulnFactory.WorkStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public void RegisterLevelSelectionMenuOptions(CampaignGameStarter campaignGameStarter)
        {

            var factoryProduction0 = new TextObject("{NULNFACTORYPRODUCTIONTEXT0}");
            var factoryProduction1 = new TextObject("{NULNFACTORYPRODUCTIONTEXT1}");
            var factoryProduction2 = new TextObject("{NULNFACTORYPRODUCTIONTEXT2}");
            var factoryProduction3 = new TextObject("{NULNFACTORYPRODUCTIONTEXT3}");
            var factoryProduction4 = new TextObject("{NULNFACTORYPRODUCTIONTEXT4}");
            var factoryProduction5 = new TextObject("{NULNFACTORYPRODUCTIONTEXT5}");
            var factoryProductionLevelUp = new TextObject("{NULNFACTORYPRODUCTIONTEXTUP}");
            SetFactoryLevelProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_0", _cityID),
                factoryProduction0.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Désactiver la production. \nPourquoi faire ça ?");
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 0;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Usine désactivée");
                },
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_1", _cityID),
                factoryProduction1.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject(_nulnFactory.GetToolTipFactoryPerLevel(1));
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 1;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 1");
                },
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_2", _cityID),
                factoryProduction2.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject(_nulnFactory.GetToolTipFactoryPerLevel(2));
                    return _nulnFactory.FactoryLevel >= 2;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 2;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 2");
                },
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_3", _cityID),
                factoryProduction3.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject(_nulnFactory.GetToolTipFactoryPerLevel(3));
                    return _nulnFactory.FactoryLevel >= 3;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 3;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 3");
                },
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_4", _cityID),
                factoryProduction4.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject(_nulnFactory.GetToolTipFactoryPerLevel(4));
                    return _nulnFactory.FactoryLevel >= 4;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 4;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 4");
                },
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_5", _cityID),
                factoryProduction5.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject(_nulnFactory.GetToolTipFactoryPerLevel(5));
                    return _nulnFactory.FactoryLevel >= 5;
                },
                _ => {
                    _nulnFactory.SelectedFactoryLevel = 5;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 5");
                },
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption("nulnFactory_menu", String.Format("{0}_nulnFactoryProduction_levelup", _cityID),
                factoryProductionLevelUp.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.IsEnabled = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpFactory();
                    a.Tooltip = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpFactory() ? null : new TextObject("Pas assez d'or");
                    return _nulnFactory.FactoryLevel < 5;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpFactory());
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.FactoryLevel += 1;
                    _nulnFactory.SelectedFactoryLevel = _nulnFactory.FactoryLevel;
                    SetFactoryLevelProductionText();
                    GameMenu.SwitchToMenu("nulnFactory_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration l'usine");
                },
                isLeave: false);

        }

        public void SetFactoryLevelProductionText()
        {
            TextObject textObject = new TextObject("");
            for (var i = 0; i < 6; i++)
            {
                if (i == 0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau " + i);

                if (_nulnFactory.SelectedFactoryLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNFACTORYPRODUCTIONTEXT" + i, textObject);
            }
            TextObject levelUpText = new TextObject("[" + _nulnFactory.CalculatePriceToLevelUpFactory() + "{GOLD_ICON}] Améliorer l'usine");
            GameTexts.SetVariable("NULNFACTORYPRODUCTIONTEXTUP", levelUpText);
        }


        #region ProductionChoice
        public void RegisterProductionChoiceMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            var production0 = new TextObject("{NULNPRODUCTIONTEXT0}");
            var production1 = new TextObject("{NULNPRODUCTIONTEXT1}");
            SetProductionText();

            NulnFactory.PossibleProduction prodActuelle = _nulnFactory.chosenProduction;
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_weapon", _cityID),
                production0.Value,
                a => { a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; return true; },
                _ => {
                    if (_nulnFactory.chosenProduction == NulnFactory.PossibleProduction.Weapon) return;
                    prodActuelle = _nulnFactory.TryChangeProduction(NulnFactory.PossibleProduction.Weapon);;
                    SetProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                },
                isLeave: false);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_machiningParts", _cityID),
                production1.Value,
                a => { a.optionLeaveType = GameMenuOption.LeaveType.StagePrisonBreak; return true; },
                _ => {
                    if (_nulnFactory.chosenProduction == NulnFactory.PossibleProduction.MachiningPart) return;
                    prodActuelle = _nulnFactory.TryChangeProduction(NulnFactory.PossibleProduction.MachiningPart);
                    SetProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                },
                isLeave: false);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("nulnFactory_menu"),
                isLeave: true, index: 999);
        }

        public void SetProductionText()
        {
            TextObject weaponText = new TextObject("Armes");
            TextObject machiningPartText = new TextObject("Pièces d'usinage");

            if (_nulnFactory.chosenProduction == NulnFactory.PossibleProduction.Weapon)
            {
                weaponText = new TextObject($"[{weaponText.Value}]");
            }
            else if (_nulnFactory.chosenProduction == NulnFactory.PossibleProduction.MachiningPart)
            {
                machiningPartText = new TextObject($"[{machiningPartText.Value}]");
            }

            GameTexts.SetVariable("NULNPRODUCTIONTEXT0", weaponText);
            GameTexts.SetVariable("NULNPRODUCTIONTEXT1", machiningPartText);


        }

        #endregion


        #region RawMaterials Production
        public void RegisterRawMaterialsProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            //Donate prisoners or troops
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_rawMaterials_donatePrisoners", _cityID),
                "Embaucher de la main d'oeuvre (ne peut pas être récupérée)",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners; return true; },
                _ => {
                    MobileParty leftParty = new MobileParty();
                    PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(
                        leftParty, 
                        delegate (PartyBase leftOwnerParty, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, bool fromCancel) 
                        {
                            if (leftPrisonRoster.Count != 0)
                            {
                                foreach (TroopRosterElement item in leftPrisonRoster.GetTroopRoster())
                                {
                                    _nulnFactory.WorkStrenght += item.Character.Tier * item.Number;
                                }
                            }
                            if (leftMemberRoster.Count != 0)
                            {
                                foreach (TroopRosterElement item in leftMemberRoster.GetTroopRoster())
                                {
                                    _nulnFactory.WorkStrenght += item.Character.Tier * item.Number;
                                }
                            }
                            CreateOrUpdateGameMenuDesc(campaignGameStarter);
                        });
                    

                },
                isLeave: false);

            //EmptySpace
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //By a woodmill => +1 level for WoodFactory
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_buy_WoodProduction", _cityID),
                "["+NulnFactory.InitialRessourcePrice+"{GOLD_ICON}] Acheter une sierie",
                a => 
                {
                    a.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice;
                    a.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice ? null:new TextObject("Pas assez d'or");
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.WoodLevel==0;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialRessourcePrice);
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.WoodLevel+=1;
                    _nulnFactory.SelectedWoodLevel = _nulnFactory.WoodLevel;
                    SetWoodProductionText();
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID));
                },
                isLeave: false);

            //RawMaterials => WoodProduction
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_woodProduction", _cityID),
                "Visiter la scierie",
                a => 
                {
                    a.IsEnabled = true;
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.WoodLevel>=1;
                },
                _ => {
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                },
                isLeave: false);
            RegisterWoodProductionMenuOptions(campaignGameStarter);

            //By a charcoal furnace => +1 level for charcaolFactory
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_buy_CharcoalProduction", _cityID),
                "["+NulnFactory.InitialRessourcePrice+"{GOLD_ICON}] Acheter un four à charbon",
                a => 
                {
                    a.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice;
                    a.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice ? null:new TextObject("Pas assez d'or");
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.CharcoalLevel==0;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialRessourcePrice);
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.CharcoalLevel+=1;
                    _nulnFactory.SelectedCharcoalLevel = _nulnFactory.CharcoalLevel;
                    SetCharcoalProductionText();
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID));
                },
                isLeave: false);

            //RawMaterials => CharcoalProduction
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_charcoalProduction", _cityID),
                "Visiter le four à charbon",
                a => 
                {
                    a.IsEnabled = true;
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.CharcoalLevel>=1;
                },
                _ => {
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                },
                isLeave: false);
            RegisterCharcoalProductionMenuOptions(campaignGameStarter);

            //By an iron mine => +1 level for mineFactory
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_buy_IronProduction", _cityID),
                "["+NulnFactory.InitialRessourcePrice+"{GOLD_ICON}] Acheter une mine de fer",
                a => 
                {
                    a.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice;
                    a.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice ? null:new TextObject("Pas assez d'or");
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.IronLevel==0;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialRessourcePrice);
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.IronLevel+=1;
                    _nulnFactory.SelectedIronLevel = _nulnFactory.IronLevel;
                    SetIronProductionText();
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID));
                },
                isLeave: false);

            //RawMaterials => Iron Production
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_ironProduction", _cityID),
                "Visiter la mine de fer",
                a => 
                {
                    a.IsEnabled = true;
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.IronLevel>=1;
                },
                _ => {
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                },
                isLeave: false);
            RegisterIronProductionMenuOptions(campaignGameStarter);

            //Empty Spaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_rawMaterials_menu_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("nulnFactory_menu"),
                isLeave: true, index: 999);
        }


        public void RegisterWoodProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            
            var woodProduction0 = new TextObject("{NULNWOODPRODUCTIONTEXT0}");
            var woodProduction1 = new TextObject("{NULNWOODPRODUCTIONTEXT1}");
            var woodProduction2 = new TextObject("{NULNWOODPRODUCTIONTEXT2}");
            var woodProduction3 = new TextObject("{NULNWOODPRODUCTIONTEXT3}");
            var woodProduction4 = new TextObject("{NULNWOODPRODUCTIONTEXT4}");
            var woodProduction5 = new TextObject("{NULNWOODPRODUCTIONTEXT5}");
            var woodProductionLevelUp = new TextObject("{NULNWOODPRODUCTIONTEXTUP}");
            SetWoodProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_0", _cityID),
                woodProduction0.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 0;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois désactivée");
                },
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_1", _cityID),
                woodProduction1.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :"+NulnFactory.WoodProductionAndWorkStrenghtPerLevel[1].Production+
                    "\nMain d'oeuvre nécessaire :" + NulnFactory.WoodProductionAndWorkStrenghtPerLevel[1].WorkStrenght);
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 1;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 1");
                },
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_2", _cityID),
                woodProduction2.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.WoodProductionAndWorkStrenghtPerLevel[2].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.WoodProductionAndWorkStrenghtPerLevel[2].WorkStrenght);
                    return _nulnFactory.WoodLevel>=2;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 2;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 2");
                },
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_3", _cityID),
                woodProduction3.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.WoodProductionAndWorkStrenghtPerLevel[3].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.WoodProductionAndWorkStrenghtPerLevel[3].WorkStrenght);
                    return _nulnFactory.WoodLevel>=3;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 3;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 3");
                },
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_4", _cityID),
                woodProduction4.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.WoodProductionAndWorkStrenghtPerLevel[4].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.WoodProductionAndWorkStrenghtPerLevel[4].WorkStrenght);
                    return _nulnFactory.WoodLevel>=4;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 4;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 4");
                },
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_5", _cityID),
                woodProduction5.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.WoodProductionAndWorkStrenghtPerLevel[5].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.WoodProductionAndWorkStrenghtPerLevel[5].WorkStrenght);
                    return _nulnFactory.WoodLevel>=5;
                },
                _ => {
                    _nulnFactory.SelectedWoodLevel = 5;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 5");
                },
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_levelup", _cityID),
                woodProductionLevelUp.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.IsEnabled = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.WoodLevel);
                    a.Tooltip = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.WoodLevel)?null:new TextObject("Pas assez d'or");
                    return _nulnFactory.WoodLevel<5;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.WoodLevel));
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.WoodLevel+=1;
                    _nulnFactory.SelectedWoodLevel = _nulnFactory.WoodLevel;
                    SetWoodProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_woodProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de la scierie");
                },
                isLeave: false);


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: true, index: 999);
        }


        public void SetWoodProductionText()
        {
            TextObject textObject = new TextObject("");
            for (var i = 0; i < 6; i++)
            {
                if (i==0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau "+i);

                if (_nulnFactory.SelectedWoodLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNWOODPRODUCTIONTEXT"+i, textObject);
            }
            TextObject levelUpText = new TextObject("["+_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.WoodLevel)+"{GOLD_ICON}] Améliorer la scierie");
            GameTexts.SetVariable("NULNWOODPRODUCTIONTEXTUP", levelUpText);
        }

        public void RegisterCharcoalProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            
            var charcoalProduction0 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT0}");
            var charcoalProduction1 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT1}");
            var charcoalProduction2 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT2}");
            var charcoalProduction3 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT3}");
            var charcoalProduction4 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT4}");
            var charcoalProduction5 = new TextObject("{NULNCHARCOALPRODUCTIONTEXT5}");
            var charcoalProductionLevelUp = new TextObject("{NULNCHARCOALPRODUCTIONTEXTUP}");
            SetCharcoalProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_0", _cityID),
                charcoalProduction0.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 0;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de charbon désactivée");
                },
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_1", _cityID),
                charcoalProduction1.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de charbon :"+NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[1].Production+
                    "\nMain d'oeuvre nécessaire :" + NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[1].WorkStrenght+
                    "\nConsommation de bois :"+ NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[1].WoodConsumption);
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 1;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 1");
                },
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_2", _cityID),
                charcoalProduction2.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[2].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[2].WorkStrenght+
                    "\nConsommation de bois :"+ NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[2].WoodConsumption);
                    return _nulnFactory.CharcoalLevel>=2;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 2;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 2");
                },
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_3", _cityID),
                charcoalProduction3.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[3].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[3].WorkStrenght+
                    "\nConsommation de bois :"+ NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[3].WoodConsumption);
                    return _nulnFactory.CharcoalLevel>=3;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 3;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 3");
                },
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_4", _cityID),
                charcoalProduction4.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[4].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[4].WorkStrenght+
                    "\nConsommation de bois :"+ NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[4].WoodConsumption);
                    return _nulnFactory.CharcoalLevel>=4;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 4;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 4");
                },
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_5", _cityID),
                charcoalProduction5.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de bois :" +NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[5].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[5].WorkStrenght+
                    "\nConsommation de bois :"+ NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[5].WoodConsumption);
                    return _nulnFactory.CharcoalLevel>=5;
                },
                _ => {
                    _nulnFactory.SelectedCharcoalLevel = 5;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de bois au niveau 5");
                },
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_levelup", _cityID),
                charcoalProductionLevelUp.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.IsEnabled = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.CharcoalLevel);
                    a.Tooltip = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.CharcoalLevel)?null:new TextObject("Pas assez d'or");
                    return _nulnFactory.CharcoalLevel<5;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.CharcoalLevel));
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.CharcoalLevel+=1;
                    _nulnFactory.SelectedCharcoalLevel = _nulnFactory.CharcoalLevel;
                    SetCharcoalProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de la scierie");
                },
                isLeave: false);

            //EmptySapces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: true, index: 999);
        }

        public void SetCharcoalProductionText()
        {
            TextObject textObject = new TextObject("");
            for (var i = 0; i < 6; i++)
            {
                if (i==0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau "+i);

                if (_nulnFactory.SelectedCharcoalLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNCHARCOALPRODUCTIONTEXT"+i, textObject);
            }
            TextObject levelUpText = new TextObject("["+_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.CharcoalLevel)+"{GOLD_ICON}] Améliorer le four à charbon");
            GameTexts.SetVariable("NULNCHARCOALPRODUCTIONTEXTUP", levelUpText);
        }

        public void RegisterIronProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            
            var ironProduction0 = new TextObject("{NULNIRONPRODUCTIONTEXT0}");
            var ironProduction1 = new TextObject("{NULNIRONPRODUCTIONTEXT1}");
            var ironProduction2 = new TextObject("{NULNIRONPRODUCTIONTEXT2}");
            var ironProduction3 = new TextObject("{NULNIRONPRODUCTIONTEXT3}");
            var ironProduction4 = new TextObject("{NULNIRONPRODUCTIONTEXT4}");
            var ironProduction5 = new TextObject("{NULNIRONPRODUCTIONTEXT5}");
            var ironProductionLevelUp = new TextObject("{NULNIRONPRODUCTIONTEXTUP}");
            SetIronProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_0", _cityID),
                ironProduction0.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 0;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer désactivée");
                },
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_1", _cityID),
                ironProduction1.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de fer :"+NulnFactory.IronProductionAndWorkStrenghtPerLevel[1].Production+
                    "\nMain d'oeuvre nécessaire :" + NulnFactory.IronProductionAndWorkStrenghtPerLevel[1].WorkStrenght);
                    return true;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 1;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer au niveau 1");
                },
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_2", _cityID),
                ironProduction2.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de fer :" +NulnFactory.IronProductionAndWorkStrenghtPerLevel[2].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.IronProductionAndWorkStrenghtPerLevel[2].WorkStrenght);
                    return _nulnFactory.IronLevel>=2;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 2;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer au niveau 2");
                },
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_3", _cityID),
                ironProduction3.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de fer :" +NulnFactory.IronProductionAndWorkStrenghtPerLevel[3].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.IronProductionAndWorkStrenghtPerLevel[3].WorkStrenght);
                    return _nulnFactory.IronLevel>=3;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 3;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer au niveau 3");
                },
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_4", _cityID),
                ironProduction4.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de fer :" +NulnFactory.IronProductionAndWorkStrenghtPerLevel[4].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.IronProductionAndWorkStrenghtPerLevel[4].WorkStrenght);
                    return _nulnFactory.IronLevel>=4;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 4;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer au niveau 4");
                },
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_5", _cityID),
                ironProduction5.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Production de fer :" +NulnFactory.IronProductionAndWorkStrenghtPerLevel[5].Production+
                    "\nMain d'oeuvre nécessaire :"+NulnFactory.IronProductionAndWorkStrenghtPerLevel[5].WorkStrenght);
                    return _nulnFactory.IronLevel>=5;
                },
                _ => {
                    _nulnFactory.SelectedIronLevel = 5;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production de fer au niveau 5");
                },
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_levelup", _cityID),
                ironProductionLevelUp.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.IsEnabled = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronLevel);
                    a.Tooltip = Hero.MainHero.Gold >=_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronLevel)?null:new TextObject("Pas assez d'or");
                    return _nulnFactory.IronLevel<5;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronLevel));
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.IronLevel+=1;
                    _nulnFactory.SelectedIronLevel = _nulnFactory.IronLevel;
                    SetIronProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de la scierie");
                },
                isLeave: false);


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: true, index: 999);
        }


        public void SetIronProductionText()
        {
            TextObject textObject = new TextObject("");
            for (var i = 0; i < 6; i++)
            {
                if (i==0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau "+i);

                if (_nulnFactory.SelectedIronLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNIRONPRODUCTIONTEXT"+i, textObject);
            }
            TextObject levelUpText = new TextObject("["+_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronLevel)+"{GOLD_ICON}] Améliorer la mine de fer");
            GameTexts.SetVariable("NULNIRONPRODUCTIONTEXTUP", levelUpText);
        }



        #endregion







    }
}
