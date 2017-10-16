﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This namespace contains all of the classes that handle loading and uploading data via the Fizzyo API, calibration
/// and breath framework
/// </summary>
namespace Fizzyo
{
    public enum FizzyoRequestReturnType { SUCCESS, INCORRECT_TOKEN, FAILED_TO_CONNECT }

    ///Fizzyo
    /// <summary>
    /// Interface class for Fizzyo Framework
    /// </summary>
    public class FizzyoFramework : MonoBehaviour
    {
        [Header("Script Behaviour")]
        [Tooltip("Automatically show login screen at start of game.")]
        public bool showLoginAutomatically = false;

        [Tooltip("Automatically show gamer tag editor if user does not have this set.")]
        public bool showSetGamerTagAutomatically = false;

        [Tooltip("Automatically show calibration screen if never calibratd by user.")]
        public bool showCalibrateAutomatically = false;

        [Tooltip("Game ID given by API.")]
        public string gameID;

        [Header("Test Harness")]

        [Tooltip("Use test harness data.")]
        public bool useTestHarnessData = false;

        //Use test harness instead of live data
        public enum TestHarnessData { p1_acapella, p1_pep, p2_acapella };
        public TestHarnessData testHarnessDataFile = TestHarnessData.p1_acapella;

        public static FizzyoFramework _instance = null;
        public FizzyoUser User { get; set; }
        public FizzyoDevice Device { get; set; }
        public FizzyoAchievments Achievments { get; set; }
        public BreathRecogniser Recogniser { get; set; }
  
        //Singleton instance
        public static FizzyoFramework Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FizzyoFramework();
                }
                return _instance;
            }
        }

        public Scene CallbackScene { get; private set; }

        private FizzyoFramework()
        {
            User = new FizzyoUser();
            Device = new FizzyoDevice();
            Recogniser = new BreathRecogniser();
            Achievments = new FizzyoAchievments();
        }

        void Awake()
        {
           DontDestroyOnLoad(gameObject);
        }

        void Start()
        {

            //handle if FizzyoFramework is attached to scene.
            if (_instance == null)
            {
                _instance = this;
            }

            Load();

            if (showCalibrateAutomatically)
            {
               //TODO: callup calibration here. 
            }

            if (useTestHarnessData)
            {
                Device.StartPreRecordedData("Fizzyo/Data/" + testHarnessDataFile.ToString() + ".fiz");
            }


            if (showCalibrateAutomatically && !Device.Calibrated)
            {
                Scene scene = SceneManager.GetActiveScene();
                CallbackScene = scene;
                SceneManager.LoadScene("Fizzyo/Scenes/Calibration");
            }



        }


        private void Update()
        {
            //update the breath recoginiser
            Recogniser.AddSample(Time.deltaTime,Device.Pressure());
        }


        /// <summary>
        /// Loads the user data from the Fizzyo API
        /// PlayerPrefs holds the users information in the following configuration:
        /// "online" - Integer - 0 or 1 - Tells the developer if the user is playing offline or online 
        /// "calDone" - Integer - 0 or 1 - Tells the developer if the user has completed calibration 
        /// "achievments" - String - Holds the achievements for the game and, if the user is online, tells the developer
        /// which achievements have been unlocked 
        /// "achievmentsToUpload" - String - Holds the achievements that have been unlocked in the current session
        /// Current players userID + "AchievementProgress" - String - Holds data on the achievement progress that the user has made in this game 
        /// "accessToken" - String - Holds the access token that is aquired for the current user
        /// "tagDone" - Integer - 0 or 1 - Tells the developer if the user has completed setting a tag
        /// "userTag" - String - Holds the user tag
        /// "calPressure" - Float - Holds the pressure that the user has set in their calibration
        /// "calTime" - Integer - Holds the breath length that the user has set in their calibration
        /// "userId" - String - Holds the user Id that is aquired for the current user
        /// "gameId" - String - Holds the game Id for this specific game
        /// "gameSecret" - String - Holds the game secret for this specific game
        /// "userLoaded" - Integer - 0 or 1 - Shows if the users access token was loaded
        /// "calLoaded" - Integer - 0 or 1 - Shows if the users calibration data was loaded
        /// "achLoaded" - Integer - 0 or 1 - Shows if the users achievement data was loaded
        /// "tagLoaded" - Integer - 0 or 1 - Shows if the users tag was loaded
        /// </summary>
        /// <param name="gameId"> 
        /// String that contains the current games ID
        /// </param>  
        /// <param name="gameSecret"> 
        /// String that contains the current games secret
        /// </param>  
        /// <returns>
        /// true if data is loaded and playing online
        /// false if daa is not loaded and playing offline
        /// </returns>  
        public bool Load()
            {
            //Login to server
            LoginReturnType loginResult =  User.Login();

            if (loginResult != LoginReturnType.SUCCESS)
            {
                PlayOffline();
                return false;
            }

            User.Load();
            Achievments.Load();

         


            return true;
        }




        /// <summary>
        /// Sets up the player preferences to allow the user to play offline
        /// </summary>
        private static void PlayOffline()
            {
                ResetPlayerPrefs();
            }

            /// <summary>
            /// Resets all of the player preferences
            /// </summary>
            private static void ResetPlayerPrefs()
            {
                PlayerPrefs.SetInt("online", 0);
                PlayerPrefs.SetInt("calDone", 0);
                PlayerPrefs.SetString("achievementsToUpload", "");
                PlayerPrefs.SetString("achievementsToProgress", "");

                PlayerPrefs.DeleteKey("accessToken");
                PlayerPrefs.DeleteKey("tagDone");
                PlayerPrefs.DeleteKey("userTag");
                PlayerPrefs.DeleteKey("calPressure");
                PlayerPrefs.DeleteKey("calTime");
                PlayerPrefs.DeleteKey("userId");
                PlayerPrefs.DeleteKey("gameId");
                PlayerPrefs.DeleteKey("gameSecret");
                PlayerPrefs.DeleteKey("userLoaded");
                PlayerPrefs.DeleteKey("calLoaded");
                PlayerPrefs.DeleteKey("achLoaded");
                PlayerPrefs.DeleteKey("tagLoaded"); 
            }

    
    



           
    }


}
