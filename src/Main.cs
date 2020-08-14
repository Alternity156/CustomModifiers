using MelonLoader;
using UnityEngine;
using Harmony;
using TMPro;
using System;
using System.Collections;

namespace AudicaModding
{
    public class AudicaMod : MelonMod
    {
        public static class BuildInfo
        {
            public const string Name = "CustomModifiers";  // Name of the Mod.  (MUST BE SET)
            public const string Author = "Alternity"; // Author of the Mod.  (Set as null if none)
            public const string Company = null; // Company that made the Mod.  (Set as null if none)
            public const string Version = "0.2.0"; // Version of the Mod.  (MUST BE SET)
            public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
        }

        public static MenuState.State oldMenuState;
        public static MenuState.State menuState;

        public static Vector3 debugTextPos = new Vector3(0f, -1f, 5f);

        public static float oldAimAssist = 1.0f;
        public static float oldTargetSpeed = 1.0f;

        public static float customAimAssist = 0.5f;
        public static float customTempoRampEndSpeed = 1.5f;
        public static float customExtraParticlesScale = 1.0f;
        public static float customTargetSpeed = 1.2f;
        public static float customPsychedeliaSpeed = 1.0f;
        public static float customSongSpeed = 1.2f;

        public static bool AimAssistEnabled = false;
        public static bool TempoRampEnabled = false;
        public static bool ExtraParticlesEnabled = false;
        public static bool FastTargetsEnabled = false;
        public static bool PsychedeliaEnabled = false;
        public static bool FasterSongEnabled = false;

        public static string selectedSong;
        public static SongCues.Cue[] songCues;

        public static float psychedeliaTimer = 0.0f;
        public static float psychedeliaSpeed = 1.0f;
        public static float forcedPsychedeliaPhase = 0.0f;
        public static float defaultPsychedeliaPhaseSeconds = 14.28f;

        public static int TempoIncrementCount = 0;
        public static int TempoIncrementTotalTargets = 0;

        public static OptionsMenuButton testButton = null;

        private void CreateConfig()
        {
            MelonPrefs.RegisterFloat("CustomModifiers", "AimAssist", 0.5f);
            MelonPrefs.RegisterFloat("CustomModifiers", "TempoRampEndSpeed", 1.5f);
            MelonPrefs.RegisterFloat("CustomModifiers", "ExtraParticlesScale", 1.0f);
            MelonPrefs.RegisterFloat("CustomModifiers", "TargetSpeed", 1.2f);
            MelonPrefs.RegisterFloat("CustomModifiers", "PsychedeliaSpeed", 1.0f);
            MelonPrefs.RegisterFloat("CustomModifiers", "SongSpeed", 1.2f);
        }

        private void LoadConfig()
        {
            customAimAssist = MelonPrefs.GetFloat("CustomModifiers", "AimAssist");
            customTempoRampEndSpeed = MelonPrefs.GetFloat("CustomModifiers", "TempoRampEndSpeed");
            customExtraParticlesScale = MelonPrefs.GetFloat("CustomModifiers", "ExtraParticlesScale");
            customTargetSpeed = MelonPrefs.GetFloat("CustomModifiers", "TargetSpeed");
            customPsychedeliaSpeed = MelonPrefs.GetFloat("CustomModifiers", "PsychedeliaSpeed");

            //Support from 0.1.0 to 0.2.0

            if (!MelonPrefs.HasKey("CustomModifiers", "SongSpeed"))
            {
                MelonPrefs.RegisterFloat("CustomModifiers", "SongSpeed", 1.2f);
            }
            else
            {
                customSongSpeed = MelonPrefs.GetFloat("CustomModifiers", "SongSpeed");
            }

            //End of support

            if (customAimAssist > 1.0f)
            {
                customAimAssist = 1.0f;
            }
            else if (customAimAssist < 0.0f)
            {
                customAimAssist = 0.0f;
            }

            if (customTempoRampEndSpeed < 1.0f)
            {
                customTempoRampEndSpeed = 1.0f;
            }
        }

        private static void SaveConfig()
        {
            MelonPrefs.SetFloat("CustomModifiers", "AimAssist", customAimAssist);
            MelonPrefs.SetFloat("CustomModifiers", "TempoRampEndSpeed", customTempoRampEndSpeed);
            MelonPrefs.SetFloat("CustomModifiers", "ExtraParticlesScale", customExtraParticlesScale);
            MelonPrefs.SetFloat("CustomModifiers", "TargetSpeed", customTargetSpeed);
            MelonPrefs.SetFloat("CustomModifiers", "PsychedeliaSpeed", customPsychedeliaSpeed);
            MelonPrefs.SetFloat("CustomModifiers", "SongSpeed", customSongSpeed);
        }

        public static void SpawnText(string text)
        {
            KataConfig.I.CreateDebugText(text, debugTextPos, 5f, null, persistent: false, 0.2f);
        }

        public static void SetSongSpeed(float speed)
        {
            AudioDriver.I.SetSpeed(speed);
        }

        public static void SetAimAssist(float aimAssist)
        {
            PlayerPreferences.I.AimAssistAmount.Set(aimAssist);
        }

        public static void SetTargetSpeed(float speed)
        {
            PlayerPreferences.I.TargetSpeedMultiplier.Set(speed);
        }

        public static void SetParticleScale(float particleAmount)
        {
            for (int i = 0; i < songCues.Length; i++)
            {
                songCues[i].particleReductionScale = particleAmount;
            }
        }

        public static void GetCues()
        {
            songCues = SongCues.I.GetCues();
            if (TempoRampEnabled)
            {
                TempoIncrementCount = 0;
                for (int i = 0; i < songCues.Length; i++)
                {
                    MelonLogger.Log("Hi");
                    if (songCues[i].behavior == Target.TargetBehavior.ChainStart ||
                        songCues[i].behavior == Target.TargetBehavior.Hold ||
                        songCues[i].behavior == Target.TargetBehavior.Horizontal ||
                        songCues[i].behavior == Target.TargetBehavior.Melee ||
                        songCues[i].behavior == Target.TargetBehavior.Standard ||
                        songCues[i].behavior == Target.TargetBehavior.Vertical)
                    {
                        TempoIncrementTotalTargets += 1;
                    }
                }
            }
            if (ExtraParticlesEnabled)
            {
                SetParticleScale(customExtraParticlesScale);
            }
            if (FasterSongEnabled)
            {
                SetSongSpeed(customSongSpeed);
            }
            
        }

        public static void SetModifiersBefore()
        {
            AimAssistEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.ReducedAimAssist);
            TempoRampEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.TempoIncrement);
            ExtraParticlesEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.MoreParticles);
            FastTargetsEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.FastTargets);
            PsychedeliaEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.Psychedelia);
            FasterSongEnabled = GameplayModifiers.I.IsModifierActive(GameplayModifiers.Modifier.SpeedUp);

            if (TempoRampEnabled)
            {
                GameplayModifiers.I.DeactivateModifier(GameplayModifiers.Modifier.TempoIncrement);
                
            }

            if (AimAssistEnabled)
            {
                oldAimAssist = PlayerPreferences.I.AimAssistAmount.Get();
                SetAimAssist(customAimAssist);
            }

            if (ExtraParticlesEnabled)
            {
                GameplayModifiers.I.DeactivateModifier(GameplayModifiers.Modifier.MoreParticles);
            }

            if (FastTargetsEnabled)
            {
                oldTargetSpeed = PlayerPreferences.I.TargetSpeedMultiplier.Get();
                SetTargetSpeed(customTargetSpeed);
            }

            if (PsychedeliaEnabled)
            {
                psychedeliaSpeed = customPsychedeliaSpeed;
            }

            if (FasterSongEnabled)
            {
                GameplayModifiers.I.DeactivateModifier(GameplayModifiers.Modifier.SpeedUp);
            }
        }

        public static void SetModifiersAfter()
        {
            if (TempoRampEnabled)
            {
                GameplayModifiers.I.ActivateModifier(GameplayModifiers.Modifier.TempoIncrement);
                TempoIncrementTotalTargets = 0;
            }

            if (AimAssistEnabled)
            {
                SetAimAssist(oldAimAssist);
            }

            if (ExtraParticlesEnabled)
            {
                GameplayModifiers.I.ActivateModifier(GameplayModifiers.Modifier.MoreParticles);
            }

            if (FastTargetsEnabled)
            {
                SetTargetSpeed(oldTargetSpeed);
            }

            if (FasterSongEnabled)
            {
                GameplayModifiers.I.ActivateModifier(GameplayModifiers.Modifier.SpeedUp);
            }
        }



        public static void TempoRamp(Target target)
        {
            ++TempoIncrementCount;
            SetSongSpeed(Mathf.Lerp(1, customTempoRampEndSpeed, (float)TempoIncrementCount / TempoIncrementTotalTargets));
        }

        public override void OnApplicationStart()
        {
            HarmonyInstance instance = HarmonyInstance.Create("AudicaMod");
        }

        public override void OnLevelWasLoaded(int level)
        {
            if (!MelonPrefs.HasKey("CustomModifiers", "AimAssist") || !MelonPrefs.HasKey("CustomModifiers", "TempoRampEndSpeed") || !MelonPrefs.HasKey("CustomModifiers", "ExtraParticlesScale") || !MelonPrefs.HasKey("CustomModifiers", "TargetSpeed") || !MelonPrefs.HasKey("CustomModifiers", "PsychedeliaSpeed"))
            {
                CreateConfig();
            }
            else
            {
                LoadConfig();
            }
        }

        public override void OnUpdate()
        {
            if (menuState == MenuState.State.Launched && PsychedeliaEnabled)
            {
                float phaseTime = defaultPsychedeliaPhaseSeconds / psychedeliaSpeed;

                if (psychedeliaTimer <= phaseTime)
                {
                    psychedeliaTimer += Time.deltaTime;

                    float forcedPsychedeliaPhase = psychedeliaTimer / (phaseTime);
                    GameplayModifiers.I.mPsychedeliaPhase = forcedPsychedeliaPhase;
                }
                else psychedeliaTimer = 0;
            }
            else { return; }
        }
    }
}



