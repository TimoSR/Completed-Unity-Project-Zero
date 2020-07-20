using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.Events;


namespace Tim.Scripts.Login_System
{
    
    public class LogInRegister : MonoBehaviourPun
    {
        #region Object Instantiator Singleton

        public static LogInRegister Instance;
        
        private void Awake()
        {
            
            Instance = this;
            
        }
        
        #endregion
        
        #region PlayFab ID handling Var

        [HideInInspector]
        public string playFabId;
        private string _playFabPlayerIdCache;

        #endregion

        #region Input Handling Var

        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;

        #endregion

        #region Login Events Handlers Var

        public TextMeshProUGUI displayText;

        public UnityEvent onLoggedIn;

        #endregion
        
        #region Login & Register Buttons

        public void OnLoginButton()
        {
            Login();
        }
        
        public void OnRegisterButton()
        {
            Register();
        }

        #endregion

        #region Account Registration

        private void Register()
        {
            //The Structure class for readying the API request. 

            RegisterPlayFabUserRequest registerRequest = new RegisterPlayFabUserRequest
            {
                Username = usernameInput.text,
                DisplayName = usernameInput.text,
                Password = passwordInput.text,
                RequireBothUsernameAndEmail = false
            };

            //The actual request. With tests testing if it was a success or not. 

            PlayFabClientAPI.RegisterPlayFabUser(registerRequest,
                result =>
                {
                    
                    SetDisplayText($"Registered a new account: {usernameInput.text} as {result.PlayFabId}", Color.green);
                },
                error =>
                {
                    SetDisplayText(error.ErrorMessage, Color.red);
                    OnPlayFabError(error);
                });
        } 

        #endregion

        #region Playfab Login & Photon Token Authentication

        /*
         * Step 1
         * We authenticate current PlayFab user normally.
         * In this case we use LoginWithCustomID API call for simplicity.
         * You can absolutely use any Login method you want.
         * We use PlayFabSettings.DeviceUniqueIdentifier as our custom ID.
         * We pass RequestPhotonToken as a callback to be our next step, if
         * authentication was successful.
         */
        private void Login()
        {
            LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest()
            {
                Username = usernameInput.text,
                Password = passwordInput.text
            };

            PlayFabClientAPI.LoginWithPlayFab(loginRequest, 
                result =>
                {
                    SetDisplayText($"Logged in as: {result.PlayFabId}", Color.green);
                    playFabId = result.PlayFabId;
                    //onLoggedIn?.Invoke();
                    RequestPhotonToken(result);
                    //Launcher.ConnectToPhoton();
                    PhotonNetwork.LoadLevel(1);
                },
                error =>
                {
                    SetDisplayText($"{error.ErrorMessage}", Color.red);
                    OnPlayFabError(error);
                }
            );
            
        }

        /*
        * Step 2
        * We request Photon authentication token from PlayFab.
        * This is a crucial step, because Photon uses different authentication tokens
        * than PlayFab. Thus, you cannot directly use PlayFab SessionTicket and
        * you need to explicitly request a token. This API call requires you to
        * pass Photon App ID. App ID may be hard coded, but, in this example,
        * We are accessing it using convenient static field on PhotonNetwork class
        * We pass in AuthenticateWithPhoton as a callback to be our next step, if
        * we have acquired token successfully
        */
        private void RequestPhotonToken(LoginResult obj) {
            
            LogMessage("PlayFab authenticated. Requesting photon token...");
            //We can player PlayFabId. This will come in handy during next step
            _playFabPlayerIdCache = obj.PlayFabId;

            PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest()
            {
                PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime,
            }, AuthenticateWithPhoton, OnPlayFabError);
        }
        
        /*
         * Step 3
         * This is the final and the simplest step. We create new AuthenticationValues instance.
         * This class describes how to authenticate a players inside Photon environment.
         * https://doc.photonengine.com/en-us/realtime/current/reference/playfab
         */
        private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj) {
            LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");

            //We set AuthType to custom, meaning we bring our own, PlayFab authentication procedure.
            var customAuth = new AuthenticationValues { AuthType = CustomAuthenticationType.Custom };
            //We add "username" parameter. Do not let it confuse you: PlayFab is expecting this parameter to contain player PlayFab ID (!) and not username.
            customAuth.AddAuthParameter("username", _playFabPlayerIdCache);    // expected by PlayFab custom auth service

            //We add "token" parameter. PlayFab expects it to contain Photon Authentication Token issues to your during previous step.
            customAuth.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);

            //We finally tell Photon to use this authentication parameters throughout the entire application.
            PhotonNetwork.AuthValues = customAuth;
        }

        #endregion

        #region Log/Text Methods

        void SetDisplayText(string text, Color color)
        {
            displayText.text = text;
            displayText.color = color;
        }

        private void OnPlayFabError(PlayFabError obj)
        {
            LogMessage(obj.GenerateErrorReport());
        }

        private void LogMessage(string message)
        {
            Debug.Log($"Playfab + Photon: {message}");
        }

        #endregion
        
    }

}