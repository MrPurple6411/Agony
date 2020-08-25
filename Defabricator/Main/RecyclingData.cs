using System;
using System.Collections.Generic;
using Agony.AssetTools.Wrappers;
using Agony.Common;
using Newtonsoft.Json;
using QModManager.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;
using UWE;
#if SUBNAUTICA
using Data = SMLHelper.V2.Crafting.TechData;
#elif BELOWZERO
using Data = SMLHelper.V2.Crafting.RecipeData;
#endif


namespace Agony.Defabricator
{
    partial class Main
    {
        private static class RecyclingData
        {
            private static readonly HashSet<TechType> blacklist = new HashSet<TechType>(); 
            private static readonly Dictionary<TechType, TechType> cache = new Dictionary<TechType, TechType>();
            private static readonly string nonRecyclableText = "<color=#FF3030FF>Non-recyclable:</color>  {0}";
            private static readonly string nonRecyclableTooltip = "Unfortunately there are no techniques that could be used in order to recycle {0}.";
            private static readonly string prefabIDPrefix = "Defabricator-Prefab-";
            private static readonly string recycleText = "<color=#00FA00FF>Recycle:</color> {0}";
            private static readonly string recycleTooltip = "Scrap for {0}.";

            public static bool IsBlackListed(TechType recyclingTech) => blacklist.Contains(recyclingTech);

            public static bool TryGet(TechType originTech, out TechType recyclingTech)
            {
                recyclingTech = TechType.None;
                if (originTech == TechType.None) { return false; }
                if (cache.TryGetValue(originTech, out recyclingTech)) { return true; }

#if SUBNAUTICA
                var originData = CraftData.Get(originTech, true);
#elif BELOWZERO
                var originData = CraftDataHandler.GetRecipeData(originTech);
#endif
                if (originData == null || !CraftDataWrapper.TryGetTechPrefab(originTech, out string originPrefab) || !PrefabDatabase.TryGetPrefabFilename(originPrefab, out string originFile))
                {
                    return false;
                }

                recyclingTech = TechTypeHandler.AddTechType($"Defabricated{originTech}", "", "");
                cache[originTech] = recyclingTech;
                if (Config.IsBlacklisted(originTech)) { blacklist.Add(recyclingTech); }
                KnownTechHandler.UnlockOnStart(recyclingTech);
                LoadRecyclingData(originTech, recyclingTech);
                LoadRecyclingSprite(originTech, recyclingTech);
                LoadRecyclingPrefab(originTech, recyclingTech);
                LoadRecyclingText(originTech, recyclingTech);
                LoadRecyclingTooltip(recyclingTech);

                return true;
            }

            

            private static void LoadRecyclingData(TechType originTech, TechType recyclingTech)
            {
                if (IsBlackListed(recyclingTech))
                {
                    CraftDataHandler.SetTechData(recyclingTech, new Data(new List<Ingredient>()));
                    return;
                }

#if SUBNAUTICA
                var originData = CraftData.Get(originTech, true);
#elif BELOWZERO
                var originData = CraftDataHandler.GetRecipeData(originTech);
#endif
                var ingredients = new Dictionary<TechType, int>();
                if (originData.craftAmount > 0) { ingredients[originTech] = originData.craftAmount; }
                for (var i = 0; i < originData.linkedItemCount; i++)
                {
                    var item = originData.GetLinkedItem(i);
                    ingredients[item] = ingredients.ContainsKey(item) ? (ingredients[item] + 1) : 1;
                }
                var resIngs = new List<Ingredient>();
                ingredients.ForEach(x => resIngs.Add(new Ingredient(x.Key, x.Value)));

                var linkedItems = new List<TechType>();
                var isTool = IsPlayerToolWithEnergyMixin(originTech);
                for(var i = 0; i < originData.ingredientCount; i++)
                {
                    var ing = originData.GetIngredient(i);
                    if (isTool && IsBattery(ing.techType)) { continue; }
                    var amount = UnityEngine.Mathf.FloorToInt(ing.amount * Config.GetYield(ing.techType));
                    for(var j = 0; j < amount; j++) { linkedItems.Add(ing.techType); }
                }
                Data Data = new Data() { craftAmount = 0, Ingredients = resIngs, LinkedItems = linkedItems };
                CraftDataHandler.SetTechData(recyclingTech, Data);
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
                    Logger.Log(Logger.Level.Warn, $"Failed to load prefabID or fileName for TechType '{originTech}'.");
                }
            }

            private static void LoadRecyclingSprite(TechType originTech, TechType recyclingTech)
            {
                SpriteHandler.RegisterSprite(recyclingTech, SpriteManager.Get(originTech));
            }

            private static void LoadRecyclingText(TechType originTech, TechType recyclingTech)
            {
                if (IsBlackListed(recyclingTech))
                {
                    var translation1 = string.Format(nonRecyclableText, originTech.AsString());
                    LanguageHandler.SetTechTypeName(recyclingTech, translation1);
                    return;
                }

                var formated = string.Format(recycleText, Language.main.Get(originTech.AsString()));
                LanguageHandler.SetTechTypeName(recyclingTech, formated);
            }

            private static void LoadRecyclingTooltip(TechType recyclingTech)
            {
                var lang = Language.main;
                if (lang == null) return;

                if (IsBlackListed(recyclingTech))
                {
                    var errorText = string.Format(nonRecyclableTooltip, recyclingTech.AsString().Replace("Defabricated", ""));
                    LanguageHandler.SetTechTypeTooltip(recyclingTech, errorText);
                    return;
                }

#if SUBNAUTICA
                var data = CraftData.Get(recyclingTech);
#elif BELOWZERO
                var data = CraftDataHandler.GetRecipeData(recyclingTech);
#endif
                if (data == null) return;
                var ings = new Dictionary<TechType, int>();
                for(var i = 0; i < data.linkedItemCount; i++)
                {
                    var item = data.GetLinkedItem(i);
                    ings[item] = ings.ContainsKey(item) ? (ings[item] + 1) : 1;
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
                if (builder.Length >= 2) { builder.Length -= 2; }
                var ingList = builder.ToString();

                var formated = string.Format(recycleTooltip, ingList);
                LanguageHandler.SetTechTypeTooltip(recyclingTech, formated);
            }
        }
    }
}