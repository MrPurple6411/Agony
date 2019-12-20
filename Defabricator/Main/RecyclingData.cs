using System.Collections.Generic;
using Agony.AssetTools.Wrappers;
using Agony.Common;
using UWE;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using System;

namespace Agony.Defabricator
{
    partial class Main
    {
        private static class RecyclingData
        {
            private static readonly HashSet<TechType> blacklist = new HashSet<TechType>(); 
            private static readonly Dictionary<TechType, TechType> cache = new Dictionary<TechType, TechType>();
            private static readonly TechType techBase = (TechType)(int)-1.000e+6;
            private static readonly string nonRecyclableText = "<color=#FF3030FF>Non-recyclable</color>";
            private static readonly string nonRecyclableTextID = "Agony_Defabricator_NonRecyclable";
            private static readonly string nonRecyclableTooltip = "Unfortunately there are no techniques that could be used in order to recycle this item.";
            private static readonly string nonRecyclableTooltipID = "Agony_Defabricator_NonRecyclable_Tooltip";
            private static readonly string prefabIDPrefix = "Agony-Defabricator-RecyclingPrefab-";
            private static readonly string recycleTextID = "Agony_Defabricator_Recycling";
            private static readonly string recycleText = "<color=#00FA00FF>Recycle:</color> {0}";
            private static readonly string recycleTooltipID = "Agony_Defabricator_Recycling_Tooltip";
            private static readonly string recycleTooltip = "Scrap for {0}.";
            private static readonly string techKeyPrefix = "AgonyDefabricatorRecycling";

            static RecyclingData()
            {
                LanguageWrapper.SetDefault(recycleTextID, recycleText);
                LanguageWrapper.SetDefault(recycleTooltipID, recycleTooltip);
                LanguageWrapper.SetDefault(nonRecyclableTextID, nonRecyclableText);
                LanguageWrapper.SetDefault(nonRecyclableTooltipID, nonRecyclableTooltip);
            }

            public static bool IsBlackListed(TechType recyclingTech) => blacklist.Contains(recyclingTech);

            public static bool TryGet(TechType originTech, out TechType recyclingTech)
            {
                recyclingTech = TechType.None;
                if (originTech == TechType.None || (int)originTech <= (int)techBase) { return false; }
                if (cache.TryGetValue(originTech, out recyclingTech)) { return true; }


                if (!TechData.TryGetValue(originTech, out JsonValue originJData))
                {
                    Logger.Error($"Failed to load TechData for TechType '{originTech}'.");
                    return false;
                }

                recyclingTech = techBase - cache.Count;
                cache[originTech] = recyclingTech;
                TechTypeExtensionsWrapper.Link(recyclingTech, techKeyPrefix + recyclingTech);
                if (Config.IsBlacklisted(originTech)) { blacklist.Add(recyclingTech); }
                KnownTechWrapper.AddDefault(recyclingTech);
                LoadRecyclingData(originTech, recyclingTech);
                LoadRecyclingPrefab(originTech, recyclingTech);
                LoadRecyclingText(originTech, recyclingTech);
                LoadRecyclingTooltip(recyclingTech);

                return true;
            }

            

            private static void LoadRecyclingData(TechType originTech, TechType recyclingTech)
            {                if (IsBlackListed(recyclingTech))
                {                    CraftDataHandler.SetTechData(recyclingTech, new RecipeData() {craftAmount = 0 });
                    return;
                }                                var ingredients = new Dictionary<TechType, int>();                if (TechData.GetCraftAmount(originTech) > 0)
                {                    ingredients[originTech] = TechData.GetCraftAmount(originTech); 
                }
                if (TechData.GetLinkedItems(originTech)!= null)
                {
                    for (var i = 0; i < TechData.GetLinkedItems(originTech).Count; i++)
                    {
                        TechType item = TechData.GetLinkedItems(originTech)[i];
                        ingredients[item] = ingredients.ContainsKey(item) ? (ingredients[item] + 1) : 1;
                    }
                }                var resIngs = new List<Ingredient>();
                ingredients.ForEach(x => resIngs.Add(new Ingredient(x.Key, x.Value)));

                var linkedItems = new List<TechType>();
                var isTool = IsPlayerToolWithEnergyMixin(originTech);
                for(var i = 0; i < TechData.GetIngredients(originTech).Count; i++)
                {
                    var ing = TechData.GetIngredients(originTech)[i];
                    if (isTool && IsBattery(ing.techType)) { continue; }
                    var amount = UnityEngine.Mathf.FloorToInt(ing.amount * Config.GetYield(ing.techType));
                    for(int j = 0; j < amount; j++) { linkedItems.Add(ing.techType); }
                }
                RecipeData recipeData = new RecipeData() { craftAmount = 0, Ingredients = resIngs, LinkedItems = linkedItems };
                CraftDataHandler.SetTechData(recyclingTech, recipeData);
                TechData.Cache();
            }

            private static bool IsPlayerToolWithEnergyMixin(TechType techType)
            {
                return TechUtil.TechTypePrefabContains<PlayerTool>(techType) && TechUtil.TechTypePrefabContains<EnergyMixin>(techType);
            }

            private static bool IsBattery(TechType techType)
            {
                return TechUtil.TechTypePrefabContains<Battery>(techType);
            }

            private static void LoadRecyclingPrefab(TechType originTech, TechType recyclingTech)
            {
                string originPrefab, originFile;
                var prefabID = prefabIDPrefix + (int)recyclingTech;
                CraftDataWrapper.SetTechPrefab(recyclingTech, prefabID);
                PrefabDatabaseWrapper.PreparePrefabDatabase();
                if (CraftDataWrapper.TryGetTechPrefab(originTech, out originPrefab) && PrefabDatabase.TryGetPrefabFilename(originPrefab, out originFile))
                {
                    PrefabDatabaseWrapper.SetPrefabFile(prefabID, originFile);
                }
                else
                {
                    Logger.Warning($"Failed to load prefabID or fileName for TechType '{originTech}'.");
                }
            }

            private static void LoadRecyclingText(TechType originTech, TechType recyclingTech)
            {
                var lang = Language.main;
                if (lang == null) return;

                if (IsBlackListed(recyclingTech))
                {
                    var translation1 = lang.Get(nonRecyclableTextID);
                    LanguageWrapper.SetDefault(recyclingTech.AsString(), translation1);
                    return;
                }

                var techName = lang.Get(originTech.AsString());
                var translation = lang.Get(recycleTextID);
                var formated = StringUtil.FormatWithFallback(translation, recycleText, techName);
                LanguageWrapper.SetDefault(recyclingTech.AsString(), formated);
            }

            private static void LoadRecyclingTooltip(TechType recyclingTech)
            {
                TooltipFactoryWrapper.RegisterTech(recyclingTech);
                var lang = Language.main;
                if (lang == null) return;

                if (IsBlackListed(recyclingTech))
                {
                    var errorText = lang.Get(nonRecyclableTooltipID);                    LanguageWrapper.SetDefault("Tooltip_" + recyclingTech.AsString(), errorText);                    return;
                }

                if (!TechData.TryGetValue(recyclingTech, out JsonValue data)) return;
                var ings = new Dictionary<TechType, int>();
                if (TechData.TryGetValue(recyclingTech, out JsonValue recyclingTechData))
                {
                    if (TechData.GetLinkedItems(recyclingTech) != null)
                    {
                        ICollection<TechType> items = TechData.GetLinkedItems(recyclingTech);
                        if (items.Count > 0)
                        {
                            foreach (TechType techType in items)
                            {
                                var item = techType;
                                ings[item] = ings.ContainsKey(item) ? (ings[item] + 1) : 1;
                            }
                        }
                    }
                }
                var builder = new System.Text.StringBuilder();
                foreach(var ing in ings)
                {
                    builder.Append(lang.Get(ing.Key.AsString()));
                    if (ing.Value > 1)
                    {
                        builder.Append(" (x");
                        builder.Append(ing.Value);
                        builder.Append(')');
                    }
                    builder.Append(", ");
                }

                if (builder.Length >= 2) { builder.Length -= 2;
                }
                var ingList = builder.ToString();
                var tooltip = lang.Get(recycleTooltipID); 
                var formated = StringUtil.FormatWithFallback(tooltip, recycleTooltip, ingList);              
                LanguageWrapper.SetDefault("Tooltip_" + recyclingTech.AsString(), formated);
            }
        }
    }
}