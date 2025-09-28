using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.Events;
using System.Collections.Generic;
using DG.Tweening;

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

    private GameObject interactPrompt;

    Vector3 moveDir;
    Vector3 playerVel;

    float shootTimer;

    int jumpCount;
    int HPOrig;
    int gunListPos;

    public bool isSprinting;

    [SerializeField] BulletTracer tracer;

    [System.NonSerialized] public UnityEvent<float> healthUpdated;
    [System.NonSerialized] public UnityEvent<int> walletUpdated;
    [System.NonSerialized] public UnityEvent<int, int> ammoUpdated;

    Transform muzzleAnchor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        healthUpdated = new UnityEvent<float>();
        walletUpdated = new UnityEvent<int>();
        ammoUpdated = new UnityEvent<int, int>();
    }
    void Start()
    {
        HPOrig = HP;
        StartCoroutine(DelayedInvoke());
        interactPrompt = GameManager.Instance.interactPrompt;

        spawnPlayer();
    }

    IEnumerator DelayedInvoke()
    {
        yield return null;
        healthUpdated.Invoke(1);
        walletUpdated.Invoke(wallet);
        ammoUpdated.Invoke(-1, -1);
    }

    // Update is called once per frame
    void Update()
    {
        movement();
        sprint();
    }
    void movement()
    {
        shootTimer += Time.deltaTime;

        moveDir = (Input.GetAxis("Horizontal") * transform.right) +
           (Input.GetAxis("Vertical") * transform.forward);

        if (controller.isGrounded && playerVel.y < 0f)
        {
            jumpCount = 0;
            playerVel.y = -2f;
        }

        jump();

        playerVel.y -= gravity * Time.deltaTime;
        Vector3 velocity = moveDir * speed + playerVel;
        controller.Move(velocity * Time.deltaTime);

        

        //controller.Move(playerVel * Time.deltaTime);

        //playerVel.y -= gravity * Time.deltaTime;

        //seletGun();

        reload();

        
        if (GameManager.Instance.menuActive == null && (Input.GetButtonDown("Fire1")))
        {
            InventoryManager.Instance.GetSelectedItem(true);
        } else if (GameManager.Instance.menuActive == null && Input.GetButton("Fire1"))
        {
            Item i = InventoryManager.Instance.GetSelectedItem(false);
            if (i is GunStates)
            {
                GunStates g = (GunStates)i;
                if (g.autoFire)
                {
                    InventoryManager.Instance.GetSelectedItem(true);
                }
            }
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
        GunStates currGun = (GunStates)InventoryManager.Instance.GetSelectedItem(false);
        if (shootTimer < currGun.shootRate) return;
             
        shootTimer = 0;
        Transform shootPos = gunModel.transform;
        Transform prefabMuzzle = null;
        
        foreach (Transform child in currGun.model.transform)
        {
            if (child.CompareTag("Muzzle"))
            {
                prefabMuzzle = child;
                break;
            }
        }

        if (prefabMuzzle)
        {
            if (muzzleAnchor == null)
            {
                muzzleAnchor = new GameObject("MuzzleAnchor").transform;
                muzzleAnchor.SetParent(gunModel.transform, worldPositionStays: false);
            }

            muzzleAnchor.localPosition = prefabMuzzle.localPosition;
            muzzleAnchor.localRotation = prefabMuzzle.localRotation;
            muzzleAnchor.localScale = prefabMuzzle.localScale;

            shootPos = muzzleAnchor;
        }


        ((IShoot)currGun).Shoot(shootPos);
        gunModel.GetComponent<Animator>().SetTrigger("Fire");
        ammoUpdated.Invoke(currGun.ammoCurr, currGun.ammoStored);
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload"))
        {
            Item currItem = InventoryManager.Instance.GetSelectedItem(false);
            if (currItem is GunStates gun)
            {
                gun.Reload();
                ammoUpdated.Invoke(gun.ammoCurr, gun.ammoStored);
            }
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        HP = Mathf.Clamp(HP, 0, HPOrig);
        healthUpdated.Invoke(Mathf.Clamp01((float)HP/(float)HPOrig));
        //updatePlayerUI();
        FlashDamageScreen();

        if (HP <= 0)
        {
            GameManager.Instance.youLose();
        }
    }

    public void Heal(int amount)
    {
        HP += amount;
        HP = Mathf.Clamp(HP, 0, HPOrig);
        FlashHealScreen();
        healthUpdated.Invoke(Mathf.Clamp01((float)HP / (float)HPOrig));
    }

    public void spawnPlayer()
    {
        controller.enabled = false;
        controller.transform.position = GameManager.Instance.playerSpawnPos.transform.position;
        controller.enabled = true;
        healthUpdated.Invoke(1);
        walletUpdated.Invoke(wallet);
        ammoUpdated.Invoke(-1, -1);
        playerVel = Vector3.zero;
    }

    void FlashDamageScreen()
    {
        GameManager.Instance.playerDamageScreen.gameObject.SetActive(true);
        Color transparent = new Color(GameManager.Instance.playerDamageScreen.color.r, GameManager.Instance.playerDamageScreen.color.g, GameManager.Instance.playerDamageScreen.color.b, 0.0f);
        Color full = new Color(GameManager.Instance.playerDamageScreen.color.r, GameManager.Instance.playerDamageScreen.color.g, GameManager.Instance.playerDamageScreen.color.b, 0.5f);
        GameManager.Instance.playerDamageScreen.DOColor(full, 0.1f).OnComplete( () =>
        {
            GameManager.Instance.playerDamageScreen.DOColor(transparent, 0.1f).OnComplete( () =>
            {
                GameManager.Instance.playerDamageScreen.gameObject.SetActive(false);
            });
        });
    }

    void FlashHealScreen()
    {
        GameManager.Instance.playerHealScreen.gameObject.SetActive(true);
        Color transparent = new Color(GameManager.Instance.playerHealScreen.color.r, GameManager.Instance.playerHealScreen.color.g, GameManager.Instance.playerHealScreen.color.b, 0.0f);
        Color full = new Color(GameManager.Instance.playerHealScreen.color.r, GameManager.Instance.playerHealScreen.color.g, GameManager.Instance.playerHealScreen.color.b, 0.5f);
        GameManager.Instance.playerHealScreen.DOColor(full, 0.1f).OnComplete(() =>
        {
            GameManager.Instance.playerHealScreen.DOColor(transparent, 0.1f).OnComplete(() =>
            {
                GameManager.Instance.playerHealScreen.gameObject.SetActive(false);
            });
        });
    }

    //public void getGunStats(GunStates gun)
    //{
    //    gunList.Add(gun);
    //    gunListPos = gunList.Count - 1;

    //    changeGun();
    //}

    public void ChangeItem()
    {
        Item currItem = InventoryManager.Instance.GetSelectedItem(false);
        int currentAmmo = -1;
        int storedAmmo = -1;
        Mesh mesh = null;
        Material[] material = new Material[] { };
        if (currItem != null)
        {
            mesh = currItem.model.GetComponent<MeshFilter>().sharedMesh;
            material = currItem.model.GetComponent<MeshRenderer>().sharedMaterials;
            gunModel.transform.localRotation = currItem.model.transform.localRotation;
            gunModel.transform.localScale = currItem.model.transform.localScale;
        }
        
        if (currItem is GunStates currGun)
        {
            currentAmmo = currGun.ammoCurr;
            storedAmmo = currGun.ammoStored;
            
            currGun.Equip(this);
        }
        ammoUpdated.Invoke(currentAmmo, storedAmmo);
        gunModel.GetComponent<MeshFilter>().sharedMesh = mesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterials = material;
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


    public void UpdateWallet(int amount)
    {
        wallet += amount;
        walletUpdated.Invoke(wallet);
    }

    public int CheckFunds()
    {
        return wallet;
    }
}
