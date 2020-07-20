using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Tim.Scripts.Login_System;

public class PlayerInfo : MonoBehaviour
{
    [HideInInspector]
    public PlayerProfileModel profile;

    public static PlayerInfo instance;

    private void Awake()
    {
        instance = this;
    }

    public void OnLoggedIn()
    {
        GetPlayerProfileRequest getPlayerProfileRequest = new GetPlayerProfileRequest
        {
            PlayFabId = LogInRegister.Instance.playFabId,
            ProfileConstraints =  new PlayerProfileViewConstraints
            {
                ShowDisplayName = true
            }
        };
        
        PlayFabClientAPI.GetPlayerProfile(getPlayerProfileRequest,
            result =>
            {
                profile = result.PlayerProfile;
                Debug.Log("Loaded in player: " + profile.DisplayName);
            },
            error =>
            {
                Debug.Log(error.ErrorMessage);
            }
        );
        
    }
}