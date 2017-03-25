using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo {

    // -- 플레이어 정보 -----------------------------------------------------------------------------------------//
    // 현재 플레이어의 모든 정보가 집약되어 있다.
    private Hero m_hero = null;

    private string m_UserName = "";
    private int m_hp = 0;
    private PLAYER_STATUS m_status = new PLAYER_STATUS();

    // -- 프로퍼티 ---------------------------------------------------------------------------------------------//
    public Hero PLAYER_HERO { set { m_hero = value; m_UserName = m_hero.USERNAME; } get{ return m_hero;}}
    public string USER_NAME { set { m_UserName = value; } get { return m_UserName; } }

    // -- 정보 구조체 ------------------------------------------------------------------------------------------//

    // Status (스탯 정보)        
    public struct PLAYER_STATUS
    {

        public JSONObject ToJson()
        {
            return new JSONObject("");
        }
    }

    // -- 기본 메서드 -----------------------------------------------------------------------------------------//

    // 모든 데이터 JSON
    public JSONObject ToJsonAllInfo()
    {
        return new JSONObject();
    }

    // 이동 JSON
    public JSONObject ToJsonPositionInfo()
    {
        Vector3 pos = m_hero.transform.position;
        return new JSONObject("{\"UserName\":" + m_UserName + ",\"x\":" + pos.x + ",\"y\":" + pos.y + "}");
    }
    
    //---------------------------------------------------------------------------------------------------------//
    
}
