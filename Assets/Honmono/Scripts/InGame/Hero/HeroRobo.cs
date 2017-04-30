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

    // Reload 속도 -- 
    private float m_reloadSpeed = 0.3f;

    // Move 속도 
    private float m_moveSpeed = 10.0f;

    //총알 인덱스
    private int m_bulletIndex = 0;
    //총 각도
    private float m_gunAngle = 0.0f;

    //-- animation --//
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";
    private const string ANI_ATTACK = "attack";


    // -- 실제 로봇의 어깨 부분에 해당하는 본
    public GameObject m_armBone = null;
    // -- 총 본 
    public GameObject m_gunBone = null;

    private float m_hp = 100.0f;

    private string m_controllName = null;

    public float HP { get { return m_hp; } set { m_hp = value; } }
    //-----------------------------------------------//
    
    private SkeletonAnimation m_skletonAnimation = null;
    //private Animator m_animator = null;

    // -- Network Message --------------------------------------------------------------//
    private Vector3 m_prevPos = Vector3.zero;
    private Vector3 m_targetPos = Vector3.zero;
    private float m_syncTime = 0.0f;
    private float m_delay = 0.0f;
    private float m_lastSyncTime = 0.0f;

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

    
    void Awake()
    {
        GameManager.Instance().HeroRoboSetup(this);

        //리시버 등록
        NetworkManager.Instance().AddNetworkMoveEventListener(this);
        NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
    }

    // Use this for initialization
    void Start()
    {
        m_prevState = m_roboState;
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        CheckAndSetAnimation(ANI_IDLE, true);
        m_gunAngle = m_armBone.transform.rotation.eulerAngles.z;
        //  LightRangeUp();
        m_skletonAnimation.state.Complete += AttackEndCheckEvent;


    }
	
	// Update is called once per frame
	void Update () {

        m_prevState = m_roboState;
        if (m_movePlayerName == GameManager.Instance().PLAYER.USER_NAME)
        {
            Control();
            MoveSend();
            StateSend();
            NetworkGunAngleLerp();
            return;
        }
        if (m_gunPlayerName == GameManager.Instance().PLAYER.USER_NAME)
        {
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

        if (distance <= 0)
            return;


        NetworkManager.Instance().SendMoveMessage(
            JSONMessageTool.ToJsonMove(m_movePlayerName + "_robot", 
            pos.x, pos.y, (int)NetworkOrderController.AreaInfo.AREA_SPACE, m_skletonAnimation.skeleton.flipX,Vector3.zero));
       
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
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            
            movex = -m_moveSpeed * Time.deltaTime;

            m_skletonAnimation.skeleton.flipX = false;

        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            movex = m_moveSpeed * Time.deltaTime;

            m_skletonAnimation.skeleton.flipX = true;
        }
        // Vertical
        if (Input.GetKey(KeyCode.UpArrow))
        {
            
            movey = m_moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            
            movey = -m_moveSpeed * Time.deltaTime;
        }
        // 로컬 전투 처리용--------------------------------------------------------------
        m_prevAngle = m_gunAngle;
        if (!m_skletonAnimation.skeleton.flipX)
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , m_gunAngle);
        else
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , -m_gunAngle);


        if (Input.GetKey(KeyCode.W))
        {
            if (m_gunAngle - 1.0f > -360.0f)
                m_gunAngle -= 1.0f;

        }
        if (Input.GetKey(KeyCode.S))
        {
            if (m_gunAngle + 1.0f < 360.0f)
                m_gunAngle += 1.0f;
        }

        if(Input.GetKey(KeyCode.G))
        {
            if(BitControl.Get(m_roboState,(int)ROBO_STATE.INTHESTAR))
            {
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderPlaceChange(""));
            }
            else
            {
                if(m_controllName == "GO_TOTHE_STAR")
                {
                    m_roboState = BitControl.Set(m_roboState , (int)ROBO_STATE.INTHESTAR);
                    NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderPlaceChange("star"));
                }
            }
        }


        if (Input.GetKey(KeyCode.Space) && !BitControl.Get(m_roboState , (int)ROBO_STATE.COOLTIME))
        {
            SoundManager.Instance().PlaySound(m_laser1);
            m_roboState = BitControl.Set(m_roboState , (int)ROBO_STATE.ATTACK);
        }

        if (BitControl.Get(m_roboState , (int)ROBO_STATE.ATTACK))
        {
            m_skletonAnimation.state.SetAnimation(0 , ANI_ATTACK , false);
            m_roboState = BitControl.Clear(m_roboState , (int)ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState , (int)ROBO_STATE.COOLTIME);
            FireBullet();
        }


        //-------------------------------------------------------------------------------

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


        // TODO 추후 MapManager 에서 가져오는 형태로 바꿀것
        TargetMoveCamera camera = Camera.main.GetComponent<TargetMoveCamera>();
        Vector3 backPos = camera.BACKGROUND_POS;
        float leftCheck = backPos.x - camera.BACKGROUND_HALF_WIDTH;
        float rightCheck = backPos.x + camera.BACKGROUND_HALF_WIDTH;
        float upCheck = backPos.y + camera.BACKGROUND_HALF_HEIGHT;
        float downCheck = backPos.x - camera.BACKGROUND_HALF_HEIGHT;

        
        //float w = (m_skletonAnimation.skeleton.flipX) ? m_skletonAnimation.skeleton.data.width / 2.0f : -m_skletonAnimation.skeleton.data.width / 2.0f;
        //float h = (movey >= 0) ? -m_skletonAnimation.skeleton.data.height / 2.0f : m_skletonAnimation.skeleton.data.height / 2.0f;
        //if (pos.x + movex + w <= leftCheck)     movex = 0.0f;
        //if (pos.x + movex + w >= rightCheck)    movex = 0.0f;
        //if (pos.y + movey >= upCheck + h)       movey = 0.0f;
        //if (pos.y + movey <= downCheck + h)     movey = 0.0f;

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

        if (Input.GetKey(KeyCode.Space) && !BitControl.Get(m_roboState, (int)ROBO_STATE.COOLTIME))
        {
            m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.ATTACK);
            
        }

        if (BitControl.Get(m_roboState, (int)ROBO_STATE.ATTACK))
        {
            m_skletonAnimation.state.SetAnimation(0, ANI_ATTACK, false);
            m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.COOLTIME);
            
            FireBullet();
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
            if (BitControl.Get(m_roboState, (int)ROBO_STATE.ATTACK))
            {
                m_skletonAnimation.state.SetAnimation(0, ANI_ATTACK, false);
                m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.ATTACK);
                m_roboState = BitControl.Set(m_roboState, (int)ROBO_STATE.COOLTIME);
               
            }
        }
    }

    // 어택 종료시 초기화
    void AttackEndCheckEvent(Spine.TrackEntry trackEntry)
    {
        if(BitControl.Get(m_roboState, (int)ROBO_STATE.COOLTIME))//trackEntry.animation.name == ANI_ATTACK)
        {
            MDebug.Log("Cool Time ");
            m_roboState = BitControl.Clear(m_roboState, (int)ROBO_STATE.COOLTIME);
            //m_effectAnimator.gameObject.SetActive(true);
            //m_effectAnimator.Play("Robo_attackEffect");
        }
    }

    void FireBullet()
    {
        bool flip = m_skletonAnimation.skeleton.flipX;
        GameObject bullet = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);
        
        Vector3 pos = m_gunBone.transform.position;

      //  bullet.transform.rotation = m_gunBone.transform.rotation; 
        bullet.transform.rotation = Quaternion.Euler(0.0f , 0.0f , m_gunBone.transform.rotation.eulerAngles.z -90.0f);
        //MDebug.Log("Z " + m_gunBone.transform.rotation.eulerAngles.z + " " + m_gunAngle);
        //if (!flip)
        //    bullet.transform.Rotate(new Vector3(0, 0.0f, -90.0f));
        //else
        //    bullet.transform.Rotate(new Vector3(0, 0.0f, 90.0f));

        

        bullet.transform.position = pos;


        Bullet b = bullet.GetComponent<Bullet>();
        // 네트워크 식별 이름
        string n = GameManager.Instance().PLAYER.USER_NAME + "_" + m_bulletIndex;
        b.SetupBullet(n, false, Vector3.zero,0.0f,m_skletonAnimation.skeleton.flipX);
        
        //NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(n, "myTeam_bullet", pos.x, pos.y, pos.z,flip));
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
        m_syncTime += Time.deltaTime;


        // 네트워크 보간( 테스트 완료 - 로컬 )
        if (m_delay > 0)
            transform.position = Vector3.Lerp(transform.position, m_targetPos, m_syncTime / m_delay);

        //P + V * D
        // 레이턴시 ->보낸다 ->받는다(시간)
        // target Pos 를 재계산
        // 클라 A  좌표 샌딩 ->  서버 -> 클라B 좌표 받음
        // 클라B가 알수 있는 레이턴시는 서버 <-> 클라B
        // 클라 A에 대한 시간을 알수 없음 
        // 클라 A가 시간을 보냄 -> 클라 B에서 계산방법 (딜레이는 현재시간 - A의 보낸 시간 )
        // (속도는 정해져있음)

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
        if (m_movePlayerName == GameManager.Instance().PLAYER.USER_NAME)
            return;

        JSONObject obj = json.GetField("Users");

        float x, y, z;
        x = y = z = 0.0f;
        bool flip = false;
        bool ck = false;
        for (int i = 0; i < obj.Count; i++)
        {
            // 이름이 다르다면 패스
           
            if (m_movePlayerName + "_robot" == obj[i].GetField(NetworkManager.USERNAME).str)
            {
                x = obj[i].GetField("X").f;
                y = obj[i].GetField("Y").f;
                z = obj[i].GetField("Z").f;
                flip = obj[i].GetField("Dir").b;
                ck = true;
                break;
            }
        }
        if (!ck)
            return;

        Vector3 newPos = new Vector3(x, y);

        float distance = Vector3.Distance(transform.position, newPos);
        if (m_skletonAnimation != null)
            this.m_skletonAnimation.skeleton.flipX = flip;

        if (distance <= 0)
        {
            return;
        }

        m_targetPos = newPos;

        m_syncTime = 0.0f;
        m_delay = Time.time - m_lastSyncTime;
        m_lastSyncTime = Time.time;
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
        else if(e.msgType == NetworkManager.PLACE_CHANGE)
                GameManager.Instance().ROBO.CURRENT_PLACE = e.msg.GetField(NetworkManager.PLACE_CHANGE).str;
        
        else if(e.msgType == NetworkManager.BOSS_SCENE_MOVE)
        {
            CameraManager.Instance().MoveCamera(gameObject , 10.0f , CameraManager.CAMERA_PLACE.BOSS);
        }
    }

    //-------------------------------------------------------------------------------//

   void OnTriggerEnter2D(Collider2D col)
    {
        
        if(col.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.tag;
        }
        else if(col.tag == "TEST_BOSS")
        {
            if (!r)
            {
                NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder("" , "boss1"));
                r = true;
            }
          //  NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJSonChangeBossScene());
        }
    }
    bool r = false;
    void OnCollisionStay2D(Collider2D col)
    {
        MDebug.Log("TEST "+col.tag);
        if (col.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.tag;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        MDebug.Log("TEST " + col.tag);
        if (col.tag == "GO_TOTHE_STAR")
        {
            m_controllName = col.tag;
        }
    }
}
