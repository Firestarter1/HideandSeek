using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Cinemachine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Menus:")]
    [SerializeField] public GameObject menuActive;
    public PauseMenu menuPause;
    [SerializeField] WinMenuController menuWin;
    [SerializeField] LoseMenuController menuLose;
    [SerializeField] GameObject menuStore;
    [SerializeField] GameObject menuInventory;

    [SerializeField] TMP_Text gameGoalCountText;

    [Header("Player References:")]
    public GameObject player;
    public PlayerController playerScript;
    public GameObject playerSpawnPos;

    [Header("Player UI:")]
    public Image playerHPBar;
    public TMP_Text walletText;
    public Image playerDamageScreen;
    public Image playerHealScreen;
    public GameObject interactPrompt;

    [Header("Gun UI:")]
    public TMP_Text ammoCurrentText;
    public TMP_Text ammoMaxText;
    public TMP_Text ammoTypeText;
    public Image ammoImage;

    public bool isPaused;

    float timeScaleOrig;

    int gameGoalCount;

    bool unpausing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();

        playerSpawnPos = GameObject.FindWithTag("Player Spawn Pos");

        
    }

    private void Start()
    {
        if (WaveManager.instance != null && gameGoalCountText.gameObject.activeInHierarchy)
        {
            gameGoalCountText.gameObject.SetActive(false);
        } else if (WaveManager.instance == null && !gameGoalCountText.gameObject.activeInHierarchy)
        {
            gameGoalCountText.gameObject.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null && !unpausing)
            {
                statePause();
                menuActive = menuPause.gameObject;
                menuActive.SetActive(true);
                menuPause.OpenMenu();
            }
            else if (!unpausing && menuActive == menuPause.gameObject && !menuPause.transitioning && menuPause.settingsOpen)
            {
                menuPause.CloseSettingsMenu();
            }
            else if (!unpausing && menuActive == menuPause.gameObject && !menuPause.transitioning)
            {
                unpausing = true;
                if (menuActive == menuPause.gameObject)
                {
                    menuPause.CloseMenu();
                } else
                {
                    stateUnpause();
                }
                
            }
        }

        OpenInventory();
    }

    public void statePause()
    {
        isPaused = !isPaused;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
        if (CinemachineImpulseManager.Instance != null)
        {
            CinemachineImpulseManager.Instance.IgnoreTimeScale = false;
            CinemachineImpulseManager.Instance.Clear();
        }
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
        unpausing = false;

    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;

        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if (gameGoalCount <= 0)
        {
            // you win;
            WinState();
        }
    }

    public void WinState()
    {

        menuActive = menuWin.gameObject;
        menuWin.gameObject.SetActive(true);
        menuWin.WinMenuIn();
        SoundManager.Instance.StopMusic(false);
        SoundManager.Instance.PlayMusic("Win", 1.0f);
        StartCoroutine(DelayedPause(1.0f));
    }

    public void youLose()
    {
        menuActive = menuLose.gameObject;
        menuLose.gameObject.SetActive(true);
        menuLose.TriggerLoseMenu();
        SoundManager.Instance.StopMusic(false);
        SoundManager.Instance.PlaySoundFXClip(SoundType.Death_Static, player.transform.position, AudioGroup.SFX);
        StartCoroutine(DelayedPause(4.5f));
    }

    IEnumerator DelayedPause(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        statePause();
    }

    public void OpenStore()
    {
        statePause();

        menuActive = menuStore;
        menuActive.SetActive(true);
    }

    public void OpenInventory()
    {
        if(Input.GetButtonDown("Inventory"))
        {
            if(menuActive == null)
            {
                statePause();
                menuActive = menuInventory;
                menuActive.SetActive(true);

            }
            else if (menuActive == menuInventory)
            {
                stateUnpause();
            }
        }
    }
}
