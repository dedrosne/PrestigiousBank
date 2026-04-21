using Helpers;
using Messages.FromClient.ToLobbyServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;
using TaleWorlds.CampaignSystem.Roster;

namespace PrestigiousBank
{
    public class YnEdrylKoiranBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int forestHarmonySolde = ((YnEdrylKoiranBank)_bank).ForestHarmonyAccountSolde;
            int forestHarmonyInterests = ((YnEdrylKoiranBank)_bank).CalculateForestHarmonyInterests();

            //Forest Harmony Account Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_forestHarmony_account", _cityID),
                String.Format("Fortune investie : {0}\nHarmonie générée : {1}", forestHarmonySolde, forestHarmonyInterests),
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);
            

            //Isha Blessings
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_ishaBlessings", _cityID),
                "La bénédiction d'Isha permet sauver une unité lorsqu'elle est sur le point de mourir.\nCette bénédiction a cependant un certain prix\nPuissance de la bénédiction : "+
                ((YnEdrylKoiranBank)_bank).BlessingAmount+
                "\n(= chance de survie aux portes de la mort)\nCoût d'entretien de la bénédiction : "+ ((YnEdrylKoiranBank)_bank).CalculateBlessingUpkeep()+"{GOLD_ICON}",
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);

        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            // Bank Menu -> ForestHarmony Account
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_forestHarmony_account", _cityID),
                "Harmoniser la forêt",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
                    var ressource = Hero.MainHero.GetCultureSpecificCustomResource();
                    if (Hero.MainHero.GetCultureSpecificCustomResource().StringId != "ForestHarmony")
                    {
                        a.Tooltip = new TextObject("Culture Asrai requise", null);
                        a.IsEnabled = false;
                    }
                    else if (((YnEdrylKoiranBank)_bank).GetCustomerLevel() <= 1)
                    {
                        a.Tooltip = new TextObject("Niveau de client Argent requis", null);
                        a.IsEnabled = false;
                    }
                    else
                    {
                        a.Tooltip = null;
                        a.IsEnabled = true;
                    }
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_forestHarmony_account", _cityID)),
                isLeave: false, index: 2);
            RegisterForestHarmonyAccountMenuOptions(campaignGameStarter);


            //Bank Menu -> Isha Blessings
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_ishaBlessings", _cityID),
                "Bénédictions d'Isha",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    a.Tooltip = _bank.GetCustomerLevel() > 3 ? null : new TextObject("Niveau de client Mythril requis", null);
                    a.IsEnabled = _bank.GetCustomerLevel() > 3;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_ishaBlessings", _cityID)),
                isLeave: false, index: 3);
            RegisterIshaBlessingMenuOptions(campaignGameStarter);

            
            
            RegisterIshaBlessingMenuOptions(campaignGameStarter);


            
                
        }




        #region Forest Harmony Account
        private void RegisterForestHarmonyAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_forestHarmony_account", _cityID), 
                    String.Format("{0}_bank_forestHarmony_account_{1}", _cityID, qty),
                    "Offrir " + qty + " {GOLD_ICON}",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositForestHarmonyAccount(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_forestHarmony_account", _cityID), 
                String.Format("{0}_bank_forestHarmony_account_all", _cityID), "Tout offrir",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
                    a.IsEnabled = Hero.MainHero.Gold > 0;
                    a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => DepositForestHarmonyAccount(Hero.MainHero.Gold, campaignGameStarter),
                isLeave: false,
                i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_forestHarmony_account", _cityID), 
                String.Format("{0}_bank_forestHarmony_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        private void DepositForestHarmonyAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            YnEdrylKoiranBankCampaignBehavior.YnEdrylKoiranBank.ForestHarmonyAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            PrestigiousBank.LogMessage(String.Format("Cadeau de {0} accepté.\nHarmonie de la forêt : {1}", amount, 
                YnEdrylKoiranBankCampaignBehavior.YnEdrylKoiranBank.CalculateForestHarmonyInterests()), 0xFFBBAA00);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_forestHarmony_account", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion

        #region Isha Blessings

        private void RegisterIshaBlessingMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int pricePerBlessing = YnEdrylKoiranBank.PricePerBlessing;
            //IncreaseBlessing
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_ishaBlessings", _cityID), String.Format("{0}_bank_ishaBlessings_increase", _cityID),
                "[" + pricePerBlessing + "{GOLD_ICON}] Augmenter la bénédiction",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    a.IsEnabled = Hero.MainHero.Gold >= pricePerBlessing && ((YnEdrylKoiranBank)_bank).BlessingAmount < 100;
                    if (((YnEdrylKoiranBank)_bank).BlessingAmount >= 100) a.Tooltip = new TextObject("Bénédiction à la puissance maximum", null);
                    else if (Hero.MainHero.Gold < pricePerBlessing) a.Tooltip = new TextObject("Pas assez d'argent", null);
                    else a.Tooltip = null;
                    return true;
                },
                _ => IncreaseBlessing(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);

            //Decrease Blessing
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_ishaBlessings", _cityID), String.Format("{0}_bank_ishaBlessings_decrease", _cityID),
                "Diminuer le bénédiction",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    a.IsEnabled = ((YnEdrylKoiranBank)_bank).BlessingAmount > 0;
                    a.Tooltip = ((YnEdrylKoiranBank)_bank).BlessingAmount > 0 ? null : new TextObject("Bénédiction déjà au minimum", null);
                    return true;
                },
                _ => DecreaseBlessing(campaignGameStarter),
                isLeave: false,
                index: 2, isRepeatable: true);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_ishaBlessings", _cityID), String.Format("{0}_bank_ishaBlessings_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 9, isRepeatable: false);
        }

        private void IncreaseBlessing(CampaignGameStarter CampaignGameStarter)
        {
            ((YnEdrylKoiranBank)_bank).BlessingAmount += 1;
            Hero.MainHero.ChangeHeroGold(-YnEdrylKoiranBank.PricePerBlessing);
            PrestigiousBank.LogMessage("Bénédiction d'Isha augmentée.\nEntretien total par jour : " + ((YnEdrylKoiranBank)_bank).CalculateBlessingUpkeep());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_ishaBlessings", _cityID));
        }

        private void DecreaseBlessing(CampaignGameStarter CampaignGameStarter)
        {
            ((YnEdrylKoiranBank)_bank).BlessingAmount -= 1;
            PrestigiousBank.LogMessage("Bénédiction d'Isha diminuée.\nEntretien total par jour : " + ((YnEdrylKoiranBank)_bank).CalculateBlessingUpkeep());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_ishaBlessings", _cityID));
        }


        #endregion


    }
}
