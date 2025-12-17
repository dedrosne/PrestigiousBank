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
        public virtual void RegisterClanAgencyMenu(CampaignGameStarter campaignGameStarter)
        {
            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");
            MBTextManager.SetTextVariable("PRESTIGE_ICON",CustomResourceManager.GetResourceObject("Prestige").GetCustomResourceIconAsText());

            CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Town -> Buy an Agency
            campaignGameStarter.AddGameMenuOption("town",
                    "clanAgency_buy",
                    "["+ClanAgency.AgencyInitialPrice+"{GOLD_ICON}] Installer une agence du Clan",
                    args =>
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Craft;//TODO
                        args.IsEnabled = Hero.MainHero.Gold >= ClanAgency.AgencyInitialPrice;
                        args.Tooltip = Hero.MainHero.Gold >= ClanAgency.AgencyInitialPrice ? null : new TextObject("Pas assez d'or");
                        if (ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId) != null) return false;
                        else return true;
                    },
                    _ => {
                        ClanAgencies.CreateAgencyFromTownID(Settlement.CurrentSettlement.Town.StringId);
                        Hero.MainHero.ChangeHeroGold(-ClanAgency.AgencyInitialPrice);
                        GameMenu.SwitchToMenu("town");
                        CreateOrUpdateGameMenuDesc(campaignGameStarter);
                        //GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUpFactory());
                    },
                    isLeave: false,
                    index: 1);

            //Town -> Agency
            campaignGameStarter.AddGameMenuOption("town",
                                                    "clanAgency",
                                                    "Agence du clan",
                                                    args =>
                                                    {
                                                        ClanAgency currentAgency = ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
                                                        args.optionLeaveType = GameMenuOption.LeaveType.Craft;//TODO
                                                        if (currentAgency == null || currentAgency.LevelAgency == 0) return false;
                                                        else return true;
                                                    },
                                                    _ => GameMenu.SwitchToMenu("clanAgency"),
                                                    isLeave: false,
                                                    index: 1);


            //AgencyMenu -> Upgrade the Agency Menu//TODO
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




            //Quitter l'agence
            campaignGameStarter.AddGameMenuOption("clanAgency", String.Format("{0}_clanAgency_menu_back", _cityID), "Quitter l'agence",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: -1);
        }

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            ClanAgency currentAgency = ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
            // int factoryLevel = _nulnFactory.FactoryLevel;
            // NulnFactory.PossibleProduction chosenProduction = _nulnFactory.chosenProduction;
            // string chosenProductionString = chosenProduction.ToString();

            // Agence Menu
            campaignGameStarter.AddGameMenu("clanAgency",
                String.Format("Agence de {0}\nNiveau de l'agence: {1}\nCoût d'entretien actuel: {1}",Settlement.CurrentSettlement.Town.Name , currentAgency.LevelAgency, currentAgency.SelectedLevel*ClanAgency.AgencyUpkeepPerLevel),
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);
        }

        #region Agency Upgrade


        #endregion



    }
}
