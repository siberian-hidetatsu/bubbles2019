using System;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Diagnostics;
#if !WITHIN_SHENGLOBAL
using cc = Shenlong.ShenGlobal;
#endif

public partial class _Default : System.Web.UI.Page 
{
	/// <summary>
	/// Page_Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
    {
		LabelDebug.Text = "";
		ButtonApply.UseSubmitBehavior = false;

		// 卵のダウンロード？
		if ( IsShelongDocDownload() )
			return;

		Session.Remove(bb.pmDevelop);
		Session.Remove(bb.ssHyperLinkHome);

		string remoteUser = bb.GetRemoteUserName(Request.Params["REMOTE_USER"], Request.Params["REMOTE_ADDR"]);
		LabelDebug.Text += remoteUser;

		bool devGroupUser = bb.IsDevGroupUser(ASP.global_asax.devGroupUsers, remoteUser, User.Identity.Name);
		if ( !devGroupUser )
		{
			if ( bb.IsDevelopMode(Request.Params[bb.pmDevelop], (string)Session[bb.pmDevelop]/*null*/) )
			{
				devGroupUser = true;
				// 開発用の識別子をセッションに格納する
				Session[bb.pmDevelop] = devGroupUser.ToString();
			}
		}
		else
		{
			string develop = (Request.Params[bb.pmDevelop] != null) ? Request.Params[bb.pmDevelop] : (Session[bb.pmDevelop] != null ? (string)Session[bb.pmDevelop] : null);
			if ( (develop != null) && !bool.Parse(develop) )
			{
				devGroupUser = false;
				Session.Remove(bb.pmDevelop);
			}
		}
		LabelDebug.Text += ((devGroupUser) ? " (devGroupUser)" : "") + "<br>";

		if ( !devGroupUser )
		{
			LabelExcelGetBorder.Visible = false;
			TextExcelGetBorder.Visible = false;
			LabelExcelFrameVisible.Visible = false;
			TextExcelFrameVisible.Visible = false;
		}

		// アクセスカウンタをインクリメントする
		IncrementAccessCounter(remoteUser, devGroupUser);

		// 出力形式のラジオボタンを設定する
		RadioOutputButtonSetting();

#if false
		for ( int i = 0; i < Session.Count; i++ )
		{
			Debug.WriteLine(Session.Keys[i] + ": " + Session[i]);
		}
#endif

		try
		{
			LabelSubTitle.Text = string.Empty;

			string shenlongDocumentsFolder = ASP.global_asax.shenlongDocumentsFolder;

			if ( (Request.Params[bb.pmShenDocName] == null)/* && (Session[cc.pmShenDocName] == null)*/ )
			{
				Session.Remove(bb.pmShenDocName);

				if ( !devGroupUser )
				{
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "端末名 " + remoteUser + " はルートフォルダにアクセスする権限がありません" + "</span>");
					Response.End();
					return;
				}
			}
			else
			{
				// 間接的な pmShenDocName から実際のシェンロンの卵フォルダを取得する
				GetShenlongDocumentsFolder(ref shenlongDocumentsFolder);

				if ( Session[bb.pmShenDocName] != null )
				{
					LabelSubTitle.Text = "[" + Session[bb.pmShenDocName] + "]";
				}
			}

			LabelDebug.Text += "<br>" + shenlongDocumentsFolder + "<br>";

#if false
			HttpCookie cookie;
			string cSubDirectory = "subDirectory";
			string cHttpCacheability = "httpCacheability" + "@" + Session[bb.pmShenDocName];

			if ( !Page.IsPostBack )
			{
				string cookieSubDirectory = String.Empty;

				if ( (cookie = Request.Cookies["bubbles"]) != null )
				{
					cookieSubDirectory = HttpUtility.UrlDecode(cookie.Values[cSubDirectory]);

					string httpCacheability = cookie.Values[cHttpCacheability];
					if ( httpCacheability != null )
					{
						CheckEnableCache.Checked = (httpCacheability == HttpCacheability.Private.ToString());
						if ( devGroupUser )
						{
							CheckEnableCache.ToolTip = cHttpCacheability + "*" + "=" + httpCacheability;
						}
					}
				}

				DropDownSubDirectory.Items.Add("ＴＯＰページ");

				string[] subDirectories = Directory.GetDirectories(shenlongDocumentsFolder);
				for ( int i = 0; i < subDirectories.Length; i++ )
				{
					DirectoryInfo _directoryInfo = new DirectoryInfo(subDirectories[i]);
					if ( (_directoryInfo.GetFiles("*.xml").Length == 0) && (_directoryInfo.GetDirectories().Length == 0) )
						continue;
					if ( _directoryInfo.GetFiles("private.txt").Length != 0 )	// "private.txt" ファイルがあるフォルダはスキップする
						continue;
					string subDirName = Path.GetFileName(subDirectories[i]);
					DropDownSubDirectory.Items.Add(subDirName);
					if ( cookieSubDirectory == subDirName )
					{
						DropDownSubDirectory.SelectedIndex = DropDownSubDirectory.Items.Count - 1;
					}
				}
			}
			else
			{
				/* HttpCookie */
				if ( (cookie = Request.Cookies["bubbles"]) == null )
				{
					cookie = new HttpCookie("bubbles");
				}
				if ( cookie.Values[cSubDirectory] == null )
				{
					cookie.Values.Add(cSubDirectory, "");
				}
				if ( cookie.Values[cHttpCacheability] == null )
				{
					cookie.Values.Add(cHttpCacheability, "");
				}

				cookie.Values[cSubDirectory] = HttpUtility.UrlEncode(DropDownSubDirectory.SelectedValue);
				cookie.Values[cHttpCacheability] = (CheckEnableCache.Checked ? HttpCacheability.Private : HttpCacheability.NoCache).ToString();
				cookie.Expires = DateTime.MaxValue;
				Response.AppendCookie(cookie);

				if ( devGroupUser )
				{
					CheckEnableCache.ToolTip = cHttpCacheability + "=" + cookie.Values[cHttpCacheability];
				}
			}
#else
			HttpCookie cookie = Request.Cookies["bubbles"];

			// サブディレクトリを列挙してドロップダウン化する
			EnumSubDirectory(shenlongDocumentsFolder, ref cookie);

			// クッキーの読み書き
			CookieReadWrite(devGroupUser, ref cookie);

			// ToggleOption コントロールを設定する
			ToggleOptionControlSetting();
#endif

			// シェンロンの卵フォルダをセッションに格納する
			Session[bb.ssShenlongDocFolder] = shenlongDocumentsFolder + "\\";

			// カテゴリ（２階層目のサブディレクトリ）を列挙してドロップダウン化する
			string subDirectory, category;
			EnumCategory(shenlongDocumentsFolder, out subDirectory, out category);

			// サブディレクトリをセッションに格納する
			Session[bb.ssSubDirectory] = (subDirectory == "ＴＯＰページ") ? "" : (subDirectory + "\\");

			// サブディレクトリが選択された？
			if ( !string.IsNullOrEmpty((string)(Session[bb.ssSubDirectory])) )
			{
				shenlongDocumentsFolder += ("\\" + subDirectory);

				// カテゴリが選択された？
				if ( !string.IsNullOrEmpty(category) )
				{
					Session[bb.ssSubDirectory] += (category + "\\");	// カテゴリを追加して格納する
					shenlongDocumentsFolder += ("\\" + category);
				}

				LabelDebug.Text += shenlongDocumentsFolder + "<br>";
			}

			// シェンロンの卵を列挙してハイパーリンク化する
			EnumShenlongDocuments(shenlongDocumentsFolder, devGroupUser);
		}
		catch ( ThreadAbortException exp )
		{
			Debug.WriteLine(exp.Message);
		}
		catch ( Exception exp )
		{
			Response.Write(exp.Message);
		}
	}

	/// <summary>
	/// 卵のダウンロード？
	/// </summary>
	/// <returns></returns>
	private bool IsShelongDocDownload()
	{
		try
		{
			if ( Request.Params[bb.pmDownload] == null )
				return false;

			string shenFolder = (string)Session[bb.ssShenlongDocFolder] + (string)Session[bb.ssSubDirectory];
			string shenFileName = HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]);
			FileInfo shenlongFile = new FileInfo(shenFolder + shenFileName);

			if ( shenlongFile.LastWriteTime.Ticks.ToString() == Request.Params[bb.pmDownload] )
			{
				XmlDocument xmlShenlongColumn = new XmlDocument();
				xmlShenlongColumn.Load(shenlongFile.FullName);
				Response.AppendHeader("Content-Disposition", "attachment; filename=" + shenFileName/*HttpUtility.UrlEncode(shenFileName)*/);
				//Response.ContentEncoding = System.Text.Encoding.GetEncoding("Shift_JIS");
				Response.Write(xmlShenlongColumn.OuterXml);
				Response.End();
				return true;
			}

			return false;
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
			return false;
		}
	}

	/// <summary>
	/// アクセスカウンタをインクリメントする
	/// </summary>
	/// <param name="remoteUser"></param>
	/// <param name="devGroupUser"></param>
	private void IncrementAccessCounter(string remoteUser, bool devGroupUser)
	{
		try
		{
			HttpCookie cookie;
			bool incCounter = false;

			if ( Request.UrlReferrer != null )
			{
				string[] mapPath = Server.MapPath(".").Split('\\');
				string currentDir = mapPath[mapPath.Length - 1];
				string absolutePath = Request.UrlReferrer.AbsolutePath;
				if ( !absolutePath.StartsWith("/" + currentDir + "/") && !absolutePath.StartsWith("/bubbles/") )
				{
					incCounter = true;
				}
				LabelDebug.Text += absolutePath + " (" + currentDir + ")<br>";
			}

			if ( Request.UrlReferrer == null || incCounter )
			{
				string cookieLastVisit = null;
				if ( (cookie = Request.Cookies["bubbAccCounter"]) != null )
				{
					cookieLastVisit = cookie.Values["lastVisit"];
				}
				else
				{
					cookie = new HttpCookie("bubbAccCounter");
					cookie.Values.Add("lastVisit", DateTime.Now.ToString());
					//cookie.Expires = DateTime.Now.AddHours(1);
					cookie.Expires = DateTime.Now.AddMinutes(5);
					//cookie.Expires = DateTime.Now.AddSeconds(10);
					Response.AppendCookie(cookie);
				}

				incCounter = (cookieLastVisit == null);
				LabelDebug.Text += ((cookieLastVisit == null) ? "cookieLastVisit is null" : cookieLastVisit) + "<br>";
			}

#if false
			remoteUser = bb.GetRemoteUserName(Request.Params["REMOTE_USER"], Request.Params["REMOTE_ADDR"]);
			LabelDebug.Text += remoteUser;
			//remoteUser = remoteUser.Replace('\\', '_');

			devGroupUser = bb.IsDevGroupUser(ASP.global_asax.devGroupUsers, remoteUser, User.Identity.Name);
			if ( !devGroupUser )
			{
				if ( bb.IsDevelopMode(Request.Params[bb.pmDevelop], (string)Session[bb.pmDevelop]/*null*/) )
				{
					devGroupUser = true;
					Session[bb.pmDevelop] = devGroupUser.ToString();
				}
			}
#endif

			if ( devGroupUser )
			{
				LabelDebug.Text = LabelDebug.Text.Replace(remoteUser, User.Identity.Name + "@" + remoteUser);
				LabelDebug.Text += Request.Browser.Browser + Request.Browser.MajorVersion + Request.Browser.MinorVersionString + "@" + Request.Browser.Platform;
				string userAgent = Page.Request.UserAgent;
				int index = userAgent.IndexOf('(');
				if ( index != -1 )
				{
					userAgent = userAgent.Substring(index + 1);
					if ( userAgent.EndsWith(")") )
					{
						userAgent = userAgent.Substring(0, userAgent.Length - 1);
					}
				}
				string[] items = userAgent.Split(';');
				foreach ( string item in items )
				{
					string _item = item.Trim();
					if ( _item.StartsWith("Win", StringComparison.CurrentCultureIgnoreCase) )
					{
						LabelDebug.Text += "/" + _item;
					}
				}
				//LabelDebug.Text += "<br> " + Page.Request.UserAgent;
				LabelDebug.Text += "<br>";
			}

			string hostName = System.Net.Dns.GetHostName();
			string accessCounterPathName = ((String.Compare(ASP.global_asax.bubblesHostNameRemote.Substring(2), hostName, true) == 0) || (String.Compare(ASP.global_asax.debugPcName, hostName, true) == 0)) ? Server.MapPath(@".\App_Data\"/*@".\bin\"*/) : ASP.global_asax.bubblesHostNameRemote + @"\bubbles_App_Data\";
			LabelDebug.Text += accessCounterPathName + "<br>";
			XmlDocument accessCounterXml = GetAccessCounter(accessCounterPathName, "AccessCounter.xml", incCounter, remoteUser, devGroupUser, ref LabelDebug);
			if ( accessCounterXml != null )
			{
				LabelCounter.Text = String.Empty;
				XmlNode accessNode = accessCounterXml.SelectSingleNode("/" + "root" + "/" + "access" + "[@" + "date" + "='" + DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd") + "']");
				if ( accessNode != null )
				{
					LabelCounter.Text = "昨日:" + accessNode.Attributes["counter"].Value + "<br>";
				}
				accessNode = accessCounterXml.SelectSingleNode("/" + "root" + "/" + "access" + "[@" + "date" + "='" + DateTime.Today.ToString("yyyy/MM/dd") + "']");
				LabelCounter.Text += "今日:" + ((accessNode != null) ? accessNode.Attributes["counter"].Value : "");
				LabelCounter.Style["vertical-align"] = "super";
			}

			//if ( ((string.Compare(remoteUser, Global.debugPcName, true) == 0) || (string.Compare(remoteUser, "thinkpadt42", true) == 0)) && (accessCounterXml != null) )
			string[] showDetailAccounts = { "localhost", "thinkpadt42" };
			int a;
			for ( a = 0; a < showDetailAccounts.Length && (String.Compare(remoteUser, showDetailAccounts[a], true) != 0); a++ ) ;
			if ( (a != showDetailAccounts.Length) && (accessCounterXml != null) )
			{
				StringBuilder accessCounter = new StringBuilder();
				XmlNodeList accessNodes = accessCounterXml["root"].ChildNodes;
				for ( int i = (2 <= accessNodes.Count) ? accessNodes.Count - 2 : 0; i < accessNodes.Count; i++ )
				{
					accessCounter.Append(accessNodes[i].Attributes["date"].Value + "<br>");
					foreach ( XmlNode remoteUserNode in accessNodes[i].ChildNodes )
					{
						accessCounter.Append("　" + remoteUserNode.Name + "　" + remoteUserNode.Attributes["counter"].Value + "　" + remoteUserNode.Attributes["eachHour"].Value + "<br>");
					}
				}
				LabelDebug.Text += accessCounter.ToString();
			}
		}
		catch ( ThreadAbortException exp )
		{
			Debug.WriteLine(exp.Message);
		}
		catch ( Exception exp )
		{
			LabelDebug.Text += exp.Message + "<br>";
		}
	}

	/// <summary>
	/// 出力形式のラジオボタンを設定する
	/// </summary>
	private void RadioOutputButtonSetting()
	{
		try
		{
			if ( !Page.IsPostBack )
			{
				RadioOutputHTML.Checked = true;

				if ( ASP.global_asax.outputType == null )
					return;

				RadioButton[] radioOutputs = { RadioOutputHTML, RadioOutputExcel };

				foreach ( RadioButton radioOutput in radioOutputs )
				{
					radioOutput.Visible = (Array.IndexOf(ASP.global_asax.outputType, radioOutput.Text) != -1);
				}
			}
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
		}
	}

	/// <summary>
	/// 間接的な pmShenDocName から実際のシェンロンの卵フォルダを取得する
	/// 正常に取得できれば pmShenDocName をセッションに格納する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	private void GetShenlongDocumentsFolder(ref string shenlongDocumentsFolder)
	{
		string shenDocName = (Request.Params[bb.pmShenDocName] != null) ? Request.Params[bb.pmShenDocName] : (string)Session[bb.pmShenDocName];
		string hostName = System.Net.Dns.GetHostName();
		string appDataPathName = ((String.Compare(ASP.global_asax.bubblesHostNameRemote.Substring(2), hostName, true) == 0) ||
								  (String.Compare(ASP.global_asax.debugPcName, hostName, true) == 0)) ? Server.MapPath(@".\App_Data\"/*@".\bin\"*/) : ASP.global_asax.bubblesHostNameRemote + @"\bubbles_App_Data\";

		XmlDocument xmlShenlongDocFolder = new XmlDocument();
		xmlShenlongDocFolder.Load(appDataPathName + "ShenlongDocFolder.xml");

		string xpath = "/" + "shenlong" + "/" + "document" + "[@" + "name" + "='" + shenDocName/*Request.Params[cc.pmShenDocName]*/ + "']";
		XmlNode shenDoc = xmlShenlongDocFolder.SelectSingleNode(xpath);

		if ( shenDoc != null )
		{
			shenlongDocumentsFolder = shenDoc.Attributes["folder"].Value;
			if ( Directory.Exists(shenlongDocumentsFolder) )
			{
				// 有効な pmShenDocName をセッションに格納する
				Session[bb.pmShenDocName] = shenDocName;
			}
			else
			{
				Response.Write("<span style=\"color:Red;font-weight:bold;\">" + shenDocName/*Request.Params[cc.pmShenDocName]*/ + " で登録されているフォルダは存在しません" + "</span>");
				Response.End();
			}
		}
		else
		{
			Response.Write("<span style=\"color:Red;font-weight:bold;\">" + shenDocName/*Request.Params[cc.pmShenDocName]*/ + " は ShenlongDocFolder.xml に登録されていません" + "</span>");
			Response.End();
		}
	}

	/// <summary>
	/// サブディレクトリを列挙してドロップダウン化する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	/// <param name="cookie"></param>
	private void EnumSubDirectory(string shenlongDocumentsFolder, ref HttpCookie cookie)
	{
		string cSubDirectory = "subDirectory";

		if ( !Page.IsPostBack )
		{
			string cookieSubDirectory = String.Empty;

			if ( cookie != null )
			{
				cookieSubDirectory = HttpUtility.UrlDecode(cookie.Values[cSubDirectory]);
			}

			DropDownSubDirectory.Items.Add("ＴＯＰページ");

			string[] subDirectories = Directory.GetDirectories(shenlongDocumentsFolder);
			for ( int i = 0; i < subDirectories.Length; i++ )
			{
				DirectoryInfo _directoryInfo = new DirectoryInfo(subDirectories[i]);
				if ( (_directoryInfo.GetFiles("*.xml").Length == 0) && (_directoryInfo.GetDirectories().Length == 0) )
					continue;
				if ( _directoryInfo.GetFiles("private.txt").Length != 0 )	// "private.txt" ファイルがあるフォルダはスキップする
					continue;

				string subDirName = Path.GetFileName(subDirectories[i]);
				DropDownSubDirectory.Items.Add(subDirName);

				if ( cookieSubDirectory == subDirName )
				{
					DropDownSubDirectory.SelectedIndex = DropDownSubDirectory.Items.Count - 1;
				}
			}
		}
		else
		{
			/* HttpCookie */
			if ( cookie == null )
			{
				cookie = new HttpCookie("bubbles");
			}
			if ( cookie.Values[cSubDirectory] == null )
			{
				cookie.Values.Add(cSubDirectory, "");
			}

			cookie.Values[cSubDirectory] = HttpUtility.UrlEncode(DropDownSubDirectory.SelectedValue);
		}
	}

	/// <summary>
	/// クッキーの読み書き
	/// </summary>
	/// <param name="devGroupUser"></param>
	/// <param name="cookie"></param>
	private void CookieReadWrite(bool devGroupUser, ref HttpCookie cookie)
	{
		string cDefaultOutput = "defaultOutput" + "@" + Session[bb.pmShenDocName];
		string cHttpCacheability = "httpCacheability" + "@" + Session[bb.pmShenDocName];
		string cExcelGetBorder = "excelGetBorder";			// javascript@shenlong2.aspx.cs でのクッキー読み込み処理の変数名と合わせる
		string cExcelFrameVisible = "excelFrameVisible";	// 〃

		if ( !Page.IsPostBack )
		{
			if ( cookie != null )
			{
				string defaultOutput = cookie.Values[cDefaultOutput];
				if ( defaultOutput != null )
				{
					RadioButton[] radioOutputs = { RadioOutputHTML, RadioOutputExcel };

					foreach ( RadioButton radioOutput in radioOutputs )
					{
						if ( !radioOutput.Visible )
							continue;
						radioOutput.Checked = (defaultOutput == radioOutput.Text);
					}
				}

				string httpCacheability = cookie.Values[cHttpCacheability];
				if ( httpCacheability != null )
				{
					CheckEnableCache.Checked = (httpCacheability == HttpCacheability.Private.ToString());
					if ( devGroupUser )
					{
						CheckEnableCache.ToolTip = cHttpCacheability + "*" + "=" + httpCacheability;
					}
				}

				string excelGetBorder = cookie.Values[cExcelGetBorder];
				if ( excelGetBorder != null )
				{
					TextExcelGetBorder.Text = excelGetBorder;
				}

				string excelFrameVisible = cookie.Values[cExcelFrameVisible];
				if ( excelFrameVisible != null )
				{
					TextExcelFrameVisible.Text = excelFrameVisible;
				}
			}
		}
		else
		{
			/* HttpCookie */
			if ( cookie == null )
			{
				cookie = new HttpCookie("bubbles");
			}
			if ( cookie.Values[cDefaultOutput] == null )
			{
				cookie.Values.Add(cDefaultOutput, "");
			}
			if ( cookie.Values[cHttpCacheability] == null )
			{
				cookie.Values.Add(cHttpCacheability, "");
			}

			cookie.Values[cDefaultOutput] = (RadioOutputHTML.Checked) ? RadioOutputHTML.Text : ((RadioOutputExcel.Checked) ? RadioOutputExcel.Text : "");
			cookie.Values[cHttpCacheability] = (CheckEnableCache.Checked ? HttpCacheability.Private : HttpCacheability.NoCache).ToString();

			if ( TextExcelGetBorder.Text.Length == 0 )
			{
				cookie.Values.Remove(cExcelGetBorder);
			}
			else
			{
				if ( cookie.Values[cExcelGetBorder] == null )
				{
					cookie.Values.Add(cExcelGetBorder, "");
				}
				cookie.Values[cExcelGetBorder] = TextExcelGetBorder.Text;
			}

			if ( TextExcelFrameVisible.Text.Length == 0 )
			{
				cookie.Values.Remove(cExcelFrameVisible);
			}
			else
			{
				if ( cookie.Values[cExcelFrameVisible] == null )
				{
					cookie.Values.Add(cExcelFrameVisible, "");
				}
				cookie.Values[cExcelFrameVisible] = TextExcelFrameVisible.Text;
			}

			cookie.Expires = DateTime.MaxValue;
			Response.AppendCookie(cookie);

			if ( devGroupUser )
			{
				CheckEnableCache.ToolTip = cHttpCacheability + "=" + cookie.Values[cHttpCacheability];
			}
		}
	}

	/// <summary>
	/// ToggleOption コントロールを設定する
	/// </summary>
	private void ToggleOptionControlSetting()
	{
		try
		{
			ToggleOption.Attributes["onmousedown"] = "TogglePanelOption()";

			StringBuilder toolTip = new StringBuilder();

			toolTip.Append(LabelDefaultOutput.Text + " ");
			toolTip.Append((RadioOutputHTML.Checked) ? RadioOutputHTML.Text : ((RadioOutputExcel.Checked) ? RadioOutputExcel.Text : ""));

			toolTip.Append("\n");	// "[br]"

			toolTip.Append(CheckEnableCache.Text + ": ");
			toolTip.Append(CheckEnableCache.Checked);

			ToggleOption.ToolTip = toolTip.ToString();

			string javaScript = "<script language='javascript'>\r\n" +
								"  InitPanelOption();\r\n" +
								"</script>\r\n";

			if ( !Page.IsPostBack )
			{
				ClientScript.RegisterStartupScript(typeof(string), "myJavaScript", javaScript);
			}
			else
			{
				string eventTarget = Request.Params["__EVENTTARGET"];
				if ( eventTarget == ButtonApply.ID )	// ButtonApply.UseSubmitBehavior="False" としておく必要がある
				{
					ToggleOption.Text = "-オプション";
				}
				else
				{
					ClientScript.RegisterStartupScript(typeof(string), "myJavaScript", javaScript);
				}
			}
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
		}
	}

	/// <summary>
	/// カテゴリ（２階層目のサブディレクトリ）を列挙してドロップダウン化する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	/// <param name="subDirectory"></param>
	/// <param name="category"></param>
	private void EnumCategory(string shenlongDocumentsFolder, out string subDirectory, out string category)
	{
		subDirectory = DropDownSubDirectory.Text;
		category = null;

		if ( subDirectory == "ＴＯＰページ" )
		{
			LabelCategory.Visible = DropDownCategory.Visible = false;
			return;
		}

		string eventTarget = Request.Params["__EVENTTARGET"];

		// ポストバックでないか、DropDownSubDirectory || ButtonApply でポストバックされた？
		if ( !Page.IsPostBack || (eventTarget == DropDownSubDirectory.ID) || (eventTarget == ButtonApply.ID) )
		{
			DropDownCategory.Items.Clear();
			DropDownCategory.Items.Add("カテゴリ選択...");

			// サブディレクトリ内のフォルダをカテゴリとして選択できるようにする
			string categoryDirectory = shenlongDocumentsFolder + "\\" + subDirectory;
			string[] categories = Directory.GetDirectories(categoryDirectory);
			for ( int i = 0; i < categories.Length; i++ )
			{
				DirectoryInfo _directoryInfo = new DirectoryInfo(categories[i]);
				if ( _directoryInfo.GetFiles("*.xml").Length == 0 )
					continue;
				if ( _directoryInfo.GetFiles("private.txt").Length != 0 )	// "private.txt" ファイルがあるフォルダはスキップする
					continue;

				string categoryName = Path.GetFileName(categories[i]);

				DropDownCategory.Items.Add(categoryName);
			}
		}

		LabelCategory.Visible = DropDownCategory.Visible = (DropDownCategory.Items.Count > 1);

		// DropDownCategory でポストバックされた？
		if ( Page.IsPostBack && ((eventTarget == DropDownCategory.ID) && (DropDownCategory.Text != "カテゴリ選択...")) )
		{
			category = DropDownCategory.Text;
		}
	}

	/// <summary>
	/// シェンロンの卵を列挙してハイパーリンク化する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	/// <param name="devGroupUser"></param>
	private void EnumShenlongDocuments(string shenlongDocumentsFolder, bool devGroupUser)
	{
		// TableShenlongFiles テーブルの設定
		TableShenlongFiles.Attributes.Add("border", "1");
		TableShenlongFiles.Attributes.Add("cellspacing", "0");
		TableShenlongFiles.Attributes.Add("cellpadding", "3");
		TableShenlongFiles.Attributes.Add("bordercolordark", "#ffffff");
		TableShenlongFiles.Attributes.Add("bordercolor", "#777777");
		TableShenlongFiles.Attributes.Add("bgcolor", "white");

		TableRow row;
		TableCell cell;

		// タイトル
		row = new TableRow();
		row.Attributes.Add("class", "mocha_head"/*"normal"*/);
		row.Attributes.Add("align", "center");

		cell = new TableCell();
		cell.Text = "シェンロンの卵";
		row.Cells.Add(cell);

		cell = new TableCell();
		cell.Text = "コメント";
		row.Cells.Add(cell);

		/*cell = new TableCell();
		cell.Text = "作成者";
		row.Cells.Add(cell);*/

		cell = new TableCell();
		cell.Text = "更新日";
		row.Cells.Add(cell);

		TableShenlongFiles.Rows.Add(row);

		bool even = false;

		DirectoryInfo directoryInfo = new DirectoryInfo(shenlongDocumentsFolder);
		FileInfo[] shenlongFiles = directoryInfo.GetFiles("*.xml");

		//string[] shenlongFiles = Directory.GetFiles(shenlongDocumentsFolder, "*.xml");
		for ( int i = 0; i < shenlongFiles.Length; i++ )
		{
			XmlDataDocument xmlShenlongColumn = new XmlDataDocument();
			xmlShenlongColumn.Load(shenlongFiles[i].FullName);

			XmlNode property = xmlShenlongColumn.DocumentElement[cc.tagProperty];

			row = new TableRow();
			if ( even )
				row.BackColor = Color.FromArgb(0xF7, 0xF7, 0xDE);/*Color.LightYellow*/

			bool download = (devGroupUser || ((property[cc.tagDownload] != null) && (property[cc.tagDownload].InnerText == cc.authority.permit.ToString())));

			string shenlongFileName = shenlongFiles[i].ToString();
			cell = new TableCell();
			cell.Text = "<a href=\"./shenlong2.aspx" +
						"?" + bb.pmShenFile + "=" + HttpUtility.UrlEncode(shenlongFileName) +
						(CheckEnableCache.Checked ? ("&" + bb.pmCacheability + "=" + (int)HttpCacheability.Private) : "") + "\"" +
						" " +
						(!download ? "class=\"widelink\"" : "") +
						" " +
						"onclick=\"WaitCursor()\">" +
						Path.GetFileNameWithoutExtension(shenlongFileName) +
						"</a>" +
						(download ?
						  " " +
						  "<a href=\"./Default.aspx" + "?" +
						    bb.pmShenFile + "=" + HttpUtility.UrlEncode(shenlongFileName) + "&" +
							bb.pmDownload + "=" + shenlongFiles[i].LastWriteTime.Ticks.ToString() + "\">" +
						  "<img border=\"0\" src=\"./images/download.gif\"/>" +
						  "</a>" : "");
			XmlNode docElem = xmlShenlongColumn.DocumentElement;
			Version verShenColumn = bb.GetShenColumnVer(docElem.Attributes[cc.attrVer]);
			string toolTip = "version " + verShenColumn.ToString() +
							 (devGroupUser ? "\r\n" + docElem.Attributes[cc.attrUserName].Value + "@" + docElem.Attributes[cc.attrSID].Value : "") +
							 (devGroupUser && cc.IsEggPermissionSet(property[cc.tagEggPermission]) ? "\r\n" + property[cc.tagEggPermission].InnerText : "");
			cell.ToolTip = toolTip;
			row.Cells.Add(cell);

			cell = new TableCell();
			cell.Text = property[cc.tagComment].InnerText;
			if ( cell.Text.Length == 0 )
			{
				cell.Text = "<br>";
			}
			row.Cells.Add(cell);

			/*cell = new TableCell();
			try	{
				cell.Text = property[cc.tagAuthor].InnerText;
			} catch ( Exception ) {}
			if ( cell.Text.Length == 0 )
			{
				cell.Text = "<br>";
			}
			row.Cells.Add(cell);*/

			cell = new TableCell();
			cell.Text = shenlongFiles[i].LastWriteTime.ToString("yyyy/MM/dd" + ((devGroupUser) ? " HH:mm" : ""));
			cell.Wrap = false;
			row.Cells.Add(cell);

			TableShenlongFiles.Rows.Add(row);

			even = !even;
		}
	}

	/// <summary>
	/// GetAccessCounter
	/// <?xml version="1.0" encoding="shift_jis"?>
	/// <root total="0">
	///   <access date="2006/08/23" counter="0" />
	///   <access date="2006/08/24" counter="0" />
	/// </root>
	/// </summary>
	private XmlDocument GetAccessCounter(string pathName, string fileName, bool increment, string remoteUser, bool devGroupUser, ref Label LabelDebug)
	{
		XmlDocument accessCounterXml = new XmlDocument();

		try
		{
			accessCounterXml.Load(pathName + fileName);

			if ( !increment )
				return accessCounterXml;

			LabelDebug.Text += "=== GetAccessCounter ===" + "<br>";
			/*string[] noCountUpAccounts = { "localhost" };
			int i;*/
			string todayDate = DateTime.Today.ToString("yyyy/MM/dd");
			LabelDebug.Text += todayDate + "<br>";

			XmlNode root = accessCounterXml["root"];

			XmlNode today = root.SelectSingleNode("access" + "[@" + "date" + "='" + todayDate + "']");
			if ( today == null )
			{
				if ( 31 <= root.ChildNodes.Count )
				{
					root.RemoveChild(root.ChildNodes[0]);
				}

				today = accessCounterXml.CreateNode(XmlNodeType.Element, "access", null);	// <access>
				XmlAttribute attr = accessCounterXml.CreateAttribute("date");				// @date
				attr.Value = todayDate;
				today.Attributes.Append(attr);
				attr = accessCounterXml.CreateAttribute("counter");							// @counter
				attr.Value = "0";
				today.Attributes.Append(attr);
				root.AppendChild(today);
				LabelDebug.Text += today.OuterXml.Replace("<", "&lt;").Replace(">", "&gt;") + "<br>";
			}

			// ユーザ毎のカウントアップ
			XmlNode nodeRemoteUser = today[remoteUser];
			if ( nodeRemoteUser == null )
			{
				nodeRemoteUser = accessCounterXml.CreateElement(remoteUser);			// <remoteUser>
				XmlAttribute attr = accessCounterXml.CreateAttribute("counter");		// @counter
				attr.Value = "0";
				nodeRemoteUser.Attributes.Append(attr);
				today.AppendChild(nodeRemoteUser);
			}
			nodeRemoteUser.Attributes["counter"].Value = (Int32.Parse(nodeRemoteUser.Attributes["counter"].Value) + 1).ToString();

			XmlAttribute attrEachHour = nodeRemoteUser.Attributes["eachHour"];
			if ( attrEachHour == null )
			{
				attrEachHour = accessCounterXml.CreateAttribute("eachHour");
				attrEachHour.Value = "0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0";
				nodeRemoteUser.Attributes.Append(attrEachHour);
			}

			string[] eachHour = attrEachHour.Value.Split(',');
			int hour = DateTime.Now.Hour;
			eachHour[hour] = (Int32.Parse(eachHour[hour]) + 1).ToString();
			attrEachHour.Value = String.Join(",", eachHour);

			// 今日のカウントアップ
			/*for ( i = 0; i < noCountUpAccounts.Length && (String.Compare(remoteUser, noCountUpAccounts[i], true) != 0); i++ ) ;
			if ( i == noCountUpAccounts.Length )*/
			if ( !devGroupUser )
			{
				int totalCounter = Int32.Parse(root.Attributes["total"].Value);
				int todayCounter = Int32.Parse(today.Attributes["counter"].Value);
				root.Attributes["total"].Value = (++totalCounter).ToString();
				today.Attributes["counter"].Value = (++todayCounter).ToString();
			}

			accessCounterXml.Save(pathName + fileName);
		}
		catch ( Exception exp )
		{
			accessCounterXml = null;
			LabelDebug.Text += exp.Message + "<br>";
		}

		LabelDebug.Text += "=== GetAccessCounter ===" + "<br>";
		return accessCounterXml;
	}
}
