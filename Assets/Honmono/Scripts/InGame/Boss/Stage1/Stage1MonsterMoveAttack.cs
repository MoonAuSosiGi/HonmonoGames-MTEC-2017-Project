using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Stage1MonsterMoveAttack : Monster,NetworkManager.NetworkMessageEventListenrer {

    // -- 기본 정보 ------------------------------------------------//
    public GameObject m_target = null;
    public AudioSource m_source = null;
    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";
    private const string ANI_ATTACK = "attack";
    // 쿨타임을 실제 계산할 변수
    private float m_coolTimeTick = 0.0f;

    // 몇초동안 쉬는지
    private float m_coolTime = 0.0f;

    MeshRenderer m_meshRenderer = null;
    private bool m_damageCoolTime = false;

    // Networking
    Vector3 m_prevPos = Vector3.zero;
    float m_lastSendTime = 0.0f;
    private string m_prevState = null;
    private string m_curState = null;

    private bool m_isNetworkObject = false;
    bool m_networkObjectCheck = false;
    public bool NETWORKING { set { m_isNetworkObject = value; } }

    public GameObject m_leftLimit = null;
    public GameObject m_rightLimit = null;
    public bool m_currentDirLeft = true;
    private float m_attackTick = 0.0f;
    // -------------------------------------------------------------//

    // attack이 공격인 타입 - 기본값은 move 가 attack
    private bool m_hasAttackAnimation = false;

    void Start()
    {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        //m_skeletonAnimation.state.Complete += State_Complete;
        m_meshRenderer = this.GetComponent<MeshRenderer>();
        m_curState = "idle";

        m_hp = 5;
        m_fullHp = 5;
    }

    bool NetworkObjectCheck()
    {
        if (m_isNetworkObject)
            return false;

        if (m_networkObjectCheck)
            return true;
        if (GameManager.Instance().m_curSceneState.Equals("play") &&
            NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            Vector3 pos = transform.position;
            m_name = "monster_" + GameManager.Instance().PLAYER.USER_NAME + "_" + this.GetHashCode();
            string dataName = m_skeletonAnimation.skeletonDataAsset.name;

            if(dataName.Equals("mon_planet3_SkeletonData"))
            {
                m_hasAttackAnimation = true;

                // 수정해라
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "PlanetMonster3" , pos.x , pos.y , -1.0f));
            }
            else if(dataName.Equals("mon_planet1_SkeletonData"))
            {
                m_hasAttackAnimation = false;
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "PlanetMonster1" , pos.x , pos.y , -1.0f));
            }
            
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));
            MoveSend();

            NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
            m_networkObjectCheck = true;
            if(m_hasAttackAnimation)
                m_skeletonAnimation.state.Complete += State_Complete;

            // attack 이 있을경우
            if(m_hasAttackAnimation)
            {
                // 초기 시작값
                this.m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
            }
            else
            {
                // 초기 시작값
                this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
            }

            return true;
        }
        else if (!string.IsNullOrEmpty(NetworkOrderController.ORDER_NAME) && !string.IsNullOrEmpty(GameManager.Instance().PLAYER.USER_NAME) &&
            !NetworkOrderController.ORDER_NAME.Equals(GameManager.Instance().PLAYER.USER_NAME))
        {
            GameObject.Destroy(gameObject);
        }
        return false;
    }

    private void State_Complete(Spine.TrackEntry trackEntry)
    {
        if (trackEntry.animation.name.Equals(ANI_ATTACK))
        {
            if (m_target == null)
                return;
            Hero hero = m_target.GetComponent<Hero>();

            if (hero != null)
            {
                hero.Damage(1 , m_skeletonAnimation.skeleton.flipX);
            }
            m_skeletonAnimation.state.SetAnimation(0 , ANI_MOVE , true);
            //  m_target = null;
        }
    }



    void Update()
    {
        m_prevState = m_curState;

        if (!NetworkObjectCheck())
            return;

        if (m_target == null)
            Move();
        else
        {
            if (CoolTime())
                return;

            SetCoolTime(Attack());
        }
    }


    protected override void Move()
    {
        if (m_damageCoolTime)
            return;
        CheckAndSetAnimation(0 , ANI_MOVE , true);
        bool flip = m_skeletonAnimation.skeleton.flipX;
        GameObject cur = (m_currentDirLeft) ? m_leftLimit : m_rightLimit;
        Vector3 dir = (new Vector3(cur.transform.position.x,transform.position.y,transform.position.z ))
            - transform.position;
        dir.Normalize();

        if (m_target == null)
            m_curState = "move";

        transform.position += dir * GameSetting.HERO_MOVE_SPEED * Time.deltaTime;

        if(m_currentDirLeft)
        {
            if(transform.position.x <= m_leftLimit.transform.position.x)
            {
                m_skeletonAnimation.skeleton.flipX = true;
                m_currentDirLeft = false;
            }
        }
        else
        {
            if(transform.position.x >= m_rightLimit.transform.position.x)
            {
                m_skeletonAnimation.skeleton.flipX = false;
                m_currentDirLeft = true;
            }
        }

        if (m_curState != m_prevState)
        {
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_MOVE , true));
        }

        MoveSend();
    }

    public override float Attack()
    {
        if (m_hasAttackAnimation)
            return 0.0f;

        m_curState = "attack";
        
        if (m_curState != m_prevState)
        {
            if (!m_source.isPlaying)
                m_source.Play();
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_ATTACK , false));
        }

        return 1.0f;
    }

    public override void Damage(int damage)
    {
        if (m_damageCoolTime)
            return;
        m_hp -= damage;
        if (m_hp <= 0)
        {
            MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
            NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
            GameObject.Destroy(gameObject);
            return;
        }

        m_damageCoolTime = true;
        float xpower = (m_skeletonAnimation.skeleton.flipX) ?
             -100.0f : 100.0f;
        InvokeRepeating("DamageEffect" , 0.1f , 0.1f);

        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonHPUdate(m_name , m_hp));

    }

    void DamageEffect()
    {
        Color color = m_meshRenderer.material.color;

        color.g -= 0.5f;
        color.b -= 0.5f;
        m_meshRenderer.material.color = color;
        if (color.g <= 0.0f)
        {
            CancelInvoke("DamageEffect");
            InvokeRepeating("DamageEffectEnd" , 0.1f , 0.1f);
        }
    }
    void DamageEffectEnd()
    {
        Color color = m_meshRenderer.material.color;

        color.g += 0.3f;
        color.b += 0.3f;
        m_meshRenderer.material.color = color;
        if (color.g >= 1.0f)
        {
            CancelInvoke("DamageEffectEnd");
            m_damageCoolTime = false;
        }
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

    void OnCollisionEnter2D(Collision2D col)
    {
        if (col.transform.tag.Equals("Player"))
        {
            m_target = col.gameObject;
            if(!m_hasAttackAnimation)
            {

            }
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (m_target == null)
            return;

        m_attackTick += Time.deltaTime;
   //     MDebug.Log("Tick " + string.Format("{0:F1}" , tick));
        if (m_attackTick <= 0.5f)
        {
            
            return;
        }
        m_attackTick = 0.0f;
        Hero hero = m_target.GetComponent<Hero>();

        if (hero != null)
        {
            if(hero.IS_PLAYER)
                hero.Damage(1 , m_skeletonAnimation.skeleton.flipX,500.0f);
            else
                NetworkManager.Instance().SendOrderMessage(
                        JSONMessageTool.ToJsonCharacterHPUpdate(hero.USERNAME , 10 , GameSetting.HERO_MAX_HP));
        }
        CheckAndSetAnimation(0 , ANI_MOVE , true);
    }

    void OnCollisionExit2D(Collision2D col)
    {
        if (col.transform.tag.Equals("Player"))
            m_target = null;
    }

    protected void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos , pos);
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));

        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_name ,
            pos.x , pos.y , 0 ,
            m_skeletonAnimation.skeleton.flipX ,
            sendPos));
        m_lastSendTime = Time.deltaTime;
    }

    void NetworkManager.NetworkMessageEventListenrer.ReceiveNetworkMessage(NetworkManager.MessageEvent e)
    {
        if (e.msgType.Equals(NetworkManager.HP_UPDATE))
        {
            // 데미지 입은것이 들어옴
            if (e.targetName.Equals(m_name) && !GameManager.Instance().PLAYER.USER_NAME.Equals(e.user))
            {
                if (m_damageCoolTime)
                    return;
                GameManager.Instance().SetCurrentEnemy(this);
               

                base.Damage((int)e.msg.GetField(NetworkManager.HP_UPDATE).i);
                if (m_hp <= 0)
                {
                    MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);
                    NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_name , "Monster"));
                    NetworkManager.Instance().RemoveNetworkOrderMessageEventListener(this);
                    GameObject.Destroy(gameObject);
                    return;
                }

                m_damageCoolTime = true;
                float xpower = (m_skeletonAnimation.skeleton.flipX) ?
                     -100.0f : 100.0f;
                InvokeRepeating("DamageEffect" , 0.1f , 0.1f);

            }
        }
    }
}
