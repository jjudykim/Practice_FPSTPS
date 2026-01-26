using System;
using UnityEngine;


[Serializable]
public class UserData
{
    public int saveVersion = 1;
    public string lastSaveIsoUtc = "";

    public int playerLevel = 1;
    public int gold = 0;

    public int chapterIndex = 0;

    public bool tutorialCompleted = false;

    public string equippedWeaponId = "";
    public string equippedSkinId = "";

    public static UserData CreateDefault()
    {
        return new UserData
        {
            saveVersion = 1,
            lastSaveIsoUtc = "",
            playerLevel = 1,
            gold = 0,
            chapterIndex = 0,
            tutorialCompleted = false,
            equippedSkinId = "",
            equippedWeaponId = ""
        };
    }

    public void MarkSavedNowUtc()
    {
        lastSaveIsoUtc = DateTime.Now.ToString("O");
    }
}