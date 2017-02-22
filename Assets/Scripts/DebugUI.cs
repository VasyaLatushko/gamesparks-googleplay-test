using System;
using UnityEngine;
using System.Collections;
using System.Text;
using GameSparks.Api.Requests;
using GameSparks.Core;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    [SerializeField] private Button _authBtn;
    [SerializeField] private Button _getAuthServerCodeBtn;
    [SerializeField] private Button _gsGpConnectRequestBtn;
    [SerializeField] private Text _logTxt;

    private StringBuilder _logStringBuilder;
    private string _authServerCode;

	// Use this for initialization
	void Start ()
	{
	    _logStringBuilder = new StringBuilder();
        Application.logMessageReceivedThreaded +=
            (condition, trace, type) =>
                LogMsg(string.Format("Condition: {0}, Trace: {1}, Type: {2}", condition, trace, type));
        _getAuthServerCodeBtn.interactable = false;
        _gsGpConnectRequestBtn.interactable = false;
        LogMsg(string.Format("Internet is available: {0}", Application.internetReachability));
        GS.GameSparksAvailable += OnGameSparksBecameAvailable;
        GS.GameSparksAuthenticated += OnGameSparksAuthenticated;
	    PlayGamesPlatform.Activate();
	}

    private void OnGameSparksAuthenticated(string playerId)
    {
        LogMsg("Player with ID: " + playerId + " was successfully authenticated");
    }

    private void OnGameSparksBecameAvailable(bool available)
    {
        if (!available)
        {
            LogMsg("GS couldn't have been connected");
            return;
        }
        new DeviceAuthenticationRequest()
            .Send(response =>
            {
                if (!response.HasErrors)
                    _gsGpConnectRequestBtn.interactable = true;
            });
    }

    // Update is called once per frame
	void Update () {
	    if(Input.GetKeyDown(KeyCode.P))
            LogMsg(DateTime.Now);
	}

    public void OnAuthenticateBtnClickHandler()
    {
        //PlayGamesPlatform.Instance.Authenticate(OnAuthenticatedCallbackHandler);
        Social.localUser.Authenticate(OnAuthenticatedCallbackHandler);
    }

    public void OnGetServerAuthCodeBtnClickHandler()
    {
        PlayGamesPlatform.Instance.GetServerAuthCode(OnGetServerAuthCodeCallbackHandler);
    }

    public void OnGetAccessTokenBtnClickHandler()
    {
        LogMsg(string.Format("GetAccessToken: {0}", PlayGamesPlatform.Instance.GetAccessToken()));
    }

    public void OnGetIdTokenBtnClickHandler()
    {
        PlayGamesPlatform.Instance.GetIdToken(s =>
        {
            LogMsg(string.Format("GetIdTokenResponse: {0}", s));
        });
    }

    public void OnGetTokenBtnClickHandler()
    {
        LogMsg(string.Format("GetToken: {0}", PlayGamesPlatform.Instance.GetToken()));
    }

    public void OnClearLogsBtnClickHandler()
    {
        _logStringBuilder = new StringBuilder();
        _logTxt.text = _logStringBuilder.ToString();
    }

    public void OnGooglePlayConnectGSBtnClickHandler()
    {
        if (!GS.Available || !GS.Authenticated)
        {
            LogMsg("GS is either unavailable or unauthenticated");
            return;
        }

        new GooglePlayConnectRequest()
            .SetAccessToken(_authServerCode)
            .SetDisplayName(PlayGamesPlatform.Instance.GetUserDisplayName())
            .SetRedirectUri("http://www.gamesparks.com/oauth2callback")
            .Send(response =>
            {
                LogMsg(string.Format("OnGooglePlayConnectResponse: hasErrors: {0}, response: {1}", response.HasErrors,
                    response.JSONString));
            });
    }

    private void OnAuthenticatedCallbackHandler(bool success/*, string str*/)
    {
        LogMsg(string.Format("OnAuthenticatedCallbackHandler(): success: {0}", success/*, str*/));
        if (success) _getAuthServerCodeBtn.interactable = true;
    }

    private void OnGetServerAuthCodeCallbackHandler(CommonStatusCodes statusCode, string authCode)
    {
        LogMsg(string.Format("OnGetServerAuthCodeCallbackHandler(statusCode: {0}, authCode: {1})", statusCode, authCode));
        _authServerCode = authCode;
    }

    private void LogMsg(object msg)
    {
        _logStringBuilder.AppendFormat("{0}: {1}\n\n", DateTime.Now, msg);
        _logTxt.text = _logStringBuilder.ToString();
    }
}
