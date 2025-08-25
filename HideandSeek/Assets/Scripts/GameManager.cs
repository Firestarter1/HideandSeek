using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] GameObject menuActive;
    [SerializeField] GameObject menuPause;
    [SerializeField] GameObject menuWin;
    [SerializeField] GameObject menuLose;
    [SerializeField] GameObject menuStore;
    [SerializeField] GameObject menuInventory;


    [SerializeField] TMP_Text gameGoalCountText;
    [SerializeField] TMP_Text walletText;

    public Image playerHPBar;
    public GameObject playerDamageScreen;
    public GameObject playerHealScreen;

    public GameObject player;
    public PlayerController playerScript;

    public bool isPaused;

    float timeScaleOrig;

    int gameGoalCount;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        playerScript = player.GetComponent<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdatePlayerUI();

        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive.SetActive(true);
            }
            else if (menuActive == menuPause || menuActive == menuStore)
            {
                stateUnpause();
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
    }

    public void stateUnpause()
    {
        isPaused = !isPaused;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        menuActive.SetActive(false);
        menuActive = null;
    }

    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;

        gameGoalCountText.text = gameGoalCount.ToString("F0");

        if (gameGoalCount <= 0)
        {
            // you win;
            statePause();
            menuActive = menuWin;
            menuActive.SetActive(true);
        }
    }

    public void youLose()
    {
        statePause();

        menuActive = menuLose;
        menuActive.SetActive(true);
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

    public void UpdatePlayerUI()
    {
        walletText.text = playerScript.CheckFunds().ToString();
    }
}
