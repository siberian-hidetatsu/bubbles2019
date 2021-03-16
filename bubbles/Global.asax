<%@ Application Language="C#" %>

<script runat="server">

    // Global.aspx.cs を使わない場合は、ASP.global_asax を変数名の頭に付ける。
    //【解決済み】Visual Studio 2005 で static 変数 (ASP.NET) http://blogs.wankuma.com/gshell/archive/2007/07/11/84753.aspx
    
    public static string bubblesHostNameRemote;
    public static string shenlongDocumentsFolder;
    //public static string dbsv01DefaultPwd;
    public static string[] devGroupUsers = null;
    public static int maxRowNum;
    public static string[] writeLogDsnUidPwd = { null, null, null };
    public static string[] outputType = null;
    public static char[,] replaceForWebAppChars = null;
    public static int excelGetBorder;

    //public const string debugPcName = "appserver";
    public static string debugPcName = "localhost";
    
    void Application_Start(object sender, EventArgs e) 
    {
        // アプリケーションのスタートアップで実行するコードです
        if ( ConfigurationManager.AppSettings["debugPcName"] != null )
        {
            debugPcName = ConfigurationManager.AppSettings["debugPcName"];
        }

        bubblesHostNameRemote = ConfigurationManager.AppSettings["BubblesHostNameRemote"];

        shenlongDocumentsFolder = ConfigurationManager.AppSettings["ShenlongDocumentsFolder"];

        //dbsv01DefaultPwd = ConfigurationManager.AppSettings["dbsv01DefaultPwd"];

        if ( ConfigurationManager.AppSettings["devGroupUsers"] != null )
        {
            devGroupUsers = ConfigurationManager.AppSettings["devGroupUsers"].Split(',');
        }

        maxRowNum = 5000;
        if ( ConfigurationManager.AppSettings["maxRowNum"] != null )
        {
            maxRowNum = int.Parse(ConfigurationManager.AppSettings["maxRowNum"]);
        }

        if ( ConfigurationManager.AppSettings["writeLogDsnUidPwd"] != null )
        {
            writeLogDsnUidPwd = ConfigurationManager.AppSettings["writeLogDsnUidPwd"].Split(',');
        }
        else
        {
#if false
            writeLogDsnUidPwd[0] = "sid";
            writeLogDsnUidPwd[1] = "uid";
            writeLogDsnUidPwd[2] = "pwd";
#endif            
        }

        if ( ConfigurationManager.AppSettings["outputType"] != null )
        {
            outputType = ConfigurationManager.AppSettings["outputType"].Split(',');
        }

        string _replaceForWebAppChars = "-―.． □";
        if ( ConfigurationManager.AppSettings["replaceForWebAppChars"] != null )
        {
            _replaceForWebAppChars = ConfigurationManager.AppSettings["replaceForWebAppChars"];
        }
        if ( _replaceForWebAppChars.Length % 2 == 0 )
        {
            replaceForWebAppChars = new char[_replaceForWebAppChars.Length / 2, 2];
            for ( int i = 0; i < _replaceForWebAppChars.Length / 2; i++ )
            {
                replaceForWebAppChars[i, 0] = _replaceForWebAppChars[i * 2 + 0];
                replaceForWebAppChars[i, 1] = _replaceForWebAppChars[i * 2 + 1];
            }
        }

        excelGetBorder = 2031;
        if ( ConfigurationManager.AppSettings["excelGetBorder"] != null )
        {
            excelGetBorder = int.Parse(ConfigurationManager.AppSettings["excelGetBorder"]);
        }
    }
    
    void Application_End(object sender, EventArgs e) 
    {
        //  アプリケーションのシャットダウンで実行するコードです

    }
        
    void Application_Error(object sender, EventArgs e) 
    { 
        // ハンドルされていないエラーが発生したときに実行するコードです

    }

    void Session_Start(object sender, EventArgs e) 
    {
        // 新規セッションを開始したときに実行するコードです

    }

    void Session_End(object sender, EventArgs e) 
    {
        // セッションが終了したときに実行するコードです 
        // メモ: Web.config ファイル内で sessionstate モードが InProc に設定されているときのみ、
        // Session_End イベントが発生します。session モードが StateServer か、または SQLServer に 
        // 設定されている場合、イベントは発生しません。

        GC.Collect();
    }
       
</script>
