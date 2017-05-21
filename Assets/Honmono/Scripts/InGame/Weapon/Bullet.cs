using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, NetworkManager.NetworkMoveEventListener
{

    // -- 기본 정보 -------------------------------------------------------------------------//
    private SpriteRenderer m_renderer = null;
    private float m_moveSpeed = 10.0f;
    private string m_bulletName = "";
    private bool m_isNetworkObject = false;

    Vector3 m_bulletDir = Vector3.left;

    public string BULLET_NAME { get { return m_bulletName; } }

    public float BULLET_SPEED { get { return m_moveSpeed; } set { m_moveSpeed = value; } }
    private bool m_filp = false;

    private bool m_alive = false;

    public bool ALIVE { get { return m_alive; } set { m_alive = value; } }

    public enum BULLET_TARGET
    {
        PLAYER,
        ENEMY
    }

    private BULLET_TARGET m_curTarget = BULLET_TARGET.PLAYER;

    // -- 네트워크 ------------------------------------------------------------------------- //
    private Vector3 m_prevPos = Vector3.zero;
    private Vector3 m_targetPos = Vector3.zero;
    private float m_lastSendTime = 0.0f;

    // ------------------------------------------------------------------------------------- //
    // Use this for initialization
    void Start()
    {
        m_renderer = this.GetComponent<SpriteRenderer>();
        m_prevPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_alive)
            return;

        if (!m_isNetworkObject)
        {

            float movex = (m_filp) ? m_moveSpeed * Time.deltaTime : -m_moveSpeed * Time.deltaTime;
            float posx = transform.position.x;

            if (m_bulletName.IndexOf("boss") >= 0)
            {
                transform.position += m_bulletDir * m_moveSpeed * Time.deltaTime;
                MoveSend();
                return;
            }

            transform.Translate(0 , -movex , 0);
            MoveSend();
        }
        else
        {
            transform.position = m_targetPos;
        }


    }

    void DeleteBullet()
    {
        CancelInvoke();
        NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_bulletName , "myTeam_bullet"));
        BulletManager.Instance().RemoveBullet(this);
    }

    public void SetupBullet(string name , bool networkObject , Vector3 dir , float speed = 0.0f , bool flip = false)
    {
        m_bulletName = name;

        m_isNetworkObject = networkObject;
        m_filp = flip;


        if(m_bulletName.IndexOf("boss")>=0)
        {
            m_bulletDir = dir;
            //float angle = (Mathf.Atan2(m_bulletDir.x , m_bulletDir.y) * Mathf.Rad2Deg);// + 45.0f;
            //transform.rotation = Quaternion.Euler(0.0f , 0.0f , angle);
            m_curTarget = BULLET_TARGET.ENEMY;
        }
        else
        {
            m_curTarget = BULLET_TARGET.PLAYER;
        }

        if (m_isNetworkObject)
        {
            this.GetComponent<Rigidbody2D>().simulated = false;
      //      NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
            //   InvokeRepeating("MoveSend", 0.0f, 1.0f / 60.0f);
            

            // ANGLE 스프라이트 방향 때문에 조절해야함 -값 붙음
            

        }
    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        if (!m_isNetworkObject)
            return;
        JSONObject obj = json;
        JSONObject users = obj.GetField("Enemies");

        //{"Enemies":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        float x = 0.0f, y = 0.0f, z = 0.0f;
        bool flip = false;
        bool ck = false;
        Vector3 drPos = Vector3.zero;
   //     MDebug.Log("bullet " +json);
        for (int i = 0; i < users.Count; i++)
        {
            if (m_bulletName.Equals(users[i].GetField("Name").str))
            {
                x = users[i].GetField("X").f;
                y = users[i].GetField("Y").f;
                z = users[i].GetField("Z").f;
                flip = users[i].GetField(NetworkManager.DIR).b;
                ck = true;
                JSONObject v = users[i].GetField(NetworkManager.DIRVECTOR);
                drPos = new Vector3(v.GetField("X").f , v.GetField("Y").f , -1.0f);
                break;
            }
        }

        if (!ck)
        {
            // 여기에 없다는 것은 삭제되어야 한다.
          //  NetworkManager.Instance().SendOrderMessage(JSONMessageTool.ToJsonRemoveOrder(m_bulletName , "myTeam_bullet"));
            return;
        }

        
        if (z > transform.rotation.eulerAngles.z)
        {

            transform.rotation = Quaternion.Euler(0 , 0 , z);
        }



        m_targetPos = new Vector3(drPos.x,drPos.y,-1.0f); //* m_delay;
                              // transform.position = newPos;
    }

    void MoveSend()
    {
        if (m_isNetworkObject)
            return;
        Vector3 pos = transform.position;
        m_prevPos = transform.position;

        Vector3 velocity = (transform.position - m_prevPos) / Time.deltaTime;
        Vector3 sendPos = m_prevPos + (velocity * (Time.deltaTime - m_lastSendTime));

        NetworkManager.Instance().SendEnemyMoveMessage(
            JSONMessageTool.ToJsonEnemyMove(m_bulletName ,
            pos.x , pos.y , 
            transform.rotation.eulerAngles.z , 
            m_filp,
            sendPos,
            (m_alive) ? null : "Delete"));
        
    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (m_isNetworkObject)
            return;

        //맵 바깥쪽에 도착했다.
        if (col.transform.tag.Equals("OUTLINE"))
        {
            BulletManager.Instance().RemoveBullet(this);
        }
        else
        {
            if (col.transform.tag.Equals("ENEMY") && m_curTarget == BULLET_TARGET.PLAYER)
            {
                Monster mon = col.GetComponent<Monster>();
                mon.Damage(1);

                BulletManager.Instance().RemoveBullet(this);
            }
            else if(col.transform.tag.Equals("BOSS") && m_curTarget == BULLET_TARGET.PLAYER)
            {
                Stage1BOSS boss = col.GetComponent<Stage1BOSS>();
                boss.Damage(1);
            }
            else if(col.transform.tag.Equals("Player") && m_curTarget == BULLET_TARGET.ENEMY)
            {
                // 데미지 받는 처리 
                //Vector3 bulletPos = transform.position;
                //Vector3 targetPos = col.transform.position;
                //Vector3 createPos = Vector3.zero;
                MapManager.Instance().AddObject(GamePath.EFFECT , transform.position);

                HeroRobo robo = col.GetComponent<HeroRobo>();
                if(robo != null)
                {
                    robo.Damage(1);
                }
                else
                {
                    Hero hero = col.GetComponent<Hero>();
                    if(hero != null)
                    {
                        hero.Damage(1);
                    }
                }
                  
            }
                
        }
        
    }
   
}
