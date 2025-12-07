using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Engine;

namespace PrestigiousBank
{
    public static class DrakenhofBankMenu
    {
        public static int _optionBankIndex = -2;


        public static void createOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            
            string clientLevelString = DrakenhofBankCampaignBehavior.BankDrakenhof.GetCustomerLevelString();//TODOW
            int currentSolde = DrakenhofBankCampaignBehavior.BankDrakenhof.Solde;
            float interestRatePercentage = DrakenhofBankCampaignBehavior.BankDrakenhof.CalculateInterestRate()*100f;
            int darkEnergySolde = DrakenhofBankCampaignBehavior.BankDrakenhof.DarkEnergyAccountSolde;
            int darkEnergyInterests = DrakenhofBankCampaignBehavior.BankDrakenhof.CalculateDarkEnergyInterests();
            // Bank Menu
            campaignGameStarter.AddGameMenu("drakenhof_bank_menu",
                String.Format("Bienvenue à la banque de Drakenhof.\nNiveau du client : {0}\nSolde : {1}\nTaux d'intérêts : {2}%/jour", clientLevelString, currentSolde,interestRatePercentage.ToString("G3")),
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //AccountMenu
            campaignGameStarter.AddGameMenu("drakenhof_account", String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //Deposit Menu
            campaignGameStarter.AddGameMenu("drakenhof_bank_deposit", String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //Withdraw Menu
            campaignGameStarter.AddGameMenu("drakenhof_bank_withdraw", String.Format("Solde : {0}\nTaux d'intérêts : {1}%/jour", currentSolde, interestRatePercentage.ToString("G3")), null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters); //TODOW

            //Prestigious Account Menu
            campaignGameStarter.AddGameMenu("drakenhof_bank_prestigious_account", 
                String.Format("Fortune investie : {0}\nÂmes attirées : {1}", darkEnergySolde, darkEnergyInterests), 
                null, 
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public static void RegisterBankMenu(CampaignGameStarter campaignGameStarter)
        {
            createOrUpdateGameMenuDesc(campaignGameStarter);
            //Town -> BankMenu
            campaignGameStarter.AddGameMenuOption("town",
                                                  "altdorf_bank_menu",
                                                  "Banque d'Altdorf",
                                                  OnConditionDelegateAltdorfBank,
                                                  _ => GameMenu.SwitchToMenu("altdorf_bank_menu"),
                                                  isLeave: false,
                                                  _optionBankIndex);



            // Bank Menu -> Compte
            campaignGameStarter.AddGameMenuOption("altdorf_bank_menu", "altdorf_account", "Accéder au compte",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_account"),
                isLeave: false);
            RegisterAccountMenuOptions(campaignGameStarter);

            // Bank Menu -> Prestigious Account
            campaignGameStarter.AddGameMenuOption("altdorf_bank_menu", "altdorf_bank_prestigious_account", "Soutenir la bureaucratie",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash;
                    a.Tooltip = AltdorfBankCampaignBehavior.BankAltdorf.GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = AltdorfBankCampaignBehavior.BankAltdorf.GetCustomerLevel() > 1;
                    return true; },
                _ => GameMenu.SwitchToMenu("altdorf_bank_prestigious_account"),
                isLeave: false);
            RegisterPrestigiousAccountMenuOptions(campaignGameStarter);
            

            //Quitter la banque
            campaignGameStarter.AddGameMenuOption("altdorf_bank_menu", "altdorf_bank_menu_back", "Quitter la banque",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true);
        }



        private static void RegisterAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            // Compte -> Dépot
            campaignGameStarter.AddGameMenuOption("altdorf_account", "altdorf_bank_deposit", "Déposer de l'argent",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Ransom; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_bank_deposit"),
                isLeave: false);
            registerDepositMenuOptions(campaignGameStarter);
            // Compte -> Retrait
            campaignGameStarter.AddGameMenuOption("altdorf_account", "altdorf_bank_withdraw", "Retirer de l'argent",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Trade; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_bank_withdraw"),
                isLeave: false);
            registerWithdrawMenuOptions(campaignGameStarter);
            // Compte -> BankMenu
            campaignGameStarter.AddGameMenuOption("altdorf_account", "altdorf_account_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_bank_menu"),
                isLeave: true);

        }

        

        #region Deposit
        private static void registerDepositMenuOptions(CampaignGameStarter campaignGameStarter)
        {

            
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties) {
                campaignGameStarter.AddGameMenuOption("altdorf_bank_deposit", String.Format("altdorf_bank_deposit_{0}", qty), String.Format("Déposer {0} pièces", qty),
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape;
                        a.IsEnabled = IsAbleToDeposit(qty); 
                        a.Tooltip = IsAbleToDeposit(qty)?null:new TextObject("Pas assez d'argent", null) ; 
                        return true; },
                    _ => DepositGold(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;
                

            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption("altdorf_bank_deposit", "altdorf_bank_deposit_all", "Tout déposer",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.BribeAndEscape; 
                        a.IsEnabled = Hero.MainHero.Gold>0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null); 
                        return true; },
                    _ => DepositGold(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption("altdorf_bank_deposit", "altdorf_bank_deposit_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_account"),
                isLeave: false, 6, isRepeatable: false);
        }


        private static bool IsAbleToDeposit(int amount)
        {
            return Hero.MainHero.Gold >= amount;
        }

        private static void DepositGold(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            AltdorfBankCampaignBehavior.BankAltdorf.Solde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Dépot de {0} validé.\nNouvelle solde de compte : {1}", amount, AltdorfBankCampaignBehavior.BankAltdorf.Solde), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu("altdorf_bank_deposit");
            createOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion

        #region Withdraw
        private static void registerWithdrawMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption("altdorf_bank_withdraw", String.Format("altdorf_bank_withdraw_{0}", qty), String.Format("Retirer {0} pièces", qty),
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Trade; 
                        a.IsEnabled = IsAbleToWithdraw(qty); 
                        a.Tooltip = IsAbleToWithdraw(qty)?null:new TextObject("Pas assez de solde", null); 
                        return true; },
                    _ => WithdrawGold(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Withdraw All
            campaignGameStarter.AddGameMenuOption("altdorf_bank_withdraw", "altdorf_bank_withdraw_all", "Tout retirer",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Trade;
                        a.IsEnabled = AltdorfBankCampaignBehavior.BankAltdorf.Solde > 0;
                        a.Tooltip = AltdorfBankCampaignBehavior.BankAltdorf.Solde > 0?null:new TextObject("Pas assez de solde", null);
                        return true;
                    },
                    _ => WithdrawGold(AltdorfBankCampaignBehavior.BankAltdorf.Solde, campaignGameStarter),
                    isLeave: false,
                    i, true);
            //Leave
            campaignGameStarter.AddGameMenuOption("altdorf_bank_withdraw", "altdorf_bank_withdraw_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_account"),
                isLeave: true, 6);
        }

        private static bool IsAbleToWithdraw(int amount)
        {
            return AltdorfBankCampaignBehavior.BankAltdorf.Solde >= amount;
        }

        private static void WithdrawGold(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (AltdorfBankCampaignBehavior.BankAltdorf.Solde < amount) { return; }

            AltdorfBankCampaignBehavior.BankAltdorf.Solde -= amount;
            Hero.MainHero.ChangeHeroGold(amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Retrait de {0} validé.\nNouvelle solde de compte : {1}", amount, AltdorfBankCampaignBehavior.BankAltdorf.Solde), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu("altdorf_bank_withdraw");
            createOrUpdateGameMenuDesc(campaignGameStarter);
            //Campaign.Current.CurrentMenuContext.Refresh();//Don't work to refresh
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_positive"));
        }

        #endregion


        #region Prestigious Account
        private static void RegisterPrestigiousAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption("altdorf_bank_prestigious_account", String.Format("altdorf_bank_prestigious_account_{0}", qty), String.Format("Offrir {0} pièces", qty),
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositPrestigiousAccount(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption("altdorf_bank_prestigious_account", "altdorf_bank_prestigious_account_all", "Tout offrir",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = Hero.MainHero.Gold > 0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositPrestigiousAccount(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption("altdorf_bank_prestigious_account", "altdorf_bank_prestigious_account_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("altdorf_bank_menu"),
                isLeave: true, 6, isRepeatable: false);
        }

        private static void DepositPrestigiousAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            AltdorfBankCampaignBehavior.BankAltdorf.prestigiousAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Cadeau de {0} accepté.\nNombre de corrompus : {1}", amount, AltdorfBankCampaignBehavior.BankAltdorf.CalculatePrestigiousInterests()), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu("altdorf_bank_prestigious_account");
            createOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion




        private static bool OnConditionDelegateAltdorfBank(MenuCallbackArgs args)
        {
            args.optionLeaveType = GameMenuOption.LeaveType.OpenStash;
            InformationManager.DisplayMessage(new InformationMessage(String.Format("TownID : {0}", Settlement.CurrentSettlement.Town.StringId)));
            InformationManager.DisplayMessage(new InformationMessage(String.Format("TownID Prosperity : {0}", Settlement.CurrentSettlement.Town.Prosperity)));
            if (Settlement.CurrentSettlement.Town.StringId == AltdorfBankCampaignBehavior._altdorfTownID) return true;
            
            return false;
        }


    }
}
