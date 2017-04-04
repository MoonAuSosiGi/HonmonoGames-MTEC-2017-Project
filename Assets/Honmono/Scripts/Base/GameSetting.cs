using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameSetting  {

    // -- 주인공 관련 값 ( 휴먼 ) -----------------------------------------------------//
    public const float HERO_MOVE_SPEED = 6.0f;
    public const float HERO_MAX_HP = 100.0f;

    // -- 주인공 관련 값 ( 로봇 ) -----------------------------------------------------//
    public const float HERO_ROBO_SPEED = 6.0f;
    public const float HERO_ROBO_BULLET_SPEED = 10.0f;
    public const float HERO_ROBO_MAX_HP = 1000.0f;

    // -- 보스 패턴에 관련된 세팅값 ---------------------------------------------------//

    public const float BOSS1_ATTACK_ABLE_COOLTIME = 4000.0f;
    public const float BOSS1_PATTERN_A_ABLE_COOLTIME = 4000.0f;
    public const float BOSS1_PATTERN_B_ABLE_COOLTIME = 15000.0f;
    public const float BOSS1_PATTERN_C_ABLE_COOLTIME = 18000.0f;

    public const float BOSS1_ATTACK_ABLE_DISTANCE = 10.0f;
    public const float BOSS1_DEF_ATTACK_COOLTIME = 1000.0f;



    public const float BOSS1_PATTERN_A_ATTACK_COOLTIME = 2000.0f;
    public const float BOSS1_PATTERN_B_ATTACK_COOLTIME = 1000.0f;
    public const float BOSS1_PATTERN_C_ATTACK_COOLTIME = 2000.0f;

    public const float BOSS1_PATTERN_A_BULLET_REACH_TIME = 2000.0f;
    public const float BOSS1_PATTERN_B_BULLET_REACH_TIME = 3000.0f;
    public const float BOSS1_PATTERN_C_BULLET_REACH_TIME = 2000.0f;

    public const float BOSS1_PATTERN_D_HP_CONDITION = 0.3f;
    public const float BOSS1_PATTERN_D_SPECIAL = 5000.0f;
    public const float BOSS1_PATTERN_D_DEF = 0.5f;
}
