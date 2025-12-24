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
    public class ClanAgencyMenu
    {
        ClanAgencies ClanAgencies { get; set; }

        public virtual void RegisterClanAgencyMenu(CampaignGameStarter campaignGameStarter)
        {
            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");
            MBTextManager.SetTextVariable("PRESTIGE_ICON",CustomResourceManager.GetResourceObject("Prestige").GetCustomResourceIconAsText());
            ClanAgencies = ClanAgenciesBehaviour.ClanAgencies;
            CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Town -> Buy an Agency
            campaignGameStarter.AddGameMenuOption("town",
                    "clanAgency_buy",
                    "["+ClanAgency.AgencyInitialPrice+"{GOLD_ICON}] Installer une agence du Clan",
                    args =>
                    {
                        args.optionLeaveType = GameMenuOption.LeaveType.Craft;//TODO
                        args.IsEnabled = Hero.MainHero.Gold >= ClanAgency.AgencyInitialPrice;
                        if (Hero.MainHero.Gold < ClanAgency.AgencyInitialPrice) { 
                            args.Tooltip = new TextObject("Pas assez d'or");
                            args.IsEnabled = false;
                        }
                        else if (ClanAgencies.GetMaxAgencies() <= ClanAgencies.GetClanAgenciesList().Count)
                        {
                            args.Tooltip = new TextObject("Nombre d'agence max atteint");
                            args.IsEnabled = false;
                        }
                        if (!ClanAgencies.DoDisplayOptionToBuyAgency()) return false;
                        if (ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId) != null) return false;
                        else return true;
                    },
                    _ => {
                        ClanAgencies.CreateAgencyFromTownID(Settlement.CurrentSettlement.Town.StringId);
                        Hero.MainHero.ChangeHeroGold(-ClanAgency.AgencyInitialPrice);
                        CreateOrUpdateGameMenuDesc(campaignGameStarter);
                        GameMenu.SwitchToMenu("town");
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
                                                    _ => { GameMenu.SwitchToMenu("clanAgency"); CreateOrUpdateGameMenuDesc(campaignGameStarter); },
                                                    isLeave: false,
                                                    index: 1);

            //Nuln Factory
            campaignGameStarter.AddGameMenuOption("clanAgency",
                                                    "clanAgency_nulnFactory",
                                                    "Gérer l'usine de Nuln",
                                                    args =>
                                                    {
                                                        ClanAgency currentAgency = ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
                                                        args.optionLeaveType = GameMenuOption.LeaveType.Craft;//TODO
                                                        if (currentAgency == null || currentAgency.LevelAgency == 0) return false;
                                                        else return true;
                                                    },
                                                    _ => { GameMenu.SwitchToMenu("nulnFactory_menu"); },
                                                    isLeave: false,
                                                    index: 1);


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanAgency", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("clanAgency", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            RegisterLevelSelectionMenuOptions(campaignGameStarter);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanAgency", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //Upgrade Max Agencies
            campaignGameStarter.AddGameMenuOption("clanAgency",
                                        "clanAgency_upgradeMaxAgency",
                                        "["+ClanAgencies.UpgradeMaxLimitAgencyCost+"{GOLD_ICON}]Augmenter le nombre maximum d'agences",
                                        args =>
                                        //Conditions : Clan Level 5 && Agence Level 5
                                        {
                                            ClanAgency currentAgency = ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
                                            args.optionLeaveType = GameMenuOption.LeaveType.Craft;//TODO
                                            args.Tooltip = Clan.PlayerClan.Tier >= 5 ? null : new TextObject("Clan Tiers 5 nécessaire");
                                            if (currentAgency == null || currentAgency.LevelAgency != 5) return false;
                                            else return true;
                                        },
                                        _ => {
                                            ClanAgencies.MaxLimitAgencyUpgradeBought += 1;
                                            GameMenu.SwitchToMenu("clanAgency"); },
                                        isLeave: false,
                                        index: 1);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanAgency", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);




            //Quitter l'agence
            campaignGameStarter.AddGameMenuOption("clanAgency", "{0}_clanAgency_menu_back", "Quitter l'agence",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: -1);
        }

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsTown)
            {
                SetClanAgencyLevelProductionText();
                ClanAgency currentAgency = ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
                // int factoryLevel = _nulnFactory.FactoryLevel;
                // NulnFactory.PossibleProduction chosenProduction = _nulnFactory.chosenProduction;
                // string chosenProductionString = chosenProduction.ToString();

                // Agence Menu
                if (currentAgency != null)
                {
                    campaignGameStarter.AddGameMenu("clanAgency",
                        "Agence de " + Settlement.CurrentSettlement.Town.Name + "\n" +
                        "Niveau de l'agence: " + currentAgency.LevelAgency +
                        "\nCoût d'entretien actuel: " + currentAgency.SelectedLevel * ClanAgency.AgencyUpkeepPerLevel + "\n\n" +
                        "Nombre d'agences actuellement :" + ClanAgencies.GetClanAgenciesList().Count + "\n" +
                        "Nombre maximum d'agences : " + ClanAgencies.GetMaxAgencies(),
                        null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);
                }
            }
        }



        public void RegisterLevelSelectionMenuOptions(CampaignGameStarter campaignGameStarter)
        {

            var factoryProduction0 = new TextObject("{CLANAGENCYPRODUCTIONTEXT0}");
            var factoryProduction1 = new TextObject("{CLANAGENCYPRODUCTIONTEXT1}");
            var factoryProduction2 = new TextObject("{CLANAGENCYPRODUCTIONTEXT2}");
            var factoryProduction3 = new TextObject("{CLANAGENCYPRODUCTIONTEXT3}");
            var factoryProduction4 = new TextObject("{CLANAGENCYPRODUCTIONTEXT4}");
            var factoryProduction5 = new TextObject("{CLANAGENCYPRODUCTIONTEXT5}");
            var factoryProductionLevelUp = new TextObject("{CLANAGENCYPRODUCTIONTEXTUP}");
            SetClanAgencyLevelProductionText();
            //Désactivé ==> SelectedLevel = 0
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_0",
                factoryProduction0.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien : 0{GOLD_ICON}");
                    return true;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 0;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Agence fermée");
                },
                isLeave: false);

            //SelectedLevel = 1
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_1",
                factoryProduction1.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel *1 + "{GOLD_ICON}");
                    return true;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 1;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 1");
                    },
                isLeave: false);

            //SelectedLevel = 2
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_2",
                factoryProduction2.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel * 2 + "{GOLD_ICON}");
                    return ClanAgencies.CurrentSettlementAgency != null && ClanAgencies.CurrentSettlementAgency.LevelAgency>=2;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 2;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 2");
                },
                isLeave: false);

            //SelectedLevel = 3
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_3",
                factoryProduction3.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel * 3 + "{GOLD_ICON}");
                    return ClanAgencies.CurrentSettlementAgency != null && ClanAgencies.CurrentSettlementAgency.LevelAgency >= 3;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 3;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 3");
                },
                isLeave: false);

            //SelectedLevel = 4
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_4",
                factoryProduction4.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel * 4 + "{GOLD_ICON}");
                    return ClanAgencies.CurrentSettlementAgency != null && ClanAgencies.CurrentSettlementAgency.LevelAgency >= 4;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 4;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 4");
                },
                isLeave: false);

            //SelectedLevel = 5
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_5",
                factoryProduction5.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel * 5 + "{GOLD_ICON}");
                    return ClanAgencies.CurrentSettlementAgency != null && ClanAgencies.CurrentSettlementAgency.LevelAgency >= 5;
                },
                _ => {
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = 5;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("L'usine tourne au niveau 5");
                },
                isLeave: false);

/*            //Niveau 1 ==> SelectedLevel = 1
            for (int i =1; i< 6;i++)
            {
                campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_"+i,
                    factoryProduction1.Value,
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                        a.Tooltip = new TextObject("Entretien/jour : " + ClanAgency.AgencyUpkeepPerLevel*i + "{GOLD_ICON}");
                        return true;
                    },
                    _ => {
                        ClanAgencies.CurrentSettlementAgency.SelectedLevel = i;
                        SetClanAgencyLevelProductionText();
                        GameMenu.SwitchToMenu("clanAgency");
                        CreateOrUpdateGameMenuDesc(campaignGameStarter);
                        PrestigiousBank.LogMessage("L'usine tourne au niveau "+i);
    },
    isLeave: false);
            }*/



            //Level UP
            campaignGameStarter.AddGameMenuOption("clanAgency", "clanAgencySelectLevel_UP",
                factoryProductionLevelUp.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.HostileAction; //TODO
                    a.IsEnabled = Hero.MainHero.Gold >= ClanAgencies.CurrentSettlementAgency.CalculatePriceToLevelUpAgency();
                    a.Tooltip = Hero.MainHero.Gold >= ClanAgencies.CurrentSettlementAgency.CalculatePriceToLevelUpAgency() ? null : new TextObject("Pas assez d'or");
                    return ClanAgencies.CurrentSettlementAgency.LevelAgency < 5;
                },
                _ => {
                    Hero.MainHero.ChangeHeroGold(-ClanAgencies.CurrentSettlementAgency.CalculatePriceToLevelUpAgency());
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    ClanAgencies.CurrentSettlementAgency.LevelAgency += 1;
                    ClanAgencies.CurrentSettlementAgency.SelectedLevel = ClanAgencies.CurrentSettlementAgency.LevelAgency;
                    SetClanAgencyLevelProductionText();
                    GameMenu.SwitchToMenu("clanAgency");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de l'agence");
                },
                isLeave: false);

        }

        public void SetClanAgencyLevelProductionText()
        {
            if (ClanAgencies.CurrentSettlementAgency != null) {
                TextObject textObject = new TextObject("");
                for (var i = 0; i < 6; i++)
                {
                    if (i == 0) textObject = new TextObject("Désactivé");
                    else textObject = new TextObject("Niveau " + i);

                    if (ClanAgencies.CurrentSettlementAgency.SelectedLevel == i)
                    {
                        textObject = new TextObject($"[{textObject.Value}]");
                    }
                    GameTexts.SetVariable("CLANAGENCYPRODUCTIONTEXT" + i, textObject);
                }
                TextObject levelUpText = new TextObject("[" + ClanAgencies.CurrentSettlementAgency.CalculatePriceToLevelUpAgency() + "{GOLD_ICON}] Améliorer l'agence");
                GameTexts.SetVariable("CLANAGENCYPRODUCTIONTEXTUP", levelUpText);
            }
        }


        #region Agency Upgrade


        #endregion



    }
}
