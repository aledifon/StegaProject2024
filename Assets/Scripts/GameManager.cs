using DG.Tweening;
using System;
using System.Collections;
using UnityEditor;
//using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

using static ScenesEnum;
using static MenuSceneStateEnum;
using static LevelSceneStateEnum;
using static UIMenuSelectEnum;
using static ItemTypeEnum;
using static GhostPathsEnum;

using TMPro;
using System.Collections.Generic;
using System.Linq;
public class GameManager : MonoBehaviour
{
    // Singleton instance of GameManager
    private static GameManager instance;
    public static GameManager Instance => instance;
    //public static GameManager Instance
    //{
    //    get 
    //    { 
    //        if (instance == null)
    //        {
    //            instance = FindAnyObjectByType<GameManager>();

    //            if (instance == null)
    //            {
    //                Debug.LogWarning("No GameManager found in scene. Creating a new one.");

    //                Debug.Log("GameManager.Instance called from:\n" + Environment.StackTrace);

    //                GameObject go = new GameObject("GameManager");
    //                instance = go.AddComponent<GameManager>();                    
    //            }
    //        }
    //        return instance; 
    //    }
    //}

    public static readonly bool isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
    //public static readonly bool isWebGL = false;     // true = WebGL, false = Windows

    [Header("Audio Clips")]
    // Level Scene
    [SerializeField] AudioClip gameOverClip;
    [SerializeField, Range(0f, 1f)] float gameOverVolume;  
    [SerializeField] AudioClip endOfLevelClip;
    [SerializeField, Range(0f, 1f)] float endOfLevelVolume;
    [SerializeField] AudioClip deathClip;
    [SerializeField, Range(0f, 1f)] float deathVolume;
    [SerializeField] AudioClip levelMusicClip;
    [SerializeField, Range(0f, 1f)] float levelMusicVolume;  // 1f
    [SerializeField] AudioClip endGameMusicClip;
    [SerializeField, Range(0f, 1f)] float endGameMusicVolume;  // 1f
    // Menu Scene
    [SerializeField] AudioClip menuMusicClip;
    [SerializeField, Range(0f, 1f)] float menuMusicVolume;  // 1f
    [SerializeField] AudioClip menuSwitchOptionClip;
    [SerializeField, Range(0f, 1f)] float menuSwitchOptionVolume;  // 1f
    [SerializeField] AudioClip menuSelectOptionClip;
    [SerializeField, Range(0f, 1f)] float menuSelectOptionVolume;  // 1f
    [SerializeField] AudioClip menuStartGameClip;
    [SerializeField, Range(0f, 1f)] float menuStartGameVolume;  // 1f

    [Header("Surface Type")]
    [SerializeField] private bool isWetSurface;
    public bool IsWetSurface => isWetSurface;

    [Header("Slow Hit Time")]
    [SerializeField] float elapsedSlowHitTime;
    [SerializeField] float durationSlowHitTime;     // 0.5f;

    [Header("Slow Hit Time DOTWeen")]
    [SerializeField] float slowMotAndDeafDelayDuration;     // 0.3f   slowduration
    [SerializeField] float DeafBwdDuration;                 // 0.8f;  returnduration

    [Header("Audio Mixer")]
    [SerializeField] AudioMixer audioMixer;    
    // Typical Range for Lowpass: 22000 (no filter) - 500 (strong filter)
    // Envolving Cave feeling --> CutoffFreq = 8 KHz; Resonance = 1.5-2.5
    [SerializeField] private float lowPassMaxFreq = 8000f;
    [SerializeField] private float lowPassMinFreq = 450f;
    [SerializeField] private float volumeMaxValue = 0f;
    [SerializeField] private float volumeMinValue = -15f;
    [SerializeField] private float filterDeafFwdDuration = 0.8f;       //0.8f  filterduration
    [SerializeField] private float volumeDropDeafFwdDuration = 0.5f;   //0.5f  volumedropduration
    private string sfxLowPassParam = "SFXLowpassFreq";
    private string sfxVolumeParam = "SFXVolume";
    private string musicLowPassParam = "MusicLowpassFreq";
    private string musicVolumeParam = "MusicVolume";    
    #region Events & Delegates
    public event Action<Vector2, float> OnHitPhysicsPlayer;
    public event Action OnPauseEnabled;
    public event Action OnPauseDisabled;
    #endregion

    [Header("UI")]    
    private GameObject canvas;

    // Menu Scene Refs
    private GameObject introPanel;          // Disable->Enable +
                                            // Fade In/Out Color Image (Black-->Red-->Black)
    private Image introPanelImage;

    private Image stegaImage;               // Fade In/Out alpha Image (0->100->0)
    private TextMeshProUGUI aledifonText;   // Fade In/Out alpha Text (0->100->0)

    private GameObject menuPanel;
    private MenuSelector menuPanelSelector; // Navigate Up/Down methods
    private Image menuPanelImage;          // Background Image
    private TextMeshProUGUI titleText;      // Fade In/Out alpha Image (0->100)
    private GameObject optionTextContainer; // Disable->Enable
    private List<RectTransform> optionPositions;
    private RectTransform selectorOption;   // Disable->Enable
    private TextMeshProUGUI buildVersionText;   // Disable->Enable
    private TextMeshProUGUI infoMenuText;   // Disable->Enable
    private TextMeshProUGUI developedByTextContainer; // Disable->Enable
    private GameObject socialNetworksContainer; // Disable->Enable

    private GameObject controlsPanel;       
    private GameObject introScenePanel;
    private Image introScenePanelImage;
    private TextMeshProUGUI introSceneText;                             // TypeWriter VFX
    [SerializeField,Range(0f,0.2f)] private float typeWritterDelay;    // TypeWriter delay < 0.15f
    
    private string introSceneTextStrEN = "And so the intrepid explorer, eager in his desire for " +
                                        "discovery, decided to venture into the silent depths in " +
                                        "search of something that had long been lost...";

    private string introSceneTextStrFR = "Et alors, l'intrépide explorateur, avide de découvertes, " +
                                        "décida de s'enfoncer dans les profondeurs silencieuses à " +
                                        "la recherche de quelque chose qui avait disparu depuis " +
                                        "longtemps...";

    private string introSceneTextStrES = "Y entonces el intrepido explorador, ávido por su deseo de " +
                                        "descubrimiento decidió adentrarse en las profundidades " +
                                        "silenciosas en busqueda de algo que llevaba mucho tiempo " +
                                        "perdido...";

    private UIMenuSelect uiMenuSelect = UIMenuSelect.StartGame;
    private MenuSceneState menuSceneCurrentState = MenuSceneState.Init;
    private bool keyPressed = false;
    private float lastMoveTimeUserInput = 0f;
    private float moveCoolDownUserInput = 0.15f;
    private Coroutine menuSceneCoroutine;

    // Level Scene Refs    
    private GameObject healthPanel;    
    private RectTransform healthUIImage;
    public RectTransform HealthUIImage => healthUIImage;

    private GameObject gemsPanel;
    private RectTransform gemsUIImage;
    public RectTransform GemsUIImage => gemsUIImage;

    private GameObject lifesPanel;
    private RectTransform lifesUIImage;
    public RectTransform LifesUIImage => lifesUIImage;

    private GameObject pausePanel;
    
    private GameObject gameOverPanel;
    private Image gameOverPanelImage;
    private TextMeshProUGUI gameOverPanelText;

    private GameObject deathPanel;    
    private TextMeshProUGUI deathPanelLifesNumText;

    private GameObject endScenePanel;
    private Image endScenePanelImage;
    private TextMeshProUGUI endSceneStoryText; // Machine writting VFX
    private TextMeshProUGUI endSceneContinueText; // Machine writting VFX
    //private string endSceneTextStrEN_Old = "And finally, after his arduous search, the explorer " +
    //                                "managed to open the door using the golden key, and found what " +
    //                                "he had been searching for for so long...";
    private string endSceneTextStrEN = "And finally, after his arduous search, the explorer " +
                                    "managed to open the door using the golden key, and... " +
                                    "he couldn't believe what he found on the other side...";

    private string endSceneTextStrFR = "Et finalement, après une recherche ardue, l'explorateur " +
                                    "réussit à ouvrir la porte à l'aide de la clé dorée et... " +
                                    "il n'en crut pas ses yeux lorsqu'il découvrit ce qui se " +
                                "trouvait de l'autre côté...";

    private string endSceneTextStrES = "Y finalmente, tras una ardua búsqueda, el explorador " +
                                    "logró abrir la puerta usando la llave dorada, y... " +
                                    "no podía creer lo que encontró al otro lado...";

    private string endSceneContinueTextStrEN = "To be continued...";

    private GameObject creditsGamePanel;
    private Image creditsGamePanelImage;
    private TextMeshProUGUI creditsStegaText;   // Fade In/Out alpha Image (0->100->0)
    private Image creditsStegaImage;            // Fade In/Out alpha Image (0->100->0)
    private TextMeshProUGUI creditsMadeByText;  // Fade In/Out alpha Text (0->100->0)
    private TextMeshProUGUI creditsAssetsText;  // Fade In/Out alpha Text (0->100->0)
    private TextMeshProUGUI creditsEndGameText; // Fade In/Out alpha Text (0->100->0)

    private LevelSceneState levelSceneCurrentState = LevelSceneState.Gameplay;
    private bool endCreditsSceneTriggered = false;
    private bool deathPanelTriggered = false;
    private bool gameOverPanelTriggered = false;
    private Coroutine levelSceneCoroutine;

    private Scenes sceneSelected = Scenes.Menu;    

    [Header("Slow Motion Test")]
    [SerializeField] private bool slowMotionEnabled;

    // Checkpoint & Starting Pos.   
    [Header("Starting Pos")]
    private Transform initPos;
    private CamBoundariesTriggerArea initCamBoundTriggerArea;
    private CamTriggerAreaData lastCheckpointData = new CamTriggerAreaData();    
    public CamTriggerAreaData LastCheckpointData => lastCheckpointData;
    [SerializeField] private bool isDebuggingMode;
    public bool IsDebuggingMode => isDebuggingMode;

    // GO Refs.
    AudioSource generalAudioSource;
    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    PlayerSFX playerSFX;
    CameraFollow cameraFollow;

    // Player Ghost Refs
    ReplayManager replayManager;
    PlayerPlayback playerPlayback;
    PlayerRecorder playerRecorder;

    // Input Player Management    
    private InputActionAsset playerInputActions;
    private PlayerInput playerInputAsset;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

    [Header("Boulder")]    
    // Events Variables
    private bool isBoulderEventDone;
    public bool IsBoulderEventDone => isBoulderEventDone;
    [SerializeField] private GameObject boulderPrefab;
    private GameObject boulderGO;

    [Header("Chests")]
    [SerializeField] private float bootsWaitingTime;
    [SerializeField] private float hookWaitingTime;
    private ColumnsDestructionHandler columnDestroyer;

    #region Unity API
    // Start is called before the first frame update
    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }        

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Events subscription
        SceneManager.sceneLoaded += OnSceneLoaded;
        SubscribeChestEvents();

        // Get GameManager refs
        generalAudioSource = GetComponent<AudioSource>();
        replayManager = GetComponent<ReplayManager>();       
    }
    private void OnDisable()
    {
        UnsubscribeChestEvents();
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;        
        instance = null;
    }
    #endregion
    #region Menu Scene
    private IEnumerator UpdateMenuSceneState()
    {
        while (menuSceneCurrentState < MenuSceneState.StartGame)
        {
            // Set the Menu Scene Sequence on the 1st State
            /*
             * 1. Enable IntroPanel & Start Intro Panel on Black Colour
             * 2. IntroPanel.Color.alpha(0->100)
             * 3. StegaImage.Color.alpha.Lerp(0->100)
             * 4. Delay 2s
             * 5. StegaImage.Color.alpha.Lerp(100->0)
             * 6. AledifonText.Color.alpha.Lerp(0->100)
             * 7. Delay 2s
             * 8. AledifonText.Color.alpha.Lerp(100->0)
             * 9. IntroPanel.Color.aplha(100->0)
             * 10. Disable IntroPanel & Enable MenuPanel
             * 11. TitleText.Color.alpha.Lerp(0->100)
             * 12. Enable OptionTextContainer, Selector & BuildVersionText
             *     Also Enable InputActionMap
             * 13. If Selected Controls --> Enable ControlsPanel
             *     Press any key (From Control Panel) --> Disable ControlsPanel
             * 14. If Selected Quit Game --> Quit Game
             * 15. If Selected Start Game --> Enable IntroScenePanel & Disable InputActionMap
             *     Start machine typewritting         
             * 16. When Text is finished --> LoadScene(LevelScene);
             */

            Color introPanelImageTargetColor;
            Color introPanelStegaImageTargetColor;
            Color introPanelAledifonTextTargetColor;

            Color menuPanelTitleTextTargetColor;
            Color menuPanelImageTargetColor;

            Color introScenePanelImageTargetColor;

            switch (menuSceneCurrentState)
            {
                // Enable IntroPanel
                case MenuSceneState.Init:

                    yield return null;      // Wait for a frame to assure all the GO's Refs
                                            // are completely initialised

                    // Enable the IntroPanel
                    introPanel.SetActive(true);                    

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.IntroPanelFadeIn;
                    break;

                // Intro Panel from Black to Red Colour Fade In
                case MenuSceneState.IntroPanelFadeIn:

                    //Set The target Color
                    introPanelImageTargetColor = introPanelImage.color;
                    introPanelImageTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    yield return introPanelImage
                        .DOColor(introPanelImageTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();                    

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.IntroPanelStegaImageFadeIn;                    
                    break;
                
                // Stega Image Fade In
                case MenuSceneState.IntroPanelStegaImageFadeIn:

                    //Set The target Color
                    introPanelStegaImageTargetColor = stegaImage.color;
                    introPanelStegaImageTargetColor.a = 1f;

                    // Alpha Color FadeIn (0->100)
                    yield return stegaImage
                        .DOColor(introPanelStegaImageTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(3f);

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.IntroPanelStegaImageFadeOut;
                    break;

                // Stega Image Fade Out
                case MenuSceneState.IntroPanelStegaImageFadeOut:

                    //Set The target Color
                    introPanelStegaImageTargetColor = stegaImage.color;
                    introPanelStegaImageTargetColor.a = 0f;

                    // Alpha Color FadeOut (100->0)
                    yield return stegaImage
                        .DOColor(introPanelStegaImageTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    //yield return new WaitForSeconds(2f);

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.IntroPanelFadeOut;
                    break;

                case MenuSceneState.IntroPanelFadeOut:

                    //Set The target Color
                    introPanelImageTargetColor = introPanelImage.color;
                    introPanelImageTargetColor.a = 0f;

                    // Color FadeIn (Black->Red)
                    yield return introPanelImage
                        .DOColor(introPanelImageTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(1f);
                    
                    menuSceneCurrentState = MenuSceneState.IntroPanelAledifonTextFadeIn;
                    break;

                // Aledifon Text Fade In
                case MenuSceneState.IntroPanelAledifonTextFadeIn:

                    //Set The target Color
                    introPanelAledifonTextTargetColor = aledifonText.color;
                    introPanelAledifonTextTargetColor.a = 1f;

                    // Alpha Color FadeIn (0->100)
                    yield return aledifonText
                        .DOColor(introPanelAledifonTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(3f);

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.IntroPanelAledifonTextFadeOut;
                    break;

                case MenuSceneState.IntroPanelAledifonTextFadeOut:

                    //Set The target Color
                    introPanelAledifonTextTargetColor = aledifonText.color;
                    introPanelAledifonTextTargetColor.a = 0f;

                    // Alpha Color FadeOut (100->0)
                    yield return aledifonText
                        .DOColor(introPanelAledifonTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(2f);

                    // Disable the IntroPanel
                    introPanel.SetActive(false);
                    // Enable the MenuPanel & the titleText GO
                    menuPanel.SetActive(true);
                    titleText.gameObject.SetActive(true);

                    // Start playing the Menu Music
                    PlayMenuMusic();
                    
                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.MenuPanelTitleTextFadeIn;
                    break;
                
                case MenuSceneState.MenuPanelTitleTextFadeIn:

                    // Assure the Init color is with alpha 0
                    Color c = titleText.color;
                    c.a = 0f;
                    titleText.color = c;

                    //Set The target Color
                    menuPanelTitleTextTargetColor = titleText.color;
                    menuPanelTitleTextTargetColor.a = 1f;

                    //Set The target Color
                    menuPanelImageTargetColor = menuPanelImage.color;
                    menuPanelImageTargetColor.a = 0.4f;

                    // Color FadeIn (Black->Red)
                    titleText
                        .DOColor(menuPanelTitleTextTargetColor, 3f)
                        .SetEase(Ease.InQuad);

                    yield return menuPanelImage
                        .DOColor(menuPanelImageTargetColor, 3f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(1f);

                    // Enable the all the subpanels
                    optionTextContainer.SetActive(true);
                    selectorOption.gameObject.SetActive(true);
                    buildVersionText.gameObject.SetActive(true);
                    infoMenuText.gameObject.SetActive(true);
                    developedByTextContainer.gameObject.SetActive(true);
                    socialNetworksContainer.SetActive(true);

                    // Enable the UI Inputs
                    EnableUIMainMenuInput();

                    // Init the MenuPanelState & the Selector State Visual on UI
                    uiMenuSelect = UIMenuSelect.StartGame;
                    //

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.MenuPanelState;
                    break;

                case MenuSceneState.MenuPanelState:

                    if (!keyPressed)
                        break;

                    // Reset the KeyPressed Boolean Flag
                    keyPressed = false;

                    if (uiMenuSelect == UIMenuSelect.QuitGame)
                    {
                        // Play the Select SFX
                        PlayMenuSelectOptionSFx();

                        // Update Menu Scene State
                        menuSceneCurrentState = MenuSceneState.QuitGameState;
                    }
                    else if (uiMenuSelect == UIMenuSelect.Controls)
                    {
                        // Enable the ControlsPanel
                        controlsPanel.SetActive(true);

                        // Play the Select SFX
                        PlayMenuSelectOptionSFx();

                        // Update Menu Scene State
                        menuSceneCurrentState = MenuSceneState.ControlPanelState;
                    }                        
                    else if (uiMenuSelect == UIMenuSelect.StartGame)
                    {
                        // Disable the MenuPanel
                        menuPanel.SetActive(false);
                        // Enable the IntroScenePanel
                        introScenePanel.SetActive(true);

                        // Disable all player's input
                        DisableAllInputs();

                        // Play the StartGame SFX
                        PlayMenuStartGameSFx();

                        // Update Menu Scene State
                        menuSceneCurrentState = MenuSceneState.IntroScenePanelShowText;
                    }
                    break;

                case MenuSceneState.ControlPanelState:

                    if (!keyPressed)
                        break;

                    // Reset the KeyPressed Boolean Flag
                    keyPressed = false;

                    // Enable the ControlsPanel
                    controlsPanel.SetActive(false);

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.MenuPanelState;

                    break;

                case MenuSceneState.QuitGameState:

                    QuitGame();
                    break;

                case MenuSceneState.IntroScenePanelShowText:

                    //Set The target Color
                    introScenePanelImageTargetColor = introScenePanelImage.color;
                    introScenePanelImageTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    introScenePanelImage
                        .DOColor(introScenePanelImageTargetColor, 3f)
                        .SetEase(Ease.InQuad);

                    // Perform Type writting machine
                    yield return StartCoroutine(TypeWrittingText(introSceneText,introSceneTextStrEN,typeWritterDelay));

                    // Delay
                    yield return new WaitForSeconds(2f);

                    // Load the Level
                    SceneManager.LoadScene(Scenes.Level.ToString());

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.StartGame;
                    break;

                case MenuSceneState.StartGame:
                    
                    break;
            }
            yield return null;
        }
    }
    private IEnumerator UpdateLevelSceneState()
    {
        while (levelSceneCurrentState < LevelSceneState.CreditsCompleted)
        {           
            Color endScenePanelImageTargetColor;
            //Color endScenePanelStoryTextTargetColor;
            //Color endScenePanelContinueTextTargetColor;

            Color gameOverPanelImageTargetColor;
            Color gameOverPanelTextTargetColor;

            Color creditsGamePanelImageTargetColor;
            Color creditsGamePanelStegaImageTargetColor;
            Color creditsGamePanelStegaTextTargetColor;
            Color creditsGamePanelMadeByTextTargetColor;
            Color creditsGamePanelAssetsTextTargetColor;
            Color creditsGamePanelEndGameTextTargetColor;

            switch (levelSceneCurrentState)
            {
                // Enable IntroPanel
                case LevelSceneState.Gameplay:
                    
                    if (endCreditsSceneTriggered)
                    {
                        // Reset the endCreditsTriggered Boolean Flag
                        SetEndCreditsSceneFlag(false);

                        // Enable the IntroPanel
                        endScenePanel.SetActive(true);

                        // Start playing the Menu Music
                        PlayEndGameMusic();

                        // Update Level Scene State
                        levelSceneCurrentState = LevelSceneState.EndScenePanelShowText;
                    }
                    else if (deathPanelTriggered)
                    {
                        // Reset the deathPanel Boolean Flag
                        SetDeathPanelFlag(false);

                        // Enable the DeathPanel
                        deathPanel.SetActive(true);
                        // Update the RemainingLifesText
                        deathPanelLifesNumText.text = playerMovement.NumLifes.ToString();

                        // Update Level Scene State
                        levelSceneCurrentState = LevelSceneState.DeathPanelShow;
                    }
                    else if (gameOverPanelTriggered)
                    {
                        // Reset the gameOver Boolean Flag
                        SetGameOverPanelFlag(false);

                        // Delay
                        //yield return new WaitForSeconds(2f);

                        // Play the GameOver SFX
                        PlayGameOverSFx();

                        // Enable the GameOverPanel
                        gameOverPanel.SetActive(true);

                        // Update Level Scene State
                        levelSceneCurrentState = LevelSceneState.GameOverPanel;
                    }
                    break;

                case LevelSceneState.DeathPanelShow:

                    // Reset the player to its initial State and to the respawn pos.
                    playerMovement.RespawnPlayer(lastCheckpointData.respawnPos);

                    // Reset the player's Health
                    playerHealth.SetMaxHealth();

                    // Update Bounding Shapes of Cinemachine Confiner
                    cameraFollow.SetConfinerFromCheckpoint(lastCheckpointData.respawnCamBoundTriggerArea);

                    // Set the default Camera Size (in case was modified on the Boulder sequence)
                    if (cameraFollow.CameraSize != cameraFollow.OriginalCamSize)
                        CameraZoomIn();

                    // Perform also Boulder Respawn (just in cas is needed)
                    if (!isBoulderEventDone)
                        BoulderRespawn();

                    // Enemies Respawn will be handled by every one in their own scripts.                                        

                    // Delay
                    yield return new WaitForSeconds(2f);

                    // Start Playing again the Level Music
                    PlayLevelMusic();

                    // Disable the DeathPanel
                    deathPanel.SetActive(false);

                    // Enable again the User Inputs
                    EnableGameplayInput();                                        

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.Gameplay;
                    break;

                case LevelSceneState.GameOverPanel:

                    // FadeIn GameOverPanel & Text

                    //Set The target Color
                    gameOverPanelImageTargetColor = gameOverPanelImage.color;
                    gameOverPanelImageTargetColor.a = 1f;

                    //Set The target Color
                    gameOverPanelTextTargetColor = gameOverPanelText.color;
                    gameOverPanelTextTargetColor.a = 1f;

                    yield return gameOverPanelImage
                        .DOColor(gameOverPanelImageTargetColor, 3f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    yield return gameOverPanelText
                       .DOColor(gameOverPanelTextTargetColor, 5f)
                       .SetEase(Ease.InQuad)
                       .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(5f);

                    // Load the Level
                    SceneManager.LoadScene(Scenes.Menu.ToString());

                    // Update Menu Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsCompleted;
                    break;

                case LevelSceneState.EndScenePanelShowText:

                    //Set The target Color
                    endScenePanelImageTargetColor = endScenePanelImage.color;
                    endScenePanelImageTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    endScenePanelImage
                        .DOColor(endScenePanelImageTargetColor, 3f)
                        .SetEase(Ease.InQuad);

                    // Perform Type writting machine
                    yield return StartCoroutine(TypeWrittingText(endSceneStoryText, endSceneTextStrEN, typeWritterDelay));

                    // Delay
                    yield return new WaitForSeconds(2f);

                    // Perform Type writting machine
                    yield return StartCoroutine(TypeWrittingText(endSceneContinueText, endSceneContinueTextStrEN, typeWritterDelay/4f));

                    // Delay
                    yield return new WaitForSeconds(2f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelFadeIn;
                    break;

                case LevelSceneState.CreditsGamePanelFadeIn:

                    // Enable the Credits Game Panel
                    creditsGamePanel.SetActive(true);

                    //Set The target Color
                    creditsGamePanelImageTargetColor = creditsGamePanelImage.color;
                    creditsGamePanelImageTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    yield return creditsGamePanelImage
                        .DOColor(creditsGamePanelImageTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(1f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelStegaFadeIn;
                    break;

                case LevelSceneState.CreditsGamePanelStegaFadeIn:

                    //Set The target Color
                    creditsGamePanelStegaImageTargetColor = creditsStegaImage.color;
                    creditsGamePanelStegaImageTargetColor.a = 1f;

                    //Set The target Color
                    creditsGamePanelStegaTextTargetColor = creditsStegaText.color;
                    creditsGamePanelStegaTextTargetColor.a = 1f;

                    // FadeIn
                    creditsStegaImage
                        .DOColor(creditsGamePanelStegaImageTargetColor, 1f)
                        .SetEase(Ease.InQuad);

                    yield return creditsStegaText
                       .DOColor(creditsGamePanelStegaTextTargetColor, 1f)
                       .SetEase(Ease.InQuad)
                       .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(3f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelStegaFadeOut;
                    break;

                case LevelSceneState.CreditsGamePanelStegaFadeOut:

                    //Set The target Color
                    creditsGamePanelStegaImageTargetColor = creditsStegaImage.color;
                    creditsGamePanelStegaImageTargetColor.a = 0f;

                    //Set The target Color
                    creditsGamePanelStegaTextTargetColor = creditsStegaText.color;
                    creditsGamePanelStegaTextTargetColor.a = 0f;

                    // FadeIn
                    creditsStegaImage
                        .DOColor(creditsGamePanelStegaImageTargetColor, 1f)
                        .SetEase(Ease.InQuad);

                    yield return creditsStegaText
                       .DOColor(creditsGamePanelStegaTextTargetColor, 1f)
                       .SetEase(Ease.InQuad)
                       .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(1f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelAledifonTextFadeIn;
                    break;

                case LevelSceneState.CreditsGamePanelAledifonTextFadeIn:

                    //Set The target Color
                    creditsGamePanelMadeByTextTargetColor = creditsMadeByText.color;
                    creditsGamePanelMadeByTextTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    yield return creditsMadeByText
                        .DOColor(creditsGamePanelMadeByTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(3f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelAledifonTextFadeOut;
                    break;

                case LevelSceneState.CreditsGamePanelAledifonTextFadeOut:

                    //Set The target Color
                    creditsGamePanelMadeByTextTargetColor = creditsMadeByText.color;
                    creditsGamePanelMadeByTextTargetColor.a = 0f;

                    // Color FadeIn (Black->Red)
                    yield return creditsMadeByText
                        .DOColor(creditsGamePanelMadeByTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(1f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelAssetsTextFadeIn;
                    break;

                case LevelSceneState.CreditsGamePanelAssetsTextFadeIn:

                    //Set The target Color
                    creditsGamePanelAssetsTextTargetColor = creditsAssetsText.color;
                    creditsGamePanelAssetsTextTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    yield return creditsAssetsText
                        .DOColor(creditsGamePanelAssetsTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(5f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelAssetsTextFadeOut;
                    break;

                case LevelSceneState.CreditsGamePanelAssetsTextFadeOut:

                    //Set The target Color
                    creditsGamePanelAssetsTextTargetColor = creditsAssetsText.color;
                    creditsGamePanelAssetsTextTargetColor.a = 0f;

                    // Color FadeIn (Black->Red)
                    yield return creditsAssetsText
                        .DOColor(creditsGamePanelAssetsTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(1f);

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsGamePanelEndGameTextFadeIn;
                    break;

                case LevelSceneState.CreditsGamePanelEndGameTextFadeIn:

                    //Set The target Color
                    creditsGamePanelEndGameTextTargetColor = creditsEndGameText.color;
                    creditsGamePanelEndGameTextTargetColor.a = 1f;

                    // Color FadeIn (Black->Red)
                    yield return creditsEndGameText
                        .DOColor(creditsGamePanelEndGameTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay
                    yield return new WaitForSeconds(5f);

                    // Load the Level
                    SceneManager.LoadScene(Scenes.Menu.ToString());

                    // Update Level Scene State
                    levelSceneCurrentState = LevelSceneState.CreditsCompleted;
                    break;

                case LevelSceneState.CreditsCompleted:                           
                    break;
            }
            yield return null;
        }
    }
    private IEnumerator TypeWrittingText(TextMeshProUGUI textBox, string str, float typeCharDelay)
    {
        textBox.text = "";

        foreach (char c in str)
        {
            textBox.text += c;
            yield return new WaitForSeconds(typeCharDelay);
        }        
    }
    public void SetEndCreditsSceneFlag(bool enable)
    {
        endCreditsSceneTriggered = enable;
    }    
    public void SetDeathPanelFlag(bool enable)
    {
        deathPanelTriggered = enable;
    }
    public void SetGameOverPanelFlag(bool enable)
    {
        gameOverPanelTriggered = enable;
    }
    public void ChooseDeathOrGameOverPanel()
    {
        if (playerMovement.NumLifes == 0)
            SetGameOverPanelFlag(true);
        else
            SetDeathPanelFlag(true);
    }
    #endregion
    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Stop them in case they are already running (Safety)
        if (menuSceneCoroutine != null)
            StopCoroutine(menuSceneCoroutine);
        
        if (levelSceneCoroutine != null)
            StopCoroutine(levelSceneCoroutine);        

        //canvas = GameObject.Find("Canvas");

        if (canvas == null)
            Debug.LogError("The Canvas object is null");

        Debug.Log($"The Current Canvas Ref is: {canvas.name} (Scene: {canvas.gameObject.scene.name})");

        // Stop the Music in case any track is currently playing.
        StopMusic();

        // Parsing the Scene Name to enum typedata
        if (System.Enum.TryParse(SceneManager.GetActiveScene().name, out Scenes currentScene))
        {
            switch (currentScene)
            {
                case Scenes.Menu:

                    // Set the new Scene as the current one
                    sceneSelected = currentScene;

                    // Set the init Menu Scene State
                    menuSceneCurrentState = MenuSceneState.Init;

                    // Get all the Menu Scene GO Refs.
                    GetMenuSceneRefs();

                    // Set the ReplayManager as disabled
                    DisableReplayManagerAndCleanRefs();

                    // Start playing Title Screen Audio
                    //PlayMainTitleAudioClip();

                    // Hide the Mouse Cursor
                    ShowMouseCursor(false);

                    // Get the Player Input Actions Refs
                    //GetPlayerInputRefs();                    

                    // Disable all the Inputs
                    DisableAllInputs();

                    // Trigger the Update Scene State Loop
                    menuSceneCoroutine = StartCoroutine(nameof(UpdateMenuSceneState));                    

                    break;
                case Scenes.Level:

                    // Set the new Scene as the current one
                    sceneSelected = currentScene;

                    // Set the init Level Scene State
                    levelSceneCurrentState = LevelSceneState.Gameplay;

                    // Get the GO Refs
                    GetLevelSceneRefs();
                    GetLevelRefs();
                    GetCameraFollowRef();

                    PlayLevelMusic();

                    EnableGameplayInput();

                    //if (slowMotionEnabled)
                    //    EnableSlowMotion();

                    // Set the filterDuration
                    //filterDuration = returnDuration;

                    // Set the Initial CheckPoint Data
                    SetInitCheckPointData();

                    // Reset the player to its initial State and to the respawn pos.
                    playerMovement.RespawnPlayer(lastCheckpointData.respawnPos);

                    // Reset the Boulder Event Done's Flag
                    SetBoulderEventDoneFlag(false);

                    // Reset the End Credit Scene Flag
                    SetEndCreditsSceneFlag(false);

                    // Reset all PowerUps as Locked
                    playerMovement.ResetAllPowerUps();                    

                    // Trigger the Update Scene State Loop
                    levelSceneCoroutine = StartCoroutine(nameof(UpdateLevelSceneState));

                    break;
                default:
                    break;
            }
        }
    }
    public void GetCanvasRef(GameObject newCanvas)
    {
        canvas = newCanvas;
    }
    private void GetMenuSceneRefs()
    {
        // Get all the Intro Panel GO's Refs                    
        introPanel = canvas.transform.Find("IntroPanel")?.gameObject;
        if (introPanel == null)
            Debug.LogError("The Intro Panel object is null");
        else
        {            
            introPanelImage = introPanel.transform?.GetComponent<Image>();
            if (introPanelImage == null)
                Debug.LogError("The introPanelImage component was not found " +
                                "on the " + introPanel.name + "GO ");

            stegaImage = introPanel.transform.Find("StegaImage")?.GetComponent<Image>();
            if (stegaImage == null)
                Debug.LogError("The stegaImage component was not found " +
                                "on the " + introPanel.name + "GO ");

            aledifonText = introPanel.transform.Find("AledifonText")?.GetComponent<TextMeshProUGUI>();
            if (aledifonText == null)
                Debug.LogError("The aledifonText component was not found " +
                                "on the " + introPanel.name + "GO ");
        }

        // Get all the Menu Panel GO's Refs                    
        menuPanel = canvas.transform.Find("MenuPanel")?.gameObject;
        if (menuPanel == null)
            Debug.LogError("The menuPanel object is null");
        else
        {
            menuPanelSelector = menuPanel.GetComponent<MenuSelector>();
            if (menuPanelSelector == null)
                Debug.LogError("The menuPanelSelector component was not found " +
                                "on the " + menuPanel.name + "GO ");
            
            menuPanelImage = menuPanel.GetComponent<Image>();
            if (menuPanelImage == null)
                Debug.LogError("The menuPanelImage component was not found " +
                                "on the " + menuPanel.name + "GO ");

            titleText = menuPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
                Debug.LogError("The titleText component was not found " +
                                "on the " + menuPanel.name + "GO ");

            optionTextContainer = menuPanel.transform.Find("OptionTextContainer")?.gameObject;
            if (optionTextContainer == null)
                Debug.LogError("The optionTextContainer GO was not found " +
                                "on the " + menuPanel.name + "GO ");
            else
            {
                optionPositions = new List<RectTransform>();
                foreach (Transform child in optionTextContainer.transform)
                {
                    var rect = child.GetComponent<RectTransform>();
                    if (rect != null)
                        optionPositions.Add(rect);
                }
            }

            selectorOption = menuPanel.transform.Find("Selector").GetComponent<RectTransform>();
            if (selectorOption == null)
                Debug.LogError("The selectorOption component was not found " +
                                "on the " + menuPanel.name + "GO ");

            buildVersionText = menuPanel.transform.Find("BuildVersionText").GetComponent<TextMeshProUGUI>();
            if (buildVersionText == null)
                Debug.LogError("The buildVersionText component was not found " +
                                "on the " + menuPanel.name + "GO ");
            else
                buildVersionText.text = "Build " + Application.version;

                infoMenuText = menuPanel.transform.Find("InfoMenuText").GetComponent<TextMeshProUGUI>();
            if (infoMenuText == null)
                Debug.LogError("The infoMenuText component was not found " +
                                "on the " + menuPanel.name + "GO ");

            developedByTextContainer = menuPanel.transform.Find("DevelopedByText").GetComponent<TextMeshProUGUI>();
            if (developedByTextContainer == null)
                Debug.LogError("The developedByText component was not found " +
                                "on the " + menuPanel.name + "GO ");

            socialNetworksContainer = menuPanel.transform.Find("SocialNetworksContainer")?.gameObject;
            if (socialNetworksContainer == null)
                Debug.LogError("The socialNetworksContainer GO was not found " +
                                "on the " + menuPanel.name + "GO ");
        }

        // Get all the Controls Panel GO's Refs                    
        controlsPanel = canvas.transform.Find("ControlsPanel")?.gameObject;
        if (controlsPanel == null)
            Debug.LogError("The controlsPanel object is null");

        // Get all the Intro Scene Panel GO's Refs                    
        introScenePanel = canvas.transform.Find("IntroScenePanel")?.gameObject;
        if (introScenePanel == null)
            Debug.LogError("The introScenePanel object is null");
        else
        {
            introScenePanelImage = introScenePanel.GetComponent<Image>();
            if (introScenePanelImage == null)
                Debug.LogError("The introScenePanelImage component was not found " +
                                "on the " + introScenePanel.name + "GO ");

            introSceneText = introScenePanel.transform.Find("StoryTextEN")?.GetComponent<TextMeshProUGUI>();
            if (introSceneText == null)
                Debug.LogError("The introSceneText component was not found " +
                                "on the " + introScenePanel.name + "GO ");
        }
    }
    public void GetPlayerInputRefs(PlayerInput pInput)
    {
        // Get all the Intro Panel GO's Refs                    
        //playerInputActions = GameObject.Find("MenuInput")?.GetComponent<PlayerInput>();
        playerInputAsset = pInput;
        if (playerInputAsset == null)
            Debug.LogError("The playerInputActions component is null or " +
                            "the MenuInput GO does not exist");

        playerInputActions = playerInputAsset.actions;        
    }
    private void GetLevelSceneRefs()
    {
        // Get all the GO's Refs                    
        healthPanel = canvas.transform.Find("HealthPanel")?.gameObject;
        if (healthPanel == null)
            Debug.LogError("The healthPanel object is null");
        else
        {
            healthUIImage = healthPanel.transform.Find("Health")?.GetComponent<RectTransform>();
            if (healthUIImage == null)
                Debug.LogError("The healthUIImage component was not found " +
                                "on the " + healthPanel.name + "GO ");
        }

        gemsPanel = canvas.transform.Find("GemsPanel")?.gameObject;
        if (gemsPanel == null)
            Debug.LogError("The gemsPanel object is null");
        else
        {
            gemsUIImage = gemsPanel.transform.Find("GemsCountImage")?.GetComponent<RectTransform>();
            if (gemsUIImage == null)
                Debug.LogError("The gemsUIImage component was not found " +
                                "on the " + gemsPanel.name + "GO ");
        }

        lifesPanel = canvas.transform.Find("LifesPanel")?.gameObject;
        if (lifesPanel == null)
            Debug.LogError("The lifesPanel object is null");
        else
        {
            lifesUIImage = lifesPanel.transform.Find("LifesCountImage")?.GetComponent<RectTransform>();
            if (lifesUIImage == null)
                Debug.LogError("The lifesUIImage component was not found " +
                                "on the " + lifesPanel.name + "GO ");
        }

        pausePanel = canvas.transform.Find("PausePanel")?.gameObject;
        if (pausePanel == null)
            Debug.LogError("The pausePanel object is null");

        gameOverPanel = canvas.transform.Find("GameOverPanel")?.gameObject;
        if (gameOverPanel == null)
            Debug.LogError("The gameOverPanel object is null");
        else
        {
            gameOverPanelImage = gameOverPanel.GetComponent<Image>();
            if (gameOverPanelImage == null)
                Debug.LogError("The gameOverPanelImage component was not found " +
                                "on the " + gameOverPanel.name + "GO ");

            gameOverPanelText = gameOverPanel.transform.Find("GameOverText")?.GetComponent<TextMeshProUGUI>();
            if (gameOverPanelText == null)
                Debug.LogError("The gameOverPanelText component was not found " +
                                "on the " + gameOverPanel.name + "GO ");
        }
        
        deathPanel = canvas.transform.Find("DeathPanel")?.gameObject;
        if (deathPanel == null)
            Debug.LogError("The deathPanel object is null");
        else
        {           
            //deathPanelLifesNumText = deathPanel.transform.Find("RemainLifesCountText2")?.GetComponent<TextMeshProUGUI>();
            deathPanelLifesNumText = deathPanel.GetComponentsInChildren<TextMeshProUGUI>()
                                                .FirstOrDefault(t => t.name == "RemainLifesCountText2");
            if (deathPanelLifesNumText == null)
                Debug.LogError("The deathPanelLifesNumText component was not found " +
                                "on the " + deathPanel.name + "GO ");
        }               

        endScenePanel = canvas.transform.Find("EndScenePanel")?.gameObject;
        if (endScenePanel == null)
            Debug.LogError("The endScenePanel object is null");
        else
        {
            endScenePanelImage = endScenePanel.GetComponent<Image>();
            if (endScenePanelImage == null)
                Debug.LogError("The endScenePanelImage component was not found " +
                                "on the " + endScenePanel.name + "GO ");

            endSceneStoryText = endScenePanel.transform.Find("StoryTextEN")?.GetComponent<TextMeshProUGUI>();
            if (endSceneStoryText == null)
                Debug.LogError("The endSceneText component was not found " +
                                "on the " + endScenePanel.name + "GO ");
            
            endSceneContinueText = endScenePanel.transform.Find("ContinueTextEN")?.GetComponent<TextMeshProUGUI>();
            if (endSceneContinueText == null)
                Debug.LogError("The endSceneContinueText component was not found " +
                                "on the " + endScenePanel.name + "GO ");
        }

        creditsGamePanel = canvas.transform.Find("CreditsGamePanel")?.gameObject;
        if (creditsGamePanel == null)
            Debug.LogError("The creditsGamePanel object is null");
        else
        {
            creditsGamePanelImage = creditsGamePanel.GetComponent<Image>();
            if (creditsGamePanelImage == null)
                Debug.LogError("The creditsGamePanelImage component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");

            creditsStegaImage = creditsGamePanel.transform.Find("StegaImage")?.GetComponent<Image>();
            if (creditsStegaImage == null)
                Debug.LogError("The creditsStegaImage component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");

            creditsStegaText = creditsGamePanel.transform.Find("StegaText")?.GetComponent<TextMeshProUGUI>();
            if (creditsStegaText == null)
                Debug.LogError("The creditsStegaText component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");

            creditsMadeByText = creditsGamePanel.transform.Find("MadeByText")?.GetComponent<TextMeshProUGUI>();
            if (creditsMadeByText == null)
                Debug.LogError("The creditsMadeByText component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");

            creditsAssetsText = creditsGamePanel.transform.Find("AssetsText")?.GetComponent<TextMeshProUGUI>();
            if (creditsAssetsText == null)
                Debug.LogError("The creditsAssetsText component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");

            creditsEndGameText = creditsGamePanel.transform.Find("EndGameText")?.GetComponent<TextMeshProUGUI>();
            if (creditsEndGameText == null)
                Debug.LogError("The creditsEndGameText component was not found " +
                                "on the " + creditsGamePanel.name + "GO ");
        }

    }
    private void GetCameraFollowRef()
    {
        cameraFollow = FindAnyObjectByType<CameraFollow>();

        if (cameraFollow == null)            
            Debug.LogError("CameraFollow component Not Found on the Scene!");        
    }
    private void GetLevelRefs()
    {
        // Get all the GO's Refs                    
        initPos = GameObject.Find("SpawnInitPos")?.transform;
        if (initPos == null)
            Debug.LogError("The initPos component is null");

        initCamBoundTriggerArea = GameObject.Find("InitAreaCamTrigger")?.GetComponent<CamBoundariesTriggerArea>();
        if (initCamBoundTriggerArea == null)
            Debug.LogError("The initCamBoundTriggerArea component is null");

        boulderGO = FindAnyObjectByType<Boulder>()?.gameObject;
        if (boulderGO == null)
            Debug.LogError("There is no any 'RoundBoulder' GO in the Scene");

        columnDestroyer = FindAnyObjectByType<ColumnsDestructionHandler>()
                        ?.gameObject
                        ?.GetComponent<ColumnsDestructionHandler>();
        if (columnDestroyer == null)
            Debug.LogError("There is no any 'ColumnsDestructionHandler' script on the Scene");
    }
    public void ShowMouseCursor(bool enable)
    {
        if (enable)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Force the updating of the mouse cursor on the next frame
            //StartCoroutine(nameof(FixCursorVisibility));
        }
    }
    #endregion
    #region Events Subscriptions
    public void SubscribeEventsOfPlayerHealth(PlayerHealth pH)
    {
        playerHealth = pH;
        playerHealth.OnHitFXPlayer += SlowMotionOnHit;
        playerHealth.OnHitFXPlayer += ApplyDeafeningSFX;
        playerHealth.OnDeathPlayer += SlowMotionOnDeath; 
        playerHealth.OnDeathPlayer += StopMusic; 
    }
    public void UnsubscribeEventsOfPlayerHealth()
    {
        if (playerHealth != null)
        {            
            playerHealth.OnHitFXPlayer -= SlowMotionOnHit;
            playerHealth.OnHitFXPlayer -= ApplyDeafeningSFX;
            playerHealth.OnDeathPlayer -= SlowMotionOnDeath;
            playerHealth.OnDeathPlayer -= StopMusic;
            playerHealth = null;
        }
    }
    public void SubscribeEventsOfPlayerSFX(PlayerSFX pSFX)
    {
        playerSFX = pSFX;
        OnPauseEnabled += playerSFX.PauseAllSFX;
        OnPauseDisabled += playerSFX.ResumeAllSFX;
    }
    public void UnsubscribeEventsOfPlayerSFX()
    {
        if (playerSFX != null)
        {
            OnPauseEnabled -= playerSFX.PauseAllSFX;
            OnPauseDisabled -= playerSFX.ResumeAllSFX;
            playerSFX = null;
        }
    }
    public void SubscribeEventsOfPlayerMovement(PlayerMovement pM)
    {
        playerMovement = pM;
        OnHitPhysicsPlayer += playerMovement.ReceiveDamage;
    }
    public void UnsubscribeEventsOfPlayerMovement()
    {
        if (playerMovement != null)
        {
            OnHitPhysicsPlayer -= playerMovement.ReceiveDamage;
            playerMovement = null;
        }
    }    
    private void SubscribeChestEvents()
    {
        Chest.OnChestOpened += HandleChestOpened;
    }
    private void UnsubscribeChestEvents()
    {
        Chest.OnChestOpened -= HandleChestOpened;
    }
    #endregion
    #region ReplayManager
    public void EnableReplayManagerAndGetRefs()
    {
        //return; // Temporary till will be completely tested

        if (playerMovement != null)
        {
            // Get the Player Recorder & Playback Refs
            playerPlayback = GameObject.Find("PlayerGhost")?.GetComponent<PlayerPlayback>();
            playerRecorder = playerMovement.gameObject.GetComponent<PlayerRecorder>();

            if (playerPlayback != null && playerRecorder != null)
            {
                replayManager.enabled = true;
                replayManager.GetPlayerRefs(playerRecorder, playerPlayback);
            }
            else
                Debug.LogWarning("Either " + playerPlayback.name + " and/or +" +
                                playerRecorder.name + " scripts were not found on the Scene");
        }
    }
    public void DisableReplayManagerAndCleanRefs()
    {
        //return; // Temporary till will be completely tested

        playerPlayback = null;
        playerRecorder = null;

        replayManager.enabled = false;
    }
    #endregion
    #region Input Action Maps        
    public void EnableUIMainMenuInput()
    {
        // Enable the UI-Main Menu Action Map & Disable the others.
        playerInputActions.FindActionMap("UI-MainMenu").Enable();
        playerInputActions.FindActionMap("Gameplay").Disable();
        playerInputActions.FindActionMap("UI-InGame").Disable();
    }
    public void EnableGameplayInput()
    {
        // Enable the Gameplay Action Map & Disable the others.
        playerInputActions.FindActionMap("Gameplay").Enable();
        playerInputActions.FindActionMap("UI-InGame").Disable();
        playerInputActions.FindActionMap("UI-MainMenu").Disable();
    }
    public void EnablePauseInput()
    {
        // Enable the Pause Action Map & Disable the others.        
        playerInputActions.FindActionMap("UI-InGame").Enable();
        playerInputActions.FindActionMap("Gameplay").Disable();
        playerInputActions.FindActionMap("UI-MainMenu").Disable();
    }
    public void DisableAllInputs()
    {
        playerInputActions.FindActionMap("Gameplay").Disable();
        playerInputActions.FindActionMap("UI-InGame").Disable();
        playerInputActions.FindActionMap("UI-MainMenu").Disable();
    }
    #endregion
    #region Input Player
    public void KeyPressedUI(InputAction.CallbackContext context)
    {
        if (menuSceneCurrentState != MenuSceneState.MenuPanelState &&
            menuSceneCurrentState != MenuSceneState.ControlPanelState)
            return;

        if (context.phase == InputActionPhase.Performed)
        {
            keyPressed = true;            
        }
    }
    public virtual void SwitchSelectionUI(InputAction.CallbackContext context)
    {
        // Assure the movement is valid
        if (!context.performed) 
            return;

        if (menuSceneCurrentState != MenuSceneState.MenuPanelState)
            return;

        Vector2 direction = context.ReadValue<Vector2>();     
        float vertical = direction.y;

        // Assure only take into account user's inputs every 200 ms (moveCoolDownUserInput)
        if (Time.time - lastMoveTimeUserInput < moveCoolDownUserInput)
            return;

        if (vertical > 0.7f)
        {
            SelectionUINavigateUp();
            lastMoveTimeUserInput = Time.time;
        }            
        else if (vertical < -0.7f)
        {
            SelectionUINavigateDown();
            lastMoveTimeUserInput = Time.time;
        }        
    }
    public void SelectionUINavigateUp()
    {
        if (uiMenuSelect == UIMenuSelect.StartGame)
        {
            uiMenuSelect = UIMenuSelect.QuitGame;            
        }

        else
        {
            uiMenuSelect++;
        }

        // Play the Switch Option SFX
        PlayMenuSwitchOptionSFx();

        // Update the Visual Select on UI        
        menuPanelSelector.UpdateSelectorPos(uiMenuSelect);
    }
    public void SelectionUINavigateDown()
    {
        if (uiMenuSelect == UIMenuSelect.QuitGame)
        {
            uiMenuSelect = UIMenuSelect.StartGame;            
        }

        else
        {
            uiMenuSelect--;
        }

        // Play the Switch Option SFX
        PlayMenuSwitchOptionSFx();

        // Update the Visual Select on UI        
        menuPanelSelector.UpdateSelectorPos(uiMenuSelect);
    }
    public void PauseResumeGameInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)                    
            TooglePause();
    }
    public void QuitGameUI(InputAction.CallbackContext context)
    {
        //if (isWebGL)
        //    SceneManager.LoadScene(Scenes.Menu.ToString());
        //else
            QuitGame();
    }
    public void QuitGame()
    {
    #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
    #else
        Application.Quit();            
    #endif
    }
    #endregion
    #region Audio Clips
    #region Level Music
    public void StopMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();
    }
    public void PlayLevelMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.loop = true;
        generalAudioSource.clip = levelMusicClip;
        generalAudioSource.volume = levelMusicVolume;
        generalAudioSource.Play();
    }
    public void PlayEndGameMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.loop = true;
        generalAudioSource.clip = endGameMusicClip;
        generalAudioSource.volume = endGameMusicVolume;
        generalAudioSource.Play();
    }
    #endregion
    #region Menu Music
    public void PlayMenuMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.loop = true;
        generalAudioSource.clip = menuMusicClip;
        generalAudioSource.volume = menuMusicVolume;
        generalAudioSource.Play();
    }
    #endregion
    #region Game Over
    public void PlayGameOverSFx()
    {
        //if (generalAudioSource.isPlaying)
        //{
        //    generalAudioSource.Stop();
        //    generalAudioSource.loop = false;
        //}            

        generalAudioSource.PlayOneShot(gameOverClip,gameOverVolume);
    }
    #endregion
    #region Death
    public void PlayDeathSFx()
    {
        //if (generalAudioSource.isPlaying)
        //{
        //    generalAudioSource.Stop();
        //    generalAudioSource.loop = false;
        //}

        generalAudioSource.PlayOneShot(deathClip, deathVolume);
    }
    #endregion
    #region EndOfLevel
    public void PlayEndOfLevelSFx()
    {
        if (generalAudioSource.isPlaying)
        {
            generalAudioSource.Stop();
            generalAudioSource.loop = false;
        }            

        generalAudioSource.PlayOneShot(endOfLevelClip,endOfLevelVolume);
    }
    #endregion
    #region Menu SFX
    public void PlayMenuSwitchOptionSFx()
    {
        //if (generalAudioSource.isPlaying)
        //{
        //    generalAudioSource.Stop();
        //    generalAudioSource.loop = false;
        //}

        generalAudioSource.PlayOneShot(menuSwitchOptionClip, menuSwitchOptionVolume);
    }
    public void PlayMenuSelectOptionSFx()
    {
        //if (generalAudioSource.isPlaying)
        //{
        //    generalAudioSource.Stop();
        //    generalAudioSource.loop = false;
        //}

        generalAudioSource.PlayOneShot(menuSelectOptionClip, menuSelectOptionVolume);
    }
    public void PlayMenuStartGameSFx()
    {
        //if (generalAudioSource.isPlaying)
        //{
        //    generalAudioSource.Stop();
        //    generalAudioSource.loop = false;
        //}

        generalAudioSource.PlayOneShot(menuStartGameClip, menuStartGameVolume);
    }
    #endregion
    #endregion
    #region Slow Motion
    public void EnableSlowMotion()
    {
        Time.timeScale = 0.2f; // Velocidad al 20%
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Ajustar física
    }
    private void SlowMotionOnHit(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        // 1. Mata cualquier tween previo con el ID "SlowTime"
        DOTween.Kill("SlowTime");

        // 2. Pone el tiempo a 0 instantáneamente
        Time.timeScale = 0.1f; //0.2f;

        // 3. Usa DOVirtual.DelayedCall para esperar slowDuration segundos en tiempo real
        DOVirtual.DelayedCall(slowMotAndDeafDelayDuration, () =>
        {            
            // 4. Cuando termina la espera, comienza el tween para interpolar Time.timeScale de 0 a 1
            DOTween.To(() => Time.timeScale, 
                    x => 
                        {
                        Time.timeScale = x;
                        Time.fixedDeltaTime = 0.02f * x;
                        }, 
                    1f, DeafBwdDuration)
                //.SetEase(Ease.OutCubic)
                .SetEase(Ease.InQuad)
                // 5. Le da un ID para poder controlar o cancelar este tween luego
                .SetId("SlowTime")
                // 6. Hace que el tween corra usando tiempo real, ignorando Time.timeScale
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    // Trigger the OnHitPhysicsPlayer Event  
                    OnHitPhysicsPlayer?.Invoke(thrustEnemyDir, thrustEnemyForce);                    
                });
        })
        // 7. Hace que la llamada retrasada también use tiempo real para contar correctamente
        .SetUpdate(true);
    }
    private void SlowMotionOnDeath()
    {
        // 1. Mata cualquier tween previo con el ID "SlowTime"
        DOTween.Kill("SlowTime");

        // 2. Pone el tiempo a 0 instantáneamente
        Time.timeScale = 0.1f; //0.2f;

        // 3. Usa DOVirtual.DelayedCall para esperar slowDuration segundos en tiempo real
        DOVirtual.DelayedCall(slowMotAndDeafDelayDuration, () =>
        {            
            // 4. Cuando termina la espera, comienza el tween para interpolar Time.timeScale de 0 a 1
            DOTween.To(() => Time.timeScale, 
                    x => 
                        {
                        Time.timeScale = x;
                        Time.fixedDeltaTime = 0.02f * x;
                        }, 
                    1f, DeafBwdDuration*2f)
                //.SetEase(Ease.OutCubic)
                .SetEase(Ease.InQuad)
                // 5. Le da un ID para poder controlar o cancelar este tween luego
                .SetId("SlowTime")
                // 6. Hace que el tween corra usando tiempo real, ignorando Time.timeScale
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    // Trigger the Camera Death Shaking
                    cameraFollow.CameraDeathShaking();
                    PlayDeathSFx();
                });
        })
        // 7. Hace que la llamada retrasada también use tiempo real para contar correctamente
        .SetUpdate(true);
    }    
    private void SlowMotionOnHit_()
    {
        StartCoroutine(nameof(SlowMotionOnHitCoroutine));
    }    
    IEnumerator SlowMotionOnHitCoroutine()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(0.3f);

        elapsedSlowHitTime = 0f;

        while (elapsedSlowHitTime < durationSlowHitTime)
        {
            float t = elapsedSlowHitTime / durationSlowHitTime;
            Time.timeScale = Mathf.Lerp(0f,1f,t);
                        
            yield return null;
            elapsedSlowHitTime += Time.unscaledDeltaTime;
        }        

        Time.timeScale = 1f;
    }
    #endregion
    #region DeafeningAudioEffect    
    private void ApplyDeafeningSFX(Vector2 thrustEnemyDir, float thrustEnemyForce)
    {
        float sfxVolumeValue1;
        float sfxVolumeValue2;
        float musicVolumeValue1;        
        float musicVolumeValue2;

        audioMixer.GetFloat(sfxVolumeParam, out sfxVolumeValue1);
        audioMixer.GetFloat(musicVolumeParam, out musicVolumeValue1);

        //Debug.Log("Music Volume Before = " + musicVolumeValue1);
        //Debug.Log("SFX Volume Before = " + sfxVolumeValue1);

        ApplyDeafSFX()
            .AppendInterval(slowMotAndDeafDelayDuration)
            .AppendCallback(() => RemoveDeafSFX());

        audioMixer.GetFloat(sfxVolumeParam, out sfxVolumeValue2);
        audioMixer.GetFloat(sfxVolumeParam, out musicVolumeValue2);

        //Debug.Log("Music Volume After = " + musicVolumeValue2);
        //Debug.Log("SFX Volume After = " + sfxVolumeValue2);
    }
    private Tween AudioMixerParamInterpolation(AudioMixer audiomixer, string param, float targetValue, float duration, Ease easeType)
    {
        // Attenuation for SFX Group
        return DOTween.To(() =>
        {
            float currentValue;
            audioMixer.GetFloat(param, out currentValue);
            return currentValue;
        },
                x => audioMixer.SetFloat(param, x),
                targetValue,
                duration
        ).SetEase(easeType);
    }
    private Sequence ApplyDeafSFX()
    {    
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Low Pass Filter applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMinFreq, filterDeafFwdDuration, Ease.OutQuad));
        // Low Pass Filter applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMinFreq, filterDeafFwdDuration, Ease.OutQuad));

        // Volume attenuation applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMinValue, volumeDropDeafFwdDuration, Ease.OutQuad));
        // Volume attenuation applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMinValue, volumeDropDeafFwdDuration, Ease.OutQuad));

        return sequence;    
    }
    private Sequence RemoveDeafSFX()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // Low Pass Filter disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxLowPassParam, lowPassMaxFreq, DeafBwdDuration, Ease.OutQuad));
        // Low Pass Filter disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicLowPassParam, lowPassMaxFreq, DeafBwdDuration, Ease.OutQuad));

        // Volume attenuation disabled applied for SFX Group        
        sequence.Join(AudioMixerParamInterpolation(audioMixer, sfxVolumeParam, volumeMaxValue, DeafBwdDuration, Ease.OutQuad));
        // Volume attenuation disabled applied for Music Group
        sequence.Join(AudioMixerParamInterpolation(audioMixer, musicVolumeParam, volumeMaxValue, DeafBwdDuration, Ease.OutQuad));

        return sequence;
    }
    #endregion
    #region Pause-Resume    
    private void TooglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            EnablePauseInput();
            PauseGame();
            OnPauseEnabled?.Invoke();
        }
        else
        {
            EnableGameplayInput();
            ResumeGame();
            OnPauseDisabled?.Invoke();
        }
    }    
    private void PauseGame()
    {
        Time.timeScale = 0f;                // Stops the game (stop the physics and pending updates which are time dependent)

        // Pause the Background Music
        generalAudioSource.Pause();

        // UI Panels Update
        pausePanel.SetActive(true);

        //if (sceneSelected != Scenes.Menu)
        //    ShowMouseCursor(true);
        // Update the Panel Selected State
        //panelSelected = PanelSelected.Pause;
    }
    private void ResumeGame()
    {
        Time.timeScale = 1f;                // Resumes the game

        // Resume the Background Music
        generalAudioSource.UnPause();

        // UI Panels Update
        pausePanel.SetActive(false);

        //if (sceneSelected != Scenes.Menu)
        //    ShowMouseCursor(false);
        //panelSelected = PanelSelected.Game;     // As the Pause can be launch from any Panel this could be wrong (NEEDED TO UPDATE!)
    }
    #endregion
    #region CheckPoint
    private void SetInitCheckPointData()
    {
        lastCheckpointData.camTriggerAreaId = CamTriggerAreaEnum.CamTriggerArea.Init;
        if (initPos == null || initCamBoundTriggerArea == null)
            Debug.LogError("No any Init Respawn Pos and/or Cam Bound Trigger Area were assigned!");
        else
        {
            lastCheckpointData.respawnPos = initPos;
            lastCheckpointData.respawnCamBoundTriggerArea = initCamBoundTriggerArea;
        }
    }
    public void SetLastCheckPointData(CamTriggerAreaData camTriggerAreaData)
    {
        lastCheckpointData = camTriggerAreaData;        

        // If we arrive to the checkpoint just after the Boulder Event then we set
        // its flag in order to avoid repeat it again.
        if (lastCheckpointData.camTriggerAreaId == CamTriggerAreaEnum.CamTriggerArea.KeyArea)
        {
            SetBoulderEventDoneFlag(true);
        }        
    }
    #endregion
    #region Boulder Spawn
    private void SetBoulderEventDoneFlag(bool enable)
    {
        isBoulderEventDone = enable;
    }
    private void BoulderRespawn()
    {
        if (boulderGO != null)
            Destroy(boulderGO);
        else        
            Debug.LogError("The Round Boulder GO was not found on the Scene");
       
        boulderGO = Instantiate(boulderPrefab);
    }
    #endregion
    #region CameraZoomInOut
    public void CameraZoomIn()
    {
        cameraFollow.ZoomIn();
    }
    public void CameraZoomOut()
    {
        cameraFollow.ZoomOut();
    }
    #endregion
    #region ChestEvents
    public void HandleChestOpened(ItemType itemType)
    {
        if (itemType == ItemType.ClimbingBoots)
            StartCoroutine(nameof(BootsAcquired));
        else
            StartCoroutine(nameof(HookAcquired));
    }
    private IEnumerator BootsAcquired()
    {
        // Disable Player Input
        DisableAllInputs();

        // Wait till seq. is finished
        yield return new WaitForSeconds(bootsWaitingTime);

        // Trigger Player Ghost Sequence
        replayManager.StartPlayback(GhostPaths.WallJumpingPath);

        // Wait for the playback to start
        yield return new WaitUntil(() => playerPlayback.IsPlaying);

        // Wait for the playback to finish
        yield return new WaitUntil(() => !playerPlayback.IsPlaying);

        // Wait till seq. is finished
        //yield return new WaitForSeconds(bootsWaitingTime);

        // Enable Player Input
        EnableGameplayInput();
    }
    private IEnumerator HookAcquired()
    {
        // Disable Player Input
        DisableAllInputs();

        // Delay
        yield return new WaitForSeconds(hookWaitingTime);

        // Trigger Cam Movement to Columns (Go to Columns Pos. + Destroy Columns + Back to player Pos.)
        yield return StartCoroutine(cameraFollow.MoveCamTargetToDestColumnsPos(columnDestroyer));        

        yield return StartCoroutine(cameraFollow.MoveCamTargetToWallJumpAccessPlatform(columnDestroyer));
        
        // Trigger Player Ghost Sequence
        replayManager.StartPlayback(GhostPaths.HookPath);

        // Wait for the playback to start
        yield return new WaitUntil(() => playerPlayback.IsPlaying);

        // Wait for the playback to finish
        yield return new WaitUntil(() => !playerPlayback.IsPlaying);

        // Wait till sequence is finished
        //yield return new WaitForSeconds(hookWaitingTime);

        // Enable Player Input
        EnableGameplayInput();
    }
    #endregion
    #region GemsUIPos    
    //public Vector3 GetGemsUIPos()
    //{                
    //    // Convert the Gem UI Pos from Screen Pos to World Pos 
    //    Vector3 screenPos = gemsUIImage.position;
    //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
    //    worldPos.z = 0;

    //    return worldPos;
    //}
    //public Vector3 GetLifesUIPos()
    //{                
    //    // Convert the Gem UI Pos from Screen Pos to World Pos 
    //    Vector3 screenPos = lifesUIImage.position;
    //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
    //    worldPos.z = 0;

    //    return worldPos;
    //}
    #endregion
}
