using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TutoRobo : MonoBehaviour
{
    public TutorialController m_tuto = null;
    public AudioClip m_laser1 = null;
    private SkeletonAnimation m_skletonAnimation = null;
    public Animator m_engineAnimator = null;
    public GameObject m_damagePoint = null;

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

    private float m_prevAngle = 0.0f;

    // -- 실제 로봇의 어깨 부분에 해당하는 본
    public GameObject m_armBone = null;
    // -- 총 본 
    public GameObject m_gunBone = null;

    // -- move / gun State
    private int m_curState = 0;

    public int CUR_STATE
    {
        get { return m_curState; }
        set { m_curState = value;}
    }

    // Use this for initialization
    void Start () {
        m_skletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_skletonAnimation.state.Complete += AttackEndCheckEvent;
        m_gunAngle = m_armBone.transform.rotation.eulerAngles.z;
    }
	
	// Update is called once per frame
	void Update () {
        if (m_curState == 1)
            Control();
        else if (m_curState == 2)
            ControlGun();
	}
    void ControlGun()
    {
        m_prevAngle = m_gunAngle;
        if (!m_skletonAnimation.skeleton.flipX)
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , m_gunAngle);
        else
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , -m_gunAngle);


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

        if (Input.GetKeyUp(KeyCode.Space) && !BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.COOLTIME))
        {
            SoundManager.Instance().PlaySound(m_laser1);
            m_roboState = BitControl.Set(m_roboState , (int)HeroRobo.ROBO_STATE.ATTACK);

        }

        if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.ATTACK))
        {
            m_skletonAnimation.state.SetAnimation(0 , ANI_ATTACK , false);
            m_roboState = BitControl.Clear(m_roboState , (int)HeroRobo.ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState , (int)HeroRobo.ROBO_STATE.COOLTIME);

            FireBullet();
        }

    }

    // 어택 종료시 초기화
    void AttackEndCheckEvent(Spine.TrackEntry trackEntry)
    {
        if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.COOLTIME))//trackEntry.animation.name == ANI_ATTACK)
        {

            m_roboState = BitControl.Clear(m_roboState , (int)HeroRobo.ROBO_STATE.COOLTIME);
            //m_effectAnimator.gameObject.SetActive(true);
            //m_effectAnimator.Play("Robo_attackEffect");
        }
    }

    bool create = false;
    void DamagePointCreate()
    {
        if (create)
            return;
        m_tuto.TutorialAction_KillMonster("");
        GameObject obj = MapManager.Instance().AddObject(
            GamePath.DAMAGE_POINT , m_damagePoint.transform.position);
        obj.transform.parent = m_damagePoint.transform.parent;
        RoboDamagePoint p = obj.GetComponent<RoboDamagePoint>();
        p.m_tuto = true;
        p.m_test = m_tuto;
        create = true;
    }

    // -- 데미지 상호작용 --------------------------------------------------------------------------------------//
    public void Damage(int damage)
    {
        DamagePointCreate();
    }
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
            m_roboState = BitControl.Set(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE);
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


        if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE) &&
            (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) ||
             Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)))
        {
            //  CheckAndSetAnimation(ANI_IDLE, true);
            m_skletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
            m_roboState = BitControl.Clear(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE);
            m_engineAnimator.SetInteger("play" , 0);
            //m_roboState = BitControl.Set(m_roboState, (int)HeroRobo.ROBO_STATE.IDLE);
        }


        if (m_roboState == (int)HeroRobo.ROBO_STATE.IDLE) //BitControl.Get(m_roboState, (int)HeroRobo.ROBO_STATE.IDLE))
        {
            CheckAndSetAnimation(ANI_IDLE , true);
            m_engineAnimator.SetInteger("play" , 0);
            // m_skletonAnimation.state.SetAnimation(0, ANI_IDLE, true);
        }
        else
        {
            if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE))
            {
                transform.Translate(movex , movey , 0);
                if (IsCurrentAnimation(ANI_IDLE))
                    m_skletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);


            }
        }
    }

    void FireBullet()
    {
        bool flip = m_skletonAnimation.skeleton.flipX;
        Bullet b = BulletManager.Instance().AddBullet(BulletManager.BULLET_TYPE.B_HERO_DEF);

        Vector3 pos = m_gunBone.transform.position;

        b.transform.rotation = Quaternion.Euler(0.0f , 0.0f , m_gunBone.transform.rotation.eulerAngles.z - 90.0f);

        b.transform.position = pos;
    }

    // -- 스파인 애니메이션용 -------------------------------------------------------//
     bool IsCurrentAnimation(string ani)
    {
        if (m_skletonAnimation == null)
            return false;
        return m_skletonAnimation.state.GetCurrent(0).animation.name == ani;
    }

    void CheckAndSetAnimation(string ani , bool loop)
    {
        if (!IsCurrentAnimation(ani))
            m_skletonAnimation.state.SetAnimation(0 , ani , loop);
    }


    //test

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.tag.Equals("Player"))
        {
            CameraManager.Instance().MoveCameraAndObject(
                gameObject , 10 , CameraManager.CAMERA_PLACE.TUTORIAL_ROBO_IN , col.gameObject);
            m_tuto.TutorialAction_ObjectInteraction("enter_door");  
        }
    }
}
