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
    public class ParravonBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string townID = "town_comp_PA1";
        public static ParravonBank _bank= null;

        public static ParravonBank ParravonBank
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
                                _bank = new ParravonBank(town.Settlement);
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

        public ParravonBankCampaignBehavior() : base()
        {
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, this.HourlyTickEvent);

        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new ParravonBankMenu().RegisterBankMenu(campaignGameStarter, ParravonBank);
        }

        private void DailyTickClan()
        {
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Leadership, ParravonBank.GetDailySkillXP());

            //Ajout des Mercenaires
            if (ParravonBank.CanRecruitMercenariesInThisBank) ParravonBank.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            ParravonBank.ApplyDiamondLevelGoldTownIncrease();
        }


        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<ParravonBank>("ParravonBank", ref _bank);
                }
                else
                {
                    dataStore.SyncData<ParravonBank>("ParravonBank", ref _bank);
                }
            }
            catch
            {
            }
        }

    }
}