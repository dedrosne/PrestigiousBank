using Messages.FromClient.ToLobbyServer;
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
    public class BankMenu
    {
        public string _cityName;
        public string _cityID;
        public Bank _bank;
        public static int _optionBankIndex = -2;

        public virtual void RegisterBankMenu(CampaignGameStarter campaignGameStarter, Bank bank)
        {
            _bank = bank;
            _cityID = bank.Ville.Town.StringId;
            _cityName = bank.Ville.Name.Value;
            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");
            MBTextManager.SetTextVariable("PRESTIGE_ICON",
                    CustomResourceManager.GetResourceObject("Prestige").GetCustomResourceIconAsText());


            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            //Town -> BankMenu
            campaignGameStarter.AddGameMenuOption("town",
                                                  String.Format("{0}_bank_menu", _cityID),
                                                  String.Format("Banque de {0}", _cityName),
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.OpenStash;
                                                      InformationManager.DisplayMessage(new InformationMessage(String.Format("TownID : {0}", Settlement.CurrentSettlement.Town.StringId)));
                                                      InformationManager.DisplayMessage(new InformationMessage(String.Format("TownID Prosperity : {0}", Settlement.CurrentSettlement.Town.Prosperity)));

                                                      if (Settlement.CurrentSettlement.Town.StringId == _cityID) return true;
                                                      else return false;
                                                  },
                                                  _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                                                  isLeave: false,
                                                  _optionBankIndex);



            // Bank Menu -> Compte
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_account", _cityID), "Accéder au compte",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_account", _cityID)),
                isLeave: false);
            RegisterAccountMenuOptions(campaignGameStarter);



            //Quitter la banque
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_menu_back", _cityID), "Quitter la banque",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: 999);
        }

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {

            string clientLevelString = _bank.GetCustomerLevelString();
            int currentSolde = _bank.Solde;
            float interestRatePercentage = _bank.CalculateInterestRate() * 100f;
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


        }



        private void RegisterAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            // Compte -> Dépot
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
                isLeave: true, index: 999);

        }



        #region Deposit
        private void RegisterDepositMenuOptions(CampaignGameStarter campaignGameStarter)
        {


            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                MBTextManager.SetTextVariable("AMOUNT", qty);
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_deposit", _cityID), 
                    String.Format("{0}_bank_deposit_{1}",_cityID, qty), 
                    "Déposer "+qties+" {GOLD_ICON}",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositGold(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_deposit", _cityID), String.Format("{0}_bank_deposit_all", _cityID), "Tout déposer",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
                        a.IsEnabled = Hero.MainHero.Gold > 0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositGold(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_deposit", _cityID), String.Format("{0}_bank_deposit_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_account", _cityID)),
                isLeave: false, 6, isRepeatable: false);
        }


        public bool IsAbleToDeposit(int amount)
        {
            return Hero.MainHero.Gold >= amount;
        }

        private void DepositGold(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            _bank.Solde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Dépot de {0} validé.\nNouvelle solde de compte : {1}", amount, _bank.Solde), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu(String.Format("{0}_bank_deposit", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion

        #region Withdraw
        private void RegisterWithdrawMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                MBTextManager.SetTextVariable("AMOUNT", qty);
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_withdraw", _cityID), 
                    String.Format("{0}_bank_withdraw_{1}",_cityID, qty),
                    "Retirer "+qties+" {GOLD_ICON}",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Trade;
                        a.IsEnabled = IsAbleToWithdraw(qty);
                        a.Tooltip = IsAbleToWithdraw(qty) ? null : new TextObject("Pas assez de solde", null);
                        return true;
                    },
                    _ => WithdrawGold(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Withdraw All
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_withdraw", _cityID), String.Format("{0}_bank_withdraw_all", _cityID), "Tout retirer",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Trade;
                        a.IsEnabled = _bank.Solde > 0;
                        a.Tooltip = _bank.Solde > 0 ? null : new TextObject("Pas assez de solde", null);
                        return true;
                    },
                    _ => WithdrawGold(_bank.Solde, campaignGameStarter),
                    isLeave: false,
                    i, true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_withdraw", _cityID), String.Format("{0}_bank_withdraw_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_account", _cityID)),
                isLeave: true, index: 999);
        }

        private bool IsAbleToWithdraw(int amount)
        {
            return _bank.Solde >= amount;
        }

        private void WithdrawGold(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (_bank.Solde < amount) { return; }

            _bank.Solde -= amount;
            Hero.MainHero.ChangeHeroGold(amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Retrait de {0} validé.\nNouvelle solde de compte : {1}", amount, _bank.Solde), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu(String.Format("{0}_bank_withdraw", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            //Campaign.Current.CurrentMenuContext.Refresh();//Don't work to refresh
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_positive"));
        }

        #endregion



    }
}
