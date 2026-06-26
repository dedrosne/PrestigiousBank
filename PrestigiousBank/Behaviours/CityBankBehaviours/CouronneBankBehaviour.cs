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
    public class CouronneBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _CouronneTownID = "town_comp_CO1";
        public static CouronneBank _bankCouronne = null;
        //public static string BankMenuLinkText = "<a style=\"Link.Settlement\" href=\"event:Concept-str_game_objects_simple_bank\"><b>" + PrestigiousBank.Config.BankName + "</b></a>";

        public static CouronneBank BankInstance
        {
            get
            {
                if (_bankCouronne == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _CouronneTownID)
                            {
                                _bankCouronne = new CouronneBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }

                }
                if (_bankCouronne != null && _bankCouronne.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _CouronneTownID)
                        {
                            _bankCouronne.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                return _bankCouronne;

            }
            set
            {
                _bankCouronne = value;
            }
        }

        public CouronneBankCampaignBehavior() : base()
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
            new CouronneBankMenu().RegisterBankMenu(campaignGameStarter, BankInstance);
        }

        private void DailyTickClan()
        {
            //Ajout de l'énergie noire
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "Chivalry")
                Hero.MainHero.AddCultureSpecificCustomResource(BankInstance.CalculateChivalryInterests());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp( DefaultSkills.Riding, BankInstance.GetDailySkillXP());

            //Ajout des Mercenaires
            if (BankInstance.CanRecruitMercenariesInThisBank) BankInstance.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            BankInstance.ApplyDiamondLevelGoldTownIncrease();
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<CouronneBank>("CouronneBank", ref _bankCouronne);
                }
                else
                {
                    dataStore.SyncData<CouronneBank>("CouronneBank", ref _bankCouronne);
                }
            }
            catch
            {
            }
        }
        

    }
}