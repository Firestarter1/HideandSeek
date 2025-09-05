using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour, IDamage, IHeal, IPickup
{
    [SerializeField] LayerMask ignoreLayer;
    [SerializeField] CharacterController controller;

    [Header("Player Settings:")]
    [SerializeField] List<Item> inventory = new List<Item>();
    [SerializeField] int HP;
    [SerializeField] int speed;
    [SerializeField] int sprintMod;
    [SerializeField] int jumpSpeed;
    [SerializeField] int jumpMax;
    [SerializeField] int gravity;
    [SerializeField] int interactDist;
    [SerializeField] int wallet;

    [Header("Gun Settings:")]
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDamage;
    [SerializeField] float shootRate;
    [SerializeField] int shootDist;

    public Transform notAiming;
    public Transform _Aiming;

    private GameObject interactPrompt;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;

    int jumpCount;
    int HPOrig;
    int gunListPos;

    bool isSprinting;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        interactPrompt = GameManager.Instance.interactPrompt;

        spawnPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);
        movement();
        sprint();
        CheckInteractable();
        updatePlayerUI();
    }
    void movement()
    {
        shootTimer += Time.deltaTime;

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
           (Input.GetAxis("Vertical") * transform.forward);

        controller.Move(moveDir * speed * Time.deltaTime);

        jump();

        controller.Move(playerVel * Time.deltaTime);

        playerVel.y -= gravity * Time.deltaTime;

        //seletGun();

        reload();

        if (Input.GetButtonDown("Use") || Input.GetButton("Fire1"))
        {
            InventoryManager.Instance.GetSelectedItem(true);
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            jumpCount++;
            playerVel.y = jumpSpeed;
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            isSprinting = true;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            isSprinting = false;
        }
    }

    public void shoot()
    {
        shootTimer = 0;
        GunStates currGun = (GunStates)InventoryManager.Instance.GetSelectedItem(false);
        currGun.ammoCurr--;
        UpdateGunUI(currGun);

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            //Debug.Log(hit.collider.name);
            Instantiate(currGun.hitEffect, hit.point, Quaternion.identity);

            IDamage dmg = hit.collider.GetComponent<IDamage>();

            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            GunStates currGun = (GunStates)InventoryManager.Instance.GetSelectedItem(false);
            currGun.ammoCurr = currGun.ammoMax;
            UpdateGunUI(currGun);
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        updatePlayerUI();
        StartCoroutine(flashDamageScreen());

        if (HP <= 0)
        {
            GameManager.Instance.youLose();
        }
    }

    public void Heal(int amount)
    {
        HP += amount;

        updatePlayerUI();
        StartCoroutine(flashHealScreen());
    }

    public void updatePlayerUI()
    {
        GameManager.Instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        GameManager.Instance.walletText.text = wallet.ToString();
    }

    public void UpdateGunUI(GunStates currGun)
    {
        GameManager.Instance.ammoCurrentText.text = currGun.ammoCurr.ToString();
        GameManager.Instance.ammoMaxText.text = currGun.ammoMax.ToString();
    }

    public void spawnPlayer()
    {
        controller.enabled = false;
        controller.transform.position = GameManager.Instance.playerSpawnPos.transform.position;
        controller.enabled = true;

        playerVel = Vector3.zero;
        HP = HPOrig;
        updatePlayerUI();
    }

    IEnumerator flashDamageScreen()
    {
        GameManager.Instance.playerDamageScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.playerDamageScreen.SetActive(false);
    }

    IEnumerator flashHealScreen()
    {
        GameManager.Instance.playerHealScreen.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        GameManager.Instance.playerHealScreen.SetActive(false);
    }

    //public void getGunStats(GunStates gun)
    //{
    //    gunList.Add(gun);
    //    gunListPos = gunList.Count - 1;

    //    changeGun();
    //}

    public void changeGun()
    {
        GunStates currGun = (GunStates)InventoryManager.Instance.GetSelectedItem(false); ;

        shootDamage = currGun.shootDamage;
        shootDist = currGun.shootDist;
        shootRate = currGun.shootRate;

        UpdateGunUI(currGun);

        gunModel.GetComponent<MeshFilter>().sharedMesh = currGun.model.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = currGun.model.GetComponent<MeshRenderer>().sharedMaterial;
    }

    //void seletGun()
    //{
    //    if(Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
    //    {
    //        gunListPos++;
    //        changeGun();
    //    }
    //    else if(Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
    //    {
    //        gunListPos--;
    //        changeGun();
    //    }
    //}

    void CheckInteractable()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit))
        {
            IInteractable interact = hit.collider.GetComponent<IInteractable>();

            if (interact != null)
            {
                interactPrompt.SetActive(true);
            }
            else if (interact == null)
            {
                interactPrompt.SetActive(false);
            }
        }
    }

    public void UpdateWallet(int amount)
    {
        wallet += amount;
    }

    public int CheckFunds()
    {
        return wallet;
    }

    public void Aiming()
    {
        if(Input.GetButtonDown("ADS"))
        {
            gunModel.transform.position = _Aiming.position;
        }
        if (Input.GetButtonDown("ADS"))
        {
            gunModel.transform.position = notAiming.position;
        }
    }
}
