# Kerbal Space Program 2
## Vue d'ensemble

À première vue, le jeu semble être une vulgaire copie de la première version de KSP avec de meilleurs graphismes, mais nécessitant des ressources excessives pour fonctionner et une physique qui n'est pas au rendez-vous. Mais il n'en est rien, ce n'est que la vue du joueur. Il semblerait que nous n'ayons accès qu'à une petite partie du jeu, et pas à toutes les fonctionnalités, peut-être pour éviter d'avoir une mauvaise première image des nouveautés.

## Vue du code

Le code de KSP 1 est assez désordonné. On sent le côté jeu indépendant qui est devenu par la suite, avec l'ajout de couches de patchs correctifs, un jeu plus sérieux. Néanmoins, le jeu garde une base qui n'était pas prévue pour le modding, encore moins pour du multijoueur ou l'ajout de voyage interstellaire.

KSP 2 est une refonte complète du code de KSP 1, avec une structure pensée dès le départ pour l'ajout du modding en profondeur et du multijoueur. Des petits indices subtils sont visibles par les joueurs, comme la création d'une agence spéciale à chaque partie que l'on souhaite créer. 
Il intègre aussi déjà la prochaine mise à jour majeure **La Science**. Le code est clairement déjà présent dans le jeu, avec l'arbre de recherche, les activations, etc. Je n'ai pas eu le temps de vérifier si les interfaces utilisateur et le modèle des bâtiments étaient présents ou non.

### Spéculation de MrCubee d'après le code incomplet

Je pense qu'en multijoueur, chaque joueur aura son agence spatiale avec ses missions propres, et que nous pourrions voir des systèmes "d'appel d'offres" et de collaboration ou de concurrence entre agences.

## Le modding

KSP2 souhaite simplifier la création de mods à sa communauté. Il proposera un équivalent du GameData de KSP1 en même temps que l'ajout du multijoueur. Le code étant déjà présent et fonctionnel en partie, on peut déjà remarquer que KSP2 proposera deux manières de faire des mods, une pour les plus expérimentés en C# et une autre pour les novices/débutants en Lua. Les deux types de mods pourront interagir dans le même environnement. Les mods Lua seront chargés grâce à la bibliothèque MoonSharp déjà présente dans le code du jeu. D'après le code, on peut voir 2 autres types de mod : un type ContentOnly, ce qui signifie du contenu sans code exécutable, et de type Shakespeare, mais je ne sais pas encore ce que c'est.

## Voici les sources que j'ai utilisé
### D'après la bibliothèque dynamique *KSP2_x64_Data\Assembly-CSharp.dll*

#### Type mods que KSP2 mettra en place
```csharp
namespace KSP.Modding {
    public enum KSP2ModType {
        Invalid,
        Lua,
        CSharp,
        ContentOnly,
        Shakespeare,
        Error,
    }
}
```

#### Instance d'une partie de KSP2
```csharp
using Game.Data;
using KSP.Assets;
using KSP.Audio;
using KSP.Contexts.Game;
using KSP.DebugTools;
using KSP.Game.Colonies;
using KSP.Game.Missions;
using KSP.Input;
using KSP.Inspector;
using KSP.IO;
using KSP.Localization;
using KSP.Logging;
using KSP.Map;
using KSP.Messages;
using KSP.Modding;
using KSP.Networking.MP;
using KSP.Networking.OnlineServices;
using KSP.OAB;
using KSP.Platform;
using KSP.Plugins;
using KSP.Rendering;
using KSP.Rendering.Planets;
using KSP.Research;
using KSP.ScriptInterop;
using KSP.ScriptInterop.impl.moonsharp;
using KSP.ScriptUI;
using KSP.Sim;
using KSP.Sim.impl;
using KSP.Sim.ResourceSystem;
using KSP.UI;
using KSP.UI.Flight;
using KSP.Utilities.Scripting;
using KSP.VFX;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KSP.Game {
    public class GameInstance: MonoBehaviour, IExceptionEventHandler, IUpdateDriver, ILateUpdateDriver {
        public
        const string EMPTY_SESSION_GUID_STRING = "";
        private string _sessionGuidString = "";
        private LocalPlayer _localPlayer = new LocalPlayer();
        private CampaignPlayerManager _campaignPlayerManager = new CampaignPlayerManager();
        private AgencyManager _agencyManager = new AgencyManager();
        [SerializeField]
        [ReadOnly]
        private AssetProvider _assets;
        [ReadOnly]
        private SaveLoadManager _saveLoadManager;
        private SettingsMenuManager _settingsMenuManager;
        [ReadOnly]
        private SessionManager _sessionManager;
        [ReadOnly]
        private PersistentProfileManager _profileManager;
        [ReadOnly]
        private ResearchManager _researchManager;
        [ReadOnly]
        private PropertyWatcherDataBroker _propertyWatcherDataBroker;
        private IScriptInterop _scriptInterop;
        private Dictionary < DifficultyModeAndLevel, DifficultyOptionsData > _difficultyOptionLevels = new Dictionary < DifficultyModeAndLevel, DifficultyOptionsData > ();
        public bool DebugMessagesEnabled;
        [SerializeField]
        private GameObject _assetPrefab;
        [SerializeField]
        private GameObject _mouseManagerPrefab;
        private bool _isInitialized;
        [SerializeField]
        [Header("UI Manager")]
        [ReadOnly]
        private UIManager _ui;
        [SerializeField]
        private GameObject _uiManagerPrefab;
        [SerializeField]
        [Header("Audio Initializer")]
        [ReadOnly]
        private KSPAudioInitializer _kspAudioInitializer;
        [Tooltip("Provide a reference to a prefab to determine the type of KSPAudioInitializer that will be created.")]
        [SerializeField]
        private KSPAudioInitializer _kspAudioInitializerPrefab;
        [SerializeField]
        private List < GameModeSO > gameModes;
        private IScriptRunner _scriptRunner;
        private DynamicConfigurationDatabase _dynamicConfigurationDatabase;
        private DynamicScriptConfigurationDatabase _dynamicScriptConfigurationDatabase;
        private bool _shutdownInProgress;
        private int _fixedUpdateCount;
        private int _updateCount;
        private int _lateUpdateCount;
        private
        const int DEFAULT_UPDATE_PRIORITY = -1;
        public
        const int DEFAULT_UNIVERSE_MODEL_PRIORITY = 0;
        public
        const int DEFAULT_PHYSICS_SPACE_PROVIDER_PRIORITY = 1;
        public
        const int DEFAULT_FLOATING_ORIGIN_PRIORITY = 2;
        public
        const int DEFAULT_RIGIDBODY_BEHAVIOR_VESSEL_PRIORITY = 3;
        public
        const int DEFAULT_RIGIDBODY_BEHAVIOR_PART_PRIORITY = 4;
        public
        const int DEFAULT_KERBAL_BEHAVIOR_PRIORITY = 5;
        public
        const int DEFAULT_SPACE_SIMULATION_PRIORITY = 6;
        public
        const int DEFAULT_CELESTIAL_BODY_BEHAVIOR_PRIORITY = 10;
        public
        const int DEFAULT_CELESTIAL_BODY_DATA_PROVIDER_PRIORITY = 25;
        public
        const int DEFAULT_PART_BEHAVIOR_PRIORITY = 40;
        public
        const int DEFAULT_MODULE_PRIORITY = 50;
        public
        const int DEFAULT_VIEWCONTROLLER_PRIORITY = 50;
        public
        const int DEFAULT_VESSEL_BEHAVIOR_PRIORITY = 150;
        public
        const int DEFAULT_UNIVERSE_VIEW_PRIORITY = 500;
        public
        const int DEFAULT_TIME_WARP_PRIORITY = 1000;
        private bool _isFixedUpdateListDirty;
        private readonly List < IFixedUpdate > _pendingFixedUpdateAdds = new List < IFixedUpdate > ();
        private readonly List < IFixedUpdate > _fixedUpdateList = new List < IFixedUpdate > (128);
        private readonly GameInstance.FixedUpdateComparer _fixedUpdateComparer = new GameInstance.FixedUpdateComparer();
        private
        const int DEFAULT_LATEUPDATE_PRIORITY = -1;
        public
        const int LATEUPDATE_WATER_INIT_PRIORITY = 0;
        public
        const int LATEUPDATE_OCEAN_UPDATE_PRIORITY = 1;
        public
        const int LATEUPDATE_CLEAR_WATER_DEPTH_PRIORITY = 2;
        public
        const int LATEUPDATE_DRAW_LANDBASED_WATER_PRIORITY = 3;
        public
        const int LATEUPDATE_DRAW_OCEAN_PRIORITY = 2001;
        private bool _isLateUpdateListDirty;
        private readonly List < ILateUpdate > _pendingLateUpdateAdds = new List < ILateUpdate > ();
        private readonly List < ILateUpdate > _lateUpdateList = new List < ILateUpdate > (8);
        private readonly GameInstance.LateUpdateComparer _lateUpdateComparer = new GameInstance.LateUpdateComparer();
        private bool _isUpdateListDirty;
        private readonly List < IUpdate > _pendingUpdateAdds = new List < IUpdate > ();
        private readonly List < IUpdate > _updateList = new List < IUpdate > (16);
        private readonly GameInstance.UpdateComparer _updateComparer = new GameInstance.UpdateComparer();

        public bool IsInitialized => this._isInitialized;

        public string SessionGuidString => this._sessionGuidString;

        public void SetSessionGuidString(string sessionGuidString) => this._sessionGuidString = sessionGuidString ?? "";

        public void ClearSessionGuidString() => this.SetSessionGuidString("");

        public LocalPlayer LocalPlayer => this._localPlayer;

        public CampaignPlayerManager CampaignPlayerManager => this._campaignPlayerManager;

        public AgencyManager AgencyManager => this._agencyManager;

        public AssetProvider Assets => this._assets;

        public MessageCenter Messages {
            get;
            set;
        }

        public GameStateMachine GlobalGameState {
            get;
            private set;
        }

        public GameInput Input {
            get;
            private set;
        }

        public InputManager InputManager {
            get;
            private set;
        }

        public MouseManager MouseManager {
            get;
            private set;
        }

        public PlatformManager PlatformManager {
            get;
            private set;
        }

        public PartProvider Parts {
            get;
            private set;
        }

        public MapProvider Map {
            get;
            private set;
        }

        public OABProvider OAB {
            get;
            private set;
        }

        public CelestialBodyProvider CelestialBodies {
            get;
            private set;
        }

        public PartsManagerCore PartsManager {
            get;
            set;
        }

        public ActionGroupManager ActionGroupManager {
            get;
            set;
        }

        public TripPlanner TripPlanner {
            get;
            set;
        }

        public FlightReportUIManager FlightReport {
            get;
            set;
        }

        public FlagSiteUIManager FlagSite {
            get;
            set;
        }

        public ResourceManagerUI ResourceManager {
            get;
            set;
        }

        public TripPlannerSystem TripPlannerSystem {
            get;
            set;
        }

        public KerbalManager KerbalManager {
            get;
            set;
        }

        public GraphicsManager GraphicsManager {
            get;
            set;
        }

        public NotificationManager Notifications {
            get;
            set;
        }

        public LogitechLightingManager LogitechLightingManager {
            get;
            set;
        }

        public SaveLoadDialog SaveLoadDialog {
            get;
            set;
        }

        public LaunchpadDialog LaunchpadDialog {
            get;
            set;
        }

        public TrainingCenterMenuController TrainingCenterDialog {
            get;
            set;
        }

        public ModManagerDialog ModManagerDialog {
            get;
            set;
        }

        public ColonyManagerDialog ColonyManagerDialog {
            get;
            set;
        }

        public MissionControlAction MissionControlDialog {
            get;
            set;
        }

        public MissionTrackerTooltip MissionTrackerTooltip {
            get;
            set;
        }

        public ResearchDevelopmentTechTreeController ResearchDevelopmentTechTree {
            get;
            set;
        }

        public ResearchDevelopmentController ResearchDevelopment {
            get;
            set;
        }

        public AeroGUI AeroGUI {
            get;
            set;
        }

        public VFXTestSuiteDialog VFXTestSuiteDialog {
            get;
            set;
        }

        public FXDebugTools FXDebugTools {
            get;
            set;
        }

        public PhysicsForceDisplaySystem PhysicsForceDisplaySystem {
            get;
            set;
        }

        public SaveLoadManager SaveLoadManager => this._saveLoadManager;

        public SettingsMenuManager SettingsMenuManager {
            get => this._settingsMenuManager;
            set => this._settingsMenuManager = value;
        }

        public SessionManager SessionManager {
            get => this._sessionManager;
            set => this._sessionManager = value;
        }

        public PersistentProfileManager ProfileManager => this._profileManager;

        public ResearchManager ResearchManager => this._researchManager;

        public PropertyWatcherDataBroker PropertyWatcherDataBroker => this._propertyWatcherDataBroker;

        public IOProvider IO {
            get;
            private set;
        }

        public ResourceDefinitionDatabase ResourceDefinitionDatabase {
            get;
            private set;
        }

        public ProceduralPartDatabase ProceduralPartDefinitionDatabase {
            get;
            private set;
        }

        public DynamicConfigurationDatabase DynamicConfigurationDatabase => this._dynamicConfigurationDatabase;

        public DynamicScriptConfigurationDatabase DynamicScriptConfigurationDatabase => this._dynamicScriptConfigurationDatabase;

        public Units Units {
            get;
            private set;
        }

        public PhysicsSettings PhysicsSettings {
            get;
            private set;
        }

        public ConsoleControlManager ConsoleContro {
            get;
            private set;
        }

        public SpaceSimulation SpaceSimulation {
            get;
            set;
        }

        public UniverseCameraManager CameraManager {
            get;
            set;
        }

        public UniverseModel UniverseModel {
            get;
            set;
        }

        public UniverseView UniverseView {
            get;
            set;
        }

        public ViewController ViewController {
            get;
            set;
        }

        public IScriptEnvironment ScriptEnvironment {
            get;
            set;
        }

        public CheatSystem CheatSystem {
            get;
            private set;
        }

        public KSP2ModManager KSP2ModManager {
            get;
            set;
        }

        public KSP2MissionManager KSP2MissionManager {
            get;
            set;
        }

        public StateReversionTracker stateRevTracker {
            get;
            private set;
        }

        public ScriptRegistrationManager scriptRegistrar {
            get;
            private set;
        }

        public MPFramework MP {
            get;
            private set;
        }

        public MPMonoBehaviour MPMonoBehaviour {
            get;
            private set;
        }

        public OnlineServicesFramework OnlineServices {
            get;
            private set;
        }

        public KSP.PlanetViewer.PlanetViewer PlanetViewer {
            get;
            private set;
        }

        public PhysicsSettingsManager PhysicsSettingsManager {
            get;
            set;
        }

        public List < GameModeSO > GameModes => this.gameModes;

        public Dictionary < DifficultyModeAndLevel, DifficultyOptionsData > DifficulyOptionLevels => this._difficultyOptionLevels;

        public DebugVisualizer DebugVisualizer {
            get;
            private set;
        }

        public bool FindGameModeByIndex(int index, out GameModeData gameModeDataOut) {
            if (index < 0) {
                gameModeDataOut = GameModeData.Defaults();
                return false;
            }
            if (index >= this.gameModes.Count) {
                gameModeDataOut = GameModeData.Defaults();
                return false;
            }
            gameModeDataOut = this.gameModes[index].data;
            return true;
        }

        public bool FindGameModeByName(string name, out GameModeData gameModeDataOut, out int indexOut) {
            if (name != null) {
                for (int index = 0; index < this.GameModes.Count; ++index) {
                    if (string.Compare(name, this.GameModes[index].data.Name) == 0) {
                        gameModeDataOut = this.GameModes[index].data;
                        indexOut = index;
                        return true;
                    }
                }
            }
            gameModeDataOut = GameModeData.Defaults();
            indexOut = -1;
            return false;
        }

        public bool FindGameModeByCampaignMode(
            CampaignMode campaignMode,
            out GameModeData gameModeDataOut) {
            for (int index = 0; index < this.GameModes.Count; ++index) {
                if (campaignMode == this.GameModes[index].data.campaignMode) {
                    gameModeDataOut = this.GameModes[index].data;
                    return true;
                }
            }
            gameModeDataOut = GameModeData.Defaults();
            return false;
        }

        public UIManager UI => this._ui;

        public KSPAudioInitializer Audio => this._kspAudioInitializer;

        public bool ShutdownInProgress => this._shutdownInProgress;

        public int FixedUpdateCount => this._fixedUpdateCount;

        public int UpdateCount => this._updateCount;

        public int LateUpdateCount => this._lateUpdateCount;

        private void Awake() {
            this.ClearSessionGuidString();
            this.PlatformManager = PlatformManager.Instance;
            this.PlatformManager.RegisterPlatform();
            this._localPlayer.Init();
            this._campaignPlayerManager.Init();
            this._agencyManager.Init();
            this.Units = new Units();
            this.Input = new GameInput();
            this.InputManager = this.gameObject.AddComponent < InputManager > ();
            this.IO = new IOProvider(this.transform);
            this.Parts = new PartProvider();
            this.CelestialBodies = new CelestialBodyProvider();
            this.ConsoleContro = new ConsoleControlManager();
            if ((UnityEngine.Object) this._assetPrefab != (UnityEngine.Object) null)
                this._assets = UnityEngine.Object.Instantiate < GameObject > (this._assetPrefab, this.transform).GetComponent < AssetProvider > ();
            MouseManager component;
            if ((UnityEngine.Object) this._mouseManagerPrefab != (UnityEngine.Object) null && UnityEngine.Object.Instantiate < GameObject > (this._mouseManagerPrefab, this.transform).TryGetComponent < MouseManager > (out component)) {
                this.MouseManager = component;
                this.MouseManager.Initialize(this.Input);
            }
            if ((UnityEngine.Object) this.Messages == (UnityEngine.Object) null)
                this.Messages = this.gameObject.AddComponent < MessageCenter > ();
            this.LogitechLightingManager = new LogitechLightingManager(this);
            this.GlobalGameState = new GameStateMachine(GameState.WarmUpLoading, this);
            this.Map = new MapProvider(this);
            this.OAB = new OABProvider(this);
            this.Notifications = new NotificationManager(this);
            if ((UnityEngine.Object) this._uiManagerPrefab != (UnityEngine.Object) null)
                this._ui = UnityEngine.Object.Instantiate < GameObject > (this._uiManagerPrefab, this.transform).GetComponent < UIManager > ();
            this._saveLoadManager = this.gameObject.AddComponent < SaveLoadManager > ();
            if ((UnityEngine.Object) this.PhysicsForceDisplaySystem == (UnityEngine.Object) null)
                this.PhysicsForceDisplaySystem = this.gameObject.AddComponent < PhysicsForceDisplaySystem > ();
            GameInput.GlobalActions global = this.Input.Global;
            GameInput.FlightActions flight = this.Input.Flight;
            this.InputManager.AddDefinition < FlightInputDefinition > (new FlightInputDefinition(this.Input));
            this.InputManager.AddDefinition < EVAInputDefinition > (new EVAInputDefinition(this.Input));
            this.InputManager.AddDefinition < OabInputDefinition > (new OabInputDefinition(this.Input));
            this.InputManager.AddDefinition < MapViewInputDefinition > (new MapViewInputDefinition(this.Input));
            this.InputManager.AddDefinition < GlobalInputDefinition > (new GlobalInputDefinition(this.Input));
            this.InputManager.AddDefinition < RDInputDefinition > (new RDInputDefinition(this.Input));
            this.InputManager.AddDefinition < KSCInputDefinition > (new KSCInputDefinition(this.Input));
            this.InputManager.AddDefinition < AudioInputDefinition > (new AudioInputDefinition(this.Input));
            this.InputManager.SetInputLock(InputLocks.GlobalInputEnabled);
            this.InputManager.SetInputLock(InputLocks.AudioInputEnabled);
            this.InputManager.SetInputLock(InputLocks.FlightInputDisabled);
            this.InputManager.SetInputLock(InputLocks.EVAInputDisabled);
            this.InputManager.SetInputLock(InputLocks.OABInputDisabled);
            this.InputManager.SetInputLock(InputLocks.MapViewInputDisabled);
            this.InputManager.SetInputLock(InputLocks.RDInputDisabled);
            this.InputManager.SetInputLock(InputLocks.KSCInputDisabled);
            FlightInputDefinition definition1;
            if (this.InputManager.TryGetInputDefinition < FlightInputDefinition > (out definition1)) {
                definition1.UnbindAction(flight.ToggleAeroGUI.name, new Action(this.OnToggleAeroGUI));
                definition1.BindAction(flight.ToggleAeroGUI.name, new Action(this.OnToggleAeroGUI));
            }
            GlobalInputDefinition definition2;
            if (!this.InputManager.TryGetInputDefinition < GlobalInputDefinition > (out definition2))
                return;
            definition2.UnbindAction(global.TogglePartsManager.name, new Action(this.OnTogglePartsManager));
            definition2.BindAction(global.TogglePartsManager.name, new Action(this.OnTogglePartsManager));
        }

        private void OnDestroy() {
            GameInput.GlobalActions global = this.Input.Global;
            GlobalInputDefinition definition1;
            if (this.InputManager.TryGetInputDefinition < GlobalInputDefinition > (out definition1))
                definition1.UnbindAction(global.TogglePartsManager.name, new Action(this.OnTogglePartsManager));
            GameInput.FlightActions flight = this.Input.Flight;
            FlightInputDefinition definition2;
            if (this.InputManager.TryGetInputDefinition < FlightInputDefinition > (out definition2))
                definition2.UnbindAction(flight.ToggleAeroGUI.name, new Action(this.OnToggleAeroGUI));
            this.InputManager.SetInputLock(InputLocks.GlobalInputDisabled);
            this.InputManager.SetInputLock(InputLocks.AudioInputDisabled);
            this.InputManager.Dispose();
            if (this._scriptInterop != null)
                this.UnregisterFixedUpdate(this._scriptInterop as IFixedUpdate);
            WaterManager.Destroy();
            this.LogitechLightingManager.Destroy();
        }

        public bool InitializeDependencies(out string error) {
            if (this._isInitialized) {
                error = "GameInstance is already initialized.";
                return false;
            }
            TypeInterop typeInterop = new TypeInterop();
            this._kspAudioInitializer = UnityEngine.Object.Instantiate < KSPAudioInitializer > (this._kspAudioInitializerPrefab, this.transform);
            new GameObject("Mods").transform.parent = this.transform;
            IScriptInterop scriptInterop = (IScriptInterop) new ScriptInteroperability(typeInterop);
            this.RegisterFixedUpdate(scriptInterop as IFixedUpdate);
            this.ScriptEnvironment = scriptInterop.RootEnvironment;
            IScriptObject scriptObject = this.ScriptEnvironment.RegisterScriptObject("LuaPipeDebug");
            scriptObject.SetCallback < Func < bool, int >> ("SetDataLinkEnabled", new Func < bool, int > (this.SetDataLinkEnabled));
            scriptObject.SetCallback < Func < bool >> ("GetDataLinkEnabled", new Func < bool > (this.GetDataLinkEnabled));
            this._scriptRunner = (IScriptRunner) new ScriptRunner(this.ScriptEnvironment);
            this._dynamicConfigurationDatabase = new DynamicConfigurationDatabase();
            this._dynamicScriptConfigurationDatabase = new DynamicScriptConfigurationDatabase(this._dynamicConfigurationDatabase, this._scriptRunner);
            this.KSP2ModManager = this.gameObject.AddComponent < KSP2ModManager > ();
            this.KSP2ModManager.RegisterEnvironment(this.ScriptEnvironment);
            SettingsMenuManager.Initialize();
            this.KSP2MissionManager = this.gameObject.AddComponent < KSP2MissionManager > ();
            this.MP = new MPFramework(this);
            this.MP.Init(this, this.ScriptEnvironment);
            this.MPMonoBehaviour = this.gameObject.AddComponent < MPMonoBehaviour > ();
            this.OnlineServices = new OnlineServicesFramework(this);
            this.stateRevTracker = new StateReversionTracker();
            this.scriptRegistrar = new ScriptRegistrationManager();
            this.PhysicsSettings = new PhysicsSettings();
            this.PhysicsSettingsManager = new PhysicsSettingsManager();
            this.PhysicsSettingsManager.Initialize();
            MapMagicValues.Initialize();
            GameObject gameObject1 = new GameObject("ToolFrameworkRoot");
            LuaDebugPanel luaDebugPanel = gameObject1.AddComponent < LuaDebugPanel > ();
            ScriptUIMgr scriptUiMgr = gameObject1.AddComponent < ScriptUIMgr > ();
            this.DebugVisualizer = gameObject1.AddComponent < DebugVisualizer > ();
            gameObject1.transform.parent = this.transform;
            IScriptEnvironment scriptEnvironment1 = this.ScriptEnvironment;
            scriptUiMgr.RegisterEnvironment(scriptEnvironment1);
            IScriptEnvironment scriptEnvironment2 = this.ScriptEnvironment;
            luaDebugPanel.RegisterEnvironment(scriptEnvironment2);
            this.DebugVisualizer.RegisterEnvironment(this.ScriptEnvironment);
            GameObject gameObject2 = new GameObject("CheatSystem");
            CheatSystem cheatSystem = gameObject2.AddComponent < CheatSystem > ();
            cheatSystem.RegisterEnvironment(this.ScriptEnvironment);
            this.CheatSystem = cheatSystem;
            gameObject2.AddComponent < SetLanguageExtended > ();
            this.ResourceDefinitionDatabase = new ResourceDefinitionDatabase();
            this.ProceduralPartDefinitionDatabase = new ProceduralPartDatabase();
            this._sessionManager = new SessionManager();
            this._sessionManager.RegisterEnvironment(this.ScriptEnvironment);
            this._profileManager = new PersistentProfileManager();
            this._profileManager.Initialize();
            this.PhysicsForceDisplaySystem.Initialize();
            VFXTestSuiteDialog.Initialize();
            FXDebugTools.Initialize();
            this._propertyWatcherDataBroker = new PropertyWatcherDataBroker();
            this.LogitechLightingManager.ConnectEvents();
            this.LoadDifficultyData();
            this.DebugVisualizer.SetShowFPS(PersistentProfileManager.ShowFPS);
            this._isInitialized = true;
            error = (string) null;
            return true;
        }

        public void KeepAliveNetworkPump() => this.MP.Pump();

        public void CreatePlanetViewer() {
            if (!((UnityEngine.Object) this.PlanetViewer == (UnityEngine.Object) null))
                return;
            this.PlanetViewer = this.gameObject.AddComponent < KSP.PlanetViewer.PlanetViewer > ();
        }

        public void ResetUniverse(Action finishedCallback) {
            if (this._shutdownInProgress)
                return;
            this._shutdownInProgress = true;
            this.StartCoroutine(this.CoroutineResetUniverse(finishedCallback));
        }

        private IEnumerator CoroutineResetUniverse(Action finishedCallback) {
            this.GlobalGameState.SetState(GameState.MainMenu);
            this._fixedUpdateList.Clear();
            if ((UnityEngine.Object) this.PartsManager != (UnityEngine.Object) null)
                this.PartsManager.Shutdown();
            yield
            return (object) null;
            this.ViewController?.Shutdown();
            this.SpaceSimulation?.UnloadAll();
            yield
            return (object) null;
            this.CelestialBodies?.Shutdown();
            yield
            return (object) null;
            this.Parts?.Shutdown(false);
            this.OAB?.Shutdown();
            yield
            return (object) null;
            this._dynamicScriptConfigurationDatabase?.Shutdown();
            yield
            return (object) null;
            this.stateRevTracker.Shutdown();
            yield
            return (object) null;
            this.UI.ClearAllModules();
            yield
            return (object) null;
            this.SessionManager.ShutdownSession();
            this._shutdownInProgress = false;
            finishedCallback();
        }

        public void Shutdown(Action finishedCallback) {
            if (this._shutdownInProgress)
                return;
            this._shutdownInProgress = true;
            this.StartCoroutine(this.CoroutineShutdown(finishedCallback));
        }

        private IEnumerator CoroutineShutdown(Action finishedCallback) {
            this.GlobalGameState.SetState(GameState.MainMenu);
            this.LogitechLightingManager.DisconnectEvents();
            if ((UnityEngine.Object) this.MP.GameInstance != (UnityEngine.Object) null && this.MP.GameInstance.LocalPlayer != null)
                this.MP.TeardownLocalPlayerInfo();
            if ((UnityEngine.Object) this.PartsManager != (UnityEngine.Object) null)
                this.PartsManager.Shutdown();
            yield
            return (object) null;
            this.SpaceSimulation?.UnloadAll();
            this.ViewController?.Shutdown();
            yield
            return (object) null;
            this.CelestialBodies?.Shutdown();
            yield
            return (object) null;
            this.Parts?.Shutdown(true);
            this.OAB?.Shutdown();
            if ((UnityEngine.Object) this.KSP2MissionManager != (UnityEngine.Object) null)
                this.KSP2MissionManager.Shutdown();
            yield
            return (object) null;
            this._dynamicScriptConfigurationDatabase?.Shutdown();
            yield
            return (object) null;
            this.stateRevTracker.Shutdown();
            yield
            return (object) null;
            this.UI.ClearAllModules();
            yield
            return (object) null;
            if ((UnityEngine.Object) this.PhysicsForceDisplaySystem != (UnityEngine.Object) null)
                this.PhysicsForceDisplaySystem.Shutdown();
            yield
            return (object) null;
            this.Messages.Shutdown();
            this.SaveLoadManager.ClearIsLoadedFlag();
            this.SessionManager.ShutdownSession();
            this._shutdownInProgress = false;
            finishedCallback();
        }

        public void HandleException(Exception e) => GlobalLog.Error((object) e.ToString());

        public void MarkExecutionPriorityDirty() {
            GlobalLog.Log(LogFilter.General, (object)
                "Execution Priority marked Dirty");
            this._isFixedUpdateListDirty = true;
            this._isUpdateListDirty = true;
        }

        public bool IsSimulationRunning() => this.SpaceSimulation != null && this.SpaceSimulation.IsEnabled;

        private void FixedUpdate() {
            ++this._fixedUpdateCount;
            if (this._pendingFixedUpdateAdds.Count > 0) {
                foreach(IFixedUpdate pendingFixedUpdateAdd in this._pendingFixedUpdateAdds)
                this._fixedUpdateList.Add(pendingFixedUpdateAdd);
                this._pendingFixedUpdateAdds.Clear();
                this._fixedUpdateList.Sort((IComparer < IFixedUpdate > ) this._fixedUpdateComparer);
            }
            float fixedDeltaTime = Time.fixedDeltaTime;
            int count = this._fixedUpdateList.Count;
            for (int index = 0; index < count; ++index) {
                IFixedUpdate fixedUpdate = this._fixedUpdateList[index];
                if (fixedUpdate != null) {
                    if (!fixedUpdate.Equals((object) null)) {
                        try {
                            fixedUpdate.OnFixedUpdate(fixedDeltaTime);
                            continue;
                        } catch (Exception ex) {
                            GlobalLog.Error(LogFilter.Physics, (object)(ex.Message + "\n\n" + ex.StackTrace + "\n"));
                            continue;
                        }
                    }
                }
                this._isFixedUpdateListDirty = true;
            }
            if (!this._isFixedUpdateListDirty)
                return;
            this._fixedUpdateList.Sort((IComparer < IFixedUpdate > ) this._fixedUpdateComparer);
            while (this._fixedUpdateList.Count > 0 && this._fixedUpdateList[this._fixedUpdateList.Count - 1] == null)
                this._fixedUpdateList.RemoveAt(this._fixedUpdateList.Count - 1);
            this._isFixedUpdateListDirty = false;
        }

        public void RegisterFixedUpdate(IFixedUpdate item) => this._pendingFixedUpdateAdds.Add(item);

        public void UnregisterFixedUpdate(IFixedUpdate item) {
            int index = this._fixedUpdateList.IndexOf(item);
            if (index >= 0) {
                this._fixedUpdateList[index] = (IFixedUpdate) null;
                this._isFixedUpdateListDirty = true;
            } else {
                index = this._pendingFixedUpdateAdds.IndexOf(item);
                if (index >= 0) {
                    this._pendingFixedUpdateAdds[index] = (IFixedUpdate) null;
                    this._isFixedUpdateListDirty = true;
                }
            }
        }

        private void LateUpdate() {
            ++this._lateUpdateCount;
            if (this._pendingLateUpdateAdds.Count > 0) {
                foreach(ILateUpdate pendingLateUpdateAdd in this._pendingLateUpdateAdds) {
                    if (pendingLateUpdateAdd != null)
                        this._lateUpdateList.Add(pendingLateUpdateAdd);
                }
                this._pendingLateUpdateAdds.Clear();
                this._lateUpdateList.Sort((IComparer < ILateUpdate > ) this._lateUpdateComparer);
            }
            int count = this._lateUpdateList.Count;
            for (int index = 0; index < count; ++index) {
                ILateUpdate lateUpdate = this._lateUpdateList[index];
                if (lateUpdate != null) {
                    if (!lateUpdate.Equals((object) null)) {
                        try {
                            lateUpdate.OnLateUpdate();
                            continue;
                        } catch (Exception ex) {
                            GlobalLog.Error(LogFilter.General, (object)(ex.Message + "\n\n" + ex.StackTrace + "\n"));
                            continue;
                        }
                    }
                }
                this._isLateUpdateListDirty = true;
            }
            if (!this._isLateUpdateListDirty)
                return;
            this._lateUpdateList.Sort((IComparer < ILateUpdate > ) this._lateUpdateComparer);
            while (this._lateUpdateList.Count > 0 && this._lateUpdateList[this._lateUpdateList.Count - 1] == null)
                this._lateUpdateList.RemoveAt(this._lateUpdateList.Count - 1);
            this._isLateUpdateListDirty = false;
        }

        public void RegisterLateUpdate(ILateUpdate item) => this._pendingLateUpdateAdds.Add(item);

        public void UnregisterLateUpdate(ILateUpdate item) {
            int index = this._lateUpdateList.IndexOf(item);
            if (index >= 0) {
                this._lateUpdateList[index] = (ILateUpdate) null;
            } else {
                index = this._pendingLateUpdateAdds.IndexOf(item);
                if (index >= 0)
                    this._pendingLateUpdateAdds[index] = (ILateUpdate) null;
            }
        }

        public void RegisterUpdate(IUpdate item) => this._pendingUpdateAdds.Add(item);

        public void UnregisterUpdate(IUpdate item) {
            int index = this._updateList.IndexOf(item);
            if (index >= 0) {
                this._updateList[index] = (IUpdate) null;
                this._isUpdateListDirty = true;
            } else {
                index = this._pendingUpdateAdds.IndexOf(item);
                if (index >= 0) {
                    this._pendingUpdateAdds[index] = (IUpdate) null;
                    this._isUpdateListDirty = true;
                }
            }
        }

        private void Update() {
            ++this._updateCount;
            if (this._pendingUpdateAdds.Count > 0) {
                foreach(IUpdate pendingUpdateAdd in this._pendingUpdateAdds) {
                    if (pendingUpdateAdd != null)
                        this._updateList.Add(pendingUpdateAdd);
                }
                this._pendingUpdateAdds.Clear();
                this._updateList.Sort((IComparer < IUpdate > ) this._updateComparer);
            }
            this.PlatformManager.RunUpdate();
            float deltaTime = Time.deltaTime;
            int count = this._updateList.Count;
            for (int index = 0; index < count; ++index) {
                IUpdate update = this._updateList[index];
                if (update != null) {
                    if (!update.Equals((object) null)) {
                        try {
                            update.OnUpdate(deltaTime);
                            continue;
                        } catch (Exception ex) {
                            GlobalLog.Error(LogFilter.General, (object)(ex.Message + "\n\n" + ex.StackTrace + "\n"));
                            continue;
                        }
                    }
                }
                this._isUpdateListDirty = true;
            }
            if (!this._isUpdateListDirty)
                return;
            this._updateList.Sort((IComparer < IUpdate > ) this._updateComparer);
            while (this._updateList.Count > 0 && this._updateList[this._updateList.Count - 1] == null)
                this._updateList.RemoveAt(this._updateList.Count - 1);
            this._isUpdateListDirty = false;
        }

        public int SetDataLinkEnabled(bool enabled) {
            this.ScriptEnvironment.DataLinkEnabled = enabled;
            return 0;
        }

        public bool GetDataLinkEnabled() => this.ScriptEnvironment.DataLinkEnabled;

        public void OnTogglePartsManager() {
            if ((UnityEngine.Object) this.PartsManager == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Parts Manager is not loaded.");
            } else {
                if (!this.PartsManager.IsVisible)
                    this.PartsManager.PartsList.SetPartOwnerId(new IGGuid());
                this.PartsManager.IsVisible = !this.PartsManager.IsVisible;
            }
        }

        public void ShowSaveLoadDialog(bool isLoading) {
            if ((UnityEngine.Object) this.SaveLoadDialog == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Save/load dialog is not loaded.");
            } else {
                this.SaveLoadDialog.transform.SetAsLastSibling();
                this.SaveLoadDialog.IsLoading = isLoading;
                this.SaveLoadDialog.SetVisiblity(true);
            }
        }

        public void HideSaveLoadDialog() {
            if ((UnityEngine.Object) this.SaveLoadDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Save/load dialog is not loaded.");
            else
                this.SaveLoadDialog.SetVisiblity(false);
        }

        public void ShowLaunchpadDialog() {
            this.Messages.Publish < DismissAllNotificationsMessage > ();
            if ((UnityEngine.Object) this.LaunchpadDialog == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Launchpad dialog is not loaded.");
            } else {
                this.LaunchpadDialog.transform.SetAsLastSibling();
                this.LaunchpadDialog.SetVisible(true);
            }
        }

        public void HideLaunchpadDialog() {
            if ((UnityEngine.Object) this.LaunchpadDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Launchpad dialog is not loaded.");
            else
                this.LaunchpadDialog.SetVisible(false);
        }

        public void TransitionToTrainingCenter() {
            this.Messages.Publish < DismissAllNotificationsMessage > ();
            this.UI.HidePause();
            this.UI.SetCurtainContext(CurtainContext.ToTrainingCenter);
            this.UI.SetCurtainVisibility(true, (Action)(() => {
                this.ShowTrainingCenter();
                Mouse.EnableVirtualCursor(true);
                this.UI.SetCurtainVisibility(false);
                this.Messages.Publish < TrainingCenterLoadedMessage > ();
            }));
        }

        private void ShowTrainingCenter() {
            if ((UnityEngine.Object) this.TrainingCenterDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mission dialog is not loaded.");
            else if (!TrainingCenterMenuController.BackgroundScene.IsValid()) {
                GlobalLog.Error((object)
                    "(ShowTrainingCenter) Background scene not valid. Returning early until fixed.");
            } else {
                if ((UnityEngine.Object) TrainingCenterMenuController.TrainingCenterBackgroundInstance == (UnityEngine.Object) null) {
                    foreach(GameObject rootGameObject in TrainingCenterMenuController.BackgroundScene.GetRootGameObjects()) {
                        if (rootGameObject.name == TrainingCenterMenuController.BackgroundAssetName)
                            TrainingCenterMenuController.TrainingCenterBackgroundInstance = rootGameObject;
                    }
                }
                if ((UnityEngine.Object) TrainingCenterMenuController.TrainingCenterBackgroundInstance != (UnityEngine.Object) null) {
                    this.TrainingCenterDialog.transform.SetAsLastSibling();
                    this.TrainingCenterDialog.SetVisible();
                } else
                    GlobalLog.Error((object)("(ShowTrainingCenter) Failed to find Training Center Background root to load named " + TrainingCenterMenuController.BackgroundAssetName));
                if (this.UI.ViewController.CurrentView == UIStateViews.TrainingCenterView || this.UI.ViewController.CurrentView == UIStateViews.MissionControl)
                    this.UI.ViewController.PushViewKeepPrevious(UIStateViews.TrainingCenterView);
                else
                    this.UI.ViewController.PushView(UIStateViews.TrainingCenterView);
            }
        }

        public void HideTrainingCenterDialog() {
            if ((UnityEngine.Object) this.TrainingCenterDialog == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mission dialog is not loaded.");
            } else {
                this.CameraManager?.RevertToPreviousPrimaryScreenCamera();
                TrainingCenterMenuController.TrainingCenterBackgroundInstance?.SetActive(false);
                this.TrainingCenterDialog.DismissDialog();
                this.UI.ViewController.RestorePreviousView();
            }
        }

        public void ShowModManagerDialog() {
            if ((UnityEngine.Object) this.ModManagerDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mod manager dialog is not loaded.");
            else
                this.ModManagerDialog.IsVisible = true;
        }

        public void HideModManagerDialog() {
            if ((UnityEngine.Object) this.ModManagerDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mod Manager dialog is not loaded.");
            else
                this.ModManagerDialog.IsVisible = false;
        }

        public void ShowColonyManagerDialog() {
            if ((UnityEngine.Object) this.ColonyManagerDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Colony manager dialog is not loaded.");
            else
                this.ColonyManagerDialog.IsVisible = true;
        }

        public void HideColonyManagerDialog() {
            if ((UnityEngine.Object) this.ColonyManagerDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Colony Manager dialog is not loaded.");
            else
                this.ColonyManagerDialog.IsVisible = false;
        }

        public void TransitionToMissionControl() {
            this.Messages.Publish < DismissAllNotificationsMessage > ();
            this.UI.HidePause();
            this.UI.SetCurtainContext(CurtainContext.ToMissionControl);
            this.UI.SetCurtainVisibility(true, (Action)(() => {
                this.ShowMissionControl();
                Mouse.EnableVirtualCursor(true);
                this.UI.SetCurtainVisibility(false);
                this.Messages.Publish < MissionControlLoadedMessage > ();
            }));
        }

        public void ShowMissionControl() {
            if ((UnityEngine.Object) this.MissionControlDialog == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mission Control is not loaded.");
            else if (!MissionControlAction.BackgroundScene.IsValid()) {
                GlobalLog.Error((object)
                    "(ShowMissionControl) Background scene not valid. Returning early until fixed.");
            } else {
                if ((UnityEngine.Object) MissionControlAction.MissionControlBackgroundInstance == (UnityEngine.Object) null) {
                    foreach(GameObject rootGameObject in MissionControlAction.BackgroundScene.GetRootGameObjects()) {
                        if (rootGameObject.name == MissionControlAction.BackgroundAssetName)
                            MissionControlAction.MissionControlBackgroundInstance = rootGameObject;
                    }
                }
                if ((UnityEngine.Object) MissionControlAction.MissionControlBackgroundInstance != (UnityEngine.Object) null) {
                    this.MissionControlDialog.ToggleMissionControlScene(true);
                    this.ShowMissionControlCamera();
                    this.MissionControlDialog.transform.SetAsLastSibling();
                    this.MissionControlDialog.SetVisible();
                    KSPAudioEventManager.onGameFlowEnterMissionControl();
                } else
                    GlobalLog.Error((object)("(ShowMissionControl) Failed to find Training Center Background root to load named " + TrainingCenterMenuController.BackgroundAssetName));
                if (this.UI.ViewController.CurrentView == UIStateViews.TrainingCenterView || this.UI.ViewController.CurrentView == UIStateViews.MissionControl)
                    this.UI.ViewController.PushViewKeepPrevious(UIStateViews.MissionControl);
                else
                    this.UI.ViewController.PushView(UIStateViews.MissionControl);
            }
        }

        private void ShowMissionControlCamera() {
            if ((UnityEngine.Object) MissionControlAction.MissionControlBackgroundInstanceCamera == (UnityEngine.Object) null)
                MissionControlAction.MissionControlBackgroundInstanceCamera = MissionControlAction.MissionControlBackgroundInstance?.GetComponentInChildren < Camera > (false);
            if ((UnityEngine.Object) MissionControlAction.MissionControlBackgroundInstanceCamera != (UnityEngine.Object) null) {
                MissionControlAction.MissionControlBackgroundInstanceCamera.gameObject.SetActive(false);
                this.CameraManager.SetPrimaryScreenCamera(CameraID.UI);
                MissionControlAction.MissionControlBackgroundInstanceCamera.gameObject.SetActive(true);
            } else
                GlobalLog.Error((object)
                    "(ShowMissionControlCamera) Failed to find camera for mission control background scene!");
        }

        public void HideMissionControl() {
            if ((UnityEngine.Object) this.MissionControlDialog == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Mission Control is not loaded.");
            } else {
                this.CameraManager?.RevertToPreviousPrimaryScreenCamera();
                this.MissionControlDialog.ToggleMissionControlScene(false);
                this.MissionControlDialog.DismissMissionControl();
                KSPAudioEventManager.onGameFlowLeaveMissionControl();
                this.UI.ViewController.RestorePreviousView();
            }
        }

        public void TransitionToResearchAndDevelopment() {
            this.Messages.Publish < DismissAllNotificationsMessage > ();
            this.UI.HidePause();
            this.UI.SetCurtainContext(CurtainContext.ToResearchAndDevelopment);
            this.UI.SetCurtainVisibility(true, (Action)(() => {
                this.ShowResearchAndDevelopment();
                Mouse.EnableVirtualCursor(true);
                this.UI.SetCurtainVisibility(false);
            }));
        }

        private void ShowResearchAndDevelopment() {
            if ((UnityEngine.Object) this.ResearchDevelopment == (UnityEngine.Object) null) {
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "Research Development is not loaded.");
            } else {
                this.ResearchDevelopment.transform.SetAsLastSibling();
                this.GlobalGameState.SetState(GameState.ResearchAndDevelopment);
            }
        }

        public void OnToggleAeroGUI() {
            if ((UnityEngine.Object) this.AeroGUI == (UnityEngine.Object) null)
                GlobalLog.Warn(LogFilter.Gameplay, (object)
                    "AeroGUI is not loaded.");
            else
                this.AeroGUI.IsVisible = !this.AeroGUI.IsVisible;
        }

        public void BroadcastCheatValueChanged(
            CheatSystemItemID cheatSystemItemID,
            bool oldValue,
            bool newValue) {
            if (cheatSystemItemID != CheatSystemItemID.UnbreakableJoints || this.UniverseModel == null || this.SpaceSimulation == null)
                return;
            List < VesselComponent > allVessels = this.UniverseModel.GetAllVessels();
            if (allVessels == null)
                return;
            for (int index = 0; index < allVessels.Count; ++index) {
                VesselComponent vesselComponent = allVessels[index];
                PartOwnerBehavior viewObjectComponent;
                if (vesselComponent != null && vesselComponent.SimulationObject != null && this.SpaceSimulation.TryGetViewObjectComponent < PartOwnerBehavior > (vesselComponent.SimulationObject, out viewObjectComponent))
                    viewObjectComponent.SetJointsUnbreakable(newValue);
            }
        }

        private void LoadDifficultyData() => this.Assets.LoadByLabel < TextAsset > ("difficulty", new Action < TextAsset > (this.OnDifficultyAssetLoaded), (Action < IList < TextAsset >> )(allDifficultyOptions => this.Assets.ReleaseAsset < IList < TextAsset >> (allDifficultyOptions)));

        private void OnDifficultyAssetLoaded(TextAsset difficultyTextAsset) {
            DifficultyOptionsData difficultyOptionsData = IOProvider.FromJson < DifficultyOptionsData > (difficultyTextAsset.text);
            this._difficultyOptionLevels.Add(difficultyOptionsData.name, difficultyOptionsData);
        }

        private class FixedUpdateComparer: GameInstance.NullComparer < IFixedUpdate > {
            protected override int CompareTo(IFixedUpdate x, IFixedUpdate y) {
                int num1 = -1;
                int num2 = -1;
                if (x is IPriorityOverride priorityOverride1)
                    num1 = priorityOverride1.ExecutionPriorityOverride;
                if (y is IPriorityOverride priorityOverride2)
                    num2 = priorityOverride2.ExecutionPriorityOverride;
                return num1.CompareTo(num2);
            }
        }

        private class LateUpdateComparer: GameInstance.NullComparer < ILateUpdate > {
            protected override int CompareTo(ILateUpdate x, ILateUpdate y) {
                int num1 = -1;
                int num2 = -1;
                if (x is IPriorityOverride priorityOverride1)
                    num1 = priorityOverride1.ExecutionPriorityOverride;
                if (y is IPriorityOverride priorityOverride2)
                    num2 = priorityOverride2.ExecutionPriorityOverride;
                return num1.CompareTo(num2);
            }
        }

        private class UpdateComparer: GameInstance.NullComparer < IUpdate > {
            protected override int CompareTo(IUpdate x, IUpdate y) {
                int num1 = -1;
                int num2 = -1;
                if (x is IPriorityOverride priorityOverride1)
                    num1 = priorityOverride1.ExecutionPriorityOverride;
                if (y is IPriorityOverride priorityOverride2)
                    num2 = priorityOverride2.ExecutionPriorityOverride;
                return num1.CompareTo(num2);
            }
        }

        private class NullComparer < T >: IComparer < T > {
            public int Compare(T x, T y) {
                if ((object) x == null && (object) y == null)
                    return 0;
                if ((object) x == null)
                    return 1;
                return (object) y == null ? -1 : this.CompareTo(x, y);
            }

            protected virtual int CompareTo(T x, T y) => 0;
        }
    }
}
```
#### Partie science de KSP2, non accessible.
```csharp
using KSP.Game;
using KSP.Game.Load;
using KSP.Logging;
using KSP.Sim;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSP.Research {
    public class ResearchManager {
        public List < Technology > Technologies;

        private GameInstance _game => GameManager.Instance.Game;

        public Technology GetTechnologyByID(string ID) {
            Technology technologyById = (Technology) null;
            int index = this.Technologies.FindIndex((Predicate < Technology > )(x => x.ID == ID));
            if (index != -1)
                technologyById = this.Technologies[index];
            return technologyById;
        }

        public void Initialize() => this.LoadTechTreeFromAddressableData();

        public void UpdateTechTreeWithSaveGameData(LoadGameData gameData) {
            if (gameData == null)
                return;
            SerializedSavedGame savedGame = gameData.SavedGame;
        }

        public void ApplyTechTreeToSaveGame(LoadGameData gameData) {}

        private void LoadTechTreeFromAddressableData() {
            this.Technologies = new List < Technology > ();
            this._game.Assets.LoadByLabel < TextAsset > ("technology", (Action < TextAsset > ) null, (Action < IList < TextAsset >> )(allTechnologies => {
                foreach(TextAsset allTechnology in (IEnumerable < TextAsset > ) allTechnologies) {
                    try {
                        this.Technologies.Add(JsonConvert.DeserializeObject < Technology > (allTechnology.text));
                    } catch (Exception ex) {
                        GlobalLog.ErrorF(LogFilter.Gameplay, "Unable to deserialize technology from asset " + allTechnology.name + " : " + ex.Message);
                    }
                }
                foreach(Technology technology in this.Technologies)
                technology.Initialize();
            }));
        }

        public ref List < Technology > GetTechnologies() => ref this.Technologies;
    }
}
```
```csharp
using KSP.Game;
using KSP.Logging;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace KSP.Research {
    public class Technology {
        public string ID;
        public string Name;
        public string IconAssetName;
        private Texture2D _icon;
        public bool IconLoadFinished;
        public TechnologyCategoryType TechnologyCategory;
        public string Description;
        public int ScienceCost;
        public List < Benefit > Benefits = new List < Benefit > ();
        public List < string > Prerequisites;
        public PrerequisiteLogicType PrerequisiteLogic;
        public TechnologyVisibilityType TechnologyVisibility;
        public Vector2 TechTreePosition;
        public List < Technology > Parents = new List < Technology > ();
        public List < Technology > Children = new List < Technology > ();

        private GameInstance _game => GameManager.Instance.Game;

        public Texture2D Icon => this._icon;

        public void Initialize() {
            if (!string.IsNullOrEmpty(this.IconAssetName))
                this._game.Assets.Load < Texture2D > (this.IconAssetName, (Action < Texture2D > )(result => {
                    if ((UnityEngine.Object) result == (UnityEngine.Object) null)
                        GlobalLog.ErrorF(LogFilter.Gameplay, "Error: unable to load icon for Technology " + this.ID);
                    this._icon = result;
                    this.IconLoadFinished = true;
                }), true);
            else
                this.IconLoadFinished = true;
            foreach(string prerequisite in this.Prerequisites) {
                Technology technologyById = this._game.ResearchManager.GetTechnologyByID(prerequisite);
                if (technologyById == null) {
                    GlobalLog.ErrorF(LogFilter.Gameplay, "Error: unable to find technology with ID " + prerequisite);
                } else {
                    technologyById.Children.Add(this);
                    this.Parents.Add(technologyById);
                }
            }
            this.UpdateVisibilityState();
        }

        public void UpdateVisibilityState() {
            if (this.TechnologyVisibility == TechnologyVisibilityType.Owned)
                return;
            this.Parents.RemoveAll((Predicate < Technology > )(x => x == null));
            if (this.Parents.Count == 0)
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            else if (this.Parents.Count == 1) {
                if (this.Parents[0].TechnologyVisibility != TechnologyVisibilityType.Owned || this.TechnologyVisibility != TechnologyVisibilityType.Hidden)
                    return;
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            } else if (this.PrerequisiteLogic == PrerequisiteLogicType.And) {
                bool flag = true;
                foreach(Technology parent in this.Parents) {
                    if (parent.TechnologyVisibility != TechnologyVisibilityType.Owned)
                        flag = false;
                }
                if (!flag)
                    return;
                this.TechnologyVisibility = TechnologyVisibilityType.Visible;
            } else {
                if (this.PrerequisiteLogic != PrerequisiteLogicType.Or)
                    return;
                foreach(Technology parent in this.Parents) {
                    if (parent.TechnologyVisibility == TechnologyVisibilityType.Owned)
                        this.TechnologyVisibility = TechnologyVisibilityType.Visible;
                }
            }
        }

        public void Unlock() {
            this.TechnologyVisibility = TechnologyVisibilityType.Owned;
            foreach(Benefit benefit in this.Benefits)
            benefit.Apply();
            foreach(Technology child in this.Children)
            child.UpdateVisibilityState();
        }
    }
}
```
