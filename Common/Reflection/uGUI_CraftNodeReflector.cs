using System.Reflection;
using System;
using System.Collections.Generic;

namespace Agony.Common.Reflection
{
    public static class uGUI_CraftNodeReflector
    {
        private static readonly PropertyInfo actionpropertyInfo = typeof(uGUI_CraftingMenu.Node).GetProperty("action", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo expandedpropertyInfo = typeof(uGUI_CraftingMenu.Node).GetProperty("expanded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo techTypePropertyInfo = typeof(uGUI_CraftingMenu.Node).GetProperty("techType", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly PropertyInfo iconPropertyInfo = typeof(uGUI_CraftingMenu.Node).GetProperty("icon", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo GetEnumeratorMethodInfo = typeof(uGUI_CraftingMenu.Node).GetMethod("GetEnumerator", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool GetExpanded(uGUI_CraftingMenu.Node node)
        {
            if (node == null) throw new ArgumentNullException("node is null");
            return (bool)expandedpropertyInfo.GetValue(node, null);
        }

        public static TechType GetTechType(uGUI_CraftingMenu.Node node)
        {
            if (node == null) throw new ArgumentNullException("node is null");
            return (TechType)techTypePropertyInfo.GetValue(node, null);
        }
        public static uGUI_ItemIcon GetIcon(uGUI_CraftingMenu.Node node)
        {
            if (node == null) throw new ArgumentNullException("node is null");
            return (uGUI_ItemIcon)iconPropertyInfo.GetValue(node, null);
        }

        public static TreeAction GetAction(uGUI_CraftingMenu.Node node)
        {
            if (node == null) throw new ArgumentNullException("node is null");
            return (TreeAction)actionpropertyInfo.GetValue(node, null);
        }

        public static IEnumerator<uGUI_CraftingMenu.Node> GetEnumerator(uGUI_CraftingMenu.Node node)
        {
            if (node == null) throw new ArgumentNullException("node is null");
            return (IEnumerator<uGUI_CraftingMenu.Node>)GetEnumeratorMethodInfo.Invoke(node, null);
        }
    }
}