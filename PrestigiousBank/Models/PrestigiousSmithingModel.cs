using PrestigiousBank;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TOR_Core.Models;


namespace PrestigiousBank
{
    public class PrestigiousSmithingModel : SmithingModel
    {
        private SmithingModel _previousModel;
        public PrestigiousSmithingModel(SmithingModel previousModel)
        {
            _previousModel = previousModel;
            if (previousModel == null) _previousModel = new DefaultSmithingModel();
        }

        public override int CalculateWeaponDesignDifficulty(WeaponDesign weaponDesign)
        {
            return _previousModel.CalculateWeaponDesignDifficulty(weaponDesign);
        }

        public override ItemModifier GetCraftedWeaponModifier(WeaponDesign weaponDesign, Hero weaponsmith)
        {
            return _previousModel.GetCraftedWeaponModifier(weaponDesign, weaponsmith);
        }

        public override ItemObject GetCraftingMaterialItem(CraftingMaterials craftingMaterial)
        {
            return _previousModel.GetCraftingMaterialItem(craftingMaterial);
        }

        public override int GetCraftingPartDifficulty(CraftingPiece craftingPiece)
        {
            return _previousModel.GetCraftingPartDifficulty(craftingPiece);
        }

        public override int GetEnergyCostForRefining(ref Crafting.RefiningFormula refineFormula, Hero hero)
        {
            return _previousModel.GetEnergyCostForRefining(ref refineFormula, hero);
        }

        public override int GetEnergyCostForSmelting(ItemObject item, Hero hero)
        {
            return _previousModel.GetEnergyCostForSmelting(item, hero);
        }

        public override int GetEnergyCostForSmithing(ItemObject item, Hero hero)
        {
            return _previousModel.GetEnergyCostForSmithing(item, hero);
        }

        public override int GetPartResearchGainForSmeltingItem(ItemObject item, Hero hero)
        {
            int result = _previousModel.GetPartResearchGainForSmeltingItem(item, hero);

            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();

            if (KarakIzorBankCampaignBehavior != null)
            {
                result = result + MathF.Floor(result * (KarakIzorBankCampaignBehavior.BankInstance.ResearchPartFactorBought * KarakIzorBank.ResearchFactorGainPerPurchaseBought));
            }

            return result;
        }

        public override int GetPartResearchGainForSmithingItem(ItemObject item, Hero hero, bool isFreeBuild)
        {
            int result = _previousModel.GetPartResearchGainForSmithingItem(item, hero, isFreeBuild);

            KarakIzorBankCampaignBehavior KarakIzorBankCampaignBehavior = Campaign.Current?.GetCampaignBehavior<KarakIzorBankCampaignBehavior>();

            if (KarakIzorBankCampaignBehavior != null)
            {
                result = result + MathF.Floor(result * (KarakIzorBankCampaignBehavior.BankInstance.ResearchPartFactorBought * KarakIzorBank.ResearchFactorGainPerPurchaseBought));
            }

            return result;
        }

        public override IEnumerable<Crafting.RefiningFormula> GetRefiningFormulas(Hero weaponsmith)
        {
            return _previousModel.GetRefiningFormulas(weaponsmith);
        }

        public override int GetSkillXpForRefining(ref Crafting.RefiningFormula refineFormula)
        {
            return _previousModel.GetSkillXpForRefining(ref refineFormula);
        }

        public override int GetSkillXpForSmelting(ItemObject item)
        {
            return _previousModel.GetSkillXpForSmelting(item);
        }

        public override int GetSkillXpForSmithingInCraftingOrderMode(ItemObject item)
        {
            return _previousModel.GetSkillXpForSmithingInCraftingOrderMode(item);
        }

        public override int GetSkillXpForSmithingInFreeBuildMode(ItemObject item)
        {
            return _previousModel.GetSkillXpForSmithingInFreeBuildMode(item);
        }

        public override int[] GetSmeltingOutputForItem(ItemObject item)
        {
            return _previousModel.GetSmeltingOutputForItem(item);
        }

        public override int[] GetSmithingCostsForWeaponDesign(WeaponDesign weaponDesign)
        {
            return _previousModel.GetSmithingCostsForWeaponDesign(weaponDesign);
        }

        public override float ResearchPointsNeedForNewPart(int totalPartCount, int openedPartCount)
        {
            return _previousModel.ResearchPointsNeedForNewPart(totalPartCount, openedPartCount);
        }
    }
}