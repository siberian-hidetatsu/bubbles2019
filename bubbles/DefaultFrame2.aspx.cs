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

public partial class DefaultFrame2 : System.Web.UI.Page
{
	/// <summary>
	/// Page_Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
	{
		LabelDebug.Text = "";

		// サブディレクトリの引数を処理する
		SubDirectoryParamProcess();

		// 卵のダウンロード？
		if ( IsShelongDocDownload() )
			return;

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
			if ( Session[bb.pmShenDocName] == null )
			{
				if ( !devGroupUser )
				{
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "端末名 " + remoteUser + " はルートフォルダにアクセスする権限がありません" + "</span>");
					Response.End();
					return;
				}
			}

			string shenlongDocumentsFolder = (string)Session[bb.ssShenlongDocFolder];
			if ( string.IsNullOrEmpty(shenlongDocumentsFolder) )
			{
				Response.Write("<span _style=\"color:Red;font-weight:bold;\">" + "セッションが切れています<br>最初のページからアクセスして下さい" + "</span>");
				Response.End();
				return;
			}
			shenlongDocumentsFolder = shenlongDocumentsFolder.EndsWith("\\") ? shenlongDocumentsFolder.Substring(0, shenlongDocumentsFolder.Length - 1) : shenlongDocumentsFolder;
			LabelDebug.Text += "<br>" + shenlongDocumentsFolder + "<br>";

			// クッキーの読み書き
			CookieReadWrite(devGroupUser);

			// ToggleOption コントロールを設定する
			ToggleOptionControlSetting();

			// サブディレクトリが選択された？
			if ( !string.IsNullOrEmpty((string)(Session[bb.ssSubDirectory])) )
			{
				shenlongDocumentsFolder += ("\\" + Session[bb.ssSubDirectory]);

				LabelDebug.Text += shenlongDocumentsFolder + "<br>";
			}

			// ホームアイコンの NavigateUrl をセッションに格納する
			Session[bb.ssHyperLinkHome] = "DefaultFrame2.aspx";

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
	/// サブディレクトリの引数を処理する
	/// </summary>
	private void SubDirectoryParamProcess()
	{
		try
		{
			Session.Remove(bb.ssSubDirectory);

			LabelCategory.Visible = false;

			if ( Request.Params[bb.ssSubDirectory] == null )
				return;

			string ssSubDirectory = Request.Params[bb.ssSubDirectory];
			
			// サブディレクトリ（カテゴリ付き）をセッションに格納する
			Session[bb.ssSubDirectory] = ssSubDirectory;

			if ( ssSubDirectory.IndexOf('\\') != -1 )
			{
				string category = ssSubDirectory.Split('\\')[1];
				LabelCategory.Text = category;
				LabelCategory.ForeColor = Color.DarkBlue;
				LabelCategory.Visible = true;
			}

			LabelDebug.Text += ssSubDirectory + "<br>";
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
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
				Response.AppendHeader("Content-Disposition", "attachment; filename=" + shenFileName);
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

				if ( Request.UrlReferrer.Segments[Request.UrlReferrer.Segments.Length - 1].StartsWith("DefaultFrame") )
				{
					incCounter = true;
				}
			}

			if ( (Request.UrlReferrer == null) || incCounter )
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

			string[] showDetailAccounts = { ASP.global_asax.debugPcName, "localhost" };
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
	/// クッキーの読み書き
	/// </summary>
	/// <param name="devGroupUser"></param>
	private void CookieReadWrite(bool devGroupUser)
	{
		HttpCookie cookie = Request.Cookies["bubbles"];
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

			if ( !Page.IsPostBack )
			{
				ClientScript.RegisterStartupScript(typeof(string), "myJavaScript",
					"<script language='javascript'>\r\n" +
					"  InitPanelOption();\r\n" +
					"</script>\r\n");
			}
			else
			{
				ToggleOption.Text = "-オプション";
			}
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
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
						  "<a href=\"./DefaultFrame2.aspx" + "?" +
							bb.ssSubDirectory + "=" + Session[bb.ssSubDirectory] + "&" +
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
