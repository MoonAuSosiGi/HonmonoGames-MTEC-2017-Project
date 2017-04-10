using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour, NetworkManager.NetworkMoveEventListener{


    private SpriteRenderer m_renderer = null;
    private float m_moveSpeed = 15.0f;
    private string m_bulletName = "";
    private Vector3 m_targetPos = Vector3.zero;
    private Vector3 m_prevPos = Vector3.zero;
    private bool m_isNetworkObject = false;
    float m_syncTime = 0.0f;
    float m_delay = 0.0f;
    float m_lastSyncTime = 0.0f;
    

    // Use this for initialization
    void Start () {
        m_renderer =  this.GetComponent<SpriteRenderer>();
        m_prevPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {

        if (!m_isNetworkObject)
        {
            float movex = (m_renderer.flipX) ? m_moveSpeed * Time.deltaTime : -m_moveSpeed * Time.deltaTime;
            float posx = transform.position.x;


            // TODO 고쳐야 할 코드
            TargetMoveCamera camera = Camera.main.GetComponent<TargetMoveCamera>();
            Vector3 backPos = camera.BACKGROUND_POS;
            float leftCheck = backPos.x - camera.BACKGROUND_HALF_WIDTH;
            float rightCheck = backPos.x + camera.BACKGROUND_HALF_WIDTH;
            //float upCheck = backPos.y + camera.BACKGROUND_HALF_HEIGHT;
            //float downCheck = backPos.x - camera.BACKGROUND_HALF_HEIGHT;

            if (posx + movex + m_renderer.bounds.size.x / 2.0f <= leftCheck) DeleteBullet();
            if (posx + movex - m_renderer.bounds.size.x / 2.0f >= rightCheck) DeleteBullet();

            transform.Translate(movex, 0, 0);
            
        }
        else
        {
            m_syncTime += Time.deltaTime;

            // 네트워크 보간( 테스트 완료 - 로컬 )
            if (m_delay > 0)
                transform.position = Vector3.Lerp(transform.position, m_targetPos, m_syncTime / m_delay);
        }

        
	}

    void DeleteBullet()
    {
        CancelInvoke();
     //   NetworkManager.Instance().m_bulletList.Remove(m_bulletName);
        NetworkManager.Instance().RemoveNetworkEnemyMoveEventListener(this);
        BulletManager.Instance().RemoveBullet(this);
    }

    public void SetupBullet(string name,bool networkObject)
    {
        m_bulletName = name;
        
        m_isNetworkObject = networkObject;

        if (m_isNetworkObject)
            NetworkManager.Instance().AddNetworkEnemyMoveEventListener(this);
        else
        {
       //     NetworkManager.Instance().m_bulletList.Add(name);
            InvokeRepeating("MoveSend", 0.0f, 0.05f);
        }
    }

    void NetworkManager.NetworkMoveEventListener.ReceiveMoveEvent(JSONObject json)
    {
        JSONObject obj = json;
        JSONObject users = obj.GetField("Enemies");

        //{"Enemies":[{"UserName":"test","x":-3.531799,"y":-0.02999991,"z":0,"dir":0}]}

        float x = 0.0f, y = 0.0f, z = 0.0f;
        bool flip = false;
        bool ck = false;
        for (int i = 0; i < users.Count; i++)
        {
            if (users[i].GetField("Name").str == m_bulletName)
            {
                x = users[i].GetField("X").f;
                y = users[i].GetField("Y").f;
                z = users[i].GetField("Z").f;
                flip = users[i].GetField(NetworkManager.DIR).b;
                ck = true;
                break;
            }
        }

        if (!ck)
            return;

        Vector3 newPos = new Vector3(x, y);

        float distance = Vector3.Distance(transform.position, newPos);
        //this.m_renderer.flipX = flip;

        if(z != transform.rotation.z)
        {
            transform.Rotate(new Vector3(0, 0, z), Space.World);
        }
        if (distance <= 0)
        {
        ////    this.m_animator.SetBool("Move", false);
            return;
        }

        m_targetPos = newPos;

        m_syncTime = 0.0f;
        m_delay = Time.time - m_lastSyncTime;
        m_lastSyncTime = Time.time;

    }

    void MoveSend()
    {
        Vector3 pos = transform.position;
        float distance = Vector3.Distance(m_prevPos, pos);
        m_prevPos = transform.position;
        //    MDebug.Log(t);
        if (distance <= 0)
            return;

        //  MDebug.Log(JSONMessageTool.ToJsonMove(pos.x, pos.y, m_renderer.flipX));
        NetworkManager.Instance().SendEnemyMoveMessage(JSONMessageTool.ToJsoinEnemyMove(m_bulletName,pos.x, pos.y,transform.rotation.z, m_renderer.flipX));
        // NetworkManager.Instance().SendMovePos(GameManager.Instance().PLAYER.ToJsonPositionInfo());
    }

}
