using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.ScreenSystem;
using TaleWorlds.ObjectSystem;
using TaleWorlds.Localization;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Engine;
using TaleWorlds.CampaignSystem.Party;
using TOR_Core.Extensions;

namespace PrestigiousBank
{
    public class DrakenhofBankMenu : BankMenu
    {
        public bool isAttributeSelectedForCompagnion;

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int darkEnergySolde = ((DrakenhofBank)_bank).DarkEnergyAccountSolde;
            int darkEnergyInterests = ((DrakenhofBank)_bank).CalculateDarkEnergyInterests();

            //Dark Ritual Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_dark_ritual_enlightment",_cityID),
                "Renforcez votre âme grâce aux rituels noirs",
                null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            // Bank Menu -> Dark Ritual Menu
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu",_cityID), 
                String.Format("{0}_dark_ritual_enlightment", _cityID), "Accéder à la salle rituelle",
                a =>
                {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.Tooltip = ((DrakenhofBank)_bank).GetCustomerLevel() > 3 ? null : new TextObject("Niveau de client Platine requis", null);
                    a.IsEnabled = ((DrakenhofBank)_bank).GetCustomerLevel() > 3;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID)),
                isLeave: false, index: 2);
            RegisterDarkRitualMenuOptions(campaignGameStarter);

        }


/*
/**/
        #region Dark Ritual
        private void RegisterDarkRitualMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), 
                "Drakenhof_bank_prestigious_account_focus", 
                "[+1 Point de Focus] Rituel de renforcement de l'âme\n["+DrakenhofBank.PricePerFocusPoint+"{GOLD_ICON}]",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.IsEnabled = IsAbleToDeposit(DrakenhofBank.PricePerFocusPoint);
                    a.Tooltip = IsAbleToDeposit(DrakenhofBank.PricePerFocusPoint) ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => PerformeDarkRitualFocus(),
                isLeave: false,
                index: 1, isRepeatable: true);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID),
                    "Drakenhof_bank_prestigious_account_attribute",
                    "[+1 Point d'Attribut] Rituel de renforcement du corps\n["+DrakenhofBank.PricePerAttributePoint+"{GOLD_ICON}]",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                        a.IsEnabled = Hero.MainHero.Gold >= DrakenhofBank.PricePerAttributePoint;
                        a.Tooltip = IsAbleToDeposit(DrakenhofBank.PricePerAttributePoint) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => PerformeDarkRitualAttribute(),
                    isLeave: false,
                    index: 2, isRepeatable: true);

            //Empty Space
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 3);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID),
                "Drakenhof_bank_prestigious_account_focus",
                "[+1 Point de Focus] Rituel de renforcement de l'âme pour un compagnion\n[" + DrakenhofBank.PricePerCompanionFocusPoint + "{GOLD_ICON}]",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.IsEnabled = IsAbleToDeposit(DrakenhofBank.PricePerCompanionFocusPoint);
                    a.Tooltip = IsAbleToDeposit(DrakenhofBank.PricePerCompanionFocusPoint) ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => {
                    isAttributeSelectedForCompagnion = false;
                    DarkRitualChooseCompagnionOptions(); },
                isLeave: false,
                index: 4, isRepeatable: true);

            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID),
                "Drakenhof_bank_prestigious_account_attribute_compagnion",
                "[+1 Point d'Attribut] Rituel de renforcement du corps pour un compagnion\n[" + DrakenhofBank.PricePerCompanionAttributePoint + "{GOLD_ICON}]",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.Devastate;
                    a.IsEnabled = Hero.MainHero.Gold >= DrakenhofBank.PricePerCompanionAttributePoint && _bank.GetCustomerLevel()>=5;
                    if (_bank.GetCustomerLevel() < 5) a.Tooltip = new TextObject("Niveau de client Diamant requis", null);
                    else if (IsAbleToDeposit(DrakenhofBank.PricePerCompanionAttributePoint)) a.Tooltip = new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => {

                    isAttributeSelectedForCompagnion = true;
                    DarkRitualChooseCompagnionOptions(); },
                isLeave: false,
                index: 5, isRepeatable: true);


            //Empty Spaces
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 6);
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), "emptySpace", "", a => { a.IsEnabled = false; return true; }, null, isLeave: false, index: 7);

            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_dark_ritual_enlightment", _cityID), String.Format("{0}_dark_ritual_enlightment_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        public void DarkRitualChooseCompagnionOptions()
        {
            List<Hero> compagnions = MobileParty.MainParty.GetMemberHeroes(); //Clan.PlayerClan.Companions.ToList();
            compagnions.Remove(Hero.MainHero);
            List<InquiryElement> list = new List<InquiryElement>();
            string CompagnionText;
            string compagnionFirstName;

            foreach (Hero hero in compagnions)
            {
                compagnionFirstName = hero.FirstName.Value;
                if (compagnionFirstName.Contains("}")) compagnionFirstName = compagnionFirstName.Split('}')[1];

                CompagnionText = hero.Name.Value;
                if (CompagnionText.Count(f=> f=='}') >= 1) CompagnionText = CompagnionText.Split('}')[CompagnionText.Count(f => f == '}')];

                CompagnionText = compagnionFirstName + CompagnionText;
                list.Add(new InquiryElement(hero, CompagnionText, null, true, null));
            }
            var inquirydata = new MultiSelectionInquiryData("Compagnions", "Choisissez un Compagnion", list, true, 1, 1, "Confirm", "Cancel", SelectedDarkRitualCompanion, null, "", true);
            MBInformationManager.ShowMultiSelectionInquiry(inquirydata, true);
        }


        public void PerformeDarkRitualFocus()
        {
            Hero.MainHero.HeroDeveloper.UnspentFocusPoints += 1;
            Hero.MainHero.ChangeHeroGold(-DrakenhofBank.PricePerFocusPoint);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/levelup"));
            GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID));
        }
        public void PerformeDarkRitualAttribute()
        {
            Hero.MainHero.HeroDeveloper.UnspentAttributePoints += 1;
            Hero.MainHero.ChangeHeroGold(-DrakenhofBank.PricePerAttributePoint);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/levelup"));
            GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID));
        }

        public void SelectedDarkRitualCompanion(List<InquiryElement> inquiryElements)
        {
            Hero hero = (Hero)inquiryElements[0].Identifier;
            if (isAttributeSelectedForCompagnion)
            {
                hero.HeroDeveloper.UnspentAttributePoints += 1;
                Hero.MainHero.ChangeHeroGold(-DrakenhofBank.PricePerCompanionAttributePoint);
            }
            else
            {
                hero.HeroDeveloper.UnspentFocusPoints += 1;
                Hero.MainHero.ChangeHeroGold(-DrakenhofBank.PricePerCompanionFocusPoint);
            }
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/levelup"));
            GameMenu.SwitchToMenu(String.Format("{0}_dark_ritual_enlightment", _cityID));
        }
        #endregion

    }
}
