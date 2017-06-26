using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class InsidePentrationMonster : Monster ,NetworkManager.NetworkMessageEventListenrer{

    // -- 기본 정보 ------------------------------------------------//
    public GameObject m_target = null;

    private const string ANI_IDLE = "idle";
    private const string ANI_MOVE = "move";
    private const string ANI_ATTACK_PREV = "jumping";
    private const string ANI_ATTACK = "jump";

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
    // -------------------------------------------------------------//

    void Start()
    {
        m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();
        m_skeletonAnimation.state.Complete += State_Complete;
        m_meshRenderer = this.GetComponent<MeshRenderer>();
        m_curState = "idle";
        this.m_skeletonAnimation.state.SetAnimation(0 , ANI_IDLE , true);

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

            NetworkManager.Instance().SendOrderMessage(
                    JSONMessageTool.ToJsonCreateOrder(m_name , "InsidePentrationMonster" , pos.x , pos.y , -1.0f));
            

            NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonOrderStateValueChange(m_name , m_hp));
            MoveSend();

            NetworkManager.Instance().AddNetworkOrderMessageEventListener(this);
            m_networkObjectCheck = true;
            m_skeletonAnimation.state.Complete += State_Complete;
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
        if(trackEntry.animation.name.Equals(ANI_ATTACK))
        {
            if (m_target == null)
                return;
            Hero hero = m_target.GetComponent<Hero>();

            if (hero != null)
            {
                hero.Damage(1,m_skeletonAnimation.skeleton.flipX);
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

        if(m_target == null)
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
        float dir = (flip) ? 1.0f : -1.0f;

        if (m_target == null)
            m_curState = "move";

        transform.Translate(
            new Vector3(GameSetting.HERO_MOVE_SPEED * Time.deltaTime * dir , 0.0f));

        if (m_curState != m_prevState)
        {
            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(m_name , "" , ANI_MOVE , true));
        }

        MoveSend();
    }

    public override float Attack()
    {
        m_curState = "attack";
        m_skeletonAnimation.state.SetAnimation(0 , ANI_ATTACK_PREV , false);
        m_skeletonAnimation.state.AddAnimation(0 , ANI_ATTACK , false,0.0f);

        AudioSource source = this.GetComponent<AudioSource>();
        if (source != null && !source.isPlaying)
            source.Play();

        if (m_curState != m_prevState)
        {

            NetworkManager.Instance().SendOrderMessage(
                JSONMessageTool.ToJsonAIMessage(m_name , "",new string[] { ANI_ATTACK_PREV , ANI_ATTACK }));
        }

        return 1.0f;
    }

    public override void Damage(int damage)
    {
        if (m_damageCoolTime)
            return;
        base.Damage(damage);
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

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.tag.Equals("Player"))
            m_target = col.gameObject;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.tag.Equals("Player"))
            m_target = null;
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.transform.GetComponent<Collider2D>().isTrigger == false)
        {
            if (m_skeletonAnimation == null)
                m_skeletonAnimation = this.GetComponent<SkeletonAnimation>();

            m_skeletonAnimation.skeleton.flipX = !m_skeletonAnimation.skeleton.flipX;
            this.GetComponent<Rigidbody2D>().velocity = new Vector2(0 , 0);
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
            if (e.targetName.Equals(m_name) && 
                !GameManager.Instance().PLAYER.USER_NAME.Equals(e.user))
            {
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
