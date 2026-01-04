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
    public class DrakenhofBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int darkEnergySolde = ((DrakenhofBank)_bank).DarkEnergyAccountSolde;
            int darkEnergyInterests = ((DrakenhofBank)_bank).CalculateDarkEnergyInterests();

            //Dark Ritual Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_dark_ritual_enlightment",_cityID),
                "Renforcez votre âme grâce aux rituels noirs",
                null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            // Bank Menu -> Dark Ritual Menu
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu",_cityID), 
                String.Format("{0}_dark_ritual_enlightment", _cityID), "Accéder à la salle rituelle",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.Tooltip = ((DrakenhofBank)_bank).GetCustomerLevel() > 3 ? null : new TextObject("Niveau de client Platine requis", null);
                    a.IsEnabled = ((DrakenhofBank)_bank).GetCustomerLevel() > 3;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID)),
                isLeave: false, index: 2);
            RegisterDarkRitualMenuOptions(campaignGameStarter);

        }


/*
/**/
        #region Dark Ritual
        private void RegisterDarkRitualMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), 
                "Drakenhof_bank_prestigious_account_focus", 
                "[+1 Point de Focus] Rituel de renforcement de l'âme\n[50 000 {GOLD_ICON}]",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.IsEnabled = IsAbleToDeposit(50_000);
                    a.Tooltip = IsAbleToDeposit(50_000) ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => PerformeDarkRitualFocus(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID),
                    "Drakenhof_bank_prestigious_account_attribute",
                    "[+1 Point d'Attribut] Rituel de renforcement du corps\n[100 000 {GOLD_ICON}]",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                        a.IsEnabled = IsAbleToDeposit(100_000);
                        a.Tooltip = IsAbleToDeposit(100_000) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => PerformeDarkRitualAttribute(campaignGameStarter),
                    isLeave: false,
                    index: 1, isRepeatable: true);

            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), String.Format("{0}_dark_ritual_enlightment_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }


        private void PerformeDarkRitualFocus(CampaignGameStarter campaignGameStarter)
        {
            Hero.MainHero.HeroDeveloper.UnspentFocusPoints += 1;
            Hero.MainHero.ChangeHeroGold(-50_000);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/levelup"));
            GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID));
        }
        private void PerformeDarkRitualAttribute(CampaignGameStarter campaignGameStarter)
        {
            Hero.MainHero.HeroDeveloper.UnspentAttributePoints += 1;
            Hero.MainHero.ChangeHeroGold(-100_000);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/levelup"));
            GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID));
        }

        #endregion

    }
}
