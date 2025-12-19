using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection.Information;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.ScreenSystem;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class DrakenhofBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _drakenhofTownID = "town_comp_SY1";
        public static DrakenhofBank _bankDrakenhof = null;
        //public static string BankMenuLinkText = "<a style=\"Link.Settlement\" href=\"event:Concept-str_game_objects_simple_bank\"><b>" + PrestigiousBank.Config.BankName + "</b></a>";

        public static DrakenhofBank BankDrakenhof
        {
            get
            {
                if (_bankDrakenhof == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _drakenhofTownID)
                            {
                                _bankDrakenhof = new DrakenhofBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }

                }
                if (_bankDrakenhof != null && _bankDrakenhof.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _drakenhofTownID)
                        {
                            _bankDrakenhof.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bankDrakenhof;

            }
            set
            {
                _bankDrakenhof = value;
            }
        }

        public DrakenhofBankCampaignBehavior() : base()
        {
            //MBTextManager.SetTextVariable("Birke_Bank_Encyclopedia_Main", PrestigiousBank.Config.BankName);
            //bankAltdorf = bankAltdorf;

            //_bankTrait = Game.Current.ObjectManager.RegisterPresumedObject<TraitObject>(new TraitObject("bank"));
            //_bankTrait.Initialize(new TextObject(GameTexts.FindText("str_trait_bankName").ToString()), new TextObject(GameTexts.FindText("str_trait_bankDescription").ToString()), false, 0, 4);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            //CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, GameMenuOpened);
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            //CampaignEvents.GameMenuOptionSelectedEvent.AddNonSerializedListener(this, GameMenuOptionSelected);
            //CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, CalculateRentEvent);
            //CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, OnGameLoadedEvent);
            //CampaignEvents.OnGameEarlyLoadedEvent.AddNonSerializedListener(this, OnGameEarlyLoadedEvent);
            //CampaignEvents.RaidCompletedEvent.AddNonSerializedListener(this, OnRaidCompletedEvent);
            //CampaignEvents.OnSettlementOwnerChangedEvent.AddNonSerializedListener(this, OnSettlementOwnerChangedEvent);
            //Game.Current.EventManager.RegisterEvent<TutorialContextChangedEvent>(new Action<TutorialContextChangedEvent>(this.OnTutorialContextChange));

            //if (PrestigiousBank.Config.DoBalancing)
            //    CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, BalanceNationEvent);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new DrakenhofBankMenu().RegisterBankMenu(campaignGameStarter, BankDrakenhof);
        }

        private void DailyTickClan()
        {
            //Ajout de l'énergie noire
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "DarkEnergy")
                Hero.MainHero.AddCultureSpecificCustomResource(BankDrakenhof.CalculateDarkEnergyInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( DefaultSkills.Charm, BankDrakenhof.GetDailySkillXP());
        }

        /*
       private void BalanceNationEvent()
       {
           //Bank.BalanceWeakestNation();
       }

       private void OnTutorialContextChange(TutorialContextChangedEvent obj)
       {
           try
           {
               if (obj.NewContext == TutorialContexts.CharacterScreen)
               {

                   if (ScreenManager.TopScreen is SandBox.GauntletUI.GauntletCharacterDeveloperScreen screen)
                   {
                       if (ScreenManager.TopScreen.Layers.Any() && ScreenManager.TopScreen.Layers[0] is GauntletLayer layer)
                       {
                           var view = layer.GetMovieIdentifier("CharacterDeveloper").DataSource as TaleWorlds.CampaignSystem.ViewModelCollection.CharacterDeveloper.CharacterDeveloperVM;
                           var traitValue = Hero.MainHero.GetTraitLevel(_bankTrait);

                           GameTexts.SetVariable("NEWLINE", "\n");
                           TextObject text = GameTexts.FindText("str_trait_name_" + _bankTrait.StringId.ToLower(), traitValue.ToString());
                           GameTexts.SetVariable("TRAIT_VALUE", traitValue);
                           GameTexts.SetVariable("TRAIT_NAME", text);
                           GameTexts.SetVariable("TRAIT", GameTexts.FindText("str_trait", _bankTrait.StringId.ToLower()));
                           GameTexts.SetVariable("TRAIT_DESCRIPTION", _bankTrait.Description);

                           var tmp = new EncyclopediaTraitItemVM(_bankTrait, view.CurrentCharacter.Hero);
                           tmp.Hint = new HintViewModel(new TextObject(GameTexts.FindText("str_trait_tooltip").ToString()));
                           view.CurrentCharacter.Traits.Add(tmp);
                       }
                   }
               }
               else if (obj.NewContext == TutorialContexts.ClanScreen)
               {

                   if (ScreenManager.TopScreen is SandBox.GauntletUI.GauntletClanScreen screen)
                   {
                       if (ScreenManager.TopScreen.Layers.Any() && ScreenManager.TopScreen.Layers[0] is GauntletLayer layer)
                       {
                           var view = layer.GetMovieIdentifier("ClanScreen").DataSource as TaleWorlds.CampaignSystem.ViewModelCollection.ClanManagement.ClanManagementVM;
                           var traitValue = Hero.MainHero.GetTraitLevel(_bankTrait);

                           GameTexts.SetVariable("NEWLINE", "\n");
                           TextObject text = GameTexts.FindText("str_trait_name_" + _bankTrait.StringId.ToLower(), traitValue.ToString());
                           GameTexts.SetVariable("TRAIT_VALUE", traitValue);
                           GameTexts.SetVariable("TRAIT_NAME", text);
                           GameTexts.SetVariable("TRAIT", GameTexts.FindText("str_trait", _bankTrait.StringId.ToLower()));
                           GameTexts.SetVariable("TRAIT_DESCRIPTION", _bankTrait.Description);

                           var tmp = new EncyclopediaTraitItemVM(_bankTrait, view.ClanMembers.CurrentSelectedMember.GetHero());
                           tmp.Hint = new HintViewModel(new TextObject(GameTexts.FindText("str_trait_tooltip").ToString()));
                           view.ClanMembers.CurrentSelectedMember.Traits.Add(tmp);
                       }
                   }
               }
           }
           catch (Exception)
           {
           }
       }
*/
        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<DrakenhofBank>("DrakenhofBank", ref _bankDrakenhof);
                }
                else
                {
                    dataStore.SyncData<DrakenhofBank>("DrakenhofBank", ref _bankDrakenhof);
                }
            }
            catch
            {
            }
        }
        /*
        private void OnGameLoadedEvent(CampaignGameStarter obj)
        {
            //Bank.RestoreCulture();
        }

        private void OnGameEarlyLoadedEvent(CampaignGameStarter obj)
        {
            Bank.RestoreCulture();
        }

        private void OnRaidCompletedEvent(BattleSideEnum battleSideEnum, RaidEventComponent raidEventComponent)
        {
            if (Bank.IsSettlementInsured(raidEventComponent.MapEventSettlement))
            {
                if (raidEventComponent.MapEventSettlement.OwnerClan == Hero.MainHero.Clan
                    && raidEventComponent.BattleState == BattleState.AttackerVictory
                    && raidEventComponent.MapEvent.HasWinner)
                {
                    var notification = new SettlementRebellionMapNotification(raidEventComponent.MapEventSettlement,
                            new TextObject(GameTexts.FindText("fief_insurance_raid_completed_body", null).ToString()
                            .Replace("-=SETTLEMENT_NAME=-", raidEventComponent.MapEventSettlement.EncyclopediaLinkWithName.ToString())
                            .Replace("-=BANK_NAME=-", BankMenuLinkText)));

                    Campaign.Current.CampaignInformationManager.NewMapNoticeAdded(notification);

                    if (raidEventComponent.MapEventSettlement.IsVillage)
                    {
                        raidEventComponent.MapEventSettlement.Village.Hearth *= (float)1.01;
                        raidEventComponent.MapEventSettlement.Village.Settlement.Party.SetLevelMaskIsDirty();
                        ChangeVillageStateAction.ApplyBySettingToNormal(raidEventComponent.MapEventSettlement);
                    }
                    var insuredSettlement = Bank.GetInsuredSettlement(raidEventComponent.MapEventSettlement);

                    if (insuredSettlement != null)
                    {
                        var currentTime = (float)Campaign.CurrentTime;
                        currentTime += PrestigiousBank.Config.RaidRestorationProtectionTimeInHouers;
                        var time = CampaignTime.Hours(currentTime);
                        insuredSettlement.CantBeRaidedUntil = (float)time.ToHours;
                    }
                }
            }
        }

        private void OnSettlementOwnerChangedEvent(Settlement settlement, bool arg2, Hero toHero, Hero fromHero, Hero arg5, ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail detail)
        {
            if (!settlement.IsTown || !settlement.IsCastle)
                return;

            var insuredVillages = settlement.Town.Villages.Where(x => Bank.IsSettlementInsured(x.Settlement));

            if (fromHero == Hero.MainHero && (settlement.IsCastle || settlement.IsTown) && insuredVillages != null && insuredVillages.Any())
            {
                switch (detail)
                {
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.BySiege:
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByRebellion:
                        InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("fief_insurance_fieflost_header", null).ToString(),
                                GameTexts.FindText("fief_insurance_fieflost_body", null).ToString()
                                        .Replace("-=SETTLEMENT_NAME=-", settlement.Name.ToString())
                                        .Replace("-=VILLAGES_NAMES=-", string.Join(", ", insuredVillages.Select(x => x.Name)))
                                        .Replace("-=AMOUNT=-", (insuredVillages.Count() * PrestigiousBank.Config.VillageLostCompensation).ToGoldString())
                                        .Replace("-=BANK_NAME=-", BankMenuLinkText)
                                , true, false, GameTexts.FindText("str_message_back", null).ToString(), "", null, null), true);

                        Bank.CancelFiefInsurances(insuredVillages);
                        Bank.AddIntoAccountOrPurse(insuredVillages.Count() * PrestigiousBank.Config.VillageLostCompensation);
                        break;
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByBarter:
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByLeaveFaction:
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByGift:
                        InformationManager.ShowInquiry(new InquiryData(GameTexts.FindText("fief_insurance_fiefgivenaway_header", null).ToString(),
                                GameTexts.FindText("fief_insurance_fiefgivenaway_body", null).ToString()
                                        .Replace("-=SETTLEMENT_NAME=-", settlement.Name.ToString())
                                        .Replace("-=VILLAGES_NAMES=-", string.Join(", ", insuredVillages.Select(x => x.Name)))
                                        .Replace("-=BANK_NAME=-", BankMenuLinkText)
                                , true, false, GameTexts.FindText("str_message_back", null).ToString(), "", null, null), true);
                        //Bank.CancelFiefInsurances(insuredVillages);
                        break;
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.Default:
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByClanDestruction:
                    case ChangeOwnerOfSettlementAction.ChangeOwnerOfSettlementDetail.ByKingDecision:
                        break;
                    default:
                        break;
                }
            }
        }
        */




        /*
                private void CalculateRentEvent()
                {
                    var time = Campaign.CurrentTime;

                    if ((int)time % 24 == 9 && Bank?.Solde != 0)
                    {
                        int amount = Bank.CalculateRent();
                        var goldAmount = Hero.MainHero.Gold;

                        if (amount < 0)
                        {
                            if (goldAmount > amount * -1)
                            {
                                if (!PrestigiousBank.Config.DisablePopups)
                                    InformationManager.ShowInquiry(new InquiryData(
                                        GameTexts.FindText("str_messagefromyourbank", null).ToString(),
                                        GameTexts.FindText("str_bankrentcalculation", null).ToString()
                                                                .Replace("-=AMOUNT=-", $"{-1 * amount}")
                                                                .Replace("-=BANK_NAME=-", BankMenuLinkText),
                                        true,
                                        false,
                                        GameTexts.FindText("str_message_back", null).ToString(),
                                        "",
                                        null,
                                        null), true);

                                Hero.MainHero.ChangeHeroGold(amount);
                            }
                            else
                            {
                                if ((float)Bank?.Solde + amount < int.MinValue)
                                {
                                    Bank.Solde = int.MinValue;
                                    return;
                                }

                                Bank.Solde += amount;

                                if (!PrestigiousBank.Config.DisablePopups)
                                    InformationManager.ShowInquiry(new InquiryData(
                                        GameTexts.FindText("str_messagefromyourbank", null).ToString(),
                                        GameTexts.FindText("str_bankrentcalculation2", null).ToString()
                                                                .Replace("-=amount=-", $"{amount}")
                                                                .Replace("-=Saldo=-", $"{Bank.Solde}"),
                                        true,
                                        false,
                                        GameTexts.FindText("str_message_back", null).ToString(),
                                        "",
                                        null,
                                        null), true);
                            }
                        }
                        else
                        {
                            if (amount > 100)
                            {
                                Hero.MainHero.AddSkillXp(DefaultSkills.Trade, Bank.GetTradingSkillAmount(amount));
                            }

                            if (PrestigiousBank.Config.PutInterestIntoAccount)
                            {
                                if ((float)Bank.Solde + amount > int.MaxValue)
                                {
                                    Bank.Solde = int.MaxValue;
                                    return;
                                }

                                Bank.Solde += amount;
                            }
                            else
                            {
                                if (!PrestigiousBank.Config.DisablePopups)
                                    InformationManager.ShowInquiry(new InquiryData(
                                        GameTexts.FindText("str_messagefromyourbank", null).ToString(),
                                        GameTexts.FindText("str_bankrentcalculation3", null).ToString()
                                                                .Replace("-=amount=-", $"{amount}")
                                                                .Replace("-=BANK_NAME=-", BankMenuLinkText),
                                        true,
                                        false,
                                        GameTexts.FindText("str_message_back", null).ToString(),
                                        "",
                                        null,
                                        null), true);

                                Hero.MainHero.ChangeHeroGold(amount);
                            }
                            UpdateTraitLevel();
                        }
                    }

                    CheckInsuredCaravans();
                    CheckInsuredSettlements();
                }

                private void CheckInsuredCaravans()
                {
                    if (BankCampaignBehavior.Bank.InsuredCaravans == null || !BankCampaignBehavior.Bank.InsuredCaravans.Any())
                        return;

                    float currentTime = (float)Campaign.CurrentTime;

                    var notInsuredCaravans = BankCampaignBehavior.Bank.InsuredCaravans.Where(x => !x.Notified && x.FreePassageUntil <= currentTime).ToList();

                    if (notInsuredCaravans.Any())
                    {
                        InformationManager.ShowInquiry(new InquiryData(
                                        GameTexts.FindText("str_messagefromyourbank", null).ToString(),
                                        GameTexts.FindText("message_from_your_bank_check_caravan", null).ToString()
                                                                .Replace("-=NOTINSUREDCARAVANSCOUNT=-", $"{notInsuredCaravans.Count()}")
                                                                .Replace("-=CARAVANLEADERNAME=-", $"{GetCaravanLeaderNames(notInsuredCaravans)}")
                                                                .Replace("-=BANK_NAME=-", BankMenuLinkText),
                                        true,
                                        false,
                                        GameTexts.FindText("str_message_back", null).ToString(),
                                        "",
                                        null,
                                        null), true);

                        notInsuredCaravans.ForEach(x => x.Notified = true);
                    }
                }

                private string GetCaravanLeaderNames(IEnumerable<InsuredCaravan> notInsuredCaravans)
                {
                    var caravanIds = notInsuredCaravans.Select(x => x.CaravanId).ToList();
                    var validCaravans = Campaign.Current.MainParty.LeaderHero.OwnedCaravans.Where(x => caravanIds.Contains(x.Party.MobileParty.Id.InternalValue));

                    return String.Join(", ", validCaravans.Select(x => x.Leader.EncyclopediaLinkWithName.ToString()));
                }

                private void CheckInsuredSettlements()
                {
                    if (BankCampaignBehavior.Bank.InsuredSettlements == null || !BankCampaignBehavior.Bank.InsuredSettlements.Any())
                        return;

                    float currentTime = (float)Campaign.CurrentTime;

                    var notInsuredSettlements = BankCampaignBehavior.Bank.InsuredSettlements.Where(x => !x.Notified && x.InsuredUntil <= currentTime).ToList();

                    if (notInsuredSettlements.Any())
                    {
                        InformationManager.ShowInquiry(new InquiryData(
                                        GameTexts.FindText("str_messagefromyourbank", null).ToString(),
                                        GameTexts.FindText("message_from_your_bank_check_insured_settlements", null).ToString()
                                                                .Replace("-=notInsuredSettlements=-", $"{notInsuredSettlements.Count()}")
                                                                .Replace("-=GetCaravanSettlementNames=-", $"{GetCaravanSettlementNames(notInsuredSettlements)}")
                                                                .Replace("-=BANK_NAME=-", BankMenuLinkText),
                                        true,
                                        false,
                                        GameTexts.FindText("str_message_back", null).ToString(),
                                        "",
                                        null,
                                        null), true);

                        notInsuredSettlements.ForEach(x => x.Notified = true);
                    }
                }

                private string GetCaravanSettlementNames(IEnumerable<InsuredSettlement> notInsuredSettlements)
                {
                    var settlementIds = notInsuredSettlements.Select(x => x.SettlementId);
                    var validSettlements = Campaign.Current.MainParty.ActualClan.Settlements.Where(x => x.IsVillage && settlementIds.Contains(x.Village.Id.InternalValue));

                    return String.Join(", ", validSettlements.Select(x => x.EncyclopediaLinkWithName.ToString()));
                }*/

    }
}