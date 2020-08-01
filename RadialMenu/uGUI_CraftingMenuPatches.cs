using Agony.Common.Reflection;
using UnityEngine;
using HarmonyLib;

namespace Agony.RadialTabs
{
    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.Action))]
    internal static class uGUI_CraftingMenuActionPatch
    {
        private static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftingMenu.Node sender)
        {
            var client = __instance._client;
            var interactable = __instance.interactable;
            if (client == null || !interactable || !__instance.ActionAvailable(sender))
            {
                if (sender.icon == null) { return; }
                var duration = 1 + Random.Range(-0.2f, 0.2f);
                sender.icon.PunchScale(5, 0.5f, duration, 0);
            }
        }
    }
    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.CreateIcon))]
    internal static class CreateIconPatch
    {
        private static void Postfix(uGUI_CraftingMenu.Node node, RectTransform canvas)
        {
            var grid = RadialCell.Create(node);
            var icon = node.icon;
            var size = new Vector2(grid.size, grid.size);
            icon.SetBackgroundSize(size);
            icon.SetActiveSize(size);
            var foregroundSize = grid.size * (float)Config.IconForegroundSizeMult;
            icon.SetForegroundSize(foregroundSize, foregroundSize, true);
            icon.SetBackgroundRadius(grid.size / 2);
            icon.rectTransform.SetParent(canvas);
            icon.SetPosition(grid.parent.Position);
        }
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.Expand))]
    internal static class ExpandPatch
    {
        private static void Postfix(uGUI_CraftingMenu __instance, uGUI_CraftingMenu.Node node)
        {
            if (node.icon == null)
                return;
            var grid = RadialCell.Create(node);
            var pos = node.icon.IsActive() ? grid.Position : grid.parent.Position;
            var speed = (grid.radius + grid.size) * (float)Config.AnimationSpeedMult;
            var fadeDistance = grid.size * (float)Config.AnimationFadeDistanceMult;
            var anim = new IconMovingAnimation(speed, fadeDistance, pos);
            anim.Play(node.icon);
        }
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.GetIconMetrics))]
    internal static class GetIconMetricsPatch
    {
        public static uGUI_CraftingMenu.Node Node;
        public static int Index;
        public static int Siblings;

        private static void Postfix(uGUI_CraftingMenu.Node node, int index, int siblings)
        {
            Node = node;
            Index = index;
            Siblings = siblings;
        }
    }

    [HarmonyPatch(typeof(uGUI_CraftingMenu), nameof(uGUI_CraftingMenu.Punch))]
    internal static class PunchPatch
    {
        private static bool Prefix()
        {
            return false;
        }
    }
}