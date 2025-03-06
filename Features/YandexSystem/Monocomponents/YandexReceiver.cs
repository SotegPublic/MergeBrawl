using Commands;
using Cysharp.Threading.Tasks;
using HECSFramework.Unity;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class YandexReceiver : MonoBehaviour, IHaveActor
{

    public Actor Actor { get; set; }

    [DllImport("__Internal")]
    private static extern int CheckYandexSDK();

    [DllImport("__Internal")]
    private static extern void CheckAuth();

    [DllImport("__Internal")]
    private static extern void SetLeaderboardInfo(string leaderbordID, int value);

    [DllImport("__Internal")]
    private static extern string GetLanguage();

    [DllImport("__Internal")]
    private static extern string GetDeviceType();

    [DllImport("__Internal")]
    private static extern string GetPlayerName();

    [DllImport("__Internal")]
    private static extern string GetPlayerID();

    [DllImport("__Internal")]
    private static extern void SendReady();

    [DllImport("__Internal")]
    private static extern int YaAuth();

    [DllImport("__Internal")]
    private static extern void RateGame();

    [DllImport("__Internal")]
    private static extern IntPtr GetServerTime();

    [DllImport("__Internal")]
    private static extern void ShowAdv();

    [DllImport("__Internal")]
    private static extern void ShowRewardedAdv(int rewardID);
    [DllImport("__Internal")]
    private static extern void Debug(string text);
    [DllImport("__Internal")]
    private static extern void GetLeaderBoardInfo(string lbName, int topQuantity, bool isUserIncluded, int aroundQuantity);


    public void YandexDebug(string text)
    {
#if !UNITY_EDITOR
        Debug("Application send: " + text);
#else
        UnityEngine.Debug.LogWarning("Application send: " + text);
#endif
    }

    public bool IsYandexInit()
    {
#if !UNITY_EDITOR
        var result = CheckYandexSDK();

        return result == 1 ? true : false;
#else
        return true;
#endif
    }

    public void CheckAuthentificated()
    {
#if !UNITY_EDITOR
        CheckAuth();
#else
        AfterAuthCheckCallback(1);
#endif
    }

    public void CallAuthentification()
    {
#if !UNITY_EDITOR
        YaAuth();
#else
        AuthCallback(0);
#endif
    }

    public string GetLang()
    {
#if !UNITY_EDITOR
        return GetLanguage();
#else
        return "ru";
#endif
    }

    public string GetDevice()
    {
#if !UNITY_EDITOR
        return GetDeviceType();
#else
        return "desktop";
#endif
    }

    public string GetName()
    {
#if !UNITY_EDITOR
        return GetPlayerName();
#else
        return "SupaPlayer";
#endif
    }

    public string GetUID()
    {
#if !UNITY_EDITOR
        return GetPlayerID();
#else
        return "10101";
#endif
    }

    public DateTime GetTime()
    {
#if !UNITY_EDITOR
        IntPtr serverTimePtr = GetServerTime();
        string serverTimeStr = Marshal.PtrToStringAuto(serverTimePtr);

        YandexDebug(serverTimeStr);

        if (long.TryParse(serverTimeStr, out long serverTime))
        {
            DateTime start = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            DateTime date = start.AddMilliseconds(serverTime).ToLocalTime();
            return date;
        }
#endif
        return DateTime.Now;
    }

    public void SetLeaderBoardValue(string leaderboardID, int value)
    {
#if !UNITY_EDITOR
        SetLeaderboardInfo(leaderboardID, value);
#endif
    }

    public void UpdateLeaderBoard(string lbName, int topQuantity, bool isUserIncluded, int aroundQuantity) 
    {
#if !UNITY_EDITOR
        GetLeaderBoardInfo(lbName, topQuantity, isUserIncluded, aroundQuantity);
#else
        if(TryLoadFromFile(out var lbJson))
        {
            GetLBInfoCallback(lbJson);
        }
#endif
    }

    public void SendGameReady()
    {
#if !UNITY_EDITOR
        SendReady();
#endif
    }

    public void SendRateRequest()
    {
#if !UNITY_EDITOR
        RateGame();
#endif
    }

    public void ShowAdvertising()
    {
        Actor.Entity.World.Command(new ShowAdvCommand());
        Actor.Entity.World.Command(new SendAdvShowEventCommand());

#if !UNITY_EDITOR
        ShowAdv();
#else
        ShowAdvCallback();
#endif
    }

    /// <summary>
    /// Launch rewarded advertising
    /// </summary>
    /// <param name="rewardID"></param>
    public void ShowRewardedAdvertising(int rewardID)
    {
        Actor.Entity.World.Command(new ShowAdvCommand());
        Actor.Entity.World.Command(new SendRewardedAdvShowEventCommand { RewardID = rewardID });

#if !UNITY_EDITOR
        ShowRewardedAdv(rewardID);
#else
        GetAdvRewardCallback(rewardID);
        ShowRewardedAdvCallback(rewardID);
#endif
    }

    public void AuthCallback(int value)
    {
        var result = value == 1 ? true : false;

        Actor.Entity.World.Command(new AuthResultCommand { IsAuthenticated = result });
    }

    /// <summary>
    /// Event about user closed common advertising
    /// Listen AdvClosedCommand
    /// </summary>
    public void ShowAdvCallback()
    {
        Actor.Entity.World.Command(new AdvClosedCommand());
    }

    /// <summary>
    /// Event about user closed rewarded advertising
    /// Listen RewardedAdvClosedCommand
    /// </summary>
    /// <param name="rewardID"></param>
    public void ShowRewardedAdvCallback(int rewardID)
    {
        Actor.Entity.World.Command(new RewardedAdvClosedCommand { RewardID = rewardID});
    }

    /// <summary>
    /// Event about full watching timer of rewarded advertising complete
    /// Listen GetAdvRewardCommand
    /// </summary>
    /// <param name="rewardID"></param>
    public void GetAdvRewardCallback(int rewardID)
    {
        Actor.Entity.World.Command(new GetAdvRewardCommand { RewardID = rewardID });
    }

    public void LoadExternCallback(string jsonData)
    {
        YandexDebug("We get json");

        Actor.Entity.World.Command(new LoadExternCallbackCommand { JsonData = jsonData });
    }

    public void AfterAuthCheckCallback(int result)
    {
        var isAuth = result == 1 ? true : false;

        Actor.Entity.World.Command(new AuthCheckResultCommand {  IsAuthenticated = isAuth });
    }

    public void GetLBInfoCallback(string lbJson)
    {
        YandexDebug(lbJson);
        Actor.Entity.World.Command(new UpdateLBCommand { LbJSON = lbJson });
    }

    private bool TryLoadFromFile(out string json)
    {
        string fileName = "lbData";
        TextAsset textAsset = Resources.Load<TextAsset>(fileName);


        if (textAsset != null)
        {
            json = textAsset.text;
            return true;
        }
        else
        {
            json = null;
            return false;
        }
    }
}

