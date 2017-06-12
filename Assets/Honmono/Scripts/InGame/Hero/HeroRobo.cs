using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine;
using Spine.Unity;
using System;

public class HeroRobo : MonoBehaviour, NetworkManager.NetworkMessageEventListenrer ,NetworkManager.NetworkMoveEventListener {

    public enum ROBO_STATE
    {
        IDLE = 0,
        MOVE = 4,
        ATTACK = 8,
        COOLTIME = 9,
        INTHESTAR=10
    }

    //------------------------------------------------//
    //Character 기본 정보
    public GameObject m_energyDummyUI = null;
    //sound list
    public AudioClip m_enginePlay = null;
    public AudioClip m_parking = null;
    public AudioClip m_laser1 = null;
    public AudioClip m_laser2 = null;

    public Animator m_effectAnimator = null;
    public Animator m_engineAnimator = null;

    // 조종하고 있는 사람
    private string m_movePlayerName = null;
    // 쏘는 사람
    private string m_gunPlayerName = null;
    // 현재 위치
    private string m_currentPlace = ""; // 이게 star


    public string CURRENT_PLACE { get { return m_currentPlace; } set { m_currentPlace = value; } }
    public string MOVE_PLYAER { get { return m_movePlayerName; } set { m_movePlayerName = value; } }
    public string GUN_PLAYER { get { return m_gunPlayerName; } set { m_gunPlayerName = value; } }


    // 이동을 제외한 상태가 지정된다.
    public int m_roboState = 0;

    // Move 속도 
    private float m_moveSpeed = 10.0f;

    //총알 인덱스
    private int m_bulletIndex = 0;
    //총 각도
    private float m_gunAngle = 0.0f;

    // 로봇 에너지
    private float m_roboEnergy = 100.0f;

    //-- animation --//
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";
    private const string ANI_ATTACK = "attack";


    // -- 실제 로봇의 어깨 부분에 해당하는 본
    public GameObject m_armBone = null;
    // -- 총 본 
    public GameObject m_gunBone = null;

    private int m_hp = GameSetting.HERO_ROBO_MAX_HP;

    private string m_controllName = null;

    public int HP {
        get { return m_hp; }
        set
        {
            m_hp = value;
            GameManager.Instance().ChangeHP(m_hp);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonHPUdate("robo" , m_hp));
        }
    }

    // -- Damge Object ------------------------------//
    public GameObject m_DamageAnchor = null;
    public List<GameObject> m_DamageAnchorList = new List<GameObject>();
    //-----------------------------------------------//
    
    private SkeletonAnimation m_skletonAnimation = null;
    //private Animator m_animator = null;

    // -- Network Message --------------------------------------------------------------//
    private Vector3 m_prevPos = Vector3.zero;
    private Vector3 m_targetPos = Vector3.zero;
    private float m_lastSendTime = 0.0f;

    // 상태값 체크
    private int m_prevState = 0;
    // 앵글체크
    private float m_prevAngle = 0.0f;
    // 앵글 보간 - 이동 보간과 비슷하게 처리해야 하니까 동일 시간값 가짐
    private float m_targetAngle = 0.0f;
    private float m_syncTime_angle = 0.0f;
    private float m_delay_angle = 0.0f;
    private float m_lastSyncTimeAngle = 0.0f;


    // --------------------------------------------------------------------------------//
  

    public float ENERGY { get { return m_roboEnergy; }
        set {
            m_roboEnergy = value;
            if (NetworkOrderController.OBSERVER_MODE)
                return;
            GameManager.Instance().ChangeEnergy(m_roboEnergy);
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonEnergyUdate("robo" , m_roboEnergy));
        } }
    

    // --------------------------------------------------------------------------------//

    void Awake()
    {
        GameManager.Instance().HeroRoboSetup(this);

        //리시버 등록
        
        NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    // Use this for initialization
    void Start()
    {
        m_targetPos = transform.position;
        m_prevState = m_roboState;
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        CheckAndSetAnimation(ANI_IDLE, true);
        m_gunAngle = m_armBone.transform.rotation.eulerAngles.z;
        //  LightRangeUp();
        m_skletonAnimation.state.Complete += AttackEndCheckEvent;

        for(int i = 0; i < m_DamageAnchor.transform.childCount; i++)
        {
            m_DamageAnchorList.Add(m_DamageAnchor.transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    void Update () {

        m_prevState = m_roboState;
        if (!string.IsNullOrEmpty(m_movePlayerName) &&
            m_movePlayerName.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            if (ENERGY < 0.5f)
                m_energyDummyUI.SetActive(true);
            else
                m_energyDummyUI.SetActive(false);

            Control();
            MoveSend();
            StateSend();
            NetworkGunAngleLerp();
            return;
        }
        if (!string.IsNullOrEmpty(m_gunPlayerName) &&
            m_gunPlayerName.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            if (ENERGY < 0.5f)
                m_energyDummyUI.SetActive(true);
            else
                m_energyDummyUI.SetActive(false);
            ControlGun();
            GunAngleSend();
            StateSend();
        }

        //그 이외의 녀석들은 애니메이션 / 러프함 
        NetworkAnimation();
        NetworkMoveLerp();
        NetworkGunAngleLerp();
    }

    // Network Move Message Send ------------------------------------!
    // 조종하는 녀석만 이걸 실행한다.
    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos, pos);
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));
        //dirPos.Normalize();

   
        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_movePlayerName + "_robot", 
            pos.x, pos.y, 
            (int)NetworkOrderController.AreaInfo.AREA_SPACE, 
            m_skletonAnimation.skeleton.flipX,
            sendPos));
        m_lastSendTime = Time.deltaTime;

    }

    // 현재 상태값 전송 (애니메이션용)
    void StateSend()
    {
        if(m_prevState != m_roboState)
        {
            
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonOrderStateValueChange(
                    m_movePlayerName +"_robot",
                    m_roboState));
        }
    }

    // 총 각도 전송
    void GunAngleSend()
    {
        if(m_prevAngle != m_gunAngle)
        {
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonOrderGunAngle(
                    m_gunPlayerName + "_gun",
                    m_gunAngle));
        }
    }

    void EnergyTestUser()
    {
        ENERGY -= 0.5f;
    }
  
    /// <summary>
    /// 캐릭터 조작에 관련된 함수
    /// </summary>
    void Control()
    {
        // 가져다 쓰기 편하기 위해 선언       
        Vector3 pos = transform.position;

        // 요생키들이 실제 이동 
        float movex = 0;
        float movey = 0;

    
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
            Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow))
        {
            m_engineAnimator.SetInteger("play" , 1);
            m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.MOVE);
        }
        
        // Horizontal
        if (Input.GetKey(KeyCode.LeftArrow) && ENERGY >= 0.5f)
        {
            if(!IsInvoking("EnergyTestUser"))
                Invoke("EnergyTestUser" , 0.5f);
            movex = -m_moveSpeed * Time.deltaTime;

            m_skletonAnimation.skeleton.flipX = false;

        }

        if (Input.GetKey(KeyCode.RightArrow) && ENERGY >= 0.5f)
        {
            if (!IsInvoking("EnergyTestUser"))
                Invoke("EnergyTestUser" , 0.5f);
            movex = m_moveSpeed * Time.deltaTime;

            m_skletonAnimation.skeleton.flipX = true;
        }
        // Vertical
        if (Input.GetKey(KeyCode.UpArrow) && ENERGY >= 0.5f)
        {
            if (!IsInvoking("EnergyTestUser"))
                Invoke("EnergyTestUser" , 0.5f);
            movey = m_moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow) && ENERGY >= 0.5f)
        {
            if (!IsInvoking("EnergyTestUser"))
                Invoke("EnergyTestUser" , 0.5f);
            movey = -m_moveSpeed * Time.deltaTime;
        }


        if (BitControl.Get(m_roboState, (int)ROBO_STATE.MOVE) &&
            (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) ||
             Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)))
        {
            //  CheckAndSetAnimation(ANI_IDLE, true);
            m_skletonAnimation.state.SetAnimation(0, ANI_IDLE, true);
            m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.MOVE);
            m_engineAnimator.SetInteger("play" , 0);
            //m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.IDLE);
        }
      

        if (m_roboState == (int)ROBO_STATE.IDLE) //BitControl.Get(m_roboState, (int)ROBO_STATE.IDLE))
        {
            CheckAndSetAnimation(ANI_IDLE, true);
            m_engineAnimator.SetInteger("play" , 0);
            // m_skletonAnimation.state.SetAnimation(0, ANI_IDLE, true);
        }
        else
        {
            if (BitControl.Get(m_roboState, (int)ROBO_STATE.MOVE))
            {
                transform.Translate(movex, movey, 0);
                if(IsCurrentAnimation(ANI_IDLE))
                    m_skletonAnimation.state.SetAnimation(0, ANI_MOVE, true);

            
            }
        }
    }

    void ControlGun()
    {
        m_prevAngle = m_gunAngle;
        if (!m_skletonAnimation.skeleton.flipX)
            m_armBone.transform.rotation = Quaternion.Euler(0, 0, m_gunAngle);
        else
            m_armBone.transform.rotation = Quaternion.Euler(0, 0, -m_gunAngle);


        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (m_gunAngle - 1.0f > -360.0f)
                m_gunAngle -= 1.0f;

        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (m_gunAngle + 1.0f < 360.0f)
                m_gunAngle += 1.0f;
        }

        if (!BitControl.Get(m_roboState , (int)ROBO_STATE.COOLTIME) && Input.GetKeyUp(KeyCode.Space))
        {
            SoundManager.Instance().PlaySound(m_laser1);
            m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.ATTACK);
            
        }

        if (BitControl.Get(m_roboState, (int)ROBO_STATE.ATTACK) && 
            !BitControl.Get(m_roboState , (int)ROBO_STATE.COOLTIME))
        {
            m_skletonAnimation.state.SetAnimation(0, ANI_ATTACK, false);
            StateSend();
            m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.COOLTIME);
            
            if(ENERGY >= 0.5f)
            {
                if (!IsInvoking("EnergyTestUser"))
                    Invoke("EnergyTestUser" , 0.5f);
                
            }
            
        }

    }

    //단순 애니메이션 작업

    void NetworkAnimation()
    {
        if (m_roboState == (int)ROBO_STATE.IDLE) //BitControl.Get(m_roboState, (int)ROBO_STATE.IDLE))
        {
            CheckAndSetAnimation(ANI_IDLE, true);
            // m_skletonAnimation.state.SetAnimation(0, ANI_IDLE, true);
            m_engineAnimator.SetInteger("play" , 0);
        }
        else
        {
            if (BitControl.Get(m_roboState, (int)ROBO_STATE.MOVE))
            {
                if (IsCurrentAnimation(ANI_IDLE))
                    m_skletonAnimation.state.SetAnimation(0, ANI_MOVE, true);
                m_engineAnimator.SetInteger("play" , 1);
            }
            
        }
        if (BitControl.Get(m_roboState , (int)ROBO_STATE.ATTACK))
        {
            m_skletonAnimation.state.SetAnimation(0 , ANI_ATTACK , false);
            m_roboState = BitControl.Clear(m_roboState , (int)ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState , (int)ROBO_STATE.COOLTIME);

        }
    }

    // 어택 종료시 초기화
    void AttackEndCheckEvent(Spine.TrackEntry trackEntry)
    {
        if(trackEntry.animation.name.Equals(ANI_ATTACK))//trackEntry.animation.name == ANI_ATTACK)
        {
            if (m_gunPlayerName.Equals(GameManager.Instance().PLAYER.USER_NAME))
                FireBullet();
            m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.COOLTIME);
            m_skletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
            //m_effectAnimator.gameObject.SetActive(true);
            //m_effectAnimator.Play("Robo_attackEffect");
        }
    }

    void FireBullet()
    {
        bool flip = m_skletonAnimation.skeleton.flipX;
        Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);
        
        Vector3 pos = m_gunBone.transform.position;

        b.transform.rotation = Quaternion.Euler(0.0f , 0.0f , m_gunBone.transform.rotation.eulerAngles.z -90.0f);
  
        b.transform.position = pos;


        // 네트워크 식별 이름
        string n = GameManager.Instance().PLAYER.USER_NAME + "_" + m_bulletIndex;
        b.SetupBullet(n, false, Vector3.zero,0.0f,m_skletonAnimation.skeleton.flipX);
        
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(n, "myTeam_bullet", pos.x, pos.y, pos.z,flip));
        m_bulletIndex++;
        
        
    }



    // -- 스파인 애니메이션용 -------------------------------------------------------//
    bool IsCurrentAnimation(string ani)
    {
        if (m_skletonAnimation == null)
            return false;
        return m_skletonAnimation.state.GetCurrent(0).animation.name == ani;
    }

    void CheckAndSetAnimation(string ani, bool loop)
    {
        if (!IsCurrentAnimation(ani))
            m_skletonAnimation.state.SetAnimation(0, ani, loop);
    }

    //-- Network Message 에 따른 이동 보간 ( 네트워크 플레이어 ) ------------------------------------//
    void NetworkMoveLerp()
    {
        if(!string.IsNullOrEmpty(m_movePlayerName) 
            && !m_movePlayerName.Equals(GameManager.Instance().PLAYER.USER_NAME))
            transform.position = m_targetPos;
    }

    // 앵글 보간
    void NetworkGunAngleLerp()
    {
        if (m_gunPlayerName == GameManager.Instance().PLAYER.USER_NAME)
            return;
        m_syncTime_angle += Time.deltaTime;

        if(m_delay_angle > 0)
        {
            m_gunAngle =  Mathf.Lerp(m_gunAngle, m_targetAngle, m_syncTime_angle / m_delay_angle);
            if (!m_skletonAnimation.skeleton.flipX)
                m_armBone.transform.rotation = Quaternion.Euler(0, 0, m_gunAngle);
            else
                m_armBone.transform.rotation = Quaternion.Euler(0, 0, -m_gunAngle);
        }
    }

    //-----------------------------------------------------------------------------------------------//
    // 이동하는 녀석이 아니면 다 받는다------------------------------------------------------
    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        if (string.IsNullOrEmpty(m_movePlayerName) || m_movePlayerName.Equals(GameManager.Instance().PLAYER.USER_NAME))
            return;
        
        JSONObject obj = json.GetField("Enemies");

        float x, y, z;
        x = y = z = 0.0f;
        bool flip = false;
        bool ck = false;
        Vector3 drPos = Vector3.zero;
        Vector3 targetPos = Vector3.zero;
        for (int i = 0; i < obj.Count; i++)
        {
            // 이름이 다르다면 패스
           
            if ((m_movePlayerName + "_robot").Equals(obj[i].GetField("Name").str))
            {
            
                x = obj[i].GetField("X").f;
                y = obj[i].GetField("Y").f;
                z = obj[i].GetField("Z").f;
                flip = obj[i].GetField(NetworkManager.DIR).b;
                ck = true;
                JSONObject v = obj[i].GetField(NetworkManager.DIRVECTOR);
                drPos = new Vector3(v.GetField("X").f , v.GetField("Y").f , v.GetField("Z").f);
                break;
            }
        }
        if (!ck)
            return;

        Vector3 newPos = new Vector3(x, y);

        float distance = Vector3.Distance(transform.position, newPos);


        // 기존 방향과 다르다면!?
        if (this.m_skletonAnimation.skeleton.flipX != flip)
        {
            this.m_skletonAnimation.skeleton.flipX = flip;
            targetPos = newPos;
        }
        else
            targetPos = drPos;

        m_targetPos = targetPos;


    }

    // 상태값 교환으로 애니메이션 
    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {

        if (e.msgType == NetworkManager.STATE_CHANGE)
        {
            if (GameManager.Instance().PLAYER.USER_NAME == m_movePlayerName)
                return;
            //상태값 뽑기
            m_roboState = (int)e.msg.GetField(NetworkManager.STATE_CHANGE).i;

            // 공격 애니메이션을 gun 을 작동시키는 녀석이 재생할 필요가 없다고 가정하고 해보자
            if (BitControl.Get(m_roboState, (int)ROBO_STATE.ATTACK) || BitControl.Get(m_roboState, (int)ROBO_STATE.COOLTIME))
            {

                if (m_gunPlayerName == GameManager.Instance().PLAYER.USER_NAME)
                {
                    m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.ATTACK);
                    m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.COOLTIME);
                }
            }
        }
        else if(e.msgType == NetworkManager.GUN_ANGLE_CHANGE)
        {
            // 앵글 또한 보간해야함
            if (GameManager.Instance().PLAYER.USER_NAME == m_gunPlayerName)
                return;
            m_targetAngle = e.msg.GetField(NetworkManager.GUN_ANGLE_CHANGE).f;
            
            m_syncTime_angle = 0.0f;
            m_delay_angle = Time.time - m_lastSyncTimeAngle;
            m_lastSyncTimeAngle = Time.time;
        }
        else if(e.msgType == NetworkManager.HP_UPDATE)
        {
            if (GameManager.Instance().PLAYER.USER_NAME.Equals(e.user) || !e.targetName.EndsWith("robo"))
                return;
            m_hp = (int)e.msg.GetField(NetworkManager.HP_UPDATE).i;
        }
        else if(e.msgType == NetworkManager.ENERGY_UPDATE)
        {
            if (GameManager.Instance().PLAYER.USER_NAME.Equals(e.user) || !e.targetName.EndsWith("robo"))
                return;
            m_roboEnergy = e.msg.GetField(NetworkManager.ENERGY_UPDATE).f;
        }
    }

    //-------------------------------------------------------------------------------//

   void OnTriggerEnter2D(Collider2D col)
    {
        string userName = GameManager.Instance().PLAYER.USER_NAME;

        if (!userName.Equals(MOVE_PLYAER) && !userName.Equals(GUN_PLAYER))
            return;

        if(col.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.tag;
        }
        else if(col.tag == "TEST_BOSS")
        {
            if (!r)
            {
                GameManager.Instance().CUR_PLACE = GameManager.ROBO_PLACE.BOSS_AREA;
                //CameraManager.Instance().MoveCameraAndObject(gameObject , 10 ,
                //    CameraManager.CAMERA_PLACE.BOSS , gameObject);

                if(userName.Equals(MOVE_PLYAER))
                    MapManager.Instance().AddMonster(
                        GamePath.BOSS1 ,
                        "boss1_" + GameManager.Instance().PLAYER.USER_NAME ,
                        new Vector3(121.06f , 6.34f));
                r = true;
            }
          
        }
        switch (col.tag)
        {
            case "BOSS_SCENE":
                GameManager.Instance().ChangeScene(GameManager.PLACE.STAGE1_BOSS);
                MapManager.Instance().AddMonster(GamePath.BOSS1 , "boss1_" + GameManager.Instance().PLAYER.USER_NAME ,
                    MapManager.Instance().m_bossCreatePlace.transform.position);
                break;
            case "BOSS_SCENE2":
                GameManager.Instance().ChangeScene(GameManager.PLACE.STAGE1_BOSS);
                MapManager.Instance().AddMonster(GamePath.BOSS1 , "boss2_" + GameManager.Instance().PLAYER.USER_NAME ,
                    MapManager.Instance().m_bossCreatePlace.transform.position);
                break;
            case "PLANET1":
            case "PLANET2":
                m_controllName = col.tag;
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRoboPlaceChange(m_controllName));
                break;
        }
    }
    bool r = false;

    void OnCollisionStay2D(Collision2D col)
    {
        
        if (col.transform.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.transform.tag;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        
        if (col.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.tag;
        }
    }

    // -- 데미지 상호작용 --------------------------------------------------------------------------------------//
    public void Damage(int damage)
    {
        HP -= damage;

        // 이곳에서 구멍 이펙트 생성 
        if(HP % 10 == 0)
        {
            // 10이상 데미지를 받을 때마다 생성
            DamagePointCreate();
        }

        if(HP <= 0)
        {
            HP = 0;
        }

    }

    public void Heal(int heal)
    {
        HP += heal;

        if (HP >= GameSetting.HERO_MAX_HP)
            HP = GameSetting.HERO_ROBO_MAX_HP;
    }

    // 특정 데미지 이상 받았을 때만 호출
    void DamagePointCreate()
    {   
        int index = UnityEngine.Random.Range(0 , m_DamageAnchorList.Count);
        
        GameObject obj = MapManager.Instance().AddObject(
            GamePath.DAMAGE_POINT, m_DamageAnchorList[index].transform.position);
        obj.transform.parent = m_DamageAnchorList[index].transform.parent;

    }
}
