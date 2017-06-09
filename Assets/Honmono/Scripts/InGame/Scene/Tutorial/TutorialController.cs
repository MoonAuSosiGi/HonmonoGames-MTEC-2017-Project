using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;

public class TutorialController : MonoBehaviour {

    // -- 튜토리얼 액션 ----------------------------------------//
    public enum TUTORIAL_ACTION
    {
        Object_Interaction = 100,   // 오브젝트 상호작용
        Kill_Monster,               // 몬스터 제거
        Fix,                        // 수리
        Heal,                       // 생명력 채우기
        Charge,                      // 에너지 채우기
        SHOW_TIME_END               // 시간이 끝남
    }

    // ---------------------------------------------------------//

    // 튜토리얼 인덱스
    private int m_curTutorialIndex = 0;

    // 얻어온 튜토리얼
    private JSONObject m_tutorialObject = null;

    // 몬스터 튜토리얼 검증
    private int m_kill = 0;

    // 튜토리얼 검증용
    private class TUTO
    {
        public TUTORIAL_ACTION action;
        public string value = null;
        public TUTO(TUTORIAL_ACTION act,string va)
        {
            action = act;
            value = va;
        }
    }
    private Queue<TUTO> m_tutorialList = new Queue<TUTO>();
    //튜토리얼
    public TutorialTalk m_talk = null;
    // ---------------------------------------------------------//

    
    // 오브젝트와 상호작용을 했다!
    public void TutorialAction_ObjectInteraction(string objName)
    {
        m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.Object_Interaction , objName));
        ProcessTutorial();
    }

    // 몬스터를 죽였다.
    public void TutorialAction_KillMonster(string monsterName)
    {
        m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.Kill_Monster , monsterName));
        ProcessTutorial();
    }

    // 수리했다.
    public void TutorialAction_Fix()
    {
        m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.Fix,"fix"));
        ProcessTutorial();
    }

    // 생명력이 회복되었다.
    public void TutorialAction_Heal(int curHp,int maxHP)
    {
        if(curHp >= maxHP)
            m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.Heal , "heal_full"));
        ProcessTutorial();
    }

    // 에너지를 충전했다
    public void TutorialAction_Charge(int curEnergy,int maxEnergy)
    {
        if (curEnergy >= maxEnergy)
            m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.Charge , "charge"));
        ProcessTutorial();
    }


    public void TutorialAction_ShoTimeEnd()
    {
        m_tutorialList.Enqueue(new TUTO(TUTORIAL_ACTION.SHOW_TIME_END , "end"));
        ProcessTutorial();
    }

    // -- 튜토리얼 셋업 --------------------------------------------------------//
    public void SetupTutorial()
    {
        StreamReader sr = new StreamReader(GamePath.TUTORIAL_FILE);
        StringBuilder builder = new StringBuilder();
        while (!sr.EndOfStream)
        {
            builder.Append(sr.ReadLine());
        }
        sr.Close();

        string test = builder.ToString();
        m_tutorialObject = new JSONObject(test);

        ShowTutorial();
    }


    // -- 튜토리얼 실행(한번씩) ------------------------------------------------//
    private void ShowTutorial()
    {
        if(m_curTutorialIndex >= m_tutorialObject.GetField("tutorial").Count)
        {
            gameObject.SetActive(false);

            InvokeRepeating("CheckFinalTuToTalk" , 0.0f , 0.1f);
           

            return;
        }
        JSONObject cur = m_tutorialObject.GetField("tutorial")[m_curTutorialIndex];

        if(cur != null)
        {
            string msg = GetCurTutorialStr(cur , "message");


            string startSound = GetCurTutorialStr(cur , "start_sound");
            string interaction = GetCurTutorialStr(cur,"show_interaction");
            string message = msg.Replace("\\n","\n");
            float showTime = GetCurTutorialTime(cur , "show_time");

            m_talk.ShowTutorial(startSound , message , showTime , interaction);
            
        }
    }

    void CheckFinalTuToTalk()
    {
        if(m_talk.TUTP_TALK_ALIVE == false)
        {
            PopupManager.Instance().AddPopup("LobbyPopup");
            CancelInvoke("CheckFinalTutoTalk");
            GameObject.Destroy(gameObject);
        }
    }

    // -- 튜토리얼 계산 --------------------------------------------------------------//
    private void ProcessTutorial()
    {
        JSONObject cur = m_tutorialObject.GetField("tutorial")[m_curTutorialIndex];

        if (cur != null)
        {
            string interaction = GetCurTutorialStr(cur , "show_interaction");   
            float showTime = GetCurTutorialTime(cur , "show_time");
            // 튜토리얼이 완수되었는지 체크

            for(int i = 0; i < m_tutorialList.Count; i++)
            {
                TUTO t = m_tutorialList.Dequeue();
                TUTORIAL_ACTION act = t.action;
                string value = t.value;
                int prev = m_curTutorialIndex;
                switch(act)
                {
                    case TUTORIAL_ACTION.SHOW_TIME_END: m_curTutorialIndex++; break;
                    case TUTORIAL_ACTION.Object_Interaction:
                        if (interaction.Equals(value))
                            m_curTutorialIndex++;
                        break;
                    case TUTORIAL_ACTION.Kill_Monster:
                        m_kill += 1;
                        if (m_kill >= GetCurTutorialCount(cur , "kill_monster"))
                            m_curTutorialIndex++;
                        break;
                    case TUTORIAL_ACTION.Charge:
                    case TUTORIAL_ACTION.Fix:
                    case TUTORIAL_ACTION.Heal:  m_curTutorialIndex++; break;
                }

                if (prev != m_curTutorialIndex)
                {
                    m_tutorialList.Clear();
                    break;
                }
            }
        }
        ShowTutorial();
    }

    // 튜토리얼 구조
    /*
     * start_sound  // 첫 시작 사운드 사운드 종료 후 메시지 출력
     * message      // 메시지
     * tip_image    // 팁 이미지가 있을경우 팁
     * show_time    // 메시지 표기 시간 없을 경우 기본 3초
     * show_interaction     // 메시지 표기 상호작용 있을 경우 
     * tip_complete_img // 튜토리얼 진행 후 img 를 띄움 (마지막으로)
     */

    private string GetCurTutorialStr(JSONObject cur,string tag)
    {
        if(cur != null && cur.GetField(tag) != null) return cur.GetField(tag).str;
        return null;
    }

    private float GetCurTutorialTime(JSONObject cur,string tag)
    {
        if (cur != null && cur.GetField(tag) != null) return cur.GetField(tag).f;
        return -1.0f;
    }

    private int GetCurTutorialCount(JSONObject cur,string tag)
    {
        if (cur != null && cur.GetField(tag) != null) return (int)cur.GetField(tag).i;
        return 0;
    }
}
