using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GamePath {

    public static string TUTORIAL_FILE = "Assets/Resources/Tutorial/tutorial.json";
    public static string PREFABS = "Prefabs/";
    public static string WEAPON_BULLET_DEF = PREFABS + "Weapon/Bullet/Def_Bullet";
    public static string WEAPON_BULLET_BOSS = PREFABS + "Weapon/Bullet/Boss_Bullet";
    public static string WEAPON_BULLET_EGG = PREFABS + "Weapon/Bullet/Egg_Bullet";

    public static string POPUP = PREFABS + "Popup/";
    public static string ENEMY = PREFABS + "Enemy/";
    public static string BOSS = ENEMY + "BOSS/";

    // BOSS
    public static string BOSS1 = BOSS + "Stage1BOSS";
    public static string BOSS2 = BOSS + "Stage2BOSS";
    public static string MONSTER1 = ENEMY + "Monster1";
    public static string MONSTER2 = ENEMY + "Monster2";
    public static string PLANET_MONSTER1 = ENEMY + "PlanetMonster1";
    public static string PLANET_MONSTER2 = ENEMY + "PlanetMonster2";
    public static string PLANET_MONSTER3 = ENEMY + "PlanetMonster3";
    public static string SPACE_MONSTER1 = ENEMY + "SpaceMonster1";
    public static string SPACE_MONSTER2 = ENEMY + "SpaceMonster2";
    public static string INSIDE_MONSTER = ENEMY + "PentrationMonsterInside";
    public static string PENTRATION_MONSTER = ENEMY + "PentrationMonsterSpace";

    public static string EFFECT = ENEMY + "EFFECT";

    public static string DAMAGE_POINT = PREFABS + "Player/DamagePoint";

    // Characters
    public static string CHARACTER1 = PREFABS + "Player/Characters/Player_CHAR1";
    public static string CHARACTER2 = PREFABS + "Player/Characters/Player_CHAR2";
    public static string CHARACTER3 = PREFABS + "Player/Characters/Player_CHAR3";

}
