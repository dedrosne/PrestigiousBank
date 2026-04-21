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
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class ParravonBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Pegase Service
            int PegaseNumber = ((ParravonBank)_bank).PegaseBought;
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_pegase", _cityID),
                "Pégases achetés : " + PegaseNumber,
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);


        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            //Bank Menu -> Pegase
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_pegase", _cityID),
                "Ecuries des pégases",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Leaderboard;
                    a.Tooltip = ((ParravonBank)_bank).GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = ((ParravonBank)_bank).GetCustomerLevel() > 1;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_pegase", _cityID)),
                isLeave: false, index: 3);
            RegisterPegaseMenuOptions(campaignGameStarter);

        }


        #region Pegase

        private void RegisterPegaseMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            GameTexts.SetVariable("PEGASE_PRICE", ((ParravonBank)_bank).CalculatePriceNewPegase());
            //Buy Pegase
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_pegase", _cityID), String.Format("{0}_bank_pegase_buy", _cityID),
                "[{PEGASE_PRICE}{GOLD_ICON}] Acheter un pégase :\n+0.01 Party Speed" ,
                a => {
                    int priceNewPegase = ((ParravonBank)_bank).CalculatePriceNewPegase();
                    a.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    a.IsEnabled = Hero.MainHero.Gold >= ((ParravonBank)_bank).CalculatePriceNewPegase();
                    a.Tooltip = Hero.MainHero.Gold >= ((ParravonBank)_bank).CalculatePriceNewPegase() ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ =>
                {
                    Hero.MainHero.ChangeHeroGold(-((ParravonBank)_bank).CalculatePriceNewPegase());
                    ((ParravonBank)_bank).PegaseBought += 1;
                    GameTexts.SetVariable("PEGASE_PRICE", ((ParravonBank)_bank).CalculatePriceNewPegase());
                    PrestigiousBank.LogMessage("Pégase acheté\nNombre de pégases :"+ ((ParravonBank)_bank).PegaseBought);
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_bank_pegase", _cityID));
                },
                isLeave: false,
                index: 1, isRepeatable: true);



            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_pegase", _cityID), String.Format("{0}_bank_pegase_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 3, isRepeatable: false);
        }



        #endregion



    }
}
