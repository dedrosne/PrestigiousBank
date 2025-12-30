using Newtonsoft.Json;
using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Extensions;
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
using TOR_Core.CharacterDevelopment;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class AltdorfBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _altdorfTownID = "town_comp_RL1";
        public static AltdorfBank _bankAltdorf = null;
        //public static string BankMenuLinkText = "<a style=\"Link.Settlement\" href=\"event:Concept-str_game_objects_simple_bank\"><b>" + PrestigiousBank.Config.BankName + "</b></a>";

        public static AltdorfBank BankAltdorf
        {
            get
            {
                if (_bankAltdorf == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _altdorfTownID)
                            {
                                _bankAltdorf = new AltdorfBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }
                }
                if (_bankAltdorf != null && _bankAltdorf.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _altdorfTownID)
                        {
                            _bankAltdorf.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bankAltdorf;

            }
            set
            {
                _bankAltdorf = value;
            }
        }

        public AltdorfBankCampaignBehavior() : base()
        {
            //MBTextManager.SetTextVariable("Birke_Bank_Encyclopedia_Main", PrestigiousBank.Config.BankName);
            //bankAltdorf = bankAltdorf;

            //_bankTrait = Game.Current.ObjectManager.RegisterPresumedObject<TraitObject>(new TraitObject("bank"));
            //_bankTrait.Initialize(new TextObject(GameTexts.FindText("str_trait_bankName").ToString()), new TextObject(GameTexts.FindText("str_trait_bankDescription").ToString()), false, 0, 4);
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, this.HourlyTickEvent);
        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new AltdorfBankMenu().RegisterBankMenu(campaignGameStarter, BankAltdorf);
        }

        private void DailyTickClan()
        {
            //Ajout du prestige
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "Prestige")
                Hero.MainHero.AddCultureSpecificCustomResource(BankAltdorf.CalculatePrestigiousInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( TORSkills.SpellCraft,BankAltdorf.GetDailySkillXP());
        }

        private void HourlyTickEvent()
        {
            BankAltdorf.ApplyDiamondLevelGoldTownIncrease();
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<AltdorfBank>("AltdorfBank", ref _bankAltdorf);
                }
                else
                {
                    dataStore.SyncData<AltdorfBank>("AltdorfBank", ref _bankAltdorf);
                }
            }
            catch
            {
            }
        }

    }
}