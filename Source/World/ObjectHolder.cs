using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Universum.World;

[Verse.StaticConstructorOnStartup]
public class ObjectHolder : RimWorld.Planet.MapParent {
    public CelestialObject celestialObject;
    public Defs.CelestialObject celestialObjectDef;

    private Texture2D _overlayIcon;
    public bool keepAfterAbandon;
    public bool hideIcon;

    private string _exposeCelestialObjectDefName;
    private int? _exposeCelestialObjectSeed;
    private int? _exposeCelestialObjectId;
    private int? _exposeCelestialObjectTargetId;
    private Vector3? _exposeCelestialObjectPosition;
    private int? _exposeCelestialObjectDeathTick;

    public override string Label => celestialObject.name;
    public override Vector3 DrawPos => celestialObject.transformedPosition;
    public override Texture2D ExpandingIcon => HasMap ? _overlayIcon : base.ExpandingIcon;
    public override Verse.MapGeneratorDef MapGeneratorDef => celestialObjectDef.objectHolder.mapGeneratorDef;

    public void Init(
        string celestialObjectDefName,
        int? celestialObjectSeed = null,
        int? celestialObjectId = null,
        int? celestialObjectTargetId = null,
        Vector3? celestialObjectPosition = null,
        int? celestialObjectDeathTick = null,
        CelestialObject celestialObjectInstance = null
    ) {
        this.celestialObject = celestialObjectInstance ?? Initialization.Create(celestialObjectDefName, celestialObjectSeed, celestialObjectId, celestialObjectTargetId, celestialObjectPosition, celestialObjectDeathTick);
        celestialObjectDef = Loader.Defs.CelestialObjects[celestialObjectDefName];

        _overlayIcon = Loader.Assets.GetTexture(celestialObjectDef.objectHolder.overlayIconPath);
        keepAfterAbandon = celestialObjectDef.objectHolder.keepAfterAbandon;

        this.celestialObject.objectHolder = this;
    }

    public override void Destroy() {
        if (!Destroyed) base.Destroy();
    }

    public void SignalDestruction() {
        celestialObject.forceDeath = true;
        Game.MainLoop.instance.dirtyCache = true;
    }

    public override void PostRemove() {
        base.PostRemove();
        if (keepAfterAbandon) {
            ObjectHolder newObjectHolder = Initialization.CreateObjectHolder(celestialObjectDef.defName, celestialObject: celestialObject);
            newObjectHolder.Tile = Tile;
        } else {
            SignalDestruction();
            Initialization.UpdateTile(Tile, Loader.Defs.OceanBiomeDef);
        }
    }

    public void Randomize() => celestialObject.Randomize();

    public override void Tick() { }

    public override void Draw() { }

    public override void Print(Verse.LayerSubMesh subMesh) { }

    public Verse.Map Settle(RimWorld.Planet.Caravan caravan) {
        if (caravan.Faction != RimWorld.Faction.OfPlayer) return null;
        
        // handle colonist memories
        if (Verse.Find.AnyPlayerHomeMap == null) {
            foreach (Verse.Pawn podsAliveColonist in RimWorld.PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_Colonists) {
                RimWorld.MemoryThoughtHandler memories = podsAliveColonist.needs?.mood?.thoughts?.memories;
                
                if (memories == null) continue;
                
                memories.RemoveMemoriesOfDef(RimWorld.ThoughtDefOf.NewColonyOptimism);
                memories.RemoveMemoriesOfDef(RimWorld.ThoughtDefOf.NewColonyHope);
                
                if (podsAliveColonist.IsFreeNonSlaveColonist) memories.TryGainMemory(RimWorld.ThoughtDefOf.NewColonyOptimism);
            }
        }
        
        // generate map
        Verse.Map map = CreateMap(caravan.Faction);
        if (map == null) return null;
        
        // spawn colonist
        Verse.LongEventHandler.QueueLongEvent(
            action: () => {
                Verse.Pawn pawn = caravan.PawnsListForReading[0];
                RimWorld.Planet.CaravanEnterMapUtility.Enter(
                    caravan,
                    map,
                    RimWorld.Planet.CaravanEnterMode.Center,
                    RimWorld.Planet.CaravanDropInventoryMode.DropInstantly,
                    extraCellValidator: x => Verse.GridsUtility.GetRoom(x, map).CellCount >= 600
                );
                Verse.CameraJumper.TryJump(pawn);
            }, 
            "SpawningColonists",
            true,
            Verse.GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap
        );

        return map;
    }

    public Verse.Map CreateMap(RimWorld.Faction faction, bool clearFog = false) {
        if (faction != RimWorld.Faction.OfPlayer) return null;
        Verse.Map map = Verse.MapGenerator.GenerateMap(Verse.Find.World.info.initialMapSize, this, MapGeneratorDef, ExtraGenStepDefs, extraInitBeforeContentGen: null);
        if (clearFog) map.fogGrid.ClearAllFog();

        SetFaction(faction);
        Verse.Find.World.WorldUpdate();
        return map;
    }

    public void CheckHideIcon() {
        hideIcon = Patch.WorldRendererUtility.ShouldHideObjectHolder(celestialObject.transformedPosition);
    }

    public bool SafeDespawn() {
        if (HasMap) return false;
        
        return !_AnyTravelingTransportPodsHere() && !_AnyCaravansHere();
    }

    public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject) {
        alsoRemoveWorldObject = false;

        if (_AnyTravelingTransportPodsHere() || _AnyCaravansHere()) return false;

        return base.ShouldRemoveMapNow(out alsoRemoveWorldObject);
    }

    private bool _AnyTravelingTransportPodsHere() {
        return Verse.Find.World.worldObjects.AllWorldObjects.OfType<RimWorld.Planet.TravelingTransportPods>().Any(
            pods => pods.initialTile == Tile || pods.destinationTile == Tile
        );
    }

    private bool _AnyCaravansHere() {
        return Verse.Find.World.worldObjects.AllWorldObjects.OfType<RimWorld.Planet.Caravan>().Any(
            caravan => caravan.Tile == Tile
        );
    }

    private string _GetDeathTimerLabel() {
        if (celestialObject.deathTick == null) return null;

        float timeLeft = (float) celestialObject.deathTick - Game.MainLoop.instance.tick;
        
        return timeLeft < 60000.0f
            ? Verse.TranslatorFormattedStringExtensions.Translate(
                "RimNauts.hours_left", args: [Math.Ceiling(timeLeft / 2500.0f)]
            )
            : Verse.TranslatorFormattedStringExtensions.Translate(
                "RimNauts.days_left", args: [(timeLeft / 60000.0f).ToString("0.00")]
            );
    }

    public override string GetDescription() {
        StringBuilder stringBuilder = new StringBuilder();

        _AppendCelestialDescription(stringBuilder);
        _AppendComponentDescriptionParts(stringBuilder);

        return stringBuilder.ToString();
    }

    public override string GetInspectString() {
        StringBuilder stringBuilder = new StringBuilder();

        _AppendCelestialDescription(stringBuilder);
        _AppendFactionIfApplicable(stringBuilder);
        _AppendComponentStrings(stringBuilder);
        RimWorld.QuestUtility.AppendInspectStringsFromQuestParts(stringBuilder, this);
        _AppendCooldownInformation(stringBuilder);
        _AppendConditionCausersIfPresent(stringBuilder);

        return stringBuilder.ToString().Trim();
    }

    private void _AppendCelestialDescription(StringBuilder sb) {
        sb.AppendLine(celestialObjectDef.objectHolder.description);
        if (celestialObject.deathTick != null && SafeDespawn()) sb.Append(_GetDeathTimerLabel());
    }

    private void _AppendFactionIfApplicable(StringBuilder sb) {
        if (Faction != null && AppendFactionToInspectString) {
            sb.AppendLine(Verse.TranslatorFormattedStringExtensions.Translate("Faction") + ": " + Faction.Name);
        }
    }

    private void _AppendComponentDescriptionParts(StringBuilder sb) {
        for (int i = 0; i < comps.Count; i++) {
            string descriptionPart = comps[i].GetDescriptionPart();
            
            if (string.IsNullOrEmpty(descriptionPart)) continue;
            
            if (sb.Length > 0) {
                sb.AppendLine();
                sb.AppendLine();
            }
            
            sb.Append(descriptionPart);
        }
    }

    private void _AppendComponentStrings(StringBuilder sb) {
        for (int i = 0; i < comps.Count; i++) {
            string text = comps[i].CompInspectStringExtra();
            
            if (string.IsNullOrEmpty(text)) continue;
            
            if (Verse.Prefs.DevMode && char.IsWhiteSpace(text[text.Length - 1])) {
                Verse.Log.ErrorOnce(
                    string.Concat(comps[i].GetType(), " CompInspectStringExtra ended with whitespace: ", text),
                    25612
                );
                text = text.TrimEnd('\r', '\n');
            }

            if (sb.Length != 0) {
                sb.AppendLine();
            }

            sb.Append(text);
        }
    }

    private void _AppendCooldownInformation(StringBuilder sb) {
        if (!RimWorld.Planet.EnterCooldownCompUtility.EnterCooldownBlocksEntering(this)) return;
        
        if (sb.Length > 0) {
            sb.AppendLine();
        }
        
        sb.AppendLine(Verse.TranslatorFormattedStringExtensions.Translate(
            key: "EnterCooldown",
            args: [
                RimWorld.GenDate.ToStringTicksToPeriod(
                    RimWorld.Planet.EnterCooldownCompUtility.EnterCooldownTicksLeft(this)
                )
            ]
        ));
    }

    private void _AppendConditionCausersIfPresent(StringBuilder sb) {
        if (HandlesConditionCausers || !HasMap) return;
        
        List<Verse.Thing> list = Map.listerThings.ThingsInGroup(Verse.ThingRequestGroup.ConditionCauser);
        for (int i = 0; i < list.Count; i++) {
            sb.AppendLine(
                list[i].LabelShortCap + " (" + Verse.TranslatorFormattedStringExtensions.Translate(
                    key: "ConditionCauserRadius",
                    args: [Verse.ThingCompUtility.TryGetComp<RimWorld.CompCauseGameCondition>(list[i]).Props.worldRange]
                ) + ")"
            );
        }
    }

    public override void ExposeData() {
        base.ExposeData();
        
        switch (Verse.Scribe.mode) {
            case Verse.LoadSaveMode.Saving:
                _SaveData();
                break;
            case Verse.LoadSaveMode.LoadingVars:
                _LoadData();
                break;
            case Verse.LoadSaveMode.PostLoadInit:
                _PostLoadData();
                break;
            case Verse.LoadSaveMode.Inactive:
                break;
            case Verse.LoadSaveMode.ResolvingCrossRefs:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void _SaveData() {
        _exposeCelestialObjectDefName = celestialObject.DEF.defName;
        _exposeCelestialObjectSeed = celestialObject.seed;
        _exposeCelestialObjectId = celestialObject.id;
        _exposeCelestialObjectTargetId = celestialObject.targetId;
        _exposeCelestialObjectPosition = celestialObject.position;
        _exposeCelestialObjectDeathTick = celestialObject.deathTick;

        Verse.Scribe_Values.Look(ref _exposeCelestialObjectDefName, "_exposeCelestialObjectDefName");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectSeed, "_exposeCelestialObjectSeed");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectId, "_exposeCelestialObjectId");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectTargetId, "_exposeCelestialObjectTargetId");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectPosition, "_exposeCelestialObjectPosition");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectDeathTick, "_exposeCelestialObjectDeathTick");
    }

    private void _LoadData() {
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectDefName, "_exposeCelestialObjectDefName");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectSeed, "_exposeCelestialObjectSeed");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectId, "_exposeCelestialObjectId");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectTargetId, "_exposeCelestialObjectTargetId");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectPosition, "_exposeCelestialObjectPosition");
        Verse.Scribe_Values.Look(ref _exposeCelestialObjectDeathTick, "_exposeCelestialObjectDeathTick");
    }

    private void _PostLoadData() {
        Init(
            _exposeCelestialObjectDefName,
            _exposeCelestialObjectSeed,
            _exposeCelestialObjectId,
            _exposeCelestialObjectTargetId,
            _exposeCelestialObjectPosition,
            _exposeCelestialObjectDeathTick
        );

        Game.MainLoop.instance.dirtyCache = true;
    }
}
