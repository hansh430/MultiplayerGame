using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [SerializeField] Transform viewPoint;
    [SerializeField] float mouseSenstivity = 1;
    [SerializeField] float moveSpeed = 5f, runSpeed=8f;
    private float activeMoveSpeed;
    [SerializeField] CharacterController charCon;
    private Vector3 moveDir,movement;
    private float verticalRotStore;
    private Vector2 mouseInput;
    private Camera cam;
    [SerializeField] float jumpForce = 12f, gravityMod=2.5f;
    public Transform groundCheckPoint;
    private bool isGrounded;
    public LayerMask groundLayers;
    [SerializeField] GameObject bulletImpact;
   // [SerializeField] float timeBetweenShots = 0.1f;
    private float shotCounter;
    private float maxHeat = 10f, /* heatperShot = 1f,*/ coolRate = 4f, overHeatCoolRate = 5;
    private float heatCounter;
    private bool overHeated;
    public Gun[] allGuns;
    private int selectedGun;
    private float muzzleDisplayTime;
    private float muzzleCounter;
    public GameObject playerHitimpact;
    public int maxHealth;
    private int currentHealth;
    public Animator anim;
    public GameObject playerModel;
    public Transform modelGunPoint,gunHolder;
    public Material[] allSkins;
    public float adsSpeed=5f;
    public Transform adsInPoint, adsOutPoint;
    public AudioSource footstepSlow, footstepFast;

    void Start()
    {
        currentHealth = maxHealth;
        cam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        UIController.instance.weaponTempSlider.maxValue = maxHeat;
       // SwitchGun();
       photonView.RPC("SetGun",RpcTarget.All,selectedGun);
        if(photonView.IsMine)
        {
            playerModel.SetActive(false);
            UIController.instance.healthSlider.maxValue = maxHealth;
            UIController.instance.healthSlider.value=currentHealth;
        }
        else{
            gunHolder.parent= modelGunPoint;
            gunHolder.localPosition = Vector3.zero;
            gunHolder.localRotation = Quaternion.identity;
        }
        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];
    }

  
    void Update()
    {
        if (photonView.IsMine)
        {
            mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * mouseSenstivity;
            transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);
            verticalRotStore += mouseInput.y;
            verticalRotStore = Mathf.Clamp(verticalRotStore, -60f, 60f);
            viewPoint.rotation = Quaternion.Euler(-verticalRotStore, viewPoint.eulerAngles.y, viewPoint.rotation.eulerAngles.z);

            PlayerMove();
            CursorLockUnlock();
            if (allGuns[selectedGun].muzleFlash.activeInHierarchy)
            {
                muzzleCounter -= Time.deltaTime;
                if (muzzleCounter <= 0)
                {
                    allGuns[selectedGun].muzleFlash.SetActive(false);
                }
            }

            if (!overHeated)
            {
                if (Input.GetMouseButtonDown(0) && ! UIController.instance.isPaused)
                {
                    Shoot();
                }
                if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic && UIController.instance.isPaused)
                {
                    shotCounter -= Time.deltaTime;
                    if (shotCounter <= 0)
                    {
                        Shoot();
                    }
                }
                heatCounter -= coolRate * Time.deltaTime;
            }
            else
            {
                heatCounter -= overHeatCoolRate * Time.deltaTime;
                if (heatCounter <= 0)
                {

                    overHeated = false;
                    UIController.instance.overHeatedMsg.gameObject.SetActive(false);
                }
            }
            if (heatCounter < 0)
            {
                heatCounter = 0;
            }
            UIController.instance.weaponTempSlider.value = heatCounter;
            if(!UIController.instance.isPaused)
            {
                GunSwitching();
            }
            anim.SetBool("grounded",isGrounded);
            anim.SetFloat("speed",moveDir.magnitude);
        }
        AdsZoom();

    }
    public void AdsZoom()
    {
        if(Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom,adsSpeed*Time.deltaTime);
            gunHolder.position = Vector3.Lerp(gunHolder.position, adsInPoint.position, adsSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);
            gunHolder.position = Vector3.Lerp(gunHolder.position, adsOutPoint.position, adsSpeed * Time.deltaTime);
        }
    }
    private void LateUpdate()
    {
        if(photonView.IsMine)
        {
            if(MatchManager.instance.state ==MatchManager.GameState.Playing)
            {
                cam.transform.position = viewPoint.transform.position;
                cam.transform.rotation = viewPoint.transform.rotation;
            }
            else
            {
                cam.transform.position = MatchManager.instance.mapCamPint.position;
                cam.transform.rotation = MatchManager.instance.mapCamPint.rotation;

            }
        }
    }

    void GunSwitching()
    {
        if(Input.GetAxisRaw("Mouse ScrollWheel")>0)
        {
            selectedGun++;
            if(selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
           // SwitchGun();
       photonView.RPC("SetGun",RpcTarget.All,selectedGun);

        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel")<0)
        {
            selectedGun--;
            if (selectedGun < 0)
            {
                selectedGun = allGuns.Length-1;
            }
          //  SwitchGun();
       photonView.RPC("SetGun",RpcTarget.All,selectedGun);

        }
        for(int i=0; i<allGuns.Length; i++)
        {
            if(Input.GetKey((i+1).ToString()))
            {
                selectedGun = i;
             //   SwitchGun();
       photonView.RPC("SetGun",RpcTarget.All,selectedGun);
            }
        }
    }
    void SwitchGun()
    {
        foreach (Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
        allGuns[selectedGun].muzleFlash.SetActive(false);
    }
    [PunRPC]
    public void SetGun(int gunToSwitch)
    {
        if(gunToSwitch<allGuns.Length)
        {
            selectedGun=gunToSwitch;
            SwitchGun();
        }
    }
    void CursorLockUnlock()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Cursor.lockState == CursorLockMode.None)
        {
            if (Input.GetMouseButtonDown(0)  && !UIController.instance.optionScreen.activeInHierarchy)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }
    void PlayerMove()
    {
        moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        if(Input.GetKey(KeyCode.LeftShift))
        {
            activeMoveSpeed = runSpeed;
            if(!footstepFast.isPlaying && moveDir != Vector3.zero)
            {
                footstepFast.Play();
                footstepSlow.Stop();
            }
        }
        else
        {
            activeMoveSpeed = moveSpeed;
            if (!footstepSlow.isPlaying && moveDir != Vector3.zero)
            {
                footstepFast.Stop();
                footstepSlow.Play();
            }
        }
        if(moveDir == Vector3.zero || !isGrounded)
        {
            footstepSlow.Stop();
            footstepFast.Stop();
        }
        float yVel = movement.y;
        movement = ((transform.forward * moveDir.z) + (transform.right * moveDir.x)).normalized*activeMoveSpeed;
        movement.y = yVel;
        if(charCon.isGrounded)
        {
            movement.y = 0;
        }

        isGrounded = Physics.Raycast(groundCheckPoint.position, Vector3.down, 0.25f, groundLayers);


        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            movement.y = jumpForce;
        }
        movement.y += Physics.gravity.y * Time.deltaTime*gravityMod;
        charCon.Move(movement*Time.deltaTime);
    }
  
    private void Shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
           // Debug.Log("we hitted: " + hit.collider.gameObject.name);
           if(hit.collider.gameObject.tag=="Player")
            {
                Debug.Log("hit: " + hit.collider.gameObject.GetPhotonView().Owner.NickName);
                PhotonNetwork.Instantiate(playerHitimpact.name, hit.point, Quaternion.identity);
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All,photonView.Owner.NickName,allGuns[selectedGun].shotDamage,PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                GameObject bulletImpactObj = Instantiate(bulletImpact, hit.point + (hit.normal * 0.002f), Quaternion.LookRotation(hit.normal, Vector3.up));
                Destroy(bulletImpactObj, 10f);
            }
        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        heatCounter += allGuns[selectedGun].heatPerShot;
        if (heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            UIController.instance.overHeatedMsg.gameObject.SetActive(true);
        }
        allGuns[selectedGun].muzleFlash.SetActive(true);
        muzzleCounter = muzzleDisplayTime;
        allGuns[selectedGun].shotSound.Stop();
        allGuns[selectedGun].shotSound.Play();
    }
    [PunRPC]
    public void DealDamage(string damager, int damageAmount,int actor)
    {
        TakeDamage(damager,damageAmount,actor);
    }
    public void TakeDamage(string damager, int damageAmount,int actor)
    {
        if(photonView.IsMine)
        {
            // Debug.Log(photonView.Owner.NickName + " have been hitted by " + damager);
            //  gameObject.SetActive(false);
            currentHealth -= damageAmount;
            if(currentHealth<=0)
            {
                currentHealth = 0;
                PlayerSpawner.instance.Die(damager);
                MatchManager.instance.UpdateStatSend(actor, 0, 1);
            }
        UIController.instance.healthSlider.value=currentHealth;
        }
    }
}
