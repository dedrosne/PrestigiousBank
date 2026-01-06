using HarmonyLib;
using Newtonsoft.Json;
using PrestigiousBank;
using PrestigiousBank.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.MapNotificationTypes;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
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
using TOR_Core.Utilities;

namespace PrestigiousBank
{
    public class ClanHideoutCampaignBehavior : CampaignBehaviorBase
    {
        public static string townID = "town_comp_NL1";//Salzenmund
        public static ClanHideout _hideout = null;

        public static ClanHideout ClanHideout
        {
            get
            {
                if (_hideout == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == townID)
                            {
                                _hideout = new ClanHideout(townID);
                                break;
                            }
                                
                        }

                    }
                }
                return _hideout;

            }
            set
            {
                _hideout = value;
            }
        }

        public ClanHideoutCampaignBehavior() : base()
        {}

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener((object)this, new Action<CampaignGameStarter>(this.OnSessionLaunched));
            CampaignEvents.SettlementEntered.AddNonSerializedListener(this, new Action<MobileParty, Settlement, Hero>(this.SettlementEntered));
            CampaignEvents.DailyTickEvent.AddNonSerializedListener(this, this.DailyTickEvent);

        }

        private void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            new ClanHideoutMenu().RegisterFactoryMenu(campaignGameStarter, ClanHideout);
            AllTimeHideoutAttackEnable();
        }
        

        private void DailyTickEvent()
        {
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Roguery, ClanHideout.GetDailySkillXP());




        }



        public void SettlementEntered(MobileParty mobileParty, Settlement settlement, Hero hero)
        {
            if (hero == Hero.MainHero) 
            {
                List<CharacterObject> list =  CharacterObject.All.Where<CharacterObject>(x=>!x.IsHero).ToList();
                list = list.Where<CharacterObject>(x => !x.IsTemplate &&(
                x.Culture.StringId == TORConstants.Cultures.HERRIMAULT |
                x.Culture.StringId == "mountain_bandits" |
                x.Culture.StringId == TORConstants.Cultures.DRUCHII |
                x.Culture.StringId == TORConstants.Cultures.CHAOS |
                x.IsBeastman() | x.IsCultist() ))
                    .ToList();

                if (settlement!= null && settlement.IsHideout)
                {
                    new ClanHideoutMenu().RefreshHideoutMenuText();
                }
            }
            if (settlement != null && settlement.IsTown && settlement.Town.StringId == ClanHideout.TownID && mobileParty != null && mobileParty.IsVillager)
                ClanHideout.Apply_Racketeering(mobileParty, settlement);
            


        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<ClanHideout>("ClanHideout", ref _hideout);
                }
                else
                {
                    dataStore.SyncData<ClanHideout>("ClanHideout", ref _hideout);
                }
            }
            catch
            {
            }
        }

        private HideoutCampaignBehavior _hideoutCampaignBehavior
        {
            get
            {
                return Campaign.Current.GetCampaignBehavior<HideoutCampaignBehavior>();
            }
        }

        private void AllTimeHideoutAttackEnable()
        {
            _attackHideoutStartField.Invoke(this._hideoutCampaignBehavior) = this._attackHideoutStartFieldValue;
            _attackHideoutEndField.Invoke(this._hideoutCampaignBehavior) = this._attackHideoutEndFieldValue;
        }

        // Token: 0x04000014 RID: 20
        private int _attackHideoutStartFieldValue = 0;

        // Token: 0x04000015 RID: 21
        private int _attackHideoutEndFieldValue = 24;

        // Token: 0x04000016 RID: 22
        private static AccessTools.FieldRef<HideoutCampaignBehavior, int> _attackHideoutStartField = AccessTools.FieldRefAccess<HideoutCampaignBehavior, int>("CanAttackHideoutStart");

        // Token: 0x04000017 RID: 23
        private static AccessTools.FieldRef<HideoutCampaignBehavior, int> _attackHideoutEndField = AccessTools.FieldRefAccess<HideoutCampaignBehavior, int>("CanAttackHideoutEnd");

    }
}