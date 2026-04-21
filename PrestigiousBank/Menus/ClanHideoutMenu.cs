using Helpers;
using Messages.FromClient.ToLobbyServer;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using TaleWorlds.ActivitySystem;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.CampaignSystem.Inventory;
using TaleWorlds.CampaignSystem.MapEvents;
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
            RefreshClanHideoutLevelUpText();
            // Clan Hideout Menu
            campaignGameStarter.AddGameMenu("clanHideoutMenu",
                "Planque du clan\nNiveau de la planque :" + hideoutLevel + "\nForce du gang : " + _clanHideout.BanditsGangStrenght,
                null, GameMenu.MenuOverlayType.SettlementWithCharacters);

            //Racketeering Menu
            campaignGameStarter.AddGameMenu("clanHideout_racketeering_menu",
                "Extorsion des villageois\n" +
                "Lorsque les villageois entrent dans la ville, les voleurs leur extorqueront leurs biens à vendre.\n\n" +
                "Niveau du Racketage : " + _clanHideout.Racketeering_Level +
                "\nForce du gang : " + _clanHideout.BanditsGangStrenght +
                "\nChance de se faire attraper par la garde :\n2%/" + ClanHideout.Racketeering_MinimumValue * _clanHideout.Racketeering_Level + "{GOLD_ICON} volés\n" +
                "Force impliqué par tentative : " + ClanHideout.Racketeering_BanditStrenghtNeededPerLevel * _clanHideout.Racketeering_Level,
                null, GameMenu.MenuOverlayType.SettlementWithCharacters);

            //Casino
            campaignGameStarter.AddGameMenu("clanHideout_casino_menu",
                "Casino\n" +
                "Niveau du Casino : " + _clanHideout.Casino_Level +
                "\nRevenus entre "+_clanHideout.Casino_CalculateMinMaxValue().Item1+" et "+_clanHideout.Casino_CalculateMinMaxValue().Item2,
                null, GameMenu.MenuOverlayType.SettlementWithCharacters);

        }

        public virtual void RegisterFactoryMenu(CampaignGameStarter campaignGameStarter, ClanHideout clanHideout)
        {
            _clanHideout = clanHideout;
            _cityID = _clanHideout.Town.StringId;
            _cityName = _clanHideout.Town.Name.Value;

            MBTextManager.SetTextVariable("GOLD_ICON", "{=!}<img src=\"General\\Icons\\Coin@2x\" extend=\"7\">");




            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            RegisterRecruitHideoutMenuOption(campaignGameStarter);
            RegisterSecretEntranceMenuOption(campaignGameStarter);
            //Town -> Buy the hideout
            campaignGameStarter.AddGameMenuOption("town",
                                                  "clanHideoutMenu_buy",
                                                  "[" + ClanHideout.HideoutLevelPrice + "{GOLD_ICON}] Acheter une planque à " + _cityName,
                                                  args =>
                                                  {
                                                      args.optionLeaveType = GameMenuOption.LeaveType.Craft;
                                                      args.IsEnabled = true;
                                                      if (Clan.PlayerClan.Tier == 0) args.IsEnabled = false;
                                                      else if (Hero.MainHero.Gold < ClanHideout.HideoutLevelPrice) args.IsEnabled = false;
                                                      if (Clan.PlayerClan.Tier==0) args.Tooltip = new TextObject("Clan Tiers 1 requis");
                                                      else if (Hero.MainHero.Gold >= ClanHideout.HideoutLevelPrice) args.Tooltip = new TextObject("Pas assez d'or");
                                                      
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
                                                      RefreshClanHideoutLevelUpText();
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


                    PartyScreenHelper.OpenScreenForManagingAlley(false, leftRoster,
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
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("clanHideoutMenu");
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
                _ => InventoryScreenHelper.OpenScreenAsStash(_clanHideout.GetItemStash()),//Crash. Why ? InventoryManager Null
                isLeave: false, index: 2);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 3);
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 4);

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
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("clanHideoutMenu");
                },
                isLeave: false, index: 5);

            //clanHideoutMenu -> racketteringMenu
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_racketeering_menu", "Activité d'extorsion",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    return _clanHideout.Racketeering_Level >= 1;
                },
                _ => { RefreshRacketeeringMenuText(); GameMenu.SwitchToMenu("clanHideout_racketeering_menu"); },
                isLeave: false, index: 5);
            RegisterRacketeeringMenuOptions(campaignGameStarter);

            //clanHideoutMenu -> Buy casino
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_casino_menu_buy", "[" + ClanHideout.Casino_LevelPrice + "{GOLD_ICON}] Construire un casino",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    if (_clanHideout.LevelHideout < 4) a.Tooltip = new TextObject("Planque niveau 4 nécessaire");
                    else if (Hero.MainHero.Gold < ClanHideout.Racketeering_LevelPrice) a.Tooltip = new TextObject("Pas assez d'or");
                    a.IsEnabled = Hero.MainHero.Gold >= ClanHideout.Casino_LevelPrice && _clanHideout.LevelHideout >= 4;
                    return _clanHideout.Casino_Level == 0;
                },
                _ =>
                {
                    _clanHideout.Casino_Level += 1;
                    Hero.MainHero.ChangeHeroGold(-ClanHideout.Casino_LevelPrice);
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("clanHideoutMenu");
                },
                isLeave: false, index: 10);

            //clanHideoutMenu -> Casino
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_casino_menu", "Casino",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
                    return _clanHideout.Casino_Level >= 1;
                },
                _ => {
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    RefreshCasinoLevelUpText(); 
                    GameMenu.SwitchToMenu("clanHideout_casino_menu"); },
                isLeave: false, index: 10);
            RegisterCasinoMenuOptions(campaignGameStarter);

            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false,index: 14);

            //clanHideoutMenu -> Unblock Secret Entrance
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideout_unblockSecretEntrance", "[" + ClanHideout.Hideout_UnblockSecretEntrancePrice + "{GOLD_ICON}] Acheter les plans des égouts",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.ForceToGiveGoods;
                    if (_clanHideout.LevelHideout < 2)
                    {
                        a.Tooltip = new TextObject("Clan Niveau 2 nécessaire");
                        a.IsEnabled = false;
                    }
                    else if (Hero.MainHero.Gold < ClanHideout.Racketeering_LevelPrice)
                    {
                        a.Tooltip = new TextObject("Pas assez d'or");
                        a.IsEnabled = false;
                    }
                    else
                    {
                        a.IsEnabled = true;
                        a.Tooltip = new TextObject("Permets de construire un passage secret dans l'agence du Clan afin de pouvoir entrer et sortir des ville à sa guise, sans risquer de se faire détecter");
                    }
                    return !_clanHideout.IsSecretEntranceUnlocked;
                },
                _ =>
                {
                    _clanHideout.IsSecretEntranceUnlocked = true;
                    Hero.MainHero.ChangeHeroGold(-ClanHideout.Hideout_UnblockSecretEntrancePrice);
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu("clanHideoutMenu");
                },
                isLeave: false, index: 15);

            //EmptySpace
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 20);

            //Upgrade Hideout
            TextObject clanHideoutLevelUpText = new TextObject("{CLANHIDEOUTUP}");
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideoutMenu_levelUp",
                clanHideoutLevelUpText.Value,
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    a.IsEnabled = true;
                    if (_clanHideout.LevelHideout >= Clan.PlayerClan.Tier) { a.Tooltip = new TextObject("Clan Tiers "+ (_clanHideout.LevelHideout+1) +" nécessaire"); a.IsEnabled = false; }
                    else if (Hero.MainHero.Gold < (_clanHideout.LevelHideout+1) * ClanHideout.HideoutLevelPrice) { a.Tooltip = new TextObject("Pas assez d'or"); a.IsEnabled = false; }
                    return _clanHideout.LevelHideout < 5;
                },
                _ =>
                {
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _clanHideout.LevelHideout += 1;
                    Hero.MainHero.ChangeHeroGold(-_clanHideout.LevelHideout * ClanHideout.HideoutLevelPrice);
                    RefreshClanHideoutLevelUpText();
                    GameMenu.SwitchToMenu("clanHideoutMenu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de la planque de Clan");
                },
                isLeave: false, index: 500);


            //EmptySpaces
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 997);
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 998);

            //Quitter la planque
            campaignGameStarter.AddGameMenuOption("clanHideoutMenu", "clanHideoutMenu_back", "Quitter la planque",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: true, index: 999);
        }
        public void RefreshClanHideoutLevelUpText()
        {
            TextObject levelUpText = new TextObject("[" + (_clanHideout.LevelHideout+1) * ClanHideout.HideoutLevelPrice + "{GOLD_ICON}] Agrandir la planque");
            GameTexts.SetVariable("CLANHIDEOUTUP", levelUpText);
        }

        #region Recruit Hideout
        public void RegisterRecruitHideoutMenuOption(CampaignGameStarter campaignGameStarter)
        {
            //hideout -> Recruit Outlaws to party
            TextObject priceValueOutlaws = new TextObject("{HIDEOUT_PRICE_VALUE}");
            campaignGameStarter.AddGameMenuOption("hideout_place", "hideout_place_recruitOutlawsParty", "[{HIDEOUT_PRICE_VALUE}{GOLD_ICON}] Recruter les hors-la-loi dans la troupe",
                a =>
                {
                    int value = ClanHideout.CalculateHideoutRecruitValue();
                    a.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
                    a.Tooltip = Hero.MainHero.Gold >= value ? null : new TextObject("Pas assez d'or");
                    a.IsEnabled = Hero.MainHero.Gold >= value;
                    return _clanHideout.Racketeering_Level > 0;
                },
                _ => RecruitHideoutOutlawPartiesConsequence(2),
                isLeave: false, index: 2);

            //hideout -> Recruit Outlaws to ClanHideout
            campaignGameStarter.AddGameMenuOption("hideout_place", "hideout_place_recruitOutlawsPlank", "[{HIDEOUT_PRICE_VALUE}{GOLD_ICON}] Recruter les hors-la-loi dans la planque",
                a =>
                {
                    int value = ClanHideout.CalculateHideoutRecruitValue();
                    a.optionLeaveType = GameMenuOption.LeaveType.TroopSelection;
                    a.Tooltip = Hero.MainHero.Gold >= value ? null : new TextObject("Pas assez d'or");
                    a.IsEnabled = Hero.MainHero.Gold >= value;
                    return _clanHideout.Racketeering_Level > 0;
                },
                _ => RecruitHideoutOutlawPartiesConsequence(1),
                isLeave: false, index: 3);
        }

        public void RefreshHideoutMenuText()
        {
            if (Settlement.CurrentSettlement != null && Settlement.CurrentSettlement.IsHideout)
            {
                GameTexts.SetVariable("HIDEOUT_PRICE_VALUE", ClanHideout.CalculateHideoutRecruitValue());
            }
        }

        //Mode = 1 => Send to ClanHideoutGangStrenght
        //Mode = 2 => Send to Party
        public void RecruitHideoutOutlawPartiesConsequence(int mode)
        {
            Hero.MainHero.ChangeHeroGold(-ClanHideout.CalculateHideoutRecruitValue());
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));

            List<PartyBase> partiesGetDefenderParties = Settlement.CurrentSettlement.Hideout.GetDefenderParties(MapEvent.BattleTypes.Hideout).Where(party => party.NumberOfAllMembers != 0).ToList();
            foreach (PartyBase partyBase in partiesGetDefenderParties)
            {
                if (partyBase.MemberRoster != null)
                {
                    if (mode == 1)
                    {
                        foreach (TroopRosterElement troop in partyBase.MemberRoster.GetTroopRoster()) _clanHideout.BanditsGangStrenght += troop.Character.Tier * troop.Number;
                    }
                    else //mode == 2
                    {
                        PartyBase.MainParty.AddMembers(partyBase.MemberRoster);
                    }

                }
            }
            Settlement settlement = Settlement.CurrentSettlement;

            //Destroy other parties
            //int IndexToCheck = 0;
/*            while (Settlement.CurrentSettlement.Parties.Count > 1)
            {
                if (!Settlement.CurrentSettlement.Parties[IndexToCheck].IsMainParty)
                {
					DestroyPartyAction.Apply(
					Settlement.CurrentSettlement.Parties[IndexToCheck].Remo();
                }
                else
                {
                    IndexToCheck = 1;
                }

            }*/

            foreach (MobileParty party in Settlement.CurrentSettlement.Parties)
            {
                if (!party.IsMainParty)
                {
                    DestroyPartyAction.Apply(party.Party,party);
                }
            }
            //Leave hideout
            PlayerEncounter.LeaveSettlement();
            PlayerEncounter.Finish(true);
            Campaign.Current.SaveHandler.SignalAutoSave();

            //Destroy Hideout
            settlement.IsActive = false;
        }
        #endregion

        #region Racketeering
        public void RegisterRacketeeringMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            TextObject racketeeringText = new TextObject("{RACKETEERINGTEXTUP}");
            //LevelUp Racketeeting
            campaignGameStarter.AddGameMenuOption("clanHideout_racketeering_menu", "clanHideout_racketeering_menu_LevelUp",
                racketeeringText.Value,
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.OrderTroopsToAttack;
                    if (_clanHideout.LevelHideout <= _clanHideout.Racketeering_Level) { a.Tooltip = new TextObject("Amélioration de la planque nécessaire"); a.IsEnabled = false; }
                    else if (Hero.MainHero.Gold < _clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice) { a.Tooltip = new TextObject("Pas assez d'or"); a.IsEnabled = false; }
                    return _clanHideout.Racketeering_Level < 5;
                },
                _ =>
                {
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _clanHideout.Racketeering_Level += 1;
                    Hero.MainHero.ChangeHeroGold(-_clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice);
                    RefreshRacketeeringMenuText();
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

        public void RefreshRacketeeringMenuText()
        {
            TextObject levelUpText = new TextObject("[" + _clanHideout.Racketeering_Level * ClanHideout.Racketeering_LevelPrice + "{GOLD_ICON}] Améliorer les techniques d'extorsion");
            GameTexts.SetVariable("RACKETEERINGTEXTUP", levelUpText);
        }
        #endregion


        #region Casino

        public void RegisterCasinoMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            TextObject casinoLevelUpText = new TextObject("{CASINOUP}");
            campaignGameStarter.AddGameMenuOption("clanHideout_casino_menu", "clanHideout_casino_menu_levelup", casinoLevelUpText.Value,
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Manage;
                    a.IsEnabled = true;
                    a.Tooltip = new TextObject("Min diminué de 1000 / Max augmenté de 1200");
                    if (_clanHideout.Casino_Level >= _clanHideout.LevelHideout) { a.Tooltip = new TextObject("Planque niveau  " + (_clanHideout.Casino_Level + 1) + " nécessaire"); a.IsEnabled = false; }
                    else if (Hero.MainHero.Gold < (_clanHideout.Casino_Level+1) * ClanHideout.Casino_LevelPrice) { a.Tooltip = new TextObject("Pas assez d'or"); a.IsEnabled = false; }
                    return _clanHideout.Casino_Level<5;
                },
                _ =>
                {
                    SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
                    _clanHideout.Casino_Level += 1;
                    Hero.MainHero.ChangeHeroGold(-_clanHideout.Casino_Level * ClanHideout.Casino_LevelPrice);
                    RefreshCasinoLevelUpText();
                    GameMenu.SwitchToMenu("clanHideout_casino_menu");
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    PrestigiousBank.LogMessage("Amélioration de la planque de Clan");
                },
                isLeave: true, index: 990);

            //Retour
            campaignGameStarter.AddGameMenuOption("clanHideout_casino_menu", "clanHideout_casino_menu_back", "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu("clanHideoutMenu"),
                isLeave: true, index: 999);
        }

        public void RefreshCasinoLevelUpText()
        {
            TextObject levelUpText = new TextObject("[" + _clanHideout.Casino_Level * (ClanHideout.Casino_LevelPrice + 1) + "{GOLD_ICON}] Agrandir le casino");
            GameTexts.SetVariable("CASINOUP", levelUpText);
        }

        #endregion

        public void RegisterSecretEntranceMenuOption(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption("town_outside", "town_outside_secret_entrance", "Rentrer dans la ville par le passage secret",
                a =>
                {
                    bool result = false;
                    if (Settlement.CurrentSettlement.IsTown)
                    {
                        ClanAgency agency = ClanAgenciesBehaviour.ClanAgencies.GetAgencyByTownStringId(Settlement.CurrentSettlement.Town.StringId);
                        if (agency != null && agency.IsSecretEntranceUnlocked)
                        {
                            a.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
                            a.Tooltip = new TextObject("Passer par le passage secret sûr");
                            result = true;
                        }
                        
                    }
                    return result;
                },
                _ => GameMenu.SwitchToMenu("town"),
                isLeave: false, index: 3);
        }


    }
}