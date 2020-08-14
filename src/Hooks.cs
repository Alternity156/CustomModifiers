using Harmony;
using System;
using MelonLoader;

namespace AudicaModding
{
    internal static class Hooks
    {
        [HarmonyPatch(typeof(MenuState), "SetState", new Type[] { typeof(MenuState.State) })]
        private static class PatchSetState
        {
            private static void Postfix(MenuState __instance, ref MenuState.State state)
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

        [HarmonyPatch(typeof(AudioDriver), "StartPlaying")]
        private static class PatchPlay
        {
            private static void Postfix(AudioDriver __instance)
            {
                AudicaMod.GetCues();
            }
        }

        [HarmonyPatch(typeof(Target), "OnHit")]
        private static class PatchOnTargetHit
        {
            private static void Postfix(Target __instance)
            {
                if (AudicaMod.TempoRampEnabled)
                {
                    AudicaMod.TempoRamp(__instance);
                }
            }
        }



    }
}