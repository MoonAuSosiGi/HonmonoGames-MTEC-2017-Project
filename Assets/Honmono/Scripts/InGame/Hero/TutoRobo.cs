using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class TutoRobo : MonoBehaviour
{


    private SkeletonAnimation m_skletonAnimation = null;
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

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
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
           // m_engineAnimator.SetInteger("play" , 1);
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
        // 로컬 전투 처리용--------------------------------------------------------------
        m_prevAngle = m_gunAngle;
        if (!m_skletonAnimation.skeleton.flipX)
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , m_gunAngle);
        else
            m_armBone.transform.rotation = Quaternion.Euler(0 , 0 , -m_gunAngle);


        if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.ATTACK))
        {
            m_skletonAnimation.state.SetAnimation(0 , ANI_ATTACK , false);
            m_roboState = BitControl.Clear(m_roboState , (int)HeroRobo.ROBO_STATE.ATTACK);
            m_roboState = BitControl.Set(m_roboState , (int)HeroRobo.ROBO_STATE.COOLTIME);
            FireBullet();
        }


        //-------------------------------------------------------------------------------

        if (BitControl.Get(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE) &&
            (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.RightArrow) ||
             Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow)))
        {
            //  CheckAndSetAnimation(ANI_IDLE, true);
            m_skletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
            m_roboState = BitControl.Clear(m_roboState , (int)HeroRobo.ROBO_STATE.MOVE);
         //   m_engineAnimator.SetInteger("play" , 0);
            //m_roboState = BitControl.Set(m_roboState, (int)HeroRobo.ROBO_STATE.IDLE);
        }


        if (m_roboState == (int)HeroRobo.ROBO_STATE.IDLE) //BitControl.Get(m_roboState, (int)HeroRobo.ROBO_STATE.IDLE))
        {
            CheckAndSetAnimation(ANI_IDLE , true);
         //   m_engineAnimator.SetInteger("play" , 0);
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


        // 네트워크 식별 이름
        string n = GameManager.Instance().PLAYER.USER_NAME + "_" + m_bulletIndex;
        b.SetupBullet(n , false , Vector3.zero , 0.0f , m_skletonAnimation.skeleton.flipX);

        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonCreateOrder(n , "myTeam_bullet" , pos.x , pos.y , pos.z , flip));
        m_bulletIndex++;


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

}
