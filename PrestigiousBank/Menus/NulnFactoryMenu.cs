using Messages.FromClient.ToLobbyServer;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;

namespace PrestigiousBank
{
    public class NulnFactoryMenu
    {
        public string _cityName;
        public string _cityID;
        public NulnFactory _nulnFactory;
        public static int _optionBankIndex = -1;

        public virtual void RegisterBankMenu(CampaignGameStarter campaignGameStarter, NulnFactory NulnFactory)
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
                                                      NulnFactory.Level += 1;
                                                      GameMenu.SwitchToMenu("town"); 
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



/*            // Bank Menu -> Compte
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_account", _cityID), "Accéder au compte",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_account", _cityID)),
                isLeave: false, index: 1);
            RegisterAccountMenuOptions(campaignGameStarter);*/



            //Quitter la banque
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_nulnFactory_menu", _cityID), String.Format("{0}_nulnFactory_menu_back", _cityID), "Quitter l'usine'",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: -1);
        }

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {

/*            string clientLevelString = _nulnFactory.GetCustomerLevelString();
            int currentSolde = _nulnFactory.Solde;
            float interestRatePercentage = _nulnFactory.CalculateInterestRate() * 100f;
            // Bank Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_menu", _cityID),
                String.Format("Bienvenue à la banque de {0}.\nNiveau du client : {1}\nSolde : {2}\nTaux d'intérêts : {3}%/jour", _cityName, clientLevelString, currentSolde, interestRatePercentage.ToString("G3")),
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            //AccountMenu
            campaignGameStarter.AddGameMenu(String.Format("{0}_account", _cityID),
                String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //Deposit Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_deposit", _cityID),
                String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //Withdraw Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_withdraw", _cityID),
                String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour",
                currentSolde,
                interestRatePercentage.ToString("G3")),
                null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);
*/

        }



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
