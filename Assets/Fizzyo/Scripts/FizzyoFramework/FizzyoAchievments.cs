﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Fizzyo
{
    public class FizzyoAchievments : MonoBehaviour
    {


        /// <summary>
        /// Loads in the users unlocked achievements and achievement progress
        /// </summary>
        private static void GetUnlockedAchievements()
        {

            PlayerPrefs.SetString("achievementsToUpload", "");

            string getUnlock = "https://api.fizzyo-ucl.co.uk/api/v1/users/" + PlayerPrefs.GetString("userId") + "/unlocked-achievements/" + PlayerPrefs.GetString("gameId");

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
            WWW sendGetUnlock = new WWW(getUnlock, null, headers);

            while (!sendGetUnlock.isDone) { }

            if (sendGetUnlock.error != null)
            {
                PlayerPrefs.SetInt("achLoaded", 0);
                return;
            }

            string unlockedJSONData = sendGetUnlock.text;
            AllAchievementData allUnlocked = JsonUtility.FromJson<AllAchievementData>(unlockedJSONData);

            AllAchievementData allAchievements = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString("achievements"));

            string progress = PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress");

            if (progress == "" || progress == null)
            {
                PlayerPrefs.SetString(PlayerPrefs.GetString("userId") + "AchievementProgress", PlayerPrefs.GetString("achievements"));
                progress = PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress");
            }

            Debug.Log(progress);

            AllAchievementData allUserProgress = JsonUtility.FromJson<AllAchievementData>(progress);

            for (int i = 0; i < allUnlocked.unlockedAchievements.Length; i++)
            {

                for (int j = 0; j < allAchievements.achievements.Length; j++)
                {

                    if (allUnlocked.unlockedAchievements[i].id == allAchievements.achievements[j].id)
                    {
                        allAchievements.achievements[j].unlock = 1;
                    }

                }

            }

            for (int j = 0; j < allAchievements.achievements.Length; j++)
            {

                for (int k = 0; k < allUserProgress.achievements.Length; k++)
                {

                    if (allUserProgress.achievements[k].id == allAchievements.achievements[j].id)
                    {
                        allAchievements.achievements[j].unlockProgress = allUserProgress.achievements[k].unlockProgress;

                    }

                }

            }

            // string allJSONUserAchievementProgress = PlayerPrefs.GetString("achievementProgress");
            // AllAchievementData allUserAchievementProgress = JsonUtility.FromJson<AllAchievementData>(allJSONUserAchievementProgress);

            Debug.Log(PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress"));

            string newAllData = JsonUtility.ToJson(allAchievements);
            PlayerPrefs.SetString("achievements", newAllData);
            PlayerPrefs.SetInt("achLoaded", 1);

        }


        /// <summary>
        /// Loads in the top 20 highscores for the current game
        /// </summary>
        /// <returns>
        /// A JSON formatted string containing tag and score for the top 20 unlocked achievements
        /// </returns> 
        public static string GetHighscores()
        {

            if (PlayerPrefs.GetInt("online") == 0)
            {
                return "Highscore Load Failed";
            }

            string getHighscores = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/highscores";

            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accessToken"));
            WWW sendGetHighscores = new WWW(getHighscores, null, headers);

            while (!sendGetHighscores.isDone) { }

            if (sendGetHighscores.error != null)
            {
                return "Highscore Load Failed";
            }

            return sendGetHighscores.text;
        }




        /// <summary>
        /// Uploads a players Score
        /// </summary>
        /// <returns>
        /// String - "High Score Upload Complete" - If upload completes  
        /// String - "High Score Upload Failed" - If upload fails
        /// </returns>
        public static string Score(int score)
        {
            if (PlayerPrefs.GetInt("online") == 0)
            {
                return "Score Upload Failed";
            }

            string uploadScore = "https://api.fizzyo-ucl.co.uk/api/v1/games/" + PlayerPrefs.GetString("gameId") + "/highscores";

            WWWForm form = new WWWForm();
            form.AddField("gameSecret", PlayerPrefs.GetString("gameSecret"));
            form.AddField("userId", PlayerPrefs.GetString("userId"));
            form.AddField("score", score);
            Dictionary<string, string> headers = form.headers;
            headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

            byte[] rawData = form.data;

            WWW sendPostUnlock = new WWW(uploadScore, rawData, headers);

            while (!sendPostUnlock.isDone) { };

            if (sendPostUnlock.error != null)
            {
                return "Score Upload Failed";
            }

            return "Score Upload Complete";
        }


        /// <summary>
        /// Uploads a players achievements for a session
        /// </summary>
        /// <returns>
        /// String - "Achievement Upload Complete" - If upload completes  
        /// String - "Achievement Upload Failed" - If upload fails
        /// </returns>
        private static string Achievements()
        {
            string achievementsToUpload = PlayerPrefs.GetString("achievementsToUpload");

            if (achievementsToUpload != "")
            {

                string[] achievementsToUploadArray = achievementsToUpload.Split(',');

                for (int i = 0; i < achievementsToUploadArray.Length; i++)
                {

                    if (achievementsToUploadArray[i] != "")
                    {

                        string postUnlock;

                        postUnlock = "https://api.fizzyo-ucl.co.uk/api/v1/game/" + PlayerPrefs.GetString("gameId") + "/achievements/" + achievementsToUploadArray[i] + "/unlock";

                        WWWForm form = new WWWForm();

                        form.AddField("gameSecret", PlayerPrefs.GetString("gameSecret"));
                        form.AddField("userId", PlayerPrefs.GetString("userId"));

                        Dictionary<string, string> headers = form.headers;
                        headers["Authorization"] = "Bearer " + PlayerPrefs.GetString("accessToken");

                        byte[] rawData = form.data;

                        WWW sendPostUnlock = new WWW(postUnlock, rawData, headers);

                        while (!sendPostUnlock.isDone) { }

                        if (sendPostUnlock.error != null)
                        {
                            return Environment.NewLine + "Achievement Upload Failed";
                        }

                    }

                }

            }

            string achievementsToProgress = PlayerPrefs.GetString("achievementsToProgress");

            string[] achievementsToProgressArray = achievementsToProgress.Split(',');

            AllAchievementData allUserProgress = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString(PlayerPrefs.GetString("userId") + "AchievementProgress"));
            AllAchievementData allAchievements = JsonUtility.FromJson<AllAchievementData>(PlayerPrefs.GetString("achievements"));

            // Add achievement progress to player preferences
            for (int i = 0; i < achievementsToProgressArray.Length; i++)
            {

                if (achievementsToProgressArray[i] != "")
                {

                    for (int j = 0; j < allUserProgress.achievements.Length; j++)
                    {

                        if (allUserProgress.achievements[j].id == achievementsToProgressArray[i])
                        {
                            for (int k = 0; k < allAchievements.achievements.Length; k++)
                            {

                                if (allUserProgress.achievements[j].id == allAchievements.achievements[k].id)
                                {
                                    allUserProgress.achievements[j].unlockProgress = allAchievements.achievements[k].unlockProgress;
                                    string newAllData = JsonUtility.ToJson(allUserProgress);
                                    PlayerPrefs.SetString(PlayerPrefs.GetString("userId") + "AchievementProgress", newAllData);
                                    break;
                                }

                            }

                            break;
                        }

                    }

                }

            }

            PlayerPrefs.SetString("achievementsToUpload", "");
            PlayerPrefs.SetString("achievementsToProgress", "");
            return Environment.NewLine + "Achievement Upload Complete";

        }



    }

    // Serializable which holds high score data
    [System.Serializable]
    public class AllHighscoreData
    {
        public HighscoreData[] highscores;

    }

    // Serializable which holds individual high score data
    [System.Serializable]
    public class HighscoreData
    {
        public string tag;
        public int score;
        public bool belongsToUser;
    }

    // Serializable which holds achievement data
    [System.Serializable]
    public class AllAchievementData
    {
        public AchievementData[] achievements;
        public AchievementData[] unlockedAchievements;
    }

    // Serializable that is used to pull and hold the data of each Achievement in the Achievements.json file
    [System.Serializable]
    public class AchievementData
    {
        public string category;
        public string id;
        public string title;
        public string description;
        public int points;
        public int unlock;
        public int unlockProgress;
        public int unlockRequirement;
        public string dependency;
        public string unlockedOn;
    }

    // Serializable which holds calibration data
    [System.Serializable]
    public class CalibrationData
    {
        public string calibratedOn;
        public float pressure;
        public int time;
    }



}

