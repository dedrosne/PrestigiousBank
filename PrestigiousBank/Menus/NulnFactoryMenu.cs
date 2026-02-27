using Helpers;
using Messages.FromClient.ToLobbyServer;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using TaleWorlds.ActivitySystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CampaignMechanics.Menagery;
using TOR_Core.Extensions;

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
                                                      _nulnFactory.SelectedFactoryLevel = 1;
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
                _ => InventoryManager.OpenScreenAsStash(_nulnFactory.GetItemStash()),//Crash. Why ? InventoryManager Null
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
            NulnFactory.PossibleProduction chosenProduction = _nulnFactory.ChosenProduction;
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

            // IRON INGOT Production
            campaignGameStarter.AddGameMenu(String.Format("{0}_ironFurnaceProduction", _cityID),
                "Fonderie :" +
                "\nProduit chaque jour des lingots de fer en fonction du niveau de production, charbon, minerai et de la force de travail disponible\n" +
                "Force de travail : " + _nulnFactory.WorkStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            // SILVER Production
            campaignGameStarter.AddGameMenu(String.Format("{0}_silverProduction", _cityID),
                "Mine d'argent :" +
                "\nProduit chaque jour de l'argent en fonction du niveau de production et de la force de travail disponible\n" +
                "Force de travail : " + _nulnFactory.WorkStrenght,
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
            TextObject textObject;
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

            NulnFactory.PossibleProduction prodActuelle = _nulnFactory.ChosenProduction;
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_weapon", _cityID),
                production0.Value,
                a => { a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; return true; },
                _ => {
                    if (_nulnFactory.ChosenProduction == NulnFactory.PossibleProduction.Weapon) return;
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
                    if (_nulnFactory.ChosenProduction == NulnFactory.PossibleProduction.MachiningPart) return;
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

            if (_nulnFactory.ChosenProduction == NulnFactory.PossibleProduction.Weapon)
            {
                weaponText = new TextObject($"[{weaponText.Value}]");
            }
            else if (_nulnFactory.ChosenProduction == NulnFactory.PossibleProduction.MachiningPart)
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
                    leftParty.SetCustomName(new TextObject("Ouvriers"));
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

            //By an iron furnace => +1 level for ironFurnace
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_buy_IronFurnace", _cityID),
                "[" + NulnFactory.InitialRessourcePrice + "{GOLD_ICON}] Acheter une fonderie",
                a =>
                {
                    a.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice;
                    a.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice ? null : new TextObject("Pas assez d'or");
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.IronFurnaceLevel == 0;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialRessourcePrice);
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.IronFurnaceLevel += 1;
                    _nulnFactory.SelectedIronFurnaceLevel = _nulnFactory.IronFurnaceLevel;
                    SetIronFurnaceProductionText();
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID));
                },
                isLeave: false);

            //RawMaterials => IronFurnace Production
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_ironFurnaceProduction", _cityID),
                "Visiter la fonderie",
                a =>
                {
                    a.IsEnabled = true;
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.IronFurnaceLevel >= 1;
                },
                _ => {
                    GameMenu.SwitchToMenu(String.Format("{0}_ironFurnaceProduction", _cityID));
                },
                isLeave: false);
            RegisterIronFurnaceProductionMenuOptions(campaignGameStarter);

            //By a silver mine => +1 level for silver
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_buy_silverMine", _cityID),
                "[" + NulnFactory.InitialRessourcePrice + "{GOLD_ICON}] Acheter une mine d'argent",
                a =>
                {
                    a.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice;
                    a.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialRessourcePrice ? null : new TextObject("Pas assez d'or");
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.SilverLevel == 0;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialRessourcePrice);
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _nulnFactory.SilverLevel += 1;
                    _nulnFactory.SelectedSilverLevel = _nulnFactory.SilverLevel;
                    SetSilverProductionText();
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID));
                },
                isLeave: false);

            //RawMaterials => IronFurnace Production
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_silverProduction", _cityID),
                "Visiter la mine d'argent",
                a =>
                {
                    a.IsEnabled = true;
                    a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners;//TOCHANGE
                    return _nulnFactory.SilverLevel >= 1;
                },
                _ => {
                    GameMenu.SwitchToMenu(String.Format("{0}_silverProduction", _cityID));
                },
                isLeave: false);
            RegisterSilverProductionMenuOptions(campaignGameStarter);

            //Empty Spaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_rawMaterials_menu_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("nulnFactory_menu"),
                isLeave: true, index: 999);
        }

        #region Wood
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
                a => WoodCondition(0, a),
                _ => WoodConsequence(0, campaignGameStarter),
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_1", _cityID),
                woodProduction1.Value,
                a => WoodCondition(1, a),
                _ => WoodConsequence(1, campaignGameStarter),
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_2", _cityID),
                woodProduction2.Value,
                a => WoodCondition(2, a),
                _ => WoodConsequence(2, campaignGameStarter),
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_3", _cityID),
                woodProduction3.Value,
                a => WoodCondition(3, a),
                _ => WoodConsequence(3, campaignGameStarter),
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_4", _cityID),
                woodProduction4.Value,
                a => WoodCondition(4, a),
                _ => WoodConsequence(4, campaignGameStarter),
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_woodProduction", _cityID), String.Format("{0}_woodProduction_5", _cityID),
                woodProduction5.Value,
                a => WoodCondition(5,a),
                _ => WoodConsequence(5,campaignGameStarter),
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

        public bool WoodCondition(int i, MenuCallbackArgs a)
        {
            a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
            if (i == 0) a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
            else
            {
                a.Tooltip = new TextObject("Production de bois :" + NulnFactory.WoodProductionAndWorkStrenghtPerLevel[i].Production +
                "\nMain d'oeuvre nécessaire :" + NulnFactory.WoodProductionAndWorkStrenghtPerLevel[i].WorkStrenght);
            }
            return _nulnFactory.WoodLevel >= i;
        }

        public void WoodConsequence(int i, CampaignGameStarter campaignGameStarter)
        {
            _nulnFactory.SelectedCharcoalLevel = i;
            SetCharcoalProductionText();
            GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            if (i == 0) PrestigiousBank.LogMessage("Production de charbon désactivée");
            else PrestigiousBank.LogMessage("Production de charbon au niveau" + i);
        }


        public void SetWoodProductionText()
        {
            TextObject textObject;
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

        #endregion

        #region Charcoal

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
                a => CharcoalCondition(0, a),
                _ => CharcoalConsequence(0, campaignGameStarter),
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_1", _cityID),
                charcoalProduction1.Value,
                a => CharcoalCondition(1, a),
                _ => CharcoalConsequence(1, campaignGameStarter),
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_2", _cityID),
                charcoalProduction2.Value,
                a => CharcoalCondition(2, a),
                _ => CharcoalConsequence(2, campaignGameStarter),
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_3", _cityID),
                charcoalProduction3.Value,
                a => CharcoalCondition(3, a),
                _ => CharcoalConsequence(3, campaignGameStarter),
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_4", _cityID),
                charcoalProduction4.Value,
                a => CharcoalCondition(4, a),
                _ => CharcoalConsequence(4, campaignGameStarter),
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_charcoalProduction", _cityID), String.Format("{0}_charcoalProduction_5", _cityID),
                charcoalProduction5.Value,
                a => CharcoalCondition(5,a),
                _ => CharcoalConsequence(5,campaignGameStarter),
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

        public bool CharcoalCondition(int i, MenuCallbackArgs a)
        {
            a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
            if (i == 0) a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
            else
            {
                a.Tooltip = new TextObject("Production de charbon :" + NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[i].Production +
                "\nMain d'oeuvre nécessaire :" + NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[i].WorkStrenght +
                "\nConsommation de bois :" + NulnFactory.CharcoalProductionWoodAndWorkStrenghtPerLevel[i].WoodConsumption);
            }
            return _nulnFactory.CharcoalLevel >= i;
        }

        public void CharcoalConsequence(int i, CampaignGameStarter campaignGameStarter)
        {
            _nulnFactory.SelectedCharcoalLevel = i;
            SetCharcoalProductionText();
            GameMenu.SwitchToMenu(String.Format("{0}_charcoalProduction", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            if (i == 0) PrestigiousBank.LogMessage("Production de charbon désactivée");
            else PrestigiousBank.LogMessage("Production de charbon au niveau" + i);
        }

        public void SetCharcoalProductionText()
        {
            TextObject textObject;
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

        #endregion

        #region Iron
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
                a => IronCondition(0, a),
                _ => IronConsequence(0, campaignGameStarter),
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_1", _cityID),
                ironProduction1.Value,
                a => IronCondition(1, a),
                _ => IronConsequence(1, campaignGameStarter),
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_2", _cityID),
                ironProduction2.Value,
                a => IronCondition(2, a),
                _ => IronConsequence(2, campaignGameStarter),
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_3", _cityID),
                ironProduction3.Value,
                a => IronCondition(3, a),
                _ => IronConsequence(3, campaignGameStarter),
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_4", _cityID),
                ironProduction4.Value,
                a => IronCondition(4, a),
                _ => IronConsequence(4, campaignGameStarter),
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironProduction", _cityID), String.Format("{0}_ironProduction_5", _cityID),
                ironProduction5.Value,
                a => IronCondition(5,a),
                _ =>IronConsequence(5,campaignGameStarter),
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
                    PrestigiousBank.LogMessage("Amélioration de la fonderie");
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
            TextObject textObject;
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

        public bool IronCondition(int i, MenuCallbackArgs a)
        {
            a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
            if (i==0) a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
            else a.Tooltip = new TextObject("Production de fer :" + NulnFactory.IronProductionAndWorkStrenghtPerLevel[i].Production +
                "\nMain d'oeuvre nécessaire :" + NulnFactory.IronProductionAndWorkStrenghtPerLevel[i].WorkStrenght);
            return _nulnFactory.IronLevel >= i;
        }

        public void IronConsequence(int i, CampaignGameStarter campaignGameStarter)
        {
            _nulnFactory.SelectedIronLevel = i;
            SetIronProductionText();
            GameMenu.SwitchToMenu(String.Format("{0}_ironProduction", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            if (i==0) PrestigiousBank.LogMessage("Production de fer désactivée");
            else PrestigiousBank.LogMessage("Production de fer au niveau "+i);
        }

        #endregion

        #region Iron Furnace
        public void RegisterIronFurnaceProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            var ironFurnaceProduction0 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT0}");
            var ironFurnaceProduction1 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT1}");
            var ironFurnaceProduction2 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT2}");
            var ironFurnaceProduction3 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT3}");
            var ironFurnaceProduction4 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT4}");
            var ironFurnaceProduction5 = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXT5}");
            var ironFurnaceProductionLevelUp = new TextObject("{NULNIRONFURNACEPRODUCTIONTEXTUP}");
            SetIronFurnaceProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_0", _cityID),
                ironFurnaceProduction0.Value,
                a => IronFurnaceCondition(0,a),
                _ => IronFurnaceConsequence(0,campaignGameStarter),
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_1", _cityID),
                ironFurnaceProduction1.Value,
                a => IronFurnaceCondition(1, a),
                _ => IronFurnaceConsequence(1, campaignGameStarter),
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_2", _cityID),
                ironFurnaceProduction2.Value,
                a => IronFurnaceCondition(2, a),
                _ => IronFurnaceConsequence(2, campaignGameStarter),
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_3", _cityID),
                ironFurnaceProduction3.Value,
                a => IronFurnaceCondition(3, a),
                _ => IronFurnaceConsequence(3, campaignGameStarter),
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_4", _cityID),
                ironFurnaceProduction4.Value,
                a => IronFurnaceCondition(4, a),
                _ => IronFurnaceConsequence(4, campaignGameStarter),
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_5", _cityID),
                ironFurnaceProduction5.Value,
                a => IronFurnaceCondition(5, a),
                _ => IronFurnaceConsequence(5, campaignGameStarter),
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_levelup", _cityID),
                            ironFurnaceProductionLevelUp.Value,
                            a => {
                                a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                                a.IsEnabled = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronFurnaceLevel);
                                a.Tooltip = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronFurnaceLevel) ? null : new TextObject("Pas assez d'or");
                                return _nulnFactory.IronFurnaceLevel < 5;
                            },
                            _ => {
                                Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronFurnaceLevel));
                                SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                                _nulnFactory.IronFurnaceLevel += 1;
                                _nulnFactory.SelectedIronFurnaceLevel = _nulnFactory.IronFurnaceLevel;
                                SetIronFurnaceProductionText();
                                GameMenu.SwitchToMenu(String.Format("{0}_ironFurnaceProduction", _cityID));
                                CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                PrestigiousBank.LogMessage("Amélioration de la fonderie");
                            },
                            isLeave: false);

            //EmptySapces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_ironFurnaceProduction", _cityID), String.Format("{0}_ironFurnaceProduction_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: true, index: 999);
        }

        public bool IronFurnaceCondition (int i,  MenuCallbackArgs a) 
        {
            a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
            if (i == 0) a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
            else
            {
                a.Tooltip = new TextObject("Production de lingots de fer :" + NulnFactory.IronIngotProductionCharcoalOreAndWorkStrenghtPerLevel[i].Production +
                    "\nMain d'oeuvre nécessaire :" + NulnFactory.IronIngotProductionCharcoalOreAndWorkStrenghtPerLevel[i].WorkStrenght +
                    "\nConsommation de charbon :" + NulnFactory.IronIngotProductionCharcoalOreAndWorkStrenghtPerLevel[i].CharcoalConsumption +
                    "\nConsommation de minerai :" + NulnFactory.IronIngotProductionCharcoalOreAndWorkStrenghtPerLevel[i].IronOreConsumption);
            }
            return _nulnFactory.IronFurnaceLevel >= i;
        }

        public void IronFurnaceConsequence(int i, CampaignGameStarter campaignGameStarter)
        {
            _nulnFactory.SelectedIronFurnaceLevel = i;
            SetIronFurnaceProductionText();
            GameMenu.SwitchToMenu(String.Format("{0}_ironFurnaceProduction", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            if (i == 0) PrestigiousBank.LogMessage("Production de lingots désactivée");
            else PrestigiousBank.LogMessage("Production de lingots au niveau" + i);
        }

        public void SetIronFurnaceProductionText()
        {
            TextObject textObject;
            for (var i = 0; i < 6; i++)
            {
                if (i == 0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau " + i);

                if (_nulnFactory.SelectedIronFurnaceLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNIRONFURNACEPRODUCTIONTEXT" + i, textObject);
            }
            TextObject levelUpText = new TextObject("[" + _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.IronFurnaceLevel) + "{GOLD_ICON}] Améliorer la fonderie");
            GameTexts.SetVariable("NULNIRONFURNACEPRODUCTIONTEXTUP", levelUpText);
        }

        #endregion

        #region Silver

        public void RegisterSilverProductionMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            var silverProduction0 = new TextObject("{NULNSILVERPRODUCTIONTEXT0}");
            var silverProduction1 = new TextObject("{NULNSILVERPRODUCTIONTEXT1}");
            var silverProduction2 = new TextObject("{NULNSILVERPRODUCTIONTEXT2}");
            var silverProduction3 = new TextObject("{NULNSILVERPRODUCTIONTEXT3}");
            var silverProduction4 = new TextObject("{NULNSILVERPRODUCTIONTEXT4}");
            var silverProduction5 = new TextObject("{NULNSILVERPRODUCTIONTEXT5}");
            var silverProductionLevelUp = new TextObject("{NULNSILVERPRODUCTIONTEXTUP}");
            SetSilverProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_0", _cityID),
                silverProduction0.Value,
                a => SilverCondition(0, a),
                _ => SilverConsequence(0, campaignGameStarter),
                isLeave: false);

            //Niveau 1 ==> SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_1", _cityID),
                silverProduction1.Value,
                a => SilverCondition(1, a),
                _ => SilverConsequence(1, campaignGameStarter),
                isLeave: false);

            //Niveau 2 ==> SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_2", _cityID),
                silverProduction2.Value,
                a => SilverCondition(2, a),
                _ => SilverConsequence(2, campaignGameStarter),
                isLeave: false);
            //Niveau 3 ==> SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_3", _cityID),
                silverProduction3.Value,
                a => SilverCondition(3, a),
                _ => SilverConsequence(3, campaignGameStarter),
                isLeave: false);
            //Niveau 4 ==> SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_4", _cityID),
                silverProduction4.Value,
                a => SilverCondition(4, a),
                _ => SilverConsequence(4, campaignGameStarter),
                isLeave: false);
            //Niveau 5 ==> SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_5", _cityID),
                silverProduction5.Value,
                a => SilverCondition(5, a),
                _ => SilverConsequence(5, campaignGameStarter),
                isLeave: false);

            //Level UP
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_levelup", _cityID),
                            silverProductionLevelUp.Value,
                            a => {
                                a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                                a.IsEnabled = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.SilverLevel);
                                a.Tooltip = Hero.MainHero.Gold >= _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.SilverLevel) ? null : new TextObject("Pas assez d'or");
                                return _nulnFactory.SilverLevel < 5;
                            },
                            _ => {
                                Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.SilverLevel));
                                SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                                _nulnFactory.SilverLevel += 1;
                                _nulnFactory.SelectedSilverLevel = _nulnFactory.SilverLevel;
                                SetSilverProductionText();
                                GameMenu.SwitchToMenu(String.Format("{0}_silverProduction", _cityID));
                                CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                PrestigiousBank.LogMessage("Amélioration de la mide d'argent");
                            },
                            isLeave: false);

            //EmptySapces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_silverProduction", _cityID), String.Format("{0}_silverProduction_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: true, index: 999);
        }

        public bool SilverCondition(int i, MenuCallbackArgs a)
        {
            a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
            if (i == 0) a.Tooltip = new TextObject("Désactiver la production. \nPas de consommation de main d'oeuvre");
            else
            {
                a.Tooltip = new TextObject("Production d'argent :" + NulnFactory.SilverProductionAndWorkStrenghtPerLevel[i].Production +
                    "\nMain d'oeuvre nécessaire :" + NulnFactory.SilverProductionAndWorkStrenghtPerLevel[i].WorkStrenght);
            }
            return _nulnFactory.SilverLevel >= i;
        }

        public void SilverConsequence(int i, CampaignGameStarter campaignGameStarter)
        {
            _nulnFactory.SelectedSilverLevel = i;
            SetSilverProductionText();
            GameMenu.SwitchToMenu(String.Format("{0}_silverProduction", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            if (i == 0) PrestigiousBank.LogMessage("Production d'argent désactivée");
            else PrestigiousBank.LogMessage("Production d'argent au niveau" + i);
        }

        public void SetSilverProductionText()
        {
            TextObject textObject;
            for (var i = 0; i < 6; i++)
            {
                if (i == 0) textObject = new TextObject("Désactivé");
                else textObject = new TextObject("Niveau " + i);

                if (_nulnFactory.SelectedSilverLevel == i)
                {
                    textObject = new TextObject($"[{textObject.Value}]");
                }
                GameTexts.SetVariable("NULNSILVERPRODUCTIONTEXT" + i, textObject);
            }
            TextObject levelUpText = new TextObject("[" + _nulnFactory.CalculatePriceToLevelUpRessource(_nulnFactory.SilverLevel) + "{GOLD_ICON}] Améliorer la mine d'argent");
            GameTexts.SetVariable("NULNSILVERPRODUCTIONTEXTUP", levelUpText);
        }
        #endregion

        #endregion







    }
}
