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
                                                  "["+NulnFactory.InitialPrice+"{GOLD_ICON}] Acheter une usine à Nuln",
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      args.IsEnabled = Hero.MainHero.Gold >= NulnFactory.InitialPrice;
                                                      args.Tooltip = Hero.MainHero.Gold >= NulnFactory.InitialPrice ? null : new TextObject("Pas assez d'or");
                                                      if (Settlement.CurrentSettlement.Town.StringId != _cityID) return false;
                                                      else if (NulnFactory.Level > 0) return false;
                                                      else return true;
                                                  },
                                                  _ => {
                                                      _nulnFactory.Level += 1;
                                                      Hero.MainHero.ChangeHeroGold(-NulnFactory.InitialPrice);
                                                      GameMenu.SwitchToMenu("town");
                                                      CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                                      GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUp());
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
                                                      else if (NulnFactory.Level == 0) return false;
                                                      else return true;
                                                  },
                                                  _ => GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)),
                                                  isLeave: false,
                                                  _optionBankIndex);


            //Upgrade the factory
            TextObject priceText = new TextObject(_nulnFactory.CalculatePriceToLevelUp());
            GameTexts.SetVariable("PRICE_LEVEL_UP", priceText);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_factoryUpgrade", _cityID), 
                "[{PRICE_LEVEL_UP}{GOLD_ICON}] Améliorer l'usine",
                a => { 
                    if (_nulnFactory.Level >= 5) return false;
                    a.optionLeaveType = GameMenuOption.LeaveType.Continue;
                    if (Hero.MainHero.Gold > _nulnFactory.CalculatePriceToLevelUp()) a.IsEnabled = true;
                    else
                    {
                        a.IsEnabled = false;
                        a.Tooltip = new TextObject("Pas assez de {GOLD_ICON}");
                    }
                        return true; },
                _ => {
                    _nulnFactory.Level += 1;
                    Hero.MainHero.ChangeHeroGold(-_nulnFactory.CalculatePriceToLevelUp());
                    GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUp());
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

            int factoryLevel = _nulnFactory.Level;
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
                "\nChaque jour, les prisonniers meurent à la tâche mais produisent 10% de leur capacité (somme de leur tiers)\n"+
                "Force de travail : "+_nulnFactory.WorkStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);


            /*            //Deposit Menu
                        campaignGameStarter.AddGameMenu(String.Format("{0}_bank_deposit", _cityID),
                            String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

                        //Withdraw Menu
                        campaignGameStarter.AddGameMenu(String.Format("{0}_bank_withdraw", _cityID),
                            String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour",
                            currentSolde,
                            interestRatePercentage.ToString("G3")),
                            null,
                            TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);*/


        }

        #region ProductionChoice
        public void RegisterProductionChoiceMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            var production0 = new TextObject("{NULNPRODUCTIONTEXT0}");
            var production1 = new TextObject("{NULNPRODUCTIONTEXT1}");
            //var activity2 = new TextObject("{HIRELINGACTIVITYTEXT2}");
            //var activity3 = new TextObject("{HIRELINGACTIVITYTEXT3}");
            //var activity4 = new TextObject("{HIRELINGACTIVITYTEXT4}");
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
                        });

                },
                isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_rawMaterials_menu", _cityID), String.Format("{0}_rawMaterials_menu_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_nulnFactory_menu", _cityID)),
                isLeave: true, index: 999);
        }

        public void onPartyScreenClosedDelegate()
        {

        }

        #endregion

        private void RegisterAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
/*            // Compte -> Dépot
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_account", _cityID), String.Format("{0}_bank_deposit", _cityID), "Déposer de l'argent",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Ransom; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_deposit", _cityID)),
                isLeave: false);
            RegisterDepositMenuOptions(campaignGameStarter);
            // Compte -> Retrait
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_account", _cityID), String.Format("{0}_bank_withdraw", _cityID), "Retirer de l'argent",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Trade; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_withdraw", _cityID)),
                isLeave: false);
            RegisterWithdrawMenuOptions(campaignGameStarter);
            // Compte -> BankMenu
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_account", _cityID), String.Format("{0}_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999);*/

        }





    }
}
