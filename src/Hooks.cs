using Harmony;
using Steamworks;
using System;
using System.Reflection;

namespace AudicaModding
{
    internal static class Hooks
    {
        public static void ApplyHooks(HarmonyInstance instance)
        {
            instance.PatchAll(Assembly.GetExecutingAssembly());
        }

        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetState
        {
            private static void Postfix(MenuState __instance, MenuState.State state)
            {
                AudicaMod.oldMenuState = AudicaMod.menuState;
                AudicaMod.menuState = state;
                if (AudicaMod.oldMenuState == MenuState.State.LaunchPage && state == MenuState.State.Launching)
                {
                    AudicaMod.SetModifiersBefore();
                }
                else if (state != MenuState.State.Launched && AudicaMod.oldMenuState == MenuState.State.Launched)
                {
                    AudicaMod.SetModifiersAfter();
                }
            }
        }

        [HarmonyPatch(typeof(SongSelectItem), "OnSelect")]
        private static class PatchOnSelect
        {
            private static void Prefix(SongSelectItem __instance)
            {
                AudicaMod.selectedSong = __instance.mSongData.songID;
            }
        }

        [HarmonyPatch(typeof(GameplayModifiers), "OnTargetHit")]
        private static class PatchOnTargetHit
        {
            private static bool Prefix(GameplayModifiers __instance)
            {
                return false;
            }

            private static void Postfix(GameplayModifiers __instance)
            {
                if (AudicaMod.TempoRampEnabled)
                {
                    AudicaMod.TempoRamp(__instance);
                }
            }
        }



    }
}