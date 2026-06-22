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
    public class TorLithanelBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string townID = "town_comp_LL1";
        public static TorLithanelBank _bank = null;

        public static TorLithanelBank TorLithanelBank
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
                                _bank = new TorLithanelBank(town.Settlement);
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
                if (!_bank.CanRecruitMercenariesInThisBank) _bank.InitMercenariesVariables();
                return _bank;

            }
            set
            {
                _bank = value;
            }
        }

        public TorLithanelBankCampaignBehavior() : base()
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
            new TorLithanelBankMenu().RegisterBankMenu(campaignGameStarter, TorLithanelBank);
        }

        private void DailyTickClan()
        {
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Bow, TorLithanelBank.GetDailySkillXP());

            //Ajout des Mercenaires
            if (TorLithanelBank.CanRecruitMercenariesInThisBank) TorLithanelBank.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            TorLithanelBank.ApplyDiamondLevelGoldTownIncrease();
        }


        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<TorLithanelBank>("TorLithanelBank", ref _bank);
                }
                else
                {
                    dataStore.SyncData<TorLithanelBank>("TorLithanelBank", ref _bank);
                }
            }
            catch
            {
            }
        }

    }
}