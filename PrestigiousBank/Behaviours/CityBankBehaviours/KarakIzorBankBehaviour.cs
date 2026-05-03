using Newtonsoft.Json;
using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
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
    public class KarakIzorBankCampaignBehavior : CampaignBehaviorBase
    {
        public static string _KarakIzorTownID = "town_comp_KI1";
        public static KarakIzorBank _bankKarakIzor = null;

        public static KarakIzorBank BankKarakIzor
        {
            get
            {
                if (_bankKarakIzor == null)
                {
                    if (Town.AllTowns != null) {
                        foreach (var town in Town.AllTowns)
                        {
                            if (town.StringId == _KarakIzorTownID)
                            {
                                _bankKarakIzor = new KarakIzorBank(town.Settlement);
                                break;
                            }
                                
                        }

                    }
                }
                if (_bankKarakIzor != null && _bankKarakIzor.Ville == null)
                {
                    foreach (var town in Town.AllTowns)
                    {
                        if (town.StringId == _KarakIzorTownID)
                        {
                            _bankKarakIzor.Ville = town.Settlement;
                            break;
                        }

                    }
                }
                if (!_bankKarakIzor.CanRecruitMercenariesInThisBank) _bankKarakIzor.InitMercenariesVariables();
                return _bankKarakIzor;

            }
            set
            {
                _bankKarakIzor = value;
            }
        }

        public KarakIzorBankCampaignBehavior() : base()
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
            new KarakIzorBankMenu().RegisterBankMenu(campaignGameStarter, BankKarakIzor);
        }

        private void DailyTickClan()
        {
            //Add of Oathgold
            if (Hero.MainHero.GetCultureSpecificCustomResource().StringId == "OathGold")
                Hero.MainHero.AddCultureSpecificCustomResource(BankKarakIzor.CalculateOathGoldGain());
            //Ajout de l'XP
            Hero.MainHero.AddSkillXp(DefaultSkills.Crafting, BankKarakIzor.GetDailySkillXP());
            //Ajout des Mercenaires
            if (BankKarakIzor.CanRecruitMercenariesInThisBank) BankKarakIzor.ApplyRegenMercenariesPerDay();
        }

        private void HourlyTickEvent()
        {
            BankKarakIzor.ApplyDiamondLevelGoldTownIncrease();

            //Stamina Regen
            var mainParty = MobileParty.MainParty;
            var campaignBehavior = Campaign.Current.GetCampaignBehavior<ICraftingCampaignBehavior>();

            foreach (var hero in mainParty.GetMemberHeroes())
            {
                var stamina = campaignBehavior.GetHeroCraftingStamina(hero);
                var max = campaignBehavior.GetMaxHeroCraftingStamina(hero);
                if (stamina >= max)
                    return;
                var value = Math.Min(max, stamina + MathF.Floor(BankKarakIzor.RegenStaminaBought*KarakIzorBank.RegenStaminaPerHourPerPurchaseBought));
                campaignBehavior.SetHeroCraftingStamina(hero, value);
                
            }
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                if (dataStore.IsLoading)
                {
                    dataStore.SyncData<KarakIzorBank>("KarakIzorBank", ref _bankKarakIzor);
                }
                else
                {
                    dataStore.SyncData<KarakIzorBank>("KarakIzorBank", ref _bankKarakIzor);
                }
            }
            catch
            {
            }
        }

    }
}