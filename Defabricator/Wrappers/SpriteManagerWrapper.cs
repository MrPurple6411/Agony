﻿using System.Collections.Generic;
using Agony.Common.Reflection;
using Agony.Common;
using UnityEngine;
using System.IO;
using HarmonyLib;
using System;

namespace Agony.AssetTools.Wrappers
{
    public static partial class SpriteManagerWrapper
    {
        private static readonly Dictionary<SpriteManager.Group, Dictionary<string, Atlas.Sprite>> spriteDatabase = new Dictionary<SpriteManager.Group, Dictionary<string, Atlas.Sprite>>();

        [HarmonyPatch(typeof(SpriteManager), "GetWithNoDefault", new Type[] { typeof(SpriteManager.Group), typeof(string) })]
        private static class GetWithNoDefaultPatch
        {
            private static bool Prefix(ref Atlas.Sprite __result, SpriteManager.Group group, string name)
            {
                Dictionary<string, Atlas.Sprite> sprites;
                if (spriteDatabase.TryGetValue(group, out sprites))
                    if (sprites.TryGetValue(name, out __result))
                        return false;
                return true;
            }
        }

        static SpriteManagerWrapper()
        {
            var groups = Enum.GetValues(typeof(SpriteManager.Group));
            for(var i = 0; i < groups.Length; i++)
            {
                var sprites = new Dictionary<string, Atlas.Sprite>(StringComparer.InvariantCultureIgnoreCase);
                spriteDatabase.Add((SpriteManager.Group)i, sprites);
            }
        }

        public static void Set(SpriteManager.Group group, string name, Atlas.Sprite sprite)
        {
            if (!spriteDatabase.ContainsKey(group)) throw new ArgumentException("group is invalid");
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name is null or empty");
            if (group == SpriteManager.Group.Background) throw new NotSupportedException("can not edit backgrounds");

            spriteDatabase[group][name] = sprite ?? throw new ArgumentNullException("sprite is null");
        }

    }
}