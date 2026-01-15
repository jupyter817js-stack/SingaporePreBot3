using System.Windows.Forms;

namespace SingaporePreBot3
{
    public enum LOGLEVEL : byte
    {
        FILE = 0,
        NOTICE,
        FULL
    }
    public delegate void WriteStatusEvent(string status);
    public delegate void StopAutomationEvent(bool force);
    public delegate void BetFinishEvent();
    public delegate void BalanceUpdateEvent();
    public delegate void UpdateDataEvent();
    public delegate void WriteLogDelegate(LOGLEVEL logLevel, string strLog);
    public delegate void GetCookieEvent();
    public delegate void RefreshBrowserEvent();
    public delegate void KeepLoginEvent();
    public delegate void UpdateArbEvent();
    public delegate string GetAuthTokenEvent();

    class Global
    {
        public static WriteStatusEvent WriteStatus; 
        public static StopAutomationEvent StopAutomation;
        public static UpdateDataEvent UpdateSingapore;
        public static UpdateDataEvent UpdateSuperodd;
        public static BetFinishEvent BetFinish;
        public static BalanceUpdateEvent BalanceUpdate;
        public static GetCookieEvent GetCookie;
        public static RefreshBrowserEvent RefreshBrowser;
        public static KeepLoginEvent KeepLogin;
        public static UpdateArbEvent UpdateArb;
        public static GetAuthTokenEvent GetAuthToken;
        public static string LogDir = $"{Application.StartupPath}\\log";
        public static bool bRun = false;
    }
}
