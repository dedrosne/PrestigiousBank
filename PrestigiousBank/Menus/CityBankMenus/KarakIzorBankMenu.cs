using Messages.FromClient.ToLobbyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.LinQuick;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.AbilitySystem.Spells;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;
using TOR_Core.Utilities;

namespace PrestigiousBank
{
    public class KarakIzorBankMenu:BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int oathgoldSolde = ((KarakIzorBank) _bank).OathgoldAccountSolde;
            int oathgoldInterests = ((KarakIzorBank)_bank).CalculateOathGoldGain();

            //Oathgold Account Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_oathgold_account",_cityID), 
                String.Format("Fortune investie : {0}\n+" + 
                Hero.MainHero.GetCultureSpecificCustomResource().GetCustomResourceIconAsText() + "+ extraits par jour : {1}", oathgoldSolde, oathgoldInterests), 
                null, 
                GameMenu.MenuOverlayType.SettlementWithCharacters);

            //Smithing Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_smithing", _cityID),
                "Ecole de forge",
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);

        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            // Bank Menu -> Oathgold Account
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_oathgold_account", _cityID), 
                "Mine de Sermendors",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                    a.Tooltip = ((KarakIzorBank)_bank).GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = ((KarakIzorBank)_bank).GetCustomerLevel() > 1;
                    return Hero.MainHero.Culture.StringId == TORConstants.Cultures.DAWI; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_oathgold_account", _cityID)),
                isLeave: false, index: 2);
            RegisterOathgoldAccountMenuOptions(campaignGameStarter);

            //Bank Menu -> Smithing School
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_smithing", _cityID), 
                "Ecole de Forge",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Craft;
                    a.Tooltip = ((KarakIzorBank)_bank).GetCustomerLevel() > 3 ? null : new TextObject("Niveau de client Mythril requis", null);
                    a.IsEnabled = ((KarakIzorBank)_bank).GetCustomerLevel() > 3;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_smithing", _cityID)),
                isLeave: false, index: 3);
            RegisterSmithingSchoolMenuOptions(campaignGameStarter);


        }




        #region Oathgold Account
        private void RegisterOathgoldAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_oathgold_account", _cityID), String.Format("{0}_bank_oathgold_account_{1}",_cityID, qty),
                    "Investir " + qty + "{GOLD_ICON}",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositOathgoldAccount(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_oathgold_account", _cityID), String.Format("{0}_bank_oathgold_account_all", _cityID), "Tout offrir",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = Hero.MainHero.Gold > 0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositOathgoldAccount(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_oathgold_account", _cityID), String.Format("{0}_bank_oathgold_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        private void DepositOathgoldAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            KarakIzorBankCampaignBehavior.BankInstance.OathgoldAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(amount+"{GOLD_ICON} investi.\nSermendors extraits journaliers : " + KarakIzorBankCampaignBehavior.BankInstance.CalculateOathGoldGain(), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu(String.Format("{0}_bank_oathgold_account", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion

        #region Smithing School

        private void RegisterSmithingSchoolMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int pricePerMaxUpgrade = KarakIzorBank.PriceMaximumStaminaUpgrade;
            int pricePerRegenUpgrade = KarakIzorBank.PriceRegenStaminaUpgrade;

            //Buy Max
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), String.Format("{0}_bank_smithing_max", _cityID),
                "[" + pricePerMaxUpgrade + " {GOLD_ICON}] Augmenter l'endurance",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.IsEnabled = Hero.MainHero.Gold >= KarakIzorBank.PriceMaximumStaminaUpgrade;
                    a.Tooltip = Hero.MainHero.Gold >= KarakIzorBank.PriceMaximumStaminaUpgrade ? new TextObject("+"+KarakIzorBank.MaximumStaminaGainPerPurchaseBought+" Stamina Max") : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => BuyMaxUpgrade(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);

            //Buy Regen
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), String.Format("{0}_bank_smithing_regen", _cityID),
                "[" + pricePerRegenUpgrade + " {GOLD_ICON}] Augmenter la récupération",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.IsEnabled = Hero.MainHero.Gold >= KarakIzorBank.PriceRegenStaminaUpgrade;
                    a.Tooltip = Hero.MainHero.Gold >= KarakIzorBank.PriceRegenStaminaUpgrade ? new TextObject("+" + KarakIzorBank.RegenStaminaPerHourPerPurchaseBought + " Stamina per Hour") : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => BuyRegenUpgrade(campaignGameStarter),
                isLeave: false,
                index: 2, isRepeatable: true);


            //Empty space
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, 3);

            //Buy Research Part
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), String.Format("{0}_bank_smithing_learningRate", _cityID),
                "[" + KarakIzorBank.PriceResearchPartFactorUpgrade + " {GOLD_ICON}] Augmenter la vitesse d'apprentissage",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.IsEnabled = Hero.MainHero.Gold >= KarakIzorBank.PriceRegenStaminaUpgrade;
                    a.Tooltip = Hero.MainHero.Gold >= KarakIzorBank.PriceRegenStaminaUpgrade ? new TextObject("+" + KarakIzorBank.ResearchFactorGainPerPurchaseBought*100 + "% de vitesse re recherche") : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => BuyResearchUpgrade(campaignGameStarter),
                isLeave: false,
                index: 4, isRepeatable: true);

            //Empty space
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, 998);

            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_smithing", _cityID), String.Format("{0}_bank_smithing_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 999, isRepeatable: false);
        }

        private void BuyMaxUpgrade(CampaignGameStarter campaignGameStarter)
        {
            ((KarakIzorBank)_bank).MaximumStaminaBought += 1;
            Hero.MainHero.ChangeHeroGold(-KarakIzorBank.PriceMaximumStaminaUpgrade);
            PrestigiousBank.LogMessage("Stamina Maximum augmentée");
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_smithing", _cityID));
        }

        private void BuyRegenUpgrade(CampaignGameStarter campaignGameStarter)
        {
            ((KarakIzorBank)_bank).RegenStaminaBought += 1;
            Hero.MainHero.ChangeHeroGold(-KarakIzorBank.PriceRegenStaminaUpgrade);
            PrestigiousBank.LogMessage("Regen de Stamina augmentée");
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_smithing", _cityID));
        }

        private void BuyResearchUpgrade(CampaignGameStarter campaignGameStarter)
        {
            ((KarakIzorBank)_bank).ResearchPartFactorBought += 1;
            Hero.MainHero.ChangeHeroGold(-KarakIzorBank.PriceResearchPartFactorUpgrade);
            PrestigiousBank.LogMessage("Vitesse de Recherche augmentée");
            PrestigiousBank.LogMessage("Vitesse actuelle : "+(1+ (((KarakIzorBank)_bank).ResearchPartFactorBought* KarakIzorBank.ResearchFactorGainPerPurchaseBought))*100 +"%");
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_smithing", _cityID));
        }




        #endregion

    }
}
