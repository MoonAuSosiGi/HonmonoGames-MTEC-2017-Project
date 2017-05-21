using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Stage1Monster : Monster , NetworkManager.NetworkMessageEventListenrer
{

    // -- 기본 정보 --------------------------------------------------------//
    private HeroRobo m_robo = null;

    // 공격 가능 범위 체크용
    private float m_attackableTick = 0.0f;

    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    private const string ANI_ATTACK = "attack";
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";

    private bool m_isNetworkObject = false;

    // ---------------------------------------------------------------------//

    void Start()
    {
        m_robo = GameManager.Instance().ROBO;
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_hp = 5;
        this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
        
        
    }

    bool NetworkObjectCheck()
    {
        if(!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME))
        {
            if (m_pattern != null)
                return true;
            
            m_name = NetworkOrderController.ORDER_NAME + "monster" + "_" + this.GetHashCode();
            m_pattern = new MonsterPattern(m_skeletonAnimation , ANI_MOVE , ANI_ATTACK , m_name);
            m_skeletonAnimation.state.Complete += State_Complete;

            if (!NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
                m_isNetworkObject = true;
            return true;
        }
        return false;
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals(ANI_ATTACK))
        {
            m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
            if (m_attackTarget != null)
            {
                HeroRobo robo = m_attackTarget.GetComponent<HeroRobo>();

                if(robo != null)
                    robo.Damage(m_power);
                else
                {
                    Hero hero = m_attackTarget.GetComponent<Hero>();

                    if (hero != null)
                        hero.Damage(m_power); 
                }
            }
            
        }
    }

    void Update()
    {
        if (!NetworkObjectCheck())
            return;
       
        if (FindMoveAbleCheck())
            Move();

        // 쿨타임 중이면 아~~무것도 안함
        if (CoolTime())
            return;

        if (!AttackAbleCheck())
            return;
        SetCoolTime(Attack());

    }



    protected override void Move()
    {
        if (m_pattern != null)
            m_pattern.Move(gameObject , m_robo.gameObject);
        

        if (m_robo.transform.position.x < transform.position.x)
            m_skeletonAnimation.skeleton.flipX = false;
        else
            m_skeletonAnimation.skeleton.flipX = true;
        base.Move();
    }

    public override void Damage(int damage)
    {
        base.Damage(damage);

        if (m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT,transform.position);

            GameObject.Destroy(gameObject);
        }
    }

    // 공격 가능 범위 체크
    bool AttackAbleCheck()
    {
        Vector3 roboPos = m_robo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_ATTACK_DISTANCE)
        {
            return true;
        }

        return false;
    }

    bool FindMoveAbleCheck()
    {
        Vector3 roboPos = m_robo.transform.position;
        Vector3 pos = transform.position;

        if (Vector3.Distance(roboPos , pos) <= GameSetting.MONSTER_FIND_DISTANCE)
        {
            return true;
        }

        return false;
    }

    void SetCoolTime(float time)
    {
        if (time <= 0.0f)
            return;
        m_coolTime = time;
        m_coolTimeTick = 0.0f;
    }

    bool CoolTime()
    {

        if (m_coolTimeTick > m_coolTime)
        {
            return false;
        }
        else
        {


            m_coolTimeTick += Time.deltaTime;
            return true;
        } 

    }
    //-------------------------------------------------------------------------------//
    public void ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        switch (e.msgType)
        {
            case NetworkManager.AI_ANI_NAME:
                {
                    if (e.targetName.Equals(m_name))
                    {

                        switch (e.msg.GetField(NetworkManager.AI_PATTERN_NAME).str)
                        {
                            //A 패턴과 B패턴은 단순 이동 / 공격 애니메이션 처리만 함
                            case "Monster":
                                m_skeletonAnimation.state.SetAnimation(0 ,
                                    e.msg.GetField(NetworkManager.AI_ANI_NAME).str ,
                                    e.msg.GetField(NetworkManager.AI_ANI_LOOP).b);
                                break;
                        }

                    }
                }
                break;

        }
    }
}
