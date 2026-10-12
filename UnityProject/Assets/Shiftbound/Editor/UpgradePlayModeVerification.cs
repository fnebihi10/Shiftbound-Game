using System;
using UnityEditor;
using UnityEngine;

// Runs the existing production input sequence in the actual Editor player loop.
// Batch entry omits -quit: the route runner supplies completion evidence.
[InitializeOnLoad]
public static class UpgradePlayModeVerification
{
    const string Active="Shiftbound.UpgradePlayCheck";
    const string Started="Shiftbound.UpgradePlayStarted";
    static int passes;
    static bool recovered,reset,failed;
    static UpgradePlayModeVerification()
    {
        if(SessionState.GetBool(Active,false))Subscribe();
    }
    public static void Run()
    {
        SliceDelivery.ValidateScene();
        SessionState.SetBool(Active,true);
        SessionState.SetString(Started,DateTime.UtcNow.ToString("O"));
        Subscribe();
        // Let startup index creation finish before requesting Play Mode. Unity
        // skips first-use Search.index creation while a play request is pending,
        // then its own startup callback tries to access an empty database list.
        // Keep the exception checks active throughout; this is startup ordering.
        EditorApplication.delayCall+=()=>EditorApplication.delayCall+=EditorApplication.EnterPlaymode;
    }
    static void Subscribe()
    {
        Application.logMessageReceived-=Log;
        Application.logMessageReceived+=Log;
        EditorApplication.update-=Tick;
        EditorApplication.update+=Tick;
    }
    static void Log(string message,string stack,LogType type)
    {
        if(message.Contains("SHIFTBOUND FULL ROUTE PASSED"))passes++;
        if(message.Contains("SHIFTBOUND RECOVERY PASS"))recovered=true;
        if(message.Contains("SHIFTBOUND RUN RESET PASS"))reset=true;
        if(type==LogType.Exception || type==LogType.Error || type==LogType.Assert)failed=true;
    }
    static void Tick()
    {
        if(!SessionState.GetBool(Active,false))return;
        bool timeout=DateTime.TryParse(SessionState.GetString(Started,""),out var start) && (DateTime.UtcNow-start.ToUniversalTime()).TotalMinutes>4;
        if(!failed && !timeout && !(passes==2 && recovered && reset))return;
        SessionState.SetBool(Active,false);
        Application.logMessageReceived-=Log;
        EditorApplication.update-=Tick;
        bool success=!failed && !timeout && passes==2 && recovered && reset;
        Debug.Log(success?"SHIFTBOUND UNITY PLAY MODE PASSED: two production-input routes, fall/checkpoint recovery, retained retry.":"SHIFTBOUND UNITY PLAY MODE FAILED: error or timeout; inspect preceding log.");
        EditorApplication.Exit(success?0:1);
    }
}
