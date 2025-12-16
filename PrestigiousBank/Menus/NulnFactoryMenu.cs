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
                                                  String.Format("{0}_nulnFactory_menu", _cityID),
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
                                                  String.Format("{0}_nulnFactory_menu", _cityID),
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
                                                      GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)); },
                                                  isLeave: false,
                                                  _optionBankIndex);


            //Upgrade the factory
            TextObject priceText = new TextObject(_nulnFactory.CalculatePriceToLevelUpFactory());
            GameTexts.SetVariable("PRICE_LEVEL_UP", priceText);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_factoryUpgrade", _cityID), 
                "[{PRICE_LEVEL_UP}{GOLD_ICON}] Améliorer l'usine",
                a => { 
                    if (_nulnFactory.FactoryLevel >= 5) return false;
                    a.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    if (Hero.MainHero.Gold > _nulnFactory.CalculatePriceToLevelUpFactory()) a.IsEnabled = true;
                    else
                    {
                        a.IsEnabled = false;
                        a.Tooltip = new TextObject("Pas assez de {GOLD_ICON}");
                    }
                        return true; },
                _ => {
                    _nulnFactory.FactoryLevel += 1;
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUpFactory());
                    GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUpFactory());
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)); },
                isLeave: false, index: 0);


            // Factory Menu -> ProductionChoice
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_productionChoice", _cityID), "Changer la production",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Manage; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID)),
                isLeave: false, index: 1);
            RegisterProductionChoiceMenuOptions(campaignGameStarter);


            //Open Stash
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_openStash", _cityID), "Ouvrir l'entrepôt",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => InventoryManager.OpenScreenAsStash(_nulnFactory.GetItemStash()),
                isLeave: false, index: 2);

            // Factory Menu -> RawMaterials Production
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_rawMaterials_menu", _cityID), "Production de matières premières",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.SneakIn; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_rawMaterials_menu", _cityID)),
                isLeave: false, index: 3);
            RegisterRawMaterialsProductionMenuOptions(campaignGameStarter);


            //Quitter la banque
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_nulnFactory_menu_back", _cityID), "Quitter l'usine",
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
            campaignGameStarter.AddGameMenu(String.Format("{0}_nulnFactory_menu", _cityID),
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
                "Force de travail : "+_nulnFactory.WorkStrenght,
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
                    prodActuelle = NulnFactory.PossibleProduction.Weapon;
                    _nulnFactory.chosenProduction = prodActuelle;
                    SetProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production lancée : Armes");
                },
                isLeave: false);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_machiningParts", _cityID),
                production1.Value,
                a => { a.optionLeaveType = GameMenuOption.LeaveType.StagePrisonBreak; return true; },
                _ => {
                    prodActuelle = NulnFactory.PossibleProduction.MachiningPart;
                    _nulnFactory.chosenProduction = prodActuelle;
                    SetProductionText();
                    GameMenu.SwitchToMenu(String.Format("{0}_productionChoice", _cityID));
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Production lancée : Pièces d'usinage");
                },
                isLeave: false);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_productionChoice", _cityID), String.Format("{0}_productionChoice_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)),
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
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("_rawMaterials_menu");
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
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("_rawMaterials_menu");
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

            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_rawMaterials_menu_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)),
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


        #endregion







    }
}
