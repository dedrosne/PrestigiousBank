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
using TOR_Core.CharacterDevelopment;

namespace PrestigiousBank
{
    public class YnEdrylKoiranBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string townID = "town_comp_AL1";
        public static YnEdrylKoiranBank _bank= null;

        public static YnEdrylKoiranBank YnEdrylKoiranBank
        {
            get
            {
                if (_bank == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == townID)
                            {
                                _bank = new YnEdrylKoiranBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }
                }
                if (_bank != null && _bank.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == townID)
                        {
                            _bank.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bank;

            }
            set
            {
                _bank = value;
            }
        }

        public YnEdrylKoiranBankCampaignBehavior() : base()
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
            new YnEdrylKoiranBankMenu().RegisterBankMenu(campaignGameStarter, YnEdrylKoiranBank);
        }

        private void DailyTickClan()
        {
            //Ajout de la ressource speciale
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "ForestHarmony")
                Hero.MainHero.AddCultureSpecificCustomResource(YnEdrylKoiranBank.CalculateForestHarmonyInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Medicine, YnEdrylKoiranBank.GetDailySkillXP());
        }


        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<YnEdrylKoiranBank>("YnEdrylKoiranBank", ref _bank);
                }
                else
                {
                    dataStore.SyncData<YnEdrylKoiranBank>("YnEdrylKoiranBank", ref _bank);
                }
            }
            catch
            {
            }
        }

    }
}