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
using TOR_Core.CampaignMechanics.CustomResources;

namespace PrestigiousBank
{
    public class AltdorfBankMenu:BankMenu
    {

        public override void CreateOrUpdateGameMenuDesc(CampaignGameStarter campaignGameStarter)
        {
            base.CreateOrUpdateGameMenuDesc(campaignGameStarter);
            int prestigiousSolde = ((AltdorfBank) _bank).PrestigiousAccountSolde;
            int prestigiousInterests = ((AltdorfBank)_bank).CalculatePrestigiousInterests();
            int hiredChanneler = ((AltdorfBank)_bank).ChannelerNumber;
            int ChannelerCostPerDay = ((AltdorfBank)_bank).CalculateChannelerCostPerDay();

            //Prestigious Account Menu
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_prestigious_account",_cityID), 
                String.Format("Fortune investie : {0}\nScribes corrompus : {1}", prestigiousSolde, prestigiousInterests), 
                null, 
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

            //Services de Magie
            campaignGameStarter.AddGameMenu(String.Format("{0}_bank_magic_services", _cityID),
                "Canalysateurs embauchés : "+ hiredChanneler + CustomResourceManager.GetResourceObject("WindsOfMagic").GetCustomResourceIconAsText()+ 
                "\nCoût par jour : "+ChannelerCostPerDay,
                null,
                TaleWorlds.CampaignSystem.Overlay.GameOverlays.MenuOverlayType.SettlementWithCharacters);

        }

        public override void CallChildrenBankMenu(CampaignGameStarter campaignGameStarter, Bank Bank)
        {
            // Bank Menu -> Prestigious Account
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_prestigious_account", _cityID), 
                "Soutenir la bureaucratie",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                    a.Tooltip = ((AltdorfBank)_bank).GetCustomerLevel() > 1 ? null : new TextObject("Niveau de client Argent requis", null);
                    a.IsEnabled = ((AltdorfBank)_bank).GetCustomerLevel() > 1;
                    return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_prestigious_account", _cityID)),
                isLeave: false, index: 2);
            RegisterPrestigiousAccountMenuOptions(campaignGameStarter);

            //Bank Menu -> Services de magie
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_magic_services", _cityID), 
                "Services de Magie",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.Tooltip = ((AltdorfBank)_bank).GetCustomerLevel() > 3 ? null : new TextObject("Niveau de client Platine requis", null);
                    a.IsEnabled = ((AltdorfBank)_bank).GetCustomerLevel() > 3;
                    return true;
                },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_magic_services", _cityID)),
                isLeave: false, index: 3);
            RegisterMagicServicesMenuOptions(campaignGameStarter);

            //Bank Menu => Buy Teleport between Clan agencies
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_menu", _cityID), String.Format("{0}_bank_teleportService", _cityID),
                "["+AltdorfBank.PriceUnblockTeleport+"{GOLD_ICON}] Téléportation entre agences",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.SneakIn;
                    if (((AltdorfBank)_bank).GetCustomerLevel() <= 4) a.Tooltip = new TextObject("Niveau de client Diamant requis", null);
                    else if (Hero.MainHero.Gold < AltdorfBank.PriceUnblockTeleport) a.Tooltip = new TextObject("Pas assez d'or", null);
                    a.IsEnabled = ((AltdorfBank)_bank).GetCustomerLevel() > 4 && Hero.MainHero.Gold >= AltdorfBank.PriceUnblockTeleport;
                    return !((AltdorfBank)_bank).IsTeleportUnblocked; //Do not display it anymore if it is bought
                },
                _ =>
                {
                    Hero.MainHero.ChangeHeroGold(-AltdorfBank.PriceUnblockTeleport);
                    ((AltdorfBank)_bank).IsTeleportUnblocked = true;
                    GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID));
                    PrestigiousBank.LogMessage("Téléportation entre agences débloquée.\nAchetez une agence et construisez-y un téléporteur.");
                },
                isLeave: false, index: 4);
        }




        #region Prestigious Account
        private void RegisterPrestigiousAccountMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int[] qties = { 100, 1000, 10000, 100000 };
            int i = 0;
            foreach (int qty in qties)
            {
                campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_{1}",_cityID, qty),
                    "Offrir " + qty + " {GOLD_ICON}",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = IsAbleToDeposit(qty);
                        a.Tooltip = IsAbleToDeposit(qty) ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositPrestigiousAccount(qty, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
                i++;


            }

            //Deposit All
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_all", _cityID), "Tout offrir",
                    a => {
                        a.optionLeaveType = GameMenuOption.LeaveType.Bribe;
                        a.IsEnabled = Hero.MainHero.Gold > 0;
                        a.Tooltip = Hero.MainHero.Gold > 0 ? null : new TextObject("Pas assez d'argent", null);
                        return true;
                    },
                    _ => DepositPrestigiousAccount(Hero.MainHero.Gold, campaignGameStarter),
                    isLeave: false,
                    i, isRepeatable: true);
            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_prestigious_account", _cityID), String.Format("{0}_bank_prestigious_account_back", _cityID), "Retour",
                a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                isLeave: true, index: 999, isRepeatable: false);
        }

        private void DepositPrestigiousAccount(int amount, CampaignGameStarter campaignGameStarter)
        {
            if (Hero.MainHero.Gold < amount) { return; }

            AltdorfBankCampaignBehavior.BankAltdorf.PrestigiousAccountSolde += amount;
            Hero.MainHero.ChangeHeroGold(-amount);
            InformationManager.DisplayMessage(new InformationMessage(String.Format("Cadeau de {0} accepté.\nNombre de corrompus : {1}", amount, AltdorfBankCampaignBehavior.BankAltdorf.CalculatePrestigiousInterests()), Color.FromUint(0xFFBBAA00)));
            GameMenu.SwitchToMenu(String.Format("{0}_bank_prestigious_account", _cityID));
            CreateOrUpdateGameMenuDesc(campaignGameStarter);
            SoundEvent.PlaySound2D(SoundEvent.GetEventIdFromString("event:/ui/notification/coins_negative"));
        }
        #endregion

        #region Magic Services

        private void RegisterMagicServicesMenuOptions(CampaignGameStarter campaignGameStarter)
        {
            int pricePerChanneler = AltdorfBank.PricePerChanneler;
            int upkeepChanneler = AltdorfBank.UpkeepPerChanneler;

            //Hire Channeler
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_magic_services", _cityID), String.Format("{0}_bank_magic_services_hire", _cityID), 
                "["+pricePerChanneler+ " {GOLD_ICON}] Embaucher un canalysateur :\n+1 Max"+ CustomResourceManager.GetResourceObject("WindsOfMagic").GetCustomResourceIconAsText()+
                "\nCout : "+upkeepChanneler+" {GOLD_ICON}/jour",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.IsEnabled = Hero.MainHero.Gold >= pricePerChanneler;
                    a.Tooltip = Hero.MainHero.Gold >= pricePerChanneler ? null : new TextObject("Pas assez d'argent", null);
                    return true;
                },
                _ => HireChalleler(campaignGameStarter),
                isLeave: false,
                index: 1, isRepeatable: true);

            //Fire Channeler
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_magic_services", _cityID), String.Format("{0}_bank_magic_services_fire", _cityID),
                "Virer un canalysateur",
                a => {
                    a.optionLeaveType = GameMenuOption.LeaveType.ShowMercy;
                    a.IsEnabled = ((AltdorfBank)_bank).ChannelerNumber>0;
                    a.Tooltip = ((AltdorfBank)_bank).ChannelerNumber > 0 ? null : new TextObject("Personne à virer", null);
                    return true;
                },
                _ => FireChalleler(campaignGameStarter),
                isLeave: false,
                index: 2, isRepeatable: true);


            //Leave
            campaignGameStarter.AddGameMenuOption(String.Format("{0}_bank_magic_services", _cityID), String.Format("{0}_bank_magic_services_back", _cityID), "Retour",
                    a => { a.optionLeaveType = GameMenuOption.LeaveType.Leave; return true; },
                    _ => GameMenu.SwitchToMenu(String.Format("{0}_bank_menu", _cityID)),
                    isLeave: true, index: 9, isRepeatable: false);
        }

        private void HireChalleler(CampaignGameStarter CampaignGameStarter)
        {
            ((AltdorfBank)_bank).ChannelerNumber += 1;
            Hero.MainHero.ChangeHeroGold(-AltdorfBank.PricePerChanneler);
            PrestigiousBank.LogMessage("Canalysateur embauché.\nEntretien total par jour : "+ ((AltdorfBank)_bank).CalculateChannelerCostPerDay());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_magic_services", _cityID));
        }

        private void FireChalleler(CampaignGameStarter CampaignGameStarter)
        {
            ((AltdorfBank)_bank).ChannelerNumber -= 1;
            PrestigiousBank.LogMessage("Canalysateur viré.\nEntretien total par jour : " + ((AltdorfBank)_bank).CalculateChannelerCostPerDay());
            CreateOrUpdateGameMenuDesc(CampaignGameStarter);
            GameMenu.SwitchToMenu(String.Format("{0}_bank_magic_services", _cityID));
        }


        #endregion

    }
}
