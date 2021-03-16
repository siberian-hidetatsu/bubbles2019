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

public partial class DefaultFrame1 : System.Web.UI.Page
{
	private const string topPageItemName = "ＴＯＰページ";

	/// <summary>
	/// Page_Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
	{
		try
		{
			string shenlongDocumentsFolder = ASP.global_asax.shenlongDocumentsFolder;

			if ( (Request.Params[bb.pmShenDocName] == null)/* && (Session[cc.pmShenDocName] == null)*/ )
			{
				LabelSubTitle.Text = "";
				Session.Remove(bb.pmShenDocName);
			}
			else
			{
				// 間接的な pmShenDocName から実際のシェンロンの卵フォルダを取得する
				GetShenlongDocumentsFolder(ref shenlongDocumentsFolder);
			}

			// サブディレクトリを列挙してドロップダウン化する
			EnumSubDirectory(shenlongDocumentsFolder);

			// シェンロンの卵フォルダをセッションに格納する
			Session[bb.ssShenlongDocFolder] = shenlongDocumentsFolder + "\\";

			Session.Remove(bb.pmDevelop);
			Session.Remove(bb.ssHyperLinkHome);

			// カテゴリ（２階層目のサブディレクトリ）を列挙してハイパーリンク化する
			string frame2LocationHref;
			EnumCategory(shenlongDocumentsFolder, out frame2LocationHref);

			if ( Request.Params[bb.pmDevelop] != null )
			{
				// 開発用の識別子をセッションに格納する
				Session[bb.pmDevelop] = Request.Params[bb.pmDevelop];
			}

			// frame2 のリンク先を動的に決める
			ClientScript.RegisterStartupScript(typeof(string), "myJavaScript",
				"<script language='javascript'>\r\n" +
				"  parent.frame2.location.href='" + frame2LocationHref + "';\r\n" +
				"  document.getElementById('PanelCategory').focus();\r\n" +
				"</script>");
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

	/// 間接的な pmShenDocName から実際のシェンロンの卵フォルダを取得する
	/// 正常に取得できれば pmShenDocName をセッションに格納する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	private void GetShenlongDocumentsFolder(ref string shenlongDocumentsFolder)
	{
		string shenDocName = (Request.Params[bb.pmShenDocName] != null) ? Request.Params[bb.pmShenDocName] : (string)Session[bb.pmShenDocName];
		string hostName = System.Net.Dns.GetHostName();
		string appDataPathName = ((String.Compare(ASP.global_asax.bubblesHostNameRemote.Substring(2), hostName, true) == 0) ||
								  (String.Compare(ASP.global_asax.debugPcName, hostName, true) == 0)) ? Server.MapPath(@".\App_Data\") : ASP.global_asax.bubblesHostNameRemote + @"\bubbles_App_Data\";

		LabelSubTitle.Text = "[" + shenDocName + "]" + "<br/>";
		LabelSubTitle.ForeColor = Color.DarkBlue;

		XmlDocument xmlShenlongDocFolder = new XmlDocument();
		xmlShenlongDocFolder.Load(appDataPathName + "ShenlongDocFolder.xml");

		string xpath = "/" + "shenlong" + "/" + "document" + "[@" + "name" + "='" + shenDocName + "']";
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
				Response.Write("<span style=\"color:Red;font-weight:bold;\">" + shenDocName + " で登録されているフォルダは存在しません" + "</span>");
				Response.End();
			}
		}
		else
		{
			Response.Write("<span style=\"color:Red;font-weight:bold;\">" + shenDocName + " は ShenlongDocFolder.xml に登録されていません" + "</span>");
			Response.End();
		}
	}

	/// <summary>
	/// サブディレクトリを列挙してドロップダウン化する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	private void EnumSubDirectory(string shenlongDocumentsFolder)
	{
		HttpCookie cookie = Request.Cookies["bubbles"];
		string cSubDirectory = "subDirectory";

		if ( !Page.IsPostBack )
		{
			string cookieSubDirectory = String.Empty;

			if ( cookie != null )
			{
				cookieSubDirectory = HttpUtility.UrlDecode(cookie.Values[cSubDirectory]);
			}

			DropDownSubDirectory.Items.Add(topPageItemName);

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
			cookie.Expires = DateTime.MaxValue;
			Response.AppendCookie(cookie);
		}
	}

	/// <summary>
	/// カテゴリ（２階層目のサブディレクトリ）を列挙してハイパーリンク化する
	/// </summary>
	/// <param name="shenlongDocumentsFolder"></param>
	/// <param name="frame2LocationHref"></param>
	private void EnumCategory(string shenlongDocumentsFolder, out string frame2LocationHref)
	{
		frame2LocationHref = "";

		string pmShenDocName = (Request.Params[bb.pmShenDocName] == null) ? "" : bb.pmShenDocName + "=" + Request.Params[bb.pmShenDocName] + "&";
		string subDirectory = DropDownSubDirectory.Text;

		if ( subDirectory == topPageItemName )
		{
			PanelCategory.Controls.Clear();
			frame2LocationHref = "./DefaultFrame2.aspx" + "?" + pmShenDocName;
			return;
		}

		string eventTarget = Request.Params["__EVENTTARGET"];

		// ポストバックでないか、DropDownSubDirectory でポストバックされた？
		if ( !Page.IsPostBack || (eventTarget == DropDownSubDirectory.ID) )
		{
			Label label = new Label();
			label.Text = "<br>[カテゴリ]<br>";
			label.ForeColor = Color.DarkBlue;
			label.Font.Bold = true;
			PanelCategory.Controls.Add(label);

			HyperLink hyperLink = new HyperLink();
			hyperLink.Text = "カテゴリＴＯＰ";
			hyperLink.NavigateUrl = "./DefaultFrame2.aspx" + "?" + pmShenDocName + (bb.ssSubDirectory + "=" + HttpUtility.UrlEncode(subDirectory + "\\"));
			hyperLink.Target = "frame2";
			PanelCategory.Controls.Add(hyperLink);
			PanelCategory.Controls.Add(new LiteralControl("<br>"));

			frame2LocationHref = hyperLink.NavigateUrl;

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

				hyperLink = new HyperLink();
				hyperLink.Text = categoryName;
				hyperLink.NavigateUrl = "./DefaultFrame2.aspx" + "?" +
										pmShenDocName +
										(bb.ssSubDirectory + "=" + HttpUtility.UrlEncode(subDirectory + "\\" + categoryName + "\\"));
				hyperLink.Target = "frame2";
				PanelCategory.Controls.Add(hyperLink);
				PanelCategory.Controls.Add(new LiteralControl("<br>"));
			}
		}
	}
}
