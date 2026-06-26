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
    public class BankBehaviour : CampaignBehaviorBase
    {
        public static string _TownID;
        public static Bank _bankInstance;
        public static Bank BankInstance
        {
            get
            {
                if (_bankInstance == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _TownID)
                            {
                                _bankInstance = new Bank(town.Settlement);
                                break;
                            }
                                
                        }

                    }
                }
                if (_bankInstance != null && _bankInstance.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _TownID)
                        {
                            _bankInstance.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                if (!_bankInstance.CanRecruitMercenariesInThisBank) _bankInstance.InitMercenariesVariables();
                return _bankInstance;

            }
            set
            {
                _bankInstance = value;
            }
        }

        public BankBehaviour() : base()
        {}

        public override void RegisterEvents()
        {
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickClan);
            CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, this.HourlyTickEvent);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
        }

        protected virtual void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
        }


        protected virtual void DailyTickClan()
        {
            //Ajout des Mercenaires
            if (BankInstance.CanRecruitMercenariesInThisBank) BankInstance.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            var time = Campaign.CurrentTime;
            if ((int)time % 24 == 14)
            {
                if (BankInstance.LoanAmount > 0)
                {
                    BankInstance.ApplyLoanRefound();
                }
            }

            BankInstance.ApplyDiamondLevelGoldTownIncrease();
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<Bank>("Bank", ref _bankInstance);
                }
                else
                {
                    dataStore.SyncData<Bank>("Bank", ref _bankInstance);
                }
            }
            catch
            {
            }
        }

    }
}