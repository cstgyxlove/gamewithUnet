﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ScoreMessage : MessageBase
{
    public int clinetid;
    public bool facedirection;
    public Vector3 scorePos;
    public int lives;
}

public class SuperPlayerCtrl : NetworkBehaviour {

    public CharacterInfo charInfo;

    Rigidbody2D rigibody;

    JoyStickerController moveJoystick;
    JoyStickerController fireJoystick;

    public bool isEnemy = true;

    GameCrtl Gamectrl;
    hpShow hpshow;

    public GameObject uilive;
    // Use this for initialization
    void Awake () {
		GetComponent<SpriteRenderer>().color = Color.red;
        rigibody = GetComponent<Rigidbody2D>();

        moveJoystick = GameObject.Find("moveJ").GetComponent<JoyStickerController>();
        fireJoystick = GameObject.Find("fireJ").GetComponent<JoyStickerController>();

        this.name = "enemy";
        hpshow = this.transform.Find("hp").GetComponent<hpShow>();
        uilive = GameObject.Find("rightlive");
    }

    public override void OnStartLocalPlayer()
    {
        Debug.Log("Change Layer");
        this.gameObject.layer = 9;

        base.OnStartLocalPlayer();
        GetComponent<SpriteRenderer>().color = Color.white;
        isEnemy = false;
        gameObject.name = "self";
        IDCODE = GameManager.Instance.ID;

        if (IDCODE == 2)
            this.transform.localPosition = Constant.RightSpawnPos;
        else
            this.transform.localPosition = Constant.LeftSpawnPos;

        uilive = GameObject.Find("leftlive");
    }

    void Start()
    {
        Gamectrl = GameObject.Find("GameCtrl").GetComponent<GameCrtl>();
        myclient = GameManager.Instance.myclient;
        if (!isLocalPlayer)
            myclient.RegisterHandler(100, OnChangePos);
    } 

    public GameObject bulletPref;
    public Transform bulletSpawn;


    public float speed = 10;
    public float MaxSpeedY = 10;

    public float MaxSpeedX = 10;

    bool LeftKey = false;
    bool RightKey = false;
    bool UpKey = false;
    bool DownKey = false;

    bool oldfaceDirect = true;
    bool faceDirect = true;

    NetworkClient myclient;
    public float angle = 0;


    public int IDCODE = 0;

    [SyncVar(hook = "changehp")]
    public int hp = Constant.MaxHp;

    [SyncVar(hook = "changeLive")]
    public int lives = Constant.MaxLive;



    public void OnChangePos(NetworkMessage netMsg)
    {
        ScoreMessage msg = netMsg.ReadMessage<ScoreMessage>();
        Debug.Log("OnScoreMessage " + msg.clinetid);
        if (msg.clinetid == 33)
            return;
        if (GameManager.Instance.ID != msg.clinetid)
            changeScale(msg.facedirection);
    }
    private void changeScale(bool flag)
    {
        if(flag)
            transform.localScale = new Vector3(transform.localScale.y, transform.localScale.y, transform.localScale.y);
        else
            transform.localScale = new Vector3(-transform.localScale.y, transform.localScale.y, transform.localScale.y);

    }

    void Update () {

        if (!isLocalPlayer)
            return;

        //Debug.Log(joystick.InputDirection);
       // var k = (joystick.InputDirection.y / joystick.InputDirection.x);
        angle = Mathf.Atan2(moveJoystick.InputDirection.z * 100, moveJoystick.InputDirection.x * 100) * Mathf.Rad2Deg;
        //Debug.Log(angle * Mathf.Rad2Deg);

        if (angle < 60 && angle > -60)
            RightKey = true;
        else
            RightKey = false;
        if (angle > 30 && angle < 150)
            UpKey = true;
        else
            UpKey = false;
        if (angle > -150 && angle < -60)
            DownKey = true;
        else
            DownKey = false;
        if (angle > 120 || angle < -120)
            LeftKey = true;
        else
            LeftKey = false;

        if (moveJoystick.InputDirection.magnitude < 0.1)
        {
            RightKey = false;
            LeftKey = false;
            UpKey = false;
            DownKey = false;
        }

        if ((LeftKey ||Input.GetKey(KeyCode.A)) && rigibody.velocity.x > -MaxSpeedY)
        {
            faceDirect = false;

            rigibody.AddForce(new Vector2(-50, 0), ForceMode2D.Force);
            if(rigibody.velocity.x > 0)
                rigibody.velocity = new Vector2(0, rigibody.velocity.y);
        }

        if ((RightKey || Input.GetKey(KeyCode.D)) && rigibody.velocity.x < MaxSpeedY)
        {
            faceDirect = true;

            rigibody.AddForce(new Vector2(50, 0), ForceMode2D.Force);
            if (rigibody.velocity.x < 0)
                rigibody.velocity = new Vector2(0, rigibody.velocity.y);
        }

        if ((UpKey || Input.GetKey(KeyCode.W)) && rigibody.velocity.y < MaxSpeedY)
        {
            if (rigibody.velocity.y < 0)
            {
                rigibody.velocity = new Vector2(rigibody.velocity.x, 0);
            }
            rigibody.AddForce(new Vector2(0, 40), ForceMode2D.Force);
        }

        if ((DownKey || Input.GetKey(KeyCode.S)) && rigibody.velocity.y > -MaxSpeedY)
        {
            rigibody.AddForce(new Vector2(0, -30), ForceMode2D.Force);
        }

        if(faceDirect != oldfaceDirect)
        {
            oldfaceDirect = faceDirect;
            CmdChangeFace(GameManager.Instance.ID,faceDirect);
            changeScale(faceDirect);
        }

        //射击逻辑
        fireUpdate();

        //if (Input.GetKeyDown(KeyCode.Space))
        //    CmdChangeFace(false);
    }

    [Command]
    private void CmdChangeFace(int id,bool lv)
    {

        ScoreMessage msg = new ScoreMessage();
        msg.clinetid = id;
        msg.facedirection = lv;
        msg.scorePos = Vector3.zero;
        msg.lives = 0;
        Debug.Log("Server cmd");
        NetworkServer.SendToAll(100, msg);
    }

    float fireCD = 0.2f;
    float lastFireTime = 0;

    private void fireUpdate()
    {
        if (fireJoystick.isClick && lastFireTime + fireCD < Time.time)
        {
            lastFireTime = Time.time;
            var direct = fireJoystick.InputDirection.normalized;
            Debug.Log(direct);
            Vector3 direct2D = new Vector3(direct.x, direct.z,0);
            CmdFire(GameManager.Instance.ID, direct2D);
        }
    }


    [Command]
    private void CmdFire(int id, Vector3 direction)
    {
        GameObject bullet = (GameObject)Instantiate(bulletPref, bulletSpawn.position + direction, bulletSpawn.rotation);
        bullet.GetComponent<Rigidbody2D>().velocity = direction * 10.0f;
        bullet.GetComponent<Bullet>().ownerid = id;
        bullet.name = id.ToString();
        NetworkServer.Spawn(bullet);
        Debug.Log("XXXXXXXXX " + id);
        Destroy(bullet, 2);
    }





    [ClientRpc]
    public void RpcChoseCharacter(int id, int choseid)
    {
        Gamectrl.choseCharCallBack(id, choseid);
    }



    [Command]
    public void CmdChoseCharacter(int fromid, int choseid)
    {
        //NetworkServer.SendToAll(100, msg);
        Debug.Log("server " + fromid + " " + choseid);
        RpcChoseCharacter(fromid, choseid);
    }


    public void TakeD(int damage)
    {
        if (!isServer)
            return;

        hp -= damage;
        if (hp <= 0)
        {
            hp = Constant.MaxHp;
            lives--;
            //RpcRespawn();
        }
    }

    public void changehp(int hp)
    {
        hpshow.setBlood(hp);
        this.hp = hp;
    }

    public void changeLive(int live)
    {
        this.lives = live;
        if (lives == 0)
            this.transform.localPosition = new Vector3(10000,10000,0);

        for(int i=lives; i<5;i++)
        {
            uilive.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}