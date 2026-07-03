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
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ObjectSystem;
using TaleWorlds.ScreenSystem;
using TOR_Core.CampaignMechanics.CustomResources;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class AverheimBankMenu : BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);

            //Sigmar Blessings
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_sigmarBlessings", _cityID),
                "La bénédiction de Sigmar permet de renforcer son corps\nCette bénédiction a cependant un certain prix\nPuissance de la bénédiction : " +
                ((AverheimBank)_bank).BlessingAmount,
                null,
                GameMenu.MenuOverlayType.SettlementWithCharacters);
            GameTexts.SetVariable("AVERHEIM_EXCELLENCE_PRICE", ((AverheimBank)_bank).CalculatePriceAdditionnalMaxSkillIncrease());
        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            //Bank Menu -> Sigmar Blessings
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_sigmarBlessings", _cityID),
                "Sigmar's blessings",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    a.Tooltip = _bank.GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = _bank.GetCustomerLevel() > 1;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_sigmarBlessings", _cityID)),
                isLeave: false, index: 3);
            RegisterSigmarBlessingMenuOptions(campaignGameStarter);

            //EmptySapces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false);


            //Sigmar Excellence training
            GameTexts.SetVariable("AVERHEIM_EXCELLENCE_PRICE", ((AverheimBank)_bank).CalculatePriceAdditionnalMaxSkillIncrease());
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_sigmarBlessings", _cityID),
                "[{AVERHEIM_EXCELLENCE_PRICE}{GOLD_ICON}] Sigmar Excellence training",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    if (_bank.GetCustomerLevel() < 4)
                    {
                        a.Tooltip = new TextObject("Mythril customer level required", null);
                        a.IsEnabled = false;
                    }
                    else if (Hero.MainHero.Gold < ((AverheimBank)_bank).CalculatePriceAdditionnalMaxSkillIncrease())
                    {
                        a.Tooltip = new TextObject("Not enough {GOLD_ICON}", null);
                        a.IsEnabled = false;
                    }
                    else
                    {
                        a.Tooltip = new TextObject(((AverheimBank)_bank).CalculatePriceAdditionnalMaxSkillIncrease() + "{GOLD_ICON}", null);
                        a.IsEnabled = true;
                    }
                    return true;
                },
                _ =>
                {
                    Hero.MainHero.ChangeHeroGold(-((AverheimBank)_bank).CalculatePriceAdditionnalMaxSkillIncrease());
                    ((AverheimBank)_bank).MaxSkillIncreaseBought += 1;
                    CreateOrUpdateGameMenuDesc(campaignGameStarter);
                    GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID));
                },
                isLeave: false);

        }

        private void RegisterSigmarBlessingMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int priceNewBlessing = ((AverheimBank)_bank).CalculatePriceAdditionnalHP();
            int BlessingAmount = ((AverheimBank)_bank).BlessingAmount;
            //IncreaseBlessing
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_sigmarBlessings", _cityID), String.Format("{0}_bank_sigmarBlessings_increase", _cityID),
                "Augmenter la bénédiction",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.DefendAction;
                    a.IsEnabled = Hero.MainHero.Gold >= ((AverheimBank)_bank).CalculatePriceAdditionnalHP();
                    if (Hero.MainHero.Gold < priceNewBlessing) a.Tooltip = new TextObject("Pas assez d'argent", null);
                    else a.Tooltip = new TextObject(((AverheimBank)_bank).CalculatePriceAdditionnalHP() + "{GOLD_ICON}", null);
                    return true;
                },
                _ => IncreaseBlessing(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);



            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_sigmarBlessings", _cityID), String.Format("{0}_bank_sigmarBlessings_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 9, isRepeatable: false);
        }

        private void IncreaseBlessing(CampaignGameStarter CampaignGameStarter)
        {
            ((AverheimBank)_bank).BlessingAmount += 1;
            Hero.MainHero.ChangeHeroGold(-((AverheimBank)_bank).CalculatePriceAdditionnalHP());
            PrestigiousBank.LogMessage("Bénédiction de Sigmar augmentée.");
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_sigmarBlessings", _cityID));
        }


    }
}
