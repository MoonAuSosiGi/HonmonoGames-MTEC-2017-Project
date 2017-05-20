using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EFFECT : MonoBehaviour {

    private AudioSource m_source = null;
    private bool m_ani = false;

    void Start()
    {
        Vector2 p = transform.position;
        NetworkManager.Instance().SendOrderMessage(
            JSONMessageTool.ToJsonCreateOrder(
               "eff_" + GameManager.Instance().PLAYER.USER_NAME +"_"+ this.GetHashCode() ,
                "effect" ,
                p.x , p.y , -1.0f));
        m_source = this.GetComponent<AudioSource>();    
    }

	void Update()
    {
        if(m_ani && !m_source.isPlaying)
        {
            GameObject.Destroy(gameObject);
        }
    }
    public void END()
    {
        m_ani = true;
        this.GetComponent<SpriteRenderer>().enabled = false;
        
    }
}
