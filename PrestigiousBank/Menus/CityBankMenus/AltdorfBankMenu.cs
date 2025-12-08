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
    public class AltdorfBankMenu:BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int prestigiousSolde = ((AltdorfBank) _bank).prestigiousAccountSolde;
            int prestigiousInterests = ((AltdorfBank)_bank).CalculatePrestigiousInterests();

            //Prestigious Account Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_prestigious_account",_cityID), 
                String.Format("Fortune investie : {0}\nScribes corrompus : {1}", prestigiousSolde, prestigiousInterests), 
                null, 
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public override void RegisterBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            base.RegisterBankMenu(campaignGameStarter,Bank);


            // Bank Menu -> Prestigious Account
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_prestigious_account", _cityID), "Soutenir la bureaucratie",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash;
                    a.Tooltip = ((AltdorfBank)_bank).GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = ((AltdorfBank)_bank).GetCustomerLevel() > 1;
                    return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_prestigious_account", _cityID)),
                isLeave: false);
            RegisterPrestigiousAccountMenuOptions(campaignGameStarter);
            
        }




        #region Prestigious Account
        private void RegisterPrestigiousAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_{1}",_cityID, qty), String.Format("Offrir {0} pièces", qty),
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
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_all", _cityID), "Tout offrir",
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
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        private void DepositPrestigiousAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            AltdorfBankCampaignBehavior.BankAltdorf.prestigiousAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Cadeau de {0} accepté.\nNombre de corrompus : {1}", amount, AltdorfBankCampaignBehavior.BankAltdorf.CalculatePrestigiousInterests()), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu(String.Format("{0}_bank_prestigious_account", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion



    }
}
