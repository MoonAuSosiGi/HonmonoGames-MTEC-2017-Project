using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutoDoor : MonoBehaviour {

    GameObject m_player = null;

    public TutorialController m_controller = null;

    void OnCollisionEnter2D(Collision2D col)
    {
        if(col.transform.tag.Equals("Player"))
        {
            m_player = col.gameObject;
            Vector3 p = new Vector3(0 , 20.67f);
            
            iTween.MoveTo(gameObject , iTween.Hash("y" , Camera.main.ScreenToWorldPoint(p).y , 
                "easeType" , "easeOutBack" , "oncompletetarget" , gameObject , "oncomplete" , "DoorEnd",
                "time",2.0f));
        }
    }

    void DoorEnd()
    {
        m_controller.TutorialAction_ObjectInteraction("open_door");

        //GameManager.Instance().ChangeScene(GameManager.PLACE.TUTORIAL_ROBO);
        //CameraManager.Instance().MoveCameraAndObject(gameObject , 10 , CameraManager.CAMERA_PLACE.TUTORIAL_ROBO , m_player , gameObject , "EndTuto",false);
    }

    void EndTuto()
    {
        transform.parent.parent.gameObject.SetActive(false);
    }
}
