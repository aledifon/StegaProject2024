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
using static UIMenuSelectEnum;

using TMPro;
using System.Collections.Generic;
public class GameManager : MonoBehaviour
{
    // Singleton instance of GameManager
    private static GameManager instance;
    public static GameManager Instance
    {
        get 
        { 
            if (instance == null)
            {
                instance = FindAnyObjectByType<GameManager>();

                if (instance == null)
                {
                    Debug.LogWarning("No GameManager found in scene. Creating a new one.");
                    GameObject go = new GameObject("GameManager");
                    instance = go.AddComponent<GameManager>();
                }
            }
            return instance; 
        }
    }

    public static readonly bool isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
    //public static readonly bool isWebGL = false;     // true = WebGL, false = Windows

    [Header("Audio Clips")]
    [SerializeField] AudioClip gameOverClip;
    [SerializeField, Range(0f, 1f)] float gameOverVolume;  
    [SerializeField] AudioClip endOfLevelClip;
    [SerializeField, Range(0f, 1f)] float endOfLevelVolume;  
    [SerializeField] AudioClip LevelMusicClip;
    [SerializeField, Range(0f, 1f)] float levelMusicVolume;  // 0.25f

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
    private TextMeshProUGUI titleText;      // Fade In/Out alpha Image (0->100)
    private GameObject optionTextContainer; // Disable->Enable
    private List<RectTransform> optionPositions;
    private RectTransform selectorOption;   // Disable->Enable
    private TextMeshProUGUI buildVersionText;   // Disable->Enable

    private GameObject controlsPanel;       
    private GameObject introScenePanel;
    private TextMeshProUGUI introSceneText; // Machine writting VFX
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

    private GameObject endScenePanel;
    private TextMeshProUGUI endSceneText; // Machine writting VFX
    private TextMeshProUGUI endSceneText2; // Machine writting VFX

    private GameObject creditsGamePanel;
    private TextMeshProUGUI creditsText;      // Fade In/Out alpha Image (0->100->0)
    private TextMeshProUGUI madeByText;       // Fade In/Out alpha Text (0->100->0)
    private TextMeshProUGUI endGameText;      // Fade In/Out alpha Text (0->100->0)

    private Scenes sceneSelected = Scenes.Menu;    

    [Header("Slow Motion Test")]
    [SerializeField] private bool slowMotionEnabled;

    // Checkpoint & Starting Pos.   
    [Header("Starting Pos")]
    private Transform initPos;
    private CamBoundariesTriggerArea initCamBoundTriggerArea;
    private CamTriggerAreaData lastCheckpointData = new CamTriggerAreaData();    
    public CamTriggerAreaData LastCheckpointData => lastCheckpointData;    

    // GO Refs.
    AudioSource generalAudioSource;
    PlayerHealth playerHealth;
    PlayerMovement playerMovement;
    PlayerSFX playerSFX;

    // Player Ghost Refs
    ReplayManager replayManager;
    PlayerPlayback playerPlayback;
    PlayerRecorder playerRecorder;

    // Input Player Management    
    private InputActionAsset inputActions;

    private bool isPaused = false;
    public bool IsPaused => isPaused;

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

        SceneManager.sceneLoaded += OnSceneLoaded;  // Subscribe to the event.

        // Get GameManager refs
        generalAudioSource = GetComponent<AudioSource>();
        replayManager = GetComponent<ReplayManager>();

        // PENDING TO BE MOVED TO ADAPTED DUE TO SCENE MANAGEMENT
        //PlayLevelMusic();

        //EnableGameplayInput();

        //if(slowMotionEnabled)
        //    EnableSlowMotion();

        //// Set the filterDuration
        ////filterDuration = returnDuration;

        //// Set the Initial CheckPoint Data
        //SetInitCheckPointData();        

        // PENDING TO BE MOVED TO ADAPTED DUE TO SCENE MANAGEMENT
    }
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;        
        instance = null;
    }
    #endregion
    #region Menu Scene
    private IEnumerator UpdateSceneState()
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

            switch (menuSceneCurrentState)
            {
                // Enable IntroPanel
                case MenuSceneState.Init:

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
                    yield return new WaitForSeconds(1f);

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
                    yield return new WaitForSeconds(1f);

                    // Update Menu Scene State
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
                    yield return new WaitForSeconds(1f);

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
                    yield return new WaitForSeconds(1f);

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

                    // Disable the IntroPanel
                    introPanel.SetActive(false);
                    // Enable the MenuPanel
                    menuPanel.SetActive(true);

                    // Update Menu Scene State
                    menuSceneCurrentState = MenuSceneState.MenuPanelTitleTextFadeIn;
                    break;

                case MenuSceneState.MenuPanelTitleTextFadeIn:

                    //Set The target Color
                    menuPanelTitleTextTargetColor = titleText.color;
                    menuPanelTitleTextTargetColor.a = 0f;

                    // Color FadeIn (Black->Red)
                    yield return titleText
                        .DOColor(menuPanelTitleTextTargetColor, 1f)
                        .SetEase(Ease.InQuad)
                        .WaitForCompletion();

                    // Delay (Keeps the for x secs)
                    yield return new WaitForSeconds(1f);

                    // Enable the all the subpanels
                    optionTextContainer.SetActive(true);
                    selectorOption.gameObject.SetActive(true);
                    buildVersionText.gameObject.SetActive(true);

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
                        // Update Menu Scene State
                        menuSceneCurrentState = MenuSceneState.QuitGameState;
                    }
                    else if (uiMenuSelect == UIMenuSelect.Controls)
                    {
                        // Enable the ControlsPanel
                        controlsPanel.SetActive(true);

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

                    // Perform Type writting machine
                    yield return StartCoroutine(TypeWrittingText(introSceneText,introSceneTextStrEN));

                    // Delay
                    yield return new WaitForSeconds(1f);

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
    private IEnumerator TypeWrittingText(TextMeshProUGUI textBox, string str)
    {
        textBox.text = "";

        foreach (char c in str)
        {
            textBox.text += c;
            yield return new WaitForSeconds(0.15f);
        }        
    }
    #endregion
    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        canvas = GameObject.Find("Canvas");
        if (canvas == null)
            Debug.LogError("The Canvas object is null");

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

                    // Disable all the Inputs
                    DisableAllInputs();

                    // Trigger the Update Scene State Loop
                    StartCoroutine(nameof(UpdateSceneState));                    

                    break;
                case Scenes.Level:

                    // Set the new Scene as the current one
                    sceneSelected = currentScene;

                    // Get the GO Refs
                    GetLevelSceneRefs();
                    GetLevelRefs();

                    PlayLevelMusic();

                    EnableGameplayInput();

                    //if (slowMotionEnabled)
                    //    EnableSlowMotion();

                    // Set the filterDuration
                    //filterDuration = returnDuration;

                    // Set the Initial CheckPoint Data
                    SetInitCheckPointData();                    

                    break;
                default:
                    break;
            }
        }
    }
    private void GetMenuSceneRefs()
    {
        // Get all the Intro Panel GO's Refs                    
        introPanel = canvas.transform.Find("IntroPanel")?.gameObject;
        if (introPanel == null)
            Debug.LogError("The " + introPanel.name + " object is null");
        else
        {            
            introPanelImage = introPanel.transform?.GetComponent<Image>();
            if (introPanelImage == null)
                Debug.LogError("The " + introPanelImage.name + " component was not found " +
                                "on the " + introPanel.name + "GO ");

            stegaImage = introPanel.transform.Find("StegaImage")?.GetComponent<Image>();
            if (stegaImage == null)
                Debug.LogError("The " + stegaImage.name + " component was not found " +
                                "on the " + introPanel.name + "GO ");

            aledifonText = introPanel.transform.Find("AledifonText")?.GetComponent<TextMeshProUGUI>();
            if (aledifonText == null)
                Debug.LogError("The " + aledifonText.name + " component was not found " +
                                "on the " + introPanel.name + "GO ");
        }

        // Get all the Menu Panel GO's Refs                    
        menuPanel = canvas.transform.Find("MenuPanel")?.gameObject;
        if (menuPanel == null)
            Debug.LogError("The " + menuPanel.name + " object is null");
        else
        {

            titleText = menuPanel.transform.Find("TitleText")?.GetComponent<TextMeshProUGUI>();
            if (titleText == null)
                Debug.LogError("The " + titleText.name + " component was not found " +
                                "on the " + menuPanel.name + "GO ");

            optionTextContainer = menuPanel.transform.Find("OptionTextContainer")?.gameObject;
            if (optionTextContainer == null)
                Debug.LogError("The " + optionTextContainer.name + " GO was not found " +
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
                Debug.LogError("The " + selectorOption.name + " component was not found " +
                                "on the " + menuPanel.name + "GO ");

            buildVersionText = menuPanel.transform.Find("BuildVersionText").GetComponent<TextMeshProUGUI>();
            if (buildVersionText == null)
                Debug.LogError("The " + buildVersionText.name + " component was not found " +
                                "on the " + menuPanel.name + "GO ");
        }

        // Get all the Controls Panel GO's Refs                    
        controlsPanel = canvas.transform.Find("ControlsPanel")?.gameObject;
        if (controlsPanel == null)
            Debug.LogError("The " + controlsPanel.name + " object is null");

        // Get all the Intro Scene Panel GO's Refs                    
        introScenePanel = canvas.transform.Find("IntroScenePanel")?.gameObject;
        if (introScenePanel == null)
            Debug.LogError("The " + introScenePanel.name + " object is null");
    }
    private void GetLevelSceneRefs()
    {
        // Get all the GO's Refs                    
        healthPanel = canvas.transform.Find("HealthPanel")?.gameObject;
        if (healthPanel == null)
            Debug.LogError("The " + healthPanel.name + " object is null");
        else
        {
            healthUIImage = healthPanel.transform.Find("Health")?.GetComponent<RectTransform>();
            if (healthUIImage == null)
                Debug.LogError("The " + healthUIImage.name + " component was not found " +
                                "on the " + healthPanel.name + "GO ");
        }

        gemsPanel = canvas.transform.Find("GemsPanel")?.gameObject;
        if (gemsPanel == null)
            Debug.LogError("The " + gemsPanel.name + " object is null");
        else
        {
            gemsUIImage = gemsPanel.transform.Find("GemsCountImage")?.GetComponent<RectTransform>();
            if (gemsUIImage == null)
                Debug.LogError("The " + gemsUIImage.name + " component was not found " +
                                "on the " + gemsPanel.name + "GO ");
        }

        lifesPanel = canvas.transform.Find("LifesPanel")?.gameObject;
        if (lifesPanel == null)
            Debug.LogError("The " + lifesPanel.name + " object is null");
        else
        {
            lifesUIImage = lifesPanel.transform.Find("LifesCountImage")?.GetComponent<RectTransform>();
            if (lifesUIImage == null)
                Debug.LogError("The " + lifesUIImage.name + " component was not found " +
                                "on the " + lifesPanel.name + "GO ");
        }

        pausePanel = canvas.transform.Find("PausePanel")?.gameObject;
        if (pausePanel == null)
            Debug.LogError("The " + pausePanel.name + " object is null");
                
        endScenePanel = canvas.transform.Find("EndScenePanel")?.gameObject;
        if (endScenePanel == null)
            Debug.LogError("The " + endScenePanel.name + " object is null");
        else
        {
            endSceneText = endScenePanel.transform.Find("StoryTextEN")?.GetComponent<TextMeshProUGUI>();
            if (endSceneText == null)
                Debug.LogError("The " + endSceneText.name + " component was not found " +
                                "on the " + endScenePanel.name + "GO ");
            
            endSceneText2 = endScenePanel.transform.Find("ContinueTextEN")?.GetComponent<TextMeshProUGUI>();
            if (endSceneText2 == null)
                Debug.LogError("The " + endSceneText2.name + " component was not found " +
                                "on the " + endScenePanel.name + "GO ");
        }

        creditsGamePanel = canvas.transform.Find("CreditsGamePanel")?.gameObject;
        if (creditsGamePanel == null)
            Debug.LogError("The " + creditsGamePanel.name + " object is null");
        //else
        //{
        //    creditsText = creditsGamePanel.transform.Find("CreditsText")?.GetComponent<TextMeshProUGUI>();
        //    if (creditsText == null)
        //        Debug.LogError("The " + creditsText.name + " component was not found " +
        //                        "on the " + creditsGamePanel.name + "GO ");
            
        //    madeByText = creditsGamePanel.transform.Find("MadeByText")?.GetComponent<TextMeshProUGUI>();
        //    if (madeByText == null)
        //        Debug.LogError("The " + madeByText.name + " component was not found " +
        //                        "on the " + creditsGamePanel.name + "GO ");
            
        //    endGameText = creditsGamePanel.transform.Find("EndGameText")?.GetComponent<TextMeshProUGUI>();
        //    if (endGameText == null)
        //        Debug.LogError("The " + endGameText.name + " component was not found " +
        //                        "on the " + creditsGamePanel.name + "GO ");
        //}           

    }
    private void GetLevelRefs()
    {
        // Get all the GO's Refs                    
        initPos = GameObject.Find("SpawnInitPos")?.transform;
        if (initPos == null)
            Debug.LogError("The " + initPos.name + " component is null");

        initCamBoundTriggerArea = GameObject.Find("InitAreaCamTrigger")?.GetComponent<CamBoundariesTriggerArea>();
        if (initCamBoundTriggerArea == null)
            Debug.LogError("The " + initCamBoundTriggerArea.name + " component is null");
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
    }
    public void UnsubscribeEventsOfPlayerHealth()
    {
        if (playerHealth != null)
        {            
            playerHealth.OnHitFXPlayer -= SlowMotionOnHit;
            playerHealth.OnHitFXPlayer -= ApplyDeafeningSFX;
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
    public void EnableReplayManagerAndGetRefs()
    {        
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
        playerPlayback = null;
        playerRecorder = null;

        replayManager.enabled = false;
    }
    #endregion
    #region Input Action Maps
    public void GetInputActionMaps(InputActionAsset inputActionsAsset)
    {
        inputActions = inputActionsAsset;
    }    
    public void EnableUIMainMenuInput()
    {
        // Enable the UI-Main Menu Action Map & Disable the others.
        inputActions.FindActionMap("UI-MainMenu").Enable();
        inputActions.FindActionMap("Gameplay").Disable();
        inputActions.FindActionMap("UI-InGame").Disable();
    }
    public void EnableGameplayInput()
    {
        // Enable the Gameplay Action Map & Disable the others.
        inputActions.FindActionMap("Gameplay").Enable();
        inputActions.FindActionMap("UI-InGame").Disable();
        inputActions.FindActionMap("UI-MainMenu").Disable();
    }
    public void EnablePauseInput()
    {
        // Enable the Pause Action Map & Disable the others.        
        inputActions.FindActionMap("UI-InGame").Enable();
        inputActions.FindActionMap("Gameplay").Disable();
        inputActions.FindActionMap("UI-MainMenu").Disable();
    }
    public void DisableAllInputs()
    {
        inputActions.FindActionMap("Gameplay").Disable();
        inputActions.FindActionMap("UI-InGame").Disable();
        inputActions.FindActionMap("UI-MainMenu").Disable();
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

            // Depending where I am ('Panel') and selection ('Start Game'/'Options')                       

            // If (currentPanel == MenuPanel) && StartText.enabled -->
            // StartText.disable
            // StartButton & OptionsButton enabled
            //uiMenuSelect = UIMenuSelectEnum.UIMenuSelect.StartGame;

            // Else if (currentPanel == MenuPanel) && 'Options' Selected -->
            // currentPanel = OptionsPanel            

            // Else if (currentPanel == MenuPanel) && 'Quit Game' Selected -->
            // QuitGame();

            // Else if (currentPanel == MenuPanel) && 'Start Game' Selected -->
            // currentPanel = IntroScenePanel
            // Delay x secs
            // LoadScene(LevelScene);

            // Else if (currentPanel == OptionsPanel) -->
            // currentPanel = MenuPanel
            //uiMenuSelect = UIMenuSelectEnum.UIMenuSelect.StartGame;
        }
    }
    public virtual void SwitchSelectionUI(InputAction.CallbackContext context)
    {
        if (menuSceneCurrentState != MenuSceneState.MenuPanelState)
            return;

        Vector2 direction = context.ReadValue<Vector2>();     
        float vertical = direction.y;        

        if (vertical > 0.5f) 
            ;//NavigateUp()
        else if (vertical < 0.5f)
            ;//NavigateDown()
    }
    public void NavigateUp()
    {
        if (uiMenuSelect == UIMenuSelect.StartGame)
        {
            uiMenuSelect = UIMenuSelect.QuitGame;            
        }

        else
        {
            uiMenuSelect++;
        }

        // Update the Visual Select on UI
        // VisualArray[uiMenuSelect]
    }
    public void NavigateDown()
    {
        if (uiMenuSelect == UIMenuSelect.QuitGame)
        {
            uiMenuSelect = UIMenuSelect.StartGame;            
        }

        else
        {
            uiMenuSelect--;
        }

        // Update the Visual Select on UI
        // VisualArray[uiMenuSelect]
    }
    public void PauseResumeGameInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)                    
            TooglePause();
    }
    public void QuitGame(InputAction.CallbackContext context)
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
    public void PlayLevelMusic()
    {
        if (generalAudioSource.isPlaying)
            generalAudioSource.Stop();

        generalAudioSource.loop = true;
        generalAudioSource.clip = LevelMusicClip;
        generalAudioSource.volume = levelMusicVolume;
        generalAudioSource.Play();
    }
    #endregion
    #region Game Over
    public void PlayGameOverSFx()
    {
        if (generalAudioSource.isPlaying)
        {
            generalAudioSource.Stop();
            generalAudioSource.loop = false;
        }            

        generalAudioSource.PlayOneShot(gameOverClip,gameOverVolume);
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

        Debug.Log("Music Volume Before = " + musicVolumeValue1);
        Debug.Log("SFX Volume Before = " + sfxVolumeValue1);

        ApplyDeafSFX()
            .AppendInterval(slowMotAndDeafDelayDuration)
            .AppendCallback(() => RemoveDeafSFX());

        audioMixer.GetFloat(sfxVolumeParam, out sfxVolumeValue2);
        audioMixer.GetFloat(sfxVolumeParam, out musicVolumeValue2);

        Debug.Log("Music Volume After = " + musicVolumeValue2);
        Debug.Log("SFX Volume After = " + sfxVolumeValue2);
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

        // Update the Player's position
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
