using System.Collections;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public TMP_Text Id;
  
    public async void StartGame()
    {
        await UnityServices.InitializeAsync();
        await AnonymousSignin();
        var playerId = AuthenticationService.Instance.PlayerId;
        Debug.Log("player id :" + playerId);
        Id.SetText(playerId);
    }

    private async Task AnonymousSignin()
    {
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("loggedIn susccesfully");
        }
        catch (Unity.Services.Authentication.AuthenticationException AuthException)
        {
            Debug.LogError(AuthException.Message);
        }
    }
}
