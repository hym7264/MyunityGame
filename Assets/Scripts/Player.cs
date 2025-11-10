using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenades;
    public GameObject grenadeObj;
    public Camera followCamera;
    public int ammo;
    public int coin;
    public int health;
    public int maxammo;
    public int maxcoin;
    public int maxhealth;
    public int maxhasGrenades;
    
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;
    bool dDown;
    bool iDown;
    bool fDown;
    bool gDown;
    bool rDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isJump;
    bool isDodge;
    bool isSwap;
    bool isReload;
    bool isFireReady=true;
   
    
    bool isBorder;
    Vector3 moveVec;
    Vector3 dodgeVec;
    Rigidbody rigid;
    Animator anim;
    GameObject nearObject;
    Weapon equipWeapon;

    int equipWeaponIndex = -1;
    float fireDelay;
    // Start is called before the first frame update
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Reload();
        Move();
        Turn();
        Jump();
        Grenade();
        Dodge();
        Interation();
        Swap();
        SwapOut();
        Attack();
        
        //Reloadout();
    }
    void GetInput()
    {
        hAxis = Input.GetAxis("Horizontal");
        vAxis = Input.GetAxis("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        dDown = Input.GetButtonDown("Dodge");
        iDown = Input.GetButtonDown("Interation");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");




    }
    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;
        if (isDodge)
            moveVec = dodgeVec;
        if (isSwap || !isFireReady)
            moveVec = Vector3.zero;
        if (!isBorder)
        {
            transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;
        }

        
        anim.SetBool("isRun", moveVec != Vector3.zero);
        anim.SetBool("isWalk", wDown);

    }
    void Turn()
    {
        //키보드에의한 회전
        transform.LookAt(transform.position + moveVec);
        //마우스에의한 회전
        Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);

        RaycastHit rayHit;
        if (fDown)
        {
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;

                transform.LookAt(transform.position + nextVec);
            }
        }


    }
    void Jump()
    {
        if (jDown && !isJump && !isDodge && !isSwap)
        {
            rigid.AddForce(Vector3.up * 10, ForceMode.Impulse);
            anim.SetBool("isJump", true);
            anim.SetTrigger("doJump");
            isJump = true;
        }
    }
    void Grenade()
    {
        if(hasGrenades==0)
            return;
        if (gDown && !isReload && !isSwap)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);

            RaycastHit rayHit;
            
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 20;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position,transform.rotation );
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);
                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
            
        }
    }
    void Dodge()
    {
        if (dDown && !isJump && !isDodge &&!isSwap)
        {
            speed *= 2;
            dodgeVec = moveVec;

            anim.SetTrigger("doDodge");
            isDodge = true;
            Invoke("DodgeOut", 0.4f);
        }
    }
    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
    }
    void Swap()
    {
        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;
        int weaponIndex = -1;
        if (sDown1) weaponIndex = 0;
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;

        if ((sDown1 || sDown2 || sDown3) && !isJump && !isDodge)
        {
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);
            equipWeaponIndex =weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>(); //weapons[weaponIndex] -> GameObject의 자료형 weapons[weaponIndex].GetComponent<Weapon> ->Weapon의 자료형
            equipWeapon.gameObject.gameObject.SetActive(true);

            anim.SetTrigger("doSwap");
            isSwap = true;
            Invoke("SwapOut", 0.4f);
        }
    }
    void SwapOut()
    {
        isSwap = false;
    }
    
    void Interation()
    {
        if (iDown && nearObject!=null && !isJump&& !isDodge)
        {
            if (nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weponIndex = item.value;
                hasWeapons[weponIndex] = true;
                Destroy(nearObject);
                 
            }

        }
            
        
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;
        if (fDown && isFireReady && !isDodge && !isSwap)
        {
            equipWeapon.Use();
            anim.SetTrigger(equipWeapon.type==Weapon.Type.Melee ? "doSwing":"doShot");
            fireDelay = 0;
        }
    }
    void Reload() 
    {
        if (equipWeapon == null) 
            return;
        if (equipWeapon.type == Weapon.Type.Melee) 
            return;
        if(ammo==0)
            return;
        if (rDown && !isJump && !isDodge && !isSwap && isFireReady) 
        {
            anim.SetTrigger("doReload");
            isReload = true;
            Invoke("ReloadOut", 0.5f);

        }

    }
    void ReloadOut() 
    {
       
        int reAmmo=ammo<equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.curAmmo = reAmmo;
        ammo -= reAmmo;
        isReload = false;


    }
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void StopToWall() 
    {
        Debug.DrawRay(transform.position, transform.forward*5, Color.red);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            anim.SetBool("isJump", false);

            isJump = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item") {
            Item item = other.GetComponent<Item>();
            switch (item.type) {

                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxammo)
                        ammo = maxammo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxcoin)
                        coin = maxcoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxhealth)
                        health = maxhealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    if (hasGrenades > maxhasGrenades)
                        hasGrenades = maxhasGrenades;

                    break;
             

            }
            Destroy(other.gameObject);

        }

            

        
        
    }
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = other.gameObject;
        
        
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Weapon")
            nearObject = null;
    }
}
