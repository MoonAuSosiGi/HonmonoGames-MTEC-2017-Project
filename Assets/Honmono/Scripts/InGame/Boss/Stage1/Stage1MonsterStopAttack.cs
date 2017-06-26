using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class Stage1MonsterStopAttack : Monster, NetworkManager.NetworkMessageEventListenrer
{
    // -- 기본 정보 ------------------------------------------------//
    public GameObject m_target = null;
    public AudioSource m_source = null;

    private const string ANI_IDLE = "idle";
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

    private Hero m_heroTarget = null;

    // -------------------------------------------------------------//

    // attack이 공격인 타입 - 기본값은 move 가 attack

    void Start()
    {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_skeletonAnimation.state.Complete += State_Complete;
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

            if (dataName.Equals("mon_planet2_SkeletonData"))
            {

                // 수정해라
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "PlanetMonster2" , pos.x , pos.y , -1.0f));
            }


            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));
            MoveSend();

            NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
            m_networkObjectCheck = true;

            // 초기 시작값
            this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
           // m_heroTarget = GameManager.Instance().PLAYER.PLAYER_HERO;
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
            m_curState = "idle";

            Hero hero = m_heroTarget;

            if (hero != null)
            {
                if (hero.IS_PLAYER)
                    hero.Damage(1 , m_skeletonAnimation.skeleton.flipX);
                else
                    NetworkManager.Instance().SendOrderMessage(
                        JSONMessageTool.ToJsonCharacterHPUpdate(hero.USERNAME , 1 , GameSetting.HERO_MAX_HP));
            }
            m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);
            //  m_target = null;
        }
    }



    void Update()
    {
        m_prevState = m_curState;

        if (!NetworkObjectCheck())
            return;

        Move();
        if (CoolTime())
            return;

        SetCoolTime(Attack());

    }


    protected override void Move()
    {
        if (m_damageCoolTime)
            return;

        MoveSend();
    }

    public override float Attack()
    {
        m_curState = "attack";

        if (m_heroTarget == null)
            return 1.0f;

        Vector3 pos = transform.position;
        Vector3 hero = m_heroTarget.transform.position;

        if (pos.x <= hero.x)
        {
            m_skeletonAnimation.skeleton.flipX = true;
        }
        else
            m_skeletonAnimation.skeleton.flipX = false;

        if (Mathf.Abs(Vector3.Distance(pos , hero) )<= 51.0f)
        {
            CheckAndSetAnimation(0 , ANI_ATTACK , false);
            if (!m_curState.Equals(m_prevState))
            {
                m_source.Play();
                NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_ATTACK , false));
            }
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

        }
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
                m_damageCoolTime = true;
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
                if(!IsInvoking("DamageEffect"))
                    InvokeRepeating("DamageEffect" , 0.1f , 0.1f);
            }
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        Hero t = col.GetComponent<Hero>();

        if (t != null)
        {
          //  if (m_heroTarget != t)
            //    MDebug.Log("다른녀석이 장전됨 " + t.USERNAME + " d " + m_heroTarget.USERNAME);
            m_heroTarget = t;
            
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.tag.Equals("Player"))
        m_heroTarget = null;
    }
}
