using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Gilzoide.ManagedJobs;
using Unity.Jobs;
using UnityEngine;

namespace Universum.Game;

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public class MainLoop : Verse.GameComponent {
    public static MainLoop instance;
    
    public static RimWorld.Planet.World world;
    public static Verse.TickManager tickManager;
    public static RimWorld.Planet.WorldCameraDriver worldCameraDriver;
    public static Camera worldCamera;
    public static Camera worldSkyboxCamera;
    public static Verse.CameraDriver colonyCameraDriver;
    public static Camera colonyCamera;

    private Verse.TickManager _tickManager;
    public int tick;
    public Verse.TimeSpeed timeSpeed = Verse.TimeSpeed.Paused;

    private Camera _camera;
    public Vector3 cameraPosition;
    public Vector3 cameraUp;
    public Vector3 cameraForward;

    private RimWorld.Planet.WorldCameraDriver _cameraDriver;
    public Vector3 currentSphereFocusPoint;
    public float altitudePercent;

    private List<World.CelestialObject> _celestialObjects = [];
    private readonly Dictionary<string, int> OBJECT_GENERATION_SPAWN_TICK = new();
    private int _spawnTickMin;

    private int _totalCelestialObjectsCached;
    private World.CelestialObject[] _celestialObjectsCache = [];
    public bool dirtyCache;

    public bool blockRendering;
    private bool _wait;
    public bool forceUpdate = true;
    private bool _prevWorldSceneRendered;
    public bool worldSceneActivated;
    public bool worldSceneDeactivated;
    public bool unpaused;
    private int _prevTick;
    public bool cameraMoved;
    private Vector3 _prevCameraPosition;
    private bool _frameChanged;

    private List<string> _exposeCelestialObjectDefNames = [];
    private List<int?> _exposeCelestialObjectSeeds = [];
    private List<int?> _exposeCelestialObjectIds = [];
    private List<int?> _exposeCelestialObjectTargetIds = [];
    private List<Vector3?> _exposeCelestialObjectPositions = [];
    private List<int?> _exposeCelestialObjectDeathTicks = [];

    public int seed = Verse.Rand.Int;

    private Queue<World.CelestialObject> _visualGenerationQueue = new();
    private Thread _visualGenerationWorker;

    public static readonly CelestialUpdateJob CELESTIAL_UPDATE_JOB = new();
    
    private readonly int UTILITY_MAIN_LOOP_PARALLELIZATION_ID = Loader.Defs.UtilityId["universum.main_loop_parallelization"];

    // ReSharper disable once UnusedParameter.Local
    public MainLoop(Verse.Game game) {
        if (instance != null) {
            instance = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        
        instance = this;
    }

    public void Destroy() {
        seed = Verse.Rand.Int;
        
        if (_celestialObjects == null) return;
        
        for (int i = 0; i < _celestialObjects.Count; i++) {
            if (_celestialObjects[i] == null) continue;
            
            _celestialObjects[i].Destroy();
            _celestialObjects[i] = null;
        }
    }

    public void FreshGame() {
        if (_celestialObjects.Count <= 0) return;
        
        Destroy();
        
        _celestialObjects = [];
        _totalCelestialObjectsCached = 0;
        _celestialObjectsCache = [];
        dirtyCache = false;
        blockRendering = false;
        _wait = false;
        forceUpdate = true;
        _prevWorldSceneRendered = false;
        worldSceneActivated = false;
        worldSceneDeactivated = false;
        unpaused = false;
        _prevTick = 0;
        cameraMoved = false;
        _frameChanged = false;
        seed = Verse.Rand.Int;
        _visualGenerationQueue = new Queue<World.CelestialObject>();
        _visualGenerationWorker = null;

        _Recache();
    }

    public override void LoadedGame() {
        if (_celestialObjects is null || _celestialObjects.Count <= 0) World.Initialization.GenerateOnStartUp();
    }

    public override void GameComponentTick() {
        if (_tickManager is null || _tickManager.TicksGame % 10 != 0) return;

        if (_visualGenerationQueue.Count > 0) dirtyCache = true;

        for (int i = 0; i < _totalCelestialObjectsCached; i++) _celestialObjectsCache[i].Tick();

        if (_tickManager.TicksGame < _spawnTickMin) return;

        foreach (Defs.ObjectGeneration objectGenerationStep in Loader.Defs.CelestialObjectGenerationRandomSteps.Values) {
            if (!OBJECT_GENERATION_SPAWN_TICK.TryGetValue(objectGenerationStep.defName, out var spawnTick)) continue;
            if (_tickManager.TicksGame > spawnTick) {
                World.Initialization.Generate(
                    objectGenerationStep,
                    despawnBetweenDays: objectGenerationStep.despawnBetweenDays,
                    amount: Verse.Rand.Range((int) objectGenerationStep.spawnAmountBetween[0], (int) objectGenerationStep.spawnAmountBetween[1])
                );

                int newSpawnTick = _GetSpawnTick(objectGenerationStep.spawnBetweenDays[0], objectGenerationStep.spawnBetweenDays[1]);
                _spawnTickMin = newSpawnTick;

                OBJECT_GENERATION_SPAWN_TICK[objectGenerationStep.defName] = newSpawnTick;
            }
        }

        foreach (var spawnTick in OBJECT_GENERATION_SPAWN_TICK.Values.Where(spawnTick => spawnTick < _spawnTickMin)) _spawnTickMin = spawnTick;
    }

    public override void GameComponentUpdate() {
        _GetFrameData();
        if (dirtyCache) _Recache();
        if (_wait && !forceUpdate) return;
        if (_frameChanged || forceUpdate) {
            _Update();
            _Render();
        }
        forceUpdate = false;
    }

    public void AddObject(List<World.CelestialObject> celestialObjects) {
        _celestialObjects.AddRange(celestialObjects);
        foreach (var celestialObject in celestialObjects) _visualGenerationQueue.Enqueue(celestialObject);
        dirtyCache = true;
    }

    public void AddObject(World.CelestialObject celestialObject) {
        _celestialObjects.Add(celestialObject);
        _visualGenerationQueue.Enqueue(celestialObject);
        dirtyCache = true;
    }

    private static void _ProcessVisualGenerationQueue(Queue<World.CelestialObject> queue) {
        int currentSeed = instance.seed;

        while (queue.Count > 0) {
            if (instance == null || currentSeed != instance.seed) return;
            World.CelestialObject celestialObject = queue.Dequeue();
            celestialObject?.GenerateVisuals();
        }
    }

    private void _GetFrameData() {
        if (_tickManager != null) {
            tick = _tickManager.TicksGame;
            timeSpeed = _tickManager.curTimeSpeed;
        }

        if (_camera != null) {
            cameraPosition = _camera.transform.position;
            cameraUp = _camera.transform.up;
            cameraForward = _camera.transform.forward;
        }

        if (_cameraDriver != null) {
            currentSphereFocusPoint = _cameraDriver.CurrentlyLookingAtPointOnSphere;
            altitudePercent = _cameraDriver.AltitudePercent;
        }

        if (_celestialObjects is null || _celestialObjects.Count <= 0 || _tickManager is null) {
            _wait = true;
            return;
        }
        _wait = !RimWorld.Planet.WorldRendererUtility.WorldRenderedNow && !forceUpdate;

        bool sceneIsWorld = RimWorld.Planet.WorldRendererUtility.WorldRenderedNow;
        bool sceneSwitched = _prevWorldSceneRendered != sceneIsWorld;
        _prevWorldSceneRendered = RimWorld.Planet.WorldRendererUtility.WorldRenderedNow;

        if (sceneSwitched) forceUpdate = true;

        worldSceneActivated = sceneSwitched && sceneIsWorld;
        worldSceneDeactivated = sceneSwitched && !sceneIsWorld;

        unpaused = _tickManager.TicksGame != _prevTick;
        _prevTick = _tickManager.TicksGame;

        cameraMoved = _camera.transform.position != _prevCameraPosition;
        _prevCameraPosition = _camera.transform.position;

        _frameChanged = unpaused || cameraMoved;
    }

    private void _Update() {
        if (Cache.Settings.UtilityEnabled(UTILITY_MAIN_LOOP_PARALLELIZATION_ID)) {
            CELESTIAL_UPDATE_JOB.celestialObjects = _celestialObjectsCache;
            new ManagedJobParallelFor(CELESTIAL_UPDATE_JOB).Schedule(_totalCelestialObjectsCached, 400).Complete();

            return;
        }

        for (int i = 0; i < _totalCelestialObjectsCached; i++) _celestialObjectsCache[i].Update();
    }

    private void _Render() {
        for (int i = 0; i < _totalCelestialObjectsCached; i++) _celestialObjectsCache[i].Render(blockRendering);
    }

    public void ForceRender() {
        _Render();
    }

    private void _Recache() {
        dirtyCache = false;
        forceUpdate = true;

        _tickManager = Verse.Find.TickManager;
        _camera = Verse.Find.WorldCamera.GetComponent<Camera>();
        _cameraDriver = Verse.Find.WorldCameraDriver;

        if (_camera != null) {
            _camera.farClipPlane = 500.0f + World.Patch.WorldCameraDriver.MAX_ALTITUDE;
            _camera.fieldOfView = World.Patch.WorldCameraDriver.FIELD_OF_VIEW;
            RimWorld.Planet.WorldCameraManager.worldSkyboxCameraInt.farClipPlane = 500.0f + World.Patch.WorldCameraDriver.MAX_ALTITUDE;
        }

        for (int i = 0; i < _celestialObjects.Count; i++) {
            if (!_celestialObjects[i].ShouldDespawn()) continue;
            
            for (int j = 0; j < _celestialObjects.Count; j++) {
                if (_celestialObjects[j].targetId == _celestialObjects[i].id) _celestialObjects[j].SetTarget(target: null);
            }

            _celestialObjects[i].Destroy();
            _celestialObjects[i] = null;
        }
        _celestialObjects = _celestialObjects.Where(item => item != null).ToList();

        _totalCelestialObjectsCached = _celestialObjects.Count;
        _celestialObjectsCache = new World.CelestialObject[_totalCelestialObjectsCached];

        for (int i = 0; i < _totalCelestialObjectsCached; i++) _celestialObjectsCache[i] = _celestialObjects[i];

        if (_tickManager != null || _camera != null || _cameraDriver != null) _Update();

        if (OBJECT_GENERATION_SPAWN_TICK.Count == 0) {
            foreach (var step in Loader.Defs.CelestialObjectGenerationRandomSteps.Values) {
                int spawnTick = _GetSpawnTick(step.spawnBetweenDays[0], step.spawnBetweenDays[1]);
                if (spawnTick < _spawnTickMin) _spawnTickMin = spawnTick;

                OBJECT_GENERATION_SPAWN_TICK.Add(
                    step.defName,
                    spawnTick
                );
            }
        }

        if (_visualGenerationQueue.Count > 0 && (_visualGenerationWorker == null || !_visualGenerationWorker.IsAlive)) {
            Queue<World.CelestialObject> copiedQueue = new Queue<World.CelestialObject>(_visualGenerationQueue);
            _visualGenerationQueue.Clear();

            _visualGenerationWorker = new Thread(() => _ProcessVisualGenerationQueue(copiedQueue));
            _visualGenerationWorker.Start();
        }

        for (int i = 0; i < _totalCelestialObjectsCached; i++) _celestialObjects[i].FindTarget(_celestialObjects);
    }

    public class CelestialUpdateJob : IJobParallelFor {
        public World.CelestialObject[] celestialObjects = [];

        public void Execute(int index) {
            celestialObjects[index].Update();
        }
    }

    public int GetTotal(Defs.CelestialObject def) => _celestialObjects.Count(celestialObject => celestialObject.DEF == def);

    public int GetTotal() => _celestialObjects.Count;

    public void ShouldDestroy(Defs.CelestialObject def) {
        var celestialObjectsToDestroy = _celestialObjects.Where(celestialObject => celestialObject.DEF == def);
        foreach (var celestialObject in celestialObjectsToDestroy) celestialObject.forceDeath = true;
        dirtyCache = true;
    }

    private int _GetSpawnTick(float betweenDaysMin, float betweenDaysMax) => (int) Verse.Rand.Range(betweenDaysMin * 60000, betweenDaysMax * 60000) + _tickManager.TicksGame;

    public override void ExposeData() {
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
        _exposeCelestialObjectDefNames = [];
        _exposeCelestialObjectSeeds = [];
        _exposeCelestialObjectIds = [];
        _exposeCelestialObjectTargetIds = [];
        _exposeCelestialObjectPositions = [];
        _exposeCelestialObjectDeathTicks = [];

        for (int i = 0; i < _totalCelestialObjectsCached; i++) {
            _celestialObjectsCache[i].GetExposeData(
                _exposeCelestialObjectDefNames,
                _exposeCelestialObjectSeeds,
                _exposeCelestialObjectIds,
                _exposeCelestialObjectTargetIds,
                _exposeCelestialObjectPositions,
                _exposeCelestialObjectDeathTicks
            );
        }

        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectDefNames, "_exposeCelestialObjectDefNames", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectSeeds, "_exposeCelestialObjectSeeds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectIds, "_exposeCelestialObjectIds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectTargetIds, "_exposeCelestialObjectTargetIds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectPositions, "_exposeCelestialObjectPositions", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectDeathTicks, "_exposeCelestialObjectDeathTicks", Verse.LookMode.Value);

        _exposeCelestialObjectDefNames = [];
        _exposeCelestialObjectSeeds = [];
        _exposeCelestialObjectIds = [];
        _exposeCelestialObjectTargetIds = [];
        _exposeCelestialObjectPositions = [];
        _exposeCelestialObjectDeathTicks = [];
    }

    private void _LoadData() {
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectDefNames, "_exposeCelestialObjectDefNames", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectSeeds, "_exposeCelestialObjectSeeds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectIds, "_exposeCelestialObjectIds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectTargetIds, "_exposeCelestialObjectTargetIds", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectPositions, "_exposeCelestialObjectPositions", Verse.LookMode.Value);
        Verse.Scribe_Collections.Look(ref _exposeCelestialObjectDeathTicks, "_exposeCelestialObjectDeathTicks", Verse.LookMode.Value);
    }

    private void _PostLoadData() {
        World.Initialization.NextId = 0;
        if (_exposeCelestialObjectIds is not null && _exposeCelestialObjectIds.Count > 0) World.Initialization.NextId = _exposeCelestialObjectIds.Max() ?? 0;

        World.Initialization.Create(
            _exposeCelestialObjectDefNames,
            _exposeCelestialObjectSeeds,
            _exposeCelestialObjectIds,
            _exposeCelestialObjectTargetIds,
            _exposeCelestialObjectPositions,
            _exposeCelestialObjectDeathTicks
        );

        int numCelestialObjects = _celestialObjects.Count;
        for (int i = 0; i < numCelestialObjects; i++) _celestialObjects[i].FindTarget(_celestialObjects);

        _exposeCelestialObjectDefNames = [];
        _exposeCelestialObjectSeeds = [];
        _exposeCelestialObjectIds = [];
        _exposeCelestialObjectTargetIds = [];
        _exposeCelestialObjectPositions = [];
        _exposeCelestialObjectDeathTicks = [];
    }
}
