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
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class CouronneBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int chivalryAccountSolde = ((CouronneBank)_bank).ChivalryAccountSolde;
            int chivalryInterests = ((CouronneBank)_bank).CalculateChivalryInterests();

            //NGO  Charity
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_chivalry_account", _cityID),
                String.Format("Somme offerte à l'orphelinat : {0}\nOrphelins recueillis : {1}", chivalryAccountSolde, chivalryInterests),
                null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public override void RegisterBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            base.RegisterBankMenu(campaignGameStarter, Bank);


            // Bank Menu -> Chivalry Account
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_chivalry_account", _cityID),
                "Financer des ONG de charité",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Escape;
                    if (Hero.MainHero.GetCultureSpecificCustomResource().Name != "Chivalry")
                    {
                        a.Tooltip = new TextObject("Culture Bretonienne requise", null);
                        a.IsEnabled = false;
                    }
                    else if (((CouronneBank)_bank).GetCustomerLevel() <= 1)
                    {
                        a.Tooltip = new TextObject("Niveau de client Argent requis", null);
                        a.IsEnabled = false;
                    }
                    else
                    {
                        a.Tooltip = null;
                        a.IsEnabled = true;
                    }
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_chivalry_account", _cityID)),
                isLeave: false);
            RegisterChivalryAccountMenuOptions(campaignGameStarter);

        }



        #region Chivalry

        private void RegisterChivalryAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_chivalry_account", _cityID), String.Format("{0}_bank_chivalry_account_{1}", _cityID, qty),
                    "Offrir " + qty + " {GOLD_ICON}",
                    a =>
                    {
                        a.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositChivalryAccount(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }


        //Deposit All
        campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_chivalry_account", _cityID), String.Format("{0}_bank_chivalry_account_all", _cityID), "Tout offrir",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Ransom;
                        a.IsEnabled = Hero.MainHero.Gold > 0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositChivalryAccount(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_chivalry_account", _cityID), String.Format("{0}_bank_chivalry_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        private void DepositChivalryAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            CouronneBankCampaignBehavior.BankCouronne.ChivalryAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            int chivalryInterests = CouronneBankCampaignBehavior.BankCouronne.CalculateChivalryInterests();
            PrestigiousBank.LogMessage("Oeuvre de charité de "+amount+"{GOLD_ICON} .\nQuantité totale offerte : "+ chivalryInterests, 0xFFBBAA00);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_chivalry_account", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }

        #endregion



        

    }
}
