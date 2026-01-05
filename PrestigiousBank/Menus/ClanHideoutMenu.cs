using Helpers;
using Messages.FromClient.ToLobbyServer;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using TaleWorlds.ActivitySystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Order;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.CampaignMechanics.Menagery;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

namespace PrestigiousBank
{
    public class ClanHideoutMenu
    {
        public string _cityName;
        public string _cityID;
        public ClanHideout _clanHideout;

        public virtual void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {

            int hideoutLevel = _clanHideout.LevelHideout;

            // Clan Hideout Menu
            campaignGameStarter.AddGameMenu("clanHideoutMenu",
                "Planque du clan\nNiveau de la planque :" + hideoutLevel + "\nForce du gang : " + _clanHideout.BanditsGangStrenght,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            //Racketeering Menu
            campaignGameStarter.AddGameMenu("clanHideout_racketeering_menu",
                "Extorsion des villageois\n" +
                "Lorsque les villageois entrent dans la ville, les voleurs leur extorqueront leurs biens à vendre\n\n" +
                "Niveau du Racketage\n" + _clanHideout.Racketeering_Level +
                "\nForce du gang : " + _clanHideout.BanditsGangStrenght +
                "Chance de se faire attraper par la garde : 2%/" + ClanHideout.Racketeering_MinimumValue * _clanHideout.Racketeering_Level + "{GOLD_ICON} volés\n" +
                "Force impliqué par tentative : " + ClanHideout.Racketeering_BanditStrenghtNeededPerLevel * _clanHideout.Racketeering_Level,
                null, TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);
        }

        public virtual void RegisterFactoryMenu(CampaignGameStarter campaignGameStarter, ClanHideout clanHideout)
        {
            _clanHideout = clanHideout;
            _cityID = _clanHideout.Town.StringId;
            _cityName = _clanHideout.Town.Name.Value;

            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");




            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            RegisterHideoutMenuOption(campaignGameStarter);
            //Town -> Buy the hideout
            campaignGameStarter.AddGameMenuOption("town",
                                                  "clanHideoutMenu_buy",
                                                  "[" + ClanHideout.HideoutLevelPrice + "{GOLD_ICON}] Acheter une planque à " + _cityName,
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      args.IsEnabled = Hero.MainHero.Gold >= ClanHideout.HideoutLevelPrice;
                                                      args.Tooltip = Hero.MainHero.Gold >= ClanHideout.HideoutLevelPrice ? null : new TextObject("Pas assez d'or");
                                                      if (Settlement.CurrentSettlement.Town.StringId != _cityID) return false;
                                                      else if (clanHideout.LevelHideout > 0) return false;
                                                      else return true;
                                                  },
                                                  _ =>
                                                  {
                                                      _clanHideout.LevelHideout += 1;
                                                      Hero.MainHero.ChangeHeroGold(-ClanHideout.HideoutLevelPrice);
                                                      GameMenu.SwitchToMenu("town");
                                                      CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                                      //GameTexts.SetVariable("PRICE_LEVEL_UP", _nulnFactory.CalculatePriceToLevelUpFactory());
                                                  },
                                                  isLeave: false,
                                                  -1);

            //Town -> ClanHideout
            campaignGameStarter.AddGameMenuOption("town",
                                                  "clanHideoutMenu",
                                                  "Planque du clan",
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      if (Settlement.CurrentSettlement.Town.StringId != _cityID) return false;
                                                      else if (_clanHideout.LevelHideout == 0) return false;
                                                      else return true;
                                                  },
                                                  _ =>
                                                  {
                                                      CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                                      GameMenu.SwitchToMenu("clanHideoutMenu");
                                                  },
                                                  isLeave: false,
                                                  -1);


            //Engage outlaws
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideoutMenu_donateOutlaws",
                "Recruter des gangsters",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.DonatePrisoners; return true; },
                _ =>
                {
                    //MobileParty leftParty = new MobileParty();
                    //leftParty.SetCustomName(new TextObject("Gangsters"));
                    TroopRoster leftRoster = new TroopRoster(null);

                    //PartyScreenManager.OpenScreenForManagingAlley  //TODO
                    PartyScreenManager.OpenScreenForManagingAlley(leftRoster,
                        isTroopTransferable: delegate (CharacterObject character, PartyScreenLogic.TroopType type, PartyScreenLogic.PartyRosterSide side, PartyBase LeftOwnerParty)
                        {
                            return !character.IsTemplate && (
                            character.Culture.StringId == TORConstants.Cultures.HERRIMAULT |
                            character.Culture.StringId == "mountain_bandits" |
                            character.Culture.StringId == TORConstants.Cultures.DRUCHII |
                            character.Culture.StringId == TORConstants.Cultures.CHAOS |
                            character.IsBeastman() | character.IsCultist());
                        },
                        doneButtonCondition: delegate (TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, int leftLimitNum, int rightLimitNum)
                        {
                            TextObject tooltip = new TextObject("");
                            if (leftMemberRoster.Count == 0)
                            {
                                tooltip = new TextObject("Aucune troupe donnée");
                            }
                            return new Tuple<bool, TextObject>(leftMemberRoster.Count != 0, tooltip);
                        },
                        onDoneClicked: delegate (TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, FlattenedTroopRoster takenPrisonerRoster, FlattenedTroopRoster releasedPrisonerRoster, bool isForced, PartyBase leftParty, PartyBase rightParty)
                        {
                            if (leftPrisonRoster.Count != 0)
                            {
                                foreach (TroopRosterElement item in leftPrisonRoster.GetTroopRoster())
                                {
                                    _clanHideout.BanditsGangStrenght += item.Character.Tier * item.Number;
                                }
                            }
                            if (leftMemberRoster.Count != 0)
                            {
                                foreach (TroopRosterElement item in leftMemberRoster.GetTroopRoster())
                                {
                                    _clanHideout.BanditsGangStrenght += item.Character.Tier * item.Number;
                                }
                            }

                            return true;
                        },
                        leftPartyName: new TextObject("Recrues du gang"),
                        onCancelButtonClicked: null
                        );
                    /*        PartyScreenManager.OpenScreenAsManageTroopsAndPrisoners(
                                leftParty,
                                onPartyScreenClosed: delegate (PartyBase leftOwnerParty, TroopRoster leftMemberRoster, TroopRoster leftPrisonRoster, PartyBase rightOwnerParty, TroopRoster rightMemberRoster, TroopRoster rightPrisonRoster, bool fromCancel)
                                {
                                    if (!fromCancel)
                                    {
                                        if (leftPrisonRoster.Count != 0)
                                        {
                                            foreach (TroopRosterElement item in leftPrisonRoster.GetTroopRoster())
                                            {
                                                _clanHideout.BanditsGangStrenght += item.Character.Tier * item.Number;
                                            }
                                        }
                                        if (leftMemberRoster.Count != 0)
                                        {
                                            foreach (TroopRosterElement item in leftMemberRoster.GetTroopRoster())
                                            {
                                                _clanHideout.BanditsGangStrenght += item.Character.Tier * item.Number;
                                            }
                                        }
                                        CreateOrUpdateGameMenuDesc(campaignGameStarter);
                                    }
                                });*/
                },
                isLeave: false);

            //Open Stash
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideoutMenu_openStash", "Ouvrir la cache",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.OpenStash; return true; },
                _ => InventoryManager.OpenScreenAsStash(_clanHideout.GetItemStash()),//Crash. Why ? InventoryManager Null
                isLeave: false, index: 2);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //clanHideoutMenu -> Buy racketeering
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_racketeering_menu_buy", "[" + ClanHideout.Racketeering_LevelPrice + "{GOLD_ICON}] Commencer une activité d'extorsion de villageois",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    a.Tooltip = Hero.MainHero.Gold >= ClanHideout.Racketeering_LevelPrice ? null : new TextObject("Pas assez d'or");
                    a.IsEnabled = Hero.MainHero.Gold >= ClanHideout.Racketeering_LevelPrice;
                    return _clanHideout.Racketeering_Level == 0;
                },
                _ =>
                {
                    _clanHideout.Racketeering_Level += 1;
                    Hero.MainHero.ChangeHeroGold(-ClanHideout.Racketeering_LevelPrice);
                    GameMenu.SwitchToMenu("clanHideoutMenu");
                },
                isLeave: false, index: 2);

            //clanHideoutMenu -> racketteringMenu
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_racketeering_menu", "Activité d'extorsion",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    return _clanHideout.Racketeering_Level >= 1;
                },
                _ => { SetRacketeeringMenuText(); GameMenu.SwitchToMenu("clanHideout_racketeering_menu"); },
                isLeave: false, index: 2);
            RegisterRacketeeringMenuOptions(campaignGameStarter);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //Upgrade Hideout
            //TODO
            //Porte dérobée
            //TODO

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);

            //Quitter la planque
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideoutMenu_back", "Quitter la planque",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: -1);
        }

        public void RegisterHideoutMenuOption(CampaignGameStarter campaignGameStarter)
        {
            //hideout -> Recruit Outlaws
            //TODO
            //campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_racketeering_menu_buy", "[" + ClanHideout.Racketeering_LevelPrice + "{GOLD_ICON}] Commencer une activité d'extorsion de villageois",
            //    a =>
            //    {
            //        a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
            //        a.Tooltip = Hero.MainHero.Gold >= ClanHideout.Racketeering_LevelPrice ? null : new TextObject("Pas assez d'or");
            //        a.IsEnabled = Hero.MainHero.Gold >= ClanHideout.Racketeering_LevelPrice;
            //        return _clanHideout.Racketeering_Level == 0;
            //    },
            //    _ =>
            //    {
            //        _clanHideout.Racketeering_Level += 1;
            //        Hero.MainHero.ChangeHeroGold(-ClanHideout.Racketeering_LevelPrice);
            //        GameMenu.SwitchToMenu("clanHideoutMenu");
            //    },
            //    isLeave: false, index: 2);
        }
        public void RegisterRacketeeringMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            TextObject racketeeringText = new TextObject("{NULNWOODPRODUCTIONTEXTUP}");
            //LevelUp Racketeeting
            campaignGameStarter.AddGameMenuOption("clanHideout_racketeering_menu", "clanHideout_racketeering_menu_LevelUp",
                racketeeringText.Value,
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
                    a.IsEnabled = Hero.MainHero.Gold >= _clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice;
                    a.Tooltip = Hero.MainHero.Gold >= _clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice ? null : new TextObject("Pas assez d'or");
                    return _clanHideout.Racketeering_Level < 5;
                },
                _ =>
                {
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _clanHideout.Racketeering_Level += 1;
                    Hero.MainHero.ChangeHeroGold(-_clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice);
                    SetRacketeeringMenuText();
                    GameMenu.SwitchToMenu("clanHideout_racketeering_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration des techniques d'extorsion");
                },
                isLeave: false);


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideout_racketeering_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Random Upgrades ?


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideout_racketeering_menu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Leave
            campaignGameStarter.AddGameMenuOption("clanHideout_racketeering_menu", "clanHideout_racketeering_menu_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("clanHideoutMenu"),
                isLeave: true, index: -1);

        }

        public void SetRacketeeringMenuText()
        {
            TextObject levelUpText = new TextObject("[" + _clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice + "{GOLD_ICON}] Améliorer les techniques d'extorsion");
            GameTexts.SetVariable("NULNWOODPRODUCTIONTEXTUP", levelUpText);
        }
    }
}




/*


    }
}
*/