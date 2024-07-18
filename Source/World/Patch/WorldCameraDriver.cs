using System.Reflection;
using HarmonyLib;
using UnityEngine;
// ReSharper disable UnusedType.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedParameter.Global

namespace Universum.World.Patch;

public static class WorldCameraDriver {
    private const string TYPE_NAME = "RimWorld.Planet.WorldCameraDriver";
    public const float FIELD_OF_VIEW = 40.0f;
    public const float MAX_ALTITUDE = 1600.0f;
    private const float MIN_ALTITUDE = 80.0f;
    private const float ZOOM_ENUM_MULTIPLIER = 0.2f;
    private const float DRAG_SENSITIVITY_MULTIPLIER = 0.5f;
    private const float ZOOM_SENSITIVITY_MULTIPLIER = 0.75f;
    private const float DRAG_VELOCITY_MULTIPLIER = 0.5f;
    
    [HarmonyPatch]
    private static class JumpTo {
        private const string METHOD_NAME = $"{TYPE_NAME}:JumpTo";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME, [typeof(int)]);

        public static bool Prefix(ref RimWorld.Planet.WorldCameraDriver __instance, int tile) {
            ObjectHolder objectHolder = Cache.ObjectHolder.Get(tile);

            if (objectHolder == null) return true;

            __instance.JumpTo(objectHolder.celestialObject.position);
            return false;
        }
    }

    [HarmonyPatch]
    private static class AltitudePercent {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_AltitudePercent";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(ref RimWorld.Planet.WorldCameraDriver __instance, ref float __result) {
            __result = Mathf.InverseLerp(RimWorld.Planet.WorldCameraDriver.MinAltitude, MAX_ALTITUDE, __instance.altitude);
            return false;
        }
    }

    [HarmonyPatch]
    private static class MinAltitude {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_MinAltitude";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(ref float __result) {
            __result = MIN_ALTITUDE + (Verse.Steam.SteamDeck.IsSteamDeck ? 17.0f : 25.0f);
            return false;
        }
    }

    [HarmonyPatch]
    private static class CurrentZoom {
        private const string METHOD_NAME = $"{TYPE_NAME}:get_CurrentZoom";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(ref RimWorld.Planet.WorldCameraDriver __instance, ref RimWorld.Planet.WorldCameraZoomRange __result) {
            float altitudePercent = __instance.AltitudePercent;
            
            if (altitudePercent < 0.025 * ZOOM_ENUM_MULTIPLIER) {
                __result = RimWorld.Planet.WorldCameraZoomRange.VeryClose;
                return false;
            }
            
            if (altitudePercent < 0.042 * ZOOM_ENUM_MULTIPLIER) {
                __result = RimWorld.Planet.WorldCameraZoomRange.Close;
                return false;
            }
            
            __result = altitudePercent < 0.125 * ZOOM_ENUM_MULTIPLIER
                ? RimWorld.Planet.WorldCameraZoomRange.Far
                : RimWorld.Planet.WorldCameraZoomRange.VeryFar;
            return false;
        }
    }

    [HarmonyPatch]
    private static class WorldCameraDriverOnGUI {
        private const string METHOD_NAME = $"{TYPE_NAME}:WorldCameraDriverOnGUI";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(ref RimWorld.Planet.WorldCameraDriver __instance) {
            _UpdateReleasedLeftWhileHoldingMiddle(ref __instance);
            _UpdateMouseCoveredByUI(ref __instance);

            if (__instance.AnythingPreventsCameraMotion) {
                return false;
            }

            _HandleMouseDrag(ref __instance);
            _HandleScrollWheelAndZoom(ref __instance);
            _HandleKeyMovements(ref __instance);

            __instance.config.ConfigOnGUI();

            return false;
        }

        private static void _UpdateReleasedLeftWhileHoldingMiddle(ref RimWorld.Planet.WorldCameraDriver __instance) {
            if (Input.GetMouseButtonUp(0) && Input.GetMouseButton(2)) {
                __instance.releasedLeftWhileHoldingMiddle = true;
            } else if (Event.current.rawType == EventType.MouseDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(2)) {
                __instance.releasedLeftWhileHoldingMiddle = false;
            }
        }

        private static void _UpdateMouseCoveredByUI(ref RimWorld.Planet.WorldCameraDriver __instance) {
            __instance.mouseCoveredByUI = Verse.Find.WindowStack.GetWindowAt(Verse.UI.MousePositionOnUIInverted) != null;
        }

        private static void _HandleMouseDrag(ref RimWorld.Planet.WorldCameraDriver __instance) {
            if (!Verse.UnityGUIBugsFixer.IsSteamDeckOrLinuxBuild && Event.current.type == EventType.MouseDrag && Event.current.button == 2 ||
                Verse.UnityGUIBugsFixer.IsSteamDeckOrLinuxBuild && Input.GetMouseButton(2) &&
                (!Verse.Steam.SteamDeck.IsSteamDeck || !Verse.Find.WorldSelector.AnyCaravanSelected)) {
                Vector2 currentEventDelta = Verse.UnityGUIBugsFixer.CurrentEventDelta;

                if (Event.current.type == EventType.MouseDrag) {
                    Event.current.Use();
                }

                if (currentEventDelta != Vector2.zero) {
                    RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(RimWorld.ConceptDefOf.WorldCameraMovement, RimWorld.KnowledgeAmount.FrameInteraction);

                    currentEventDelta.x *= -1f;
                    __instance.desiredRotationRaw += currentEventDelta / RimWorld.Planet.GenWorldUI.CurUITileSize() * 0.273f * (Verse.Prefs.MapDragSensitivity * DRAG_SENSITIVITY_MULTIPLIER);
                }
            }
        }

        private static void _HandleScrollWheelAndZoom(ref RimWorld.Planet.WorldCameraDriver __instance) {
            float num = 0.0f;

            if (Event.current.type == EventType.ScrollWheel) {
                num -= Event.current.delta.y * 0.1f;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            if (RimWorld.KeyBindingDefOf.MapZoom_In.KeyDownEvent) {
                num += 2f;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            if (RimWorld.KeyBindingDefOf.MapZoom_Out.KeyDownEvent) {
                num -= 2f;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            __instance.desiredAltitude -= num * (__instance.config.zoomSpeed * ZOOM_SENSITIVITY_MULTIPLIER) * __instance.altitude / 12.0f;
            __instance.desiredAltitude = Mathf.Clamp(__instance.desiredAltitude, RimWorld.Planet.WorldCameraDriver.MinAltitude, MAX_ALTITUDE);
        }

        private static void _HandleKeyMovements(ref RimWorld.Planet.WorldCameraDriver __instance) {
            __instance.desiredRotation = Vector2.zero;

            if (RimWorld.KeyBindingDefOf.MapDolly_Left.IsDown) {
                __instance.desiredRotation.x = -__instance.config.dollyRateKeys;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            if (RimWorld.KeyBindingDefOf.MapDolly_Right.IsDown) {
                __instance.desiredRotation.x = __instance.config.dollyRateKeys;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            if (RimWorld.KeyBindingDefOf.MapDolly_Up.IsDown) {
                __instance.desiredRotation.y = __instance.config.dollyRateKeys;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }

            if (RimWorld.KeyBindingDefOf.MapDolly_Down.IsDown) {
                __instance.desiredRotation.y = -__instance.config.dollyRateKeys;
                RimWorld.PlayerKnowledgeDatabase.KnowledgeDemonstrated(
                    RimWorld.ConceptDefOf.WorldCameraMovement,
                    RimWorld.KnowledgeAmount.SpecificInteraction
                );
            }
        }
    }

    [HarmonyPatch]
    private static class Update {
        private const string METHOD_NAME = $"{TYPE_NAME}:Update";
        private static bool _verboseError = true;

        public static bool Prepare() => Common.PatchUtilities.Prepare(METHOD_NAME, TargetMethod(), ref _verboseError);

        private static MethodBase TargetMethod() => AccessTools.Method(METHOD_NAME);

        public static bool Prefix(ref RimWorld.Planet.WorldCameraDriver __instance) {
            if (Verse.LongEventHandler.ShouldWaitForEvent)
                return false;
            if (Verse.Find.World == null) {
                __instance.MyCamera.gameObject.SetActive(false);
            } else {
                if (!Verse.Find.WorldInterface.everReset) Verse.Find.WorldInterface.Reset();
                Vector2 curInputDollyVect = __instance.CalculateCurInputDollyVect();
                if (curInputDollyVect != Vector2.zero) {
                    float num = (float) ((__instance.altitude - (double) RimWorld.Planet.WorldCameraDriver.MinAltitude) / (MAX_ALTITUDE - (double) RimWorld.Planet.WorldCameraDriver.MinAltitude) * 0.850000023841858 + 0.150000005960464);
                    __instance.rotationVelocity = new Vector2(curInputDollyVect.x, curInputDollyVect.y) * num;
                }
                if ((!Input.GetMouseButton(2) || Verse.Steam.SteamDeck.IsSteamDeck && __instance.releasedLeftWhileHoldingMiddle) && __instance.dragTimeStamps.Count != 0) {
                    __instance.rotationVelocity += Verse.CameraDriver.GetExtraVelocityFromReleasingDragButton(__instance.dragTimeStamps, 5f * DRAG_VELOCITY_MULTIPLIER);
                    __instance.dragTimeStamps.Clear();
                }
                if (!__instance.AnythingPreventsCameraMotion) {
                    float num = Time.deltaTime * Verse.CameraDriver.HitchReduceFactor;
                    __instance.sphereRotation *= Quaternion.AngleAxis(__instance.rotationVelocity.x * num * __instance.config.rotationSpeedScale, __instance.MyCamera.transform.up);
                    __instance.sphereRotation *= Quaternion.AngleAxis(-__instance.rotationVelocity.y * num * __instance.config.rotationSpeedScale, __instance.MyCamera.transform.right);
                    if (__instance.desiredRotationRaw != Vector2.zero) {
                        __instance.sphereRotation *= Quaternion.AngleAxis(__instance.desiredRotationRaw.x, __instance.MyCamera.transform.up);
                        __instance.sphereRotation *= Quaternion.AngleAxis(-__instance.desiredRotationRaw.y, __instance.MyCamera.transform.right);
                    }
                    __instance.dragTimeStamps.Add(new Verse.CameraDriver.DragTimeStamp() {
                        posDelta = __instance.desiredRotationRaw,
                        time = Time.time
                    });
                }
                __instance.desiredRotationRaw = Vector2.zero;
                int num1 = Verse.Gen.FixedTimeStepUpdate(ref __instance.fixedTimeStepBuffer, 60f);
                for (int index = 0; index < num1; ++index) {
                    if (__instance.rotationVelocity != Vector2.zero) {
                        __instance.rotationVelocity *= __instance.config.camRotationDecayFactor;
                        if (__instance.rotationVelocity.magnitude < 0.0500000007450581)
                            __instance.rotationVelocity = Vector2.zero;
                    }
                    if (__instance.config.smoothZoom) {
                        float num2 = Mathf.Lerp(__instance.altitude, __instance.desiredAltitude, 0.05f);
                        __instance.desiredAltitude += (num2 - __instance.altitude) * __instance.config.zoomPreserveFactor;
                        __instance.altitude = num2;
                    } else {
                        float num2 = (float) ((__instance.desiredAltitude - (double) __instance.altitude) * 0.400000005960464);
                        __instance.desiredAltitude += __instance.config.zoomPreserveFactor * num2;
                        __instance.altitude += num2;
                    }
                }
                __instance.rotationAnimation_lerpFactor += Time.deltaTime * 8f;
                if (Verse.Find.PlaySettings.lockNorthUp) {
                    __instance.RotateSoNorthIsUp(false);
                    __instance.ClampXRotation(ref __instance.sphereRotation);
                }
                for (int index = 0; index < num1; ++index)
                    __instance.config.ConfigFixedUpdate_60(ref __instance.rotationVelocity);
                __instance.ApplyPositionToGameObject();
            }
            return false;
        }
    }
}
