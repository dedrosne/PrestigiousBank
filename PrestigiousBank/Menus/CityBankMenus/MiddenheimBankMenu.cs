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
    public class MiddenheimBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Party Helper Service
            int partyHelperCount = ((MiddenheimBank)_bank).PartyHelperCount;
            int partyHelperCostPerDay = ((MiddenheimBank)_bank).CalculatePartyHelperUpkeep();
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_party_helper", _cityID),
                "Aides de camp embauchés : " + partyHelperCount +
                "\nCoût par jour : " + partyHelperCostPerDay+"{GOLD_ICON}",
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);


        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            //Bank Menu -> Party Helper
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_party_helper", _cityID),
                "Services des aides de camp",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
                    a.Tooltip = ((MiddenheimBank)_bank).GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = ((MiddenheimBank)_bank).GetCustomerLevel() > 1;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_party_helper", _cityID)),
                isLeave: false, index: 3);
            RegisterPartyHelperMenuOptions(campaignGameStarter);

        }


        #region Party Helper

        private void RegisterPartyHelperMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int pricePerHelper = MiddenheimBank.PriceHirePartyHelper;
            int initialUpkeepHelper = MiddenheimBank.InitialPartyHelperUpkeep;
            int additionalUpkeep = initialUpkeepHelper + ((MiddenheimBank)_bank).PartyHelperCount;

            //Hire Party Helper
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_party_helper", _cityID), String.Format("{0}_bank_party_helper_hire", _cityID),
                "[" + pricePerHelper + " {GOLD_ICON}] Embaucher un aide de camp :\n+1 Taille d'armée" +
                "\nEntretien supplémentaire : " + additionalUpkeep + " {GOLD_ICON}/jour",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Recruit;
                    a.IsEnabled = Hero.MainHero.Gold >= pricePerHelper;
                    a.Tooltip = Hero.MainHero.Gold >= pricePerHelper ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => HirePartyHelper(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);

            //Fire Party Helper
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_party_helper", _cityID), String.Format("{0}_bank_party_helper_fire", _cityID),
                "Virer un aide de camp",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.LeaveTroopsAndFlee;
                    a.IsEnabled = ((MiddenheimBank)_bank).PartyHelperCount > 0;
                    a.Tooltip = ((MiddenheimBank)_bank).PartyHelperCount > 0 ? null : new TextObject("Personne à virer", null);
                    return true;
                },
                _ => FirePartyHelper(campaignGameStarter),
                isLeave: false,
                index: 2, isRepeatable: true);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_party_helper", _cityID), String.Format("{0}_bank_party_helper_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 3, isRepeatable: false);
        }

        private void HirePartyHelper(CampaignGameStarter CampaignGameStarter)
        {
            ((MiddenheimBank)_bank).PartyHelperCount += 1;
            Hero.MainHero.ChangeHeroGold(-MiddenheimBank.PriceHirePartyHelper);
            PrestigiousBank.LogMessage("Aide de camp embauché.\nEntretien total par jour : " + ((MiddenheimBank)_bank).CalculatePartyHelperUpkeep());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_party_helper", _cityID));
        }

        private void FirePartyHelper(CampaignGameStarter CampaignGameStarter)
        {
            ((MiddenheimBank)_bank).PartyHelperCount -= 1;
            PrestigiousBank.LogMessage("Aide de camp viré.\nEntretien total par jour : " + ((MiddenheimBank)_bank).CalculatePartyHelperUpkeep());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_party_helper", _cityID));
        }


        #endregion



    }
}
