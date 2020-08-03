using System.Collections.Generic;
using Agony.Common.Reflection;
using System;
using QModManager.Utility;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Handlers;

namespace Agony.Defabricator
{
    internal static partial class Main
    {
        public static bool Active { get; private set; }
        private static Dictionary<uGUI_CraftingMenu.Node, TechType> replacedNodeTechs = new Dictionary<uGUI_CraftingMenu.Node, TechType>();

        public static void Patch() { KeyInputHandler.Patch(); }

        public static bool IsCurrentCrafter(Crafter crafter)
        {
            if (!Active) return false;
            if (!crafter) return false;
            return crafter as ITreeActionReceiver == GUIHandler.CurrentMenu.client;
        }

        private static void Activate()
        {
            if (Active) return;
            if (!GUIHandler.CurrentMenu) return;
            Active = true;

            int c = 0, n = 0;
            var menuRoot = GUIHandler.CurrentMenu.icons;

            menuRoot.ForEach(j => {
                ForeachChildRecursively(j.Value, x => ReplaceNodeTech(x, true));
                GUIHandler.CurrentMenu.UpdateNotifications(j.Value, ref c, ref n);
                ForeachChildRecursively(j.Value, x => GUIFormatter.PaintNodeColorAnimated(x));
            });
        }

        private static void Deactivate()
        {
            if (!Active) return;
            Active = false;

            int c = 0, n = 0;
            var menuRoot = GUIHandler.CurrentMenu.icons;

            menuRoot.ForEach(j => {
                replacedNodeTechs.ForEach(x => x.Key.techType = x.Value);
                replacedNodeTechs.Clear();
                ForeachChildRecursively(j.Value, x => ReplaceNodeTech(x, false));
                GUIHandler.CurrentMenu.UpdateNotifications(j.Value, ref c, ref n);
                ForeachChildRecursively(j.Value, x => GUIFormatter.RevertNodeColorAnimated(x));
            });
        }

        private static void ForeachChildRecursively(uGUI_CraftingMenu.Node node, Action<uGUI_CraftingMenu.Node> action)
        {
            if (node == null) return;
            foreach (var child in node)
            {
                action(child);
                ForeachChildRecursively(child, action);
            }
        }

        private static void ReplaceNodeTech(uGUI_CraftingMenu.Node node, bool activate)
        {
            if (node.action != TreeAction.Craft)
                return;

            if (!node.techType.ToString().StartsWith("Defabricated") && activate && RecyclingData.TryGet(node.techType, out TechType recyclingTech))
            {
                Logger.Log(Logger.Level.Info, $"", null, true);
                replacedNodeTechs[node] = node.techType;
                node.techType = recyclingTech;
            }
            else if (node.techType.ToString().StartsWith("Defabricated") && !activate)
            {
                Logger.Log(Logger.Level.Info, $"{node.techType} ", null, true);
                string techString = node.techType.ToString().Replace("Defabricated", "");
                if(Enum.TryParse(techString, out TechType techType))
                {
                    Logger.Log(Logger.Level.Info, $"", null, true);
                    replacedNodeTechs[node] = node.techType;
                    node.techType = techType;
                }
            }
        }
    }
}