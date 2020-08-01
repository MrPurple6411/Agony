using Agony.Common.Reflection;
using HarmonyLib;

namespace Agony.Defabricator
{
    partial class Main
    {
        private static partial class GUIHandler
        {
            [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.OnSelect))]
            private static class uGUI_CraftingMenuOnSelectPatch
            {
                private static void Postfix(uGUI_CraftingMenu __instance) { OnCraftingMenuSelected(__instance); }
            }

            [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.OnDeselect))]
            private static class uGUI_CraftingMenuOnDeselectPatch
            {
                private static void Postfix(uGUI_CraftingMenu __instance) { OnCraftingMenuDeselected(__instance); }
            }

            [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.ActionAvailable))]
            private static class uGUI_CraftingMenuActionAvailablePatch
            {
                private static void Postfix(uGUI_CraftingMenu __instance, ref bool __result, uGUI_CraftingMenu.Node sender)
                {
                    if (!Active) return;
                    if (CurrentMenu != __instance) return;
                    if (sender.action != TreeAction.Craft) return;
                    __result &= !RecyclingData.IsBlackListed(sender.techType);
                }
            }

            [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.CreateIcon))]
            private static class CreateIconPatch
            {
                private static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftingMenu.Node node)
                {
                    if (!Active) return;
                    if (__instance != CurrentMenu) return;
                    GUIFormatter.PaintNodeColor(node);
                }
            }

            [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.UpdateIcons))]
            private static class uGUI_CraftingMenuUpdateIconsPatch
            {
                private static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftingMenu.Node node)
                {
                    if (!Active)
                        return;
                    if (__instance != CurrentMenu)
                        return;
                    GUIFormatter.SetNodeChroma(node, true);
                }
            }

            public static uGUI_CraftingMenu CurrentMenu { get; private set; }

            private static void OnCraftingMenuSelected(uGUI_CraftingMenu sender)
            {
                Deactivate();
                CurrentMenu = sender;
            }

            private static void OnCraftingMenuDeselected(uGUI_CraftingMenu sender)
            {
                Deactivate();
                CurrentMenu = null;
            }
        }
    }
}