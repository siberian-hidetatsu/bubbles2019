#define xUSE_ORACONN_CTRL
#define	ALLOW_SORTING
#define	ENABLED_SUBQUERY			// サブクエリのロジックを有効にする
#define	NEW_GETPLAINTABLEFIELDNAME	// 新しいGetPlainTableFieldName関数を使う
#define	WITH_CALENDAR_CONTROL
#define	FOR_WINDOW_OPEN
#define	SAVE_BUILDSQL_TO_SESSION
#define	UPDATE_20190314
using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Threading;
using System.IO;
using System.Xml;
using System.Text;
using System.Drawing;
//using System.Data.OleDb;
using System.Collections.Generic;
using System.Diagnostics;
#if !WITHIN_SHENGLOBAL
using cc = Shenlong.ShenGlobal;
#endif
using Oracle.ManagedDataAccess.Client;

public partial class shenlong2 : System.Web.UI.Page
{
	string appDataPathName = null;		// App_Data のパス名

	string columnComments = null;		// カラムのコメント（タブ区切り）
	string[] dataTypeName = null;		// フィールド毎のデータ型
	string[] hyperLink = null;
	string[] classify = null;

	/// <summary>
	/// Page_Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
	{
		bool devGroupUser = false;

		LabelDebug.Text = "";

		try
		{
			string hostName = System.Net.Dns.GetHostName();
			string remoteUser = bb.GetRemoteUserName(Request.Params["REMOTE_USER"], Request.Params["REMOTE_ADDR"]);

			bool debugMode = (string.Compare(hostName, ASP.global_asax.debugPcName, true) == 0);
			/*bool */devGroupUser = bb.IsDevGroupUser(ASP.global_asax.devGroupUsers, remoteUser, User.Identity.Name);
			devGroupUser = !devGroupUser ? bb.IsDevelopMode(Request.Params[bb.pmDevelop], (string)Session[bb.pmDevelop]) : devGroupUser;

			appDataPathName = ((String.Compare(ASP.global_asax.bubblesHostNameRemote.Substring(2), hostName, true) == 0) || (String.Compare(ASP.global_asax.debugPcName, hostName, true) == 0)) ? Server.MapPath(@".\App_Data\"/*@".\bin\"*/) : ASP.global_asax.bubblesHostNameRemote + @"\bubbles_App_Data\";

			const string ckExcelOption = "excelOption";
			const string cvExcelXml = "excelXml";
			const string cvWriteCondi = "writeCondi";

			string javaScript =
					"<script language='javascript'>" +
					"  document.body.style.cursor = 'default';\r\n" +
					"  var excelGetBorder = " + ASP.global_asax.excelGetBorder + ";\r\n" +
					"  var excelFrameVisible = 'hidden';\r\n" +
					"  ReadCookie()\r\n" +
					"  var iframe = CreateShenExcelFrame();\r\n" +
#if USE_ORACONN_CTRL
					//"  document.getElementById('TextPWD').focus();" +
					"  if (document.form1.TextPWD != null) {document.form1.TextPWD.focus();}" +
					((ASP.global_asax.dbsv01DefaultPwd != null) ? "  if ((document.form1.TextPWD != null) && (document.getElementById('LabelSID').innerText == 'dbsv01') && (document.getElementById('LabelUID').innerText == 'origin')) {document.form1.TextPWD.value = '" + ASP.global_asax.dbsv01DefaultPwd + "';}" : "") +
#endif
#if false
					"  //DropDownOutputTypeにExcelが無ければPanelExcelOptionを非表示にする\r\n" +
					"  var dropdownOutputType = document.getElementById('DropDownOutputType');\r\n" +
					"  if (document.getElementById('DropDownOutputType') != null) {\r\n" +
					"    var optExcel = false;\r\n" +
					"    for (i = 0; i < dropdownOutputType.options.length; i++) {\r\n" +
					"      var option = dropdownOutputType.options[i];\r\n" +
					"      if (option.value == 'Excel') {\r\n" +
					"        optExcel = true;\r\n" +
					"        break;\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"    if (!optExcel) {\r\n" +
					//"      document.getElementById('PanelExcelOption').style.display = (optExcel)? 'block': 'none';\r\n" +
					"      var panelExcelOption = document.getElementById('PanelExcelOption');\r\n" +
					"      var childs = panelExcelOption.childNodes;\r\n" + 
					"      for (i = 0; i < childs.length; i++) {\r\n" +
					"        if (childs[i].style != null)\r\n" +
					"          childs[i].style.display = (optExcel)? 'block': 'none';\r\n" +
					"        else if (childs[i].disabled != null)\r\n" +
					"          childs[i].disabled = optExcel;\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"  }\r\n" +
					"  \r\n" +
#endif
					"  SetControlStatusByOutputType();\r\n" +

					"//cookie を読み込む\r\n" +
					"function ReadCookie()\r\n" +
					"{\r\n" +
					//"  window.alert(document.cookie);\r\n" +
					"  //bubbles=key=value&key=value...; bubbAccCounter=...\r\n" +
					"  var cookies = document.cookie.split('; ');\r\n" +
					"  for (i = 0; i < cookies.length; i++) {\r\n" +
					"    if (cookies[i].substring(0,8) != 'bubbles=')\r\n" +
					"      continue;\r\n" +
					"    var values = cookies[i].substring(8).split('&');\r\n" +
					"    for (j = 0; j < values.length; j++) {\r\n" +
					//"      window.alert(values[j]);\r\n" +
					"      var keyValue = values[j].split('=');\r\n" +
					"      if (keyValue[0] == 'excelGetBorder') {\r\n" +
					"        excelGetBorder = keyValue[1];\r\n" +
					"      } else if (keyValue[0] == 'excelFrameVisible') {\r\n" +
					"        excelFrameVisible = keyValue[1];\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"    break;\r\n" +
					"  }\r\n" +
					//"  window.alert(excelGetBorder + ',' + excelFrameVisible);\r\n" +
					"}\r\n" +
					
					"//Excel ダウンロード用の iframe を作成する\r\n" +
					"function CreateShenExcelFrame()\r\n" +
					"{\r\n" +
					"  var iframe = document.createElement('iframe');\r\n" +
					"  iframe.style.visibility = excelFrameVisible;\r\n" +
					"  iframe.name = 'shenexcel';\r\n" +
					"  iframe.src = 'about:blank';\r\n" +
					"  document.body.appendChild(iframe);\r\n" +
					"  iframe.contentWindow.name = iframe.name;\r\n" +
					"  return iframe\r\n" +
					"}\r\n" +

					"//出力形式によってコントロールの状態を設定する\r\n" +
					"function SetControlStatusByOutputType()\r\n" +
					"{\r\n" +
					"  if (document.getElementById('DropDownOutputType') == null)\r\n" +
					"    return;\r\n" +
					"  var excel = (document.getElementById('DropDownOutputType').value == 'Excel');\r\n" +
					"  if (document.getElementById('PanelExcelOption') != null) {\r\n" +
					"    document.getElementById('PanelExcelOption').disabled = !excel;\r\n" +
					"  }\r\n" +
					"  if (document.getElementById('LabelOneClick') != null) {\r\n" +
					"    document.getElementById('LabelOneClick').style.display = (excel)? 'block': 'none';\r\n" +
					"  }\r\n" +
					"  if (document.getElementById('LabelRowCountPage') != null ) {\r\n" +
					"    document.getElementById('LabelRowCountPage').disabled = excel;\r\n" +
					"  }\r\n" +
					"  if (document.getElementById('DropDownRowCountPage') != null ) {\r\n" +
					"    document.getElementById('DropDownRowCountPage').disabled = excel;\r\n" +
					"  }\r\n" +
					"}\r\n" +

#if WITH_CALENDAR_CONTROL
					"//カレンダーコントロールを表示する\r\n" +
					"function CalendarPicker(field,date,format,eventtarget)\r\n" +
					"{\r\n" +
#if FOR_WINDOW_OPEN
					//"  document.getElementById(eventtarget).style.cursor = 'wait';\r\n" +
					"  window.open('DatePicker.aspx?'+" +
								   "'" + DatePicker.pmField + "='+field" + "+'&'+" +
								   "'" + DatePicker.pmDate + "='+date" + "+'&'+" +
								   "'" + DatePicker.pmFormat + "='+format" + "+'&'+" +
								   "'" + DatePicker.pmEventtarget + "='+eventtarget" +
								   ",'calendarPopup'" +
								   ",'left='+event.screenX+',top='+event.screenY+',width=240,height=210,titlebar=no,resizable=yes');\r\n" +
#else
					"  yyyymmdd = showModalDialog('DatePicker.aspx?'+" +
								   "'" + DatePicker.pmField + "='+field" + "+'&'+" +
								   "'" + DatePicker.pmDate + "='+date" + "+'&'+" +
								   "'" + DatePicker.pmFormat + "='+format" + "+'&'+" +
								   "'" + DatePicker.pmEventtarget + "='+eventtarget" +
								   ",'calendarPopup'" +
								   ",'dialogLeft:'+event.screenX+';dialogTop:'+event.screenY+';dialogWidth:240px;dialogHeight:210px;');\r\n" +
					//" window.alert(yyyymmdd);\r\n" +
					"  if ( yyyymmdd != null ) {\r\n" +
					"    var _field = decodeURI(field.substring(6));\r\n" + 
					"    document.getElementById(_field).value=yyyymmdd;\r\n" +
					"  }\r\n" +
#endif
					"}\r\n" +
#endif
					"//固定の条件値とラベルを収集する\r\n" +
					"function CollectFixCondition()\r\n" +
					"{\r\n" +
					"  var result = '';\r\n" +
					"  var elements = document.getElementsByTagName('span');\r\n" +
					"  for (i = 0; i < elements.length; i++) {\r\n" +
					//"    forDebug = elements[i].id.substring(0);\r\n" +	// id を name に変えるとエラーになってデバッグできる
					"    if (elements[i].id.substring(0," + bb.pmShenlongFixTextID.Length + ") == '" + bb.pmShenlongFixTextID + "') {\r\n" +
					"      var innerText = encodeURIComponent(elements[i].innerText);\r\n" +
					"      result += GetLabelOfValue(elements[i].id.substring(" + bb.pmShenlongFixTextID.Length + "), innerText);\r\n" +
					"    }\r\n" +
					"    else if (elements[i].id.substring(0," + bb.pmShenlongLabelID.Length + ") == '" + bb.pmShenlongLabelID + "') {\r\n" +
					"      var innerText = elements[i].innerText;\r\n" +
					"      if (innerText.indexOf(' NULL') != -1) {\r\n" +
					"        result += innerText + ', ';\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"  }\r\n" +
					"  return(result.replace(/\\r\\n/,''));\r\n" +
					"}\r\n" +

					"//値に対応しているラベルを取得する\r\n" +
					"function GetLabelOfValue(id,value)\r\n" +
					"{\r\n" +
					"  if (value == '') {return('');}\r\n" +
					"  var labelID = " + "'" + bb.pmShenlongLabelID + "'+id;\r\n" +
					"  if ( document.getElementById(labelID) == null ) {\r\n" +
					"    return('');\r\n" +
					"  } else {\r\n" +
					"    var labelText = document.getElementById(labelID).innerText;\r\n" +
					"    var result = labelText + value;\r\n" +
					"    if (labelText.indexOf('BETWEEN') != -1) {\r\n" +
					"      var textID = " + "'" + bb.pmShenlongTextID + "'+id+'HI';\r\n" +
					"      if (document.getElementById(textID) != null) {\r\n" +
					"        result += ' AND ' + document.getElementById(textID).value;\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"    return(result.replace(/\\r\\n/,'') + ', ');\r\n" +
					"  }\r\n" +
					"}\r\n" +
					"</script>";
			ClientScript.RegisterStartupScript(typeof(string), "startupJavaScript", javaScript);

#if true
			string jsOnSubmit =
#if USE_ORACONN_CTRL
					"if ((document.form1.TextPWD != null) && (document.form1.TextPWD.value == \"\")) {" +
					"  window.alert('パスワードを入力して下さい');" +
					"  document.form1.TextPWD.focus();" +
					"  return(false);" +
					"}" +
#endif
					"if (document.getElementById('DropDownOutputType') != null) {\r\n" +
					"  document.body.style.cursor = 'wait';\r\n" +
					"  window.form1.ButtonSubmit.disabled = true;\r\n" +
					"  if (document.getElementById('DropDownOutputType').value == 'Excel') {\r\n" +
					"    var ie6 = false;\r\n" +
					//"    window.alert(navigator.userAgent);\r\n" +
#if UPDATE_20190314
					"    var userAgent = navigator.userAgent;\r\n" +
					"    if ((userAgent.indexOf('MSIE 6.0') != -1) || (userAgent.indexOf('Chrome/') != -1)) {\r\n" +
#else
					"    if (navigator.userAgent.indexOf('MSIE 6.0') != -1) {\r\n" +
#endif
					"      document.body.style.cursor = 'default';\r\n" +
					"      window.form1.ButtonSubmit.disabled = false;\r\n" +
					"      ie6 = true;\r\n" +
					"    }\r\n" +
#if USE_ORACONN_CTRL
					"    var values = '" + cc.pmTextPWD + "=' + document.form1.TextPWD.value + '\t';\r\n" +
#else
					//"    var values = '" + bb.pmTextPWD + "=' + document.getElementById('LabelPWD').innerText + encodeURI('\t');\r\n" +
					"    var values = '" + bb.pmTextPWD + "=' + document.getElementById('LabelPWD').innerText + '\t';\r\n" +
#endif
					"    var writeCondi = (document.getElementById('CheckWriteCondi') != null && document.getElementById('CheckWriteCondi').checked);\r\n" +
					"    var conditions = '';\r\n" +
					"    if (writeCondi) {\r\n" +
					"      conditions += CollectFixCondition();\r\n" +
					"    }\r\n" +
					"    var elements = document.getElementsByTagName('input');\r\n" +
					"    for (i = 0; i < elements.length; i++) {\r\n" +
					//"      if (elements[i].name.substring(0,2) == '__') continue;\r\n" +
					"      if (elements[i].name.substring(0," + bb.pmShenlongTextID.Length + ") == '" + bb.pmShenlongTextID + "') {\r\n" +
					//"        values += (encodeURI(elements[i].name) + '=' + encodeURIComponent(elements[i].value) + encodeURI('\t'));\r\n" +
					"        values += (elements[i].name + '=' + encodeURIComponent(elements[i].value) + '\t');\r\n" +
					"        if (writeCondi) {\r\n" +
					"          conditions += GetLabelOfValue(elements[i].name.substring(" + bb.pmShenlongTextID.Length + "), encodeURIComponent(elements[i].value));\r\n" +
					"        }\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"    var elements = document.getElementsByTagName('select');\r\n" +
					"    for (i = 0; i < elements.length; i++) {\r\n" +
					"      if (elements[i].name.substring(0," + bb.pmShenlongTextID.Length + ") == '" + bb.pmShenlongTextID + "') {\r\n" +
					//"        values += (encodeURI(elements[i].name) + '=' + encodeURIComponent(elements[i].value) + encodeURI('\t'));\r\n" +
					"        values += (elements[i].name + '=' + encodeURIComponent(elements[i].value) + '\t');\r\n" +
					"        if (writeCondi) {\r\n" +
					"          var selectedText = encodeURIComponent(elements[i].options.item(elements[i].selectedIndex).text);\r\n" +
					"          conditions += GetLabelOfValue(elements[i].name.substring(" + bb.pmShenlongTextID.Length + "), selectedText);\r\n" +
					"        }\r\n" +
					"      }\r\n" +
					"    }\r\n" +
					"    var blankValue = (document.getElementById('LabelBlankText') == null || document.getElementById('RadioUseDefault').checked)? 'UseDefault': 'VoidExpression';\r\n" +
					"    var excelXml = document.getElementById('CheckExcelXml').checked;\r\n" +
					//"    var writeCondi = document.getElementById('CheckWriteCondi').checked;\r\n" +
					//"    window.alert(values.length);\r\n" +
					//"    document.write(values.length + ' ' + values);//デバッグ中にJavaScriptエラーになるが、続行すると出力される\r\n" +
					"    if (values.length <= excelGetBorder) {\r\n" +
					"      var target = ie6? '_self': '_blank';\r\n" +
					"      window.open('./shenexcel.aspx" + "?'+" +
					"        '" + bb.pmValues + "='+" + "encodeURI(values)" + "+'&'+" +
					"        '" + bb.pmBlankValue + "='+" + "blankValue" + "+'&'+" +
					"        '" + bb.pmExcelXml + "='+" + "excelXml" + "+'&'+" + 
					"        '" + bb.pmWriteCondi + "='+" + "conditions"/*"writeCondi"*/ + "" +
					"        ,target,'left=0,top=0,width=10,height=10,menubar=no,toolbar=no,location=no,status=no,resizable=no,scrollbars=no');\r\n" +
					"    } else {\r\n" +
					//"      values = values.replace(/%09/g,'\t');\r\n" +
					//"      values = decodeURI(values);\r\n" +
					"      conditions = decodeURIComponent(conditions);\r\n" +
					"      //formを生成\r\n" +
					"      var form = document.createElement('form');\r\n" +
					"      form.action = './shenexcel.aspx';\r\n" +
					"      form.target = iframe.name;\r\n" +
					"      form.method = 'post';\r\n" +
					"      //input-hidden生成と設定\r\n" +
					"      var qs = [{type:'hidden',name:'" + bb.pmValues + "',    value:values}," +
					"                {type:'hidden',name:'" + bb.pmBlankValue + "',value:blankValue}," +
					"                {type:'hidden',name:'" + bb.pmExcelXml + "',  value:excelXml}," +
					"                {type:'hidden',name:'" + bb.pmWriteCondi + "',value:conditions}];\r\n" +
					"      for(var i = 0; i < qs.length; i++) {\r\n" +
					"        var ol = qs[i];\r\n" +
					"        var input = document.createElement('input');\r\n" +
					"        for(var p in ol) {\r\n" +
					"          input.setAttribute(p, ol[p]);\r\n" +
					"        }\r\n" +
					"        form.appendChild(input);\r\n" +
					"      }\r\n" +
					"      //formをbodyに追加して、サブミットする。その後、formを削除\r\n" +
					"      var body = document.getElementsByTagName('body')[0];\r\n" +
					"      body.appendChild(form);\r\n" +
					"      form.submit();\r\n" +
					"      body.removeChild(form);\r\n" +
					"    }\r\n" +
					// ckExcelOption cookie 処理 ココから
					//"    var date = new Date();\r\n" +
					//"    var expires = new Date(date.getTime() + (1 * (24-16) * 60 * 60 * 1000));\r\n" +
					//"    alert(expires);\r\n" +
					"    document.cookie='" + ckExcelOption + "=" + cvExcelXml + "='+excelXml+'&'+'" + cvWriteCondi + "='+writeCondi+'; '+\r\n" +
					//"                    'expires='+expires.toGMTString()+';';\r\n" +	// expires 有り: cookie が保存される
					"                    '';\r\n" +										// expires 無し: ブラウザが起動している間のみ有効
					//"    alert(document.cookie);\r\n" +
					// ckExcelOption cookie 処理 ココまで
					"    if (ie6) {\r\n" +
					"      return false;\r\n" +
					"    }\r\n" +
					// ポストバックさせる時は、ココから
					"    else {\r\n" +
					"      document.body.style.cursor = 'default';\r\n" +
					"      window.form1.ButtonSubmit.disabled = false;\r\n" +
					"      return false;\r\n" +
					"    }\r\n" +
					// ココまでをコメントにする
					"  }\r\n" +
					"}\r\n";
#else
			string jsOnSubmit =
					"if ((document.form1.DropDownOutputType != null) && (document.form1.DropDownOutputType.value == 'HTML')) {" +
					"  document.body.style.cursor = 'wait';" +
					"  window.form1.ButtonSubmit.disabled = true;" +
					"}";
#endif
			//form1.Attributes.Add("onsubmit", jsOnSubmit);

			DropDownOutputType.Attributes.Add("onchange", "SetControlStatusByOutputType()");

			ButtonSubmit.Attributes.Add("onclick", "window.form1.__EVENTTARGET.value = '" + ButtonSubmit.ID + "';");

#if SAVE_BUILDSQL_TO_SESSION
			/*this.PanelHeader.Controls.Remove(LabelBuildSql);
			this.PanelHeader.Controls.Remove(LabelColumnComments);
			this.PanelHeader.Controls.Remove(LabelLogTableNames);*/
#endif

			//Response.Cache.SetCacheability(HttpCacheability.NoCache);	// キャッシュを無効にする
			HttpCacheability httpCacheability = (Request.Params[bb.pmCacheability] == null) ? HttpCacheability.NoCache : (HttpCacheability)int.Parse(Request.Params[bb.pmCacheability]);

			HttpCookie _cookie;
			if ( (_cookie = Request.Cookies["bubbles"]) != null )
			{
				// 直リンクされて、キャッシュの指定はない？
				if ( (Request.Params[bb.pmShenDocName] != null) && (httpCacheability == HttpCacheability.NoCache) )
				{
					string cHttpCacheability = "httpCacheability" + "@" + Request.Params[bb.pmShenDocName];
					string _httpCacheability = _cookie.Values[cHttpCacheability];
					if ( _httpCacheability != null )
					{
						httpCacheability = (_httpCacheability == HttpCacheability.Private.ToString()) ? HttpCacheability.Private : HttpCacheability.NoCache;
					}
				}
			}

			Response.Cache.SetCacheability(httpCacheability);	// キャッシュを設定する

			gridView.RowDataBound += new GridViewRowEventHandler(gridView_RowDataBound);
			gridView.PreRender += new EventHandler(gridView_PreRender);
			gridView.RowCreated += new GridViewRowEventHandler(gridView_RowCreated);
			gridView.PageIndexChanging += new GridViewPageEventHandler(GridView_PageIndexChanging);

#if ALLOW_SORTING
			gridView.Sorting += new GridViewSortEventHandler(gridView_Sorting);
			gridView.AllowSorting = true;
#endif

#if !USE_ORACONN_CTRL
			PanelOraConnCtrl.Visible = false;
			LabelParamMess.Visible = false;
#endif

#if false
			Debug.WriteLine(DateTime.Now);
			Debug.WriteLine("[Session]");
			foreach ( string key in Session.Keys )
			{
				Debug.WriteLine(key + ":" + Session[key]);
			}
			/*Debug.WriteLine("[Params]");
			foreach ( string key in Request.Params )
			{
				Debug.WriteLine(key + ":" + Request.Params[key]);
			}*/
			/*Debug.WriteLine("[ViewState]");
			foreach ( string key in ViewState.Keys )
			{
				Debug.WriteLine(key + ":" + ViewState[key]);
			}*/
			Debug.WriteLine(bb.ssUrlReferer + ":" + Session[bb.ssUrlReferer]);
#endif

#if true
			if ( cc.replaceForWebAppChars == null )
			{
				cc.replaceForWebAppChars = ASP.global_asax.replaceForWebAppChars;
			}
#endif

			HttpCookie cExcelOption;

			if ( !Page.IsPostBack )
			{
				if ( Request.Params[bb.pmShenDirect] != null )	// ダイレクトに呼び出された？
				{
					ShenlongDirectManager(devGroupUser, remoteUser);
					return;
				}

				if ( Request.Params[bb.pmShenFile] == null )
					return;

#if true
				if ( ASP.global_asax.outputType != null )
				{
					DropDownOutputType.Items.Clear();
					foreach ( string item in ASP.global_asax.outputType )
					{
						DropDownOutputType.Items.Add(item);
					}
				}
#endif

#if true
				if ( (_cookie = Request.Cookies["bubbles"]) != null )
				{
					string cDefaultOutput = "defaultOutput" + "@" + Session[bb.pmShenDocName];
					string _defaultOutput = _cookie.Values[cDefaultOutput];
					if ( _defaultOutput != null )
					{
						for ( int i = 0; i < DropDownOutputType.Items.Count; i++ )
						{
							if ( DropDownOutputType.Items[i].Text == _defaultOutput )
							{
								DropDownOutputType.SelectedIndex = i;
								break;
							}
						}
					}
				}
#endif

#if true
				Session.Remove(bb.ssUrlReferer);

				SetShenDocFolderSession(Request.Params[bb.pmShenDocName]);
#endif

				//string fileName = ASP.global_asax.shenlongDocumentsFolder + "\\" + Session[ssSubDirectory] + HttpUtility.UrlDecode(Request.Params[pmShenFile]);
				string shenFileName = (string)Session[bb.ssShenlongDocFolder] + (string)Session[bb.ssSubDirectory] + HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]);
				if ( !File.Exists(shenFileName) )
				{
					string message = (devGroupUser ? (string)Session[bb.ssShenlongDocFolder] + "<br>" : "") +
									 (string)Session[bb.ssSubDirectory] + HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]) + " が見つかりません.<br>前のページに戻って [F5] キーを押してみて下さい.";
					throw new Exception(message);
				}

				Session[bb.ssShenFileName] = HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]);
				LabelFullPath.Text = HttpUtility.UrlEncode(shenFileName);
				LabelPWD.Text = string.Empty;
#if SAVE_BUILDSQL_TO_SESSION
				Session["LabelBuildSql"] = string.Empty;
				Session["LabelColumnComments"] = string.Empty;
				Session["LabelLogTableNames"] = string.Empty;
				Session["LabelPWD"] = string.Empty;
#else
				LabelBuildSql.Text = string.Empty;
				LabelColumnComments.Text = string.Empty;
				LabelLogTableNames.Text = string.Empty;
#endif

				SetHyperLinkHome(HyperLinkHome0, true);
				HyperLinkShenExcel.Visible = false;

				/*XmlDocument xmlShenlongColumn = new XmlDocument();
				xmlShenlongColumn.Load(shenFileName);*/
				Version verShenColumn;
				XmlDocument xmlShenlongColumn = bb.ReadShenlongColumnFile(shenFileName, out verShenColumn);

#if true
				if ( cc.IsEggPermissionSet(xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagEggPermission]) )
				{
					string eggPermission = xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagEggPermission].InnerText;
					string message;
					if ( !PermittedRemoteUser(eggPermission, out message) )
					{
						string errorMessage = "接続された端末からのアクセスは拒否されました．";
						if ( devGroupUser )
						{
							errorMessage += "<br>" + message +
											"<br>" + Request.Params["REMOTE_USER"] + "@" + remoteUser + "(" + Request.Params["REMOTE_ADDR"] + ")";
						}
						throw new Exception(errorMessage);
					}
					if ( devGroupUser && !string.IsNullOrEmpty(message) )
					{
						LabelDebug.Text += message + "<br>";
					}
				}
#endif

				string fileComment = xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagComment].InnerText;
				LabelComment.Text = "[" + Path.GetFileNameWithoutExtension(shenFileName) + ((fileComment.Length != 0) ? " : " + fileComment : "") + "]";
				//LabelComment.ToolTip = "ver " + verShenColumn.ToString();
				LabelSID.Text = xmlShenlongColumn.DocumentElement.Attributes[cc.attrSID].Value;
				LabelUID.Text = xmlShenlongColumn.DocumentElement.Attributes[cc.attrUserName].Value;
				if ( !debugMode )
				{
					TextPWD.TextMode = TextBoxMode.Password;
				}
				/*if ( (string.Compare(LabelSID.Text, "dbsv01", true) == 0) && (string.Compare(LabelUID.Text, "origin", true) == 0) )
				{
					if ( ASP.global_asax.dbsv01DefaultPwd != null )
					{
						TextPWD.Visible = false;
						Label4.Text += "●●●●";
						LabelPWD.Text = ASP.global_asax.dbsv01DefaultPwd;
					}
				}*/

#if USE_ORACONN_CTRL
				// 開発者のホストから接続された時の処理
				if ( devGroupUser )
				{
					string password = GetLogOnPassword();
					if ( password != null )
					{
						ClientScript.RegisterStartupScript(typeof(string), "jsSetPassword",
							"<script language='javascript'>" +
							"document.form1.TextPWD.value = '" + password + "';" +
							"</script>");
					}
				}
#else
				string password = GetLogOnPassword();
				if ( password == null )
				{
					LabelParamMess.Visible = true;
					LabelParamMess.Text = "<br/>LogOn.xml でのオラクル接続文字列が未登録です。<br/>(ICSG)担当者に連絡して下さい。<br/>";
					LabelParamMess.Font.Bold = true;
					LabelParamMess.ForeColor = Color.Red;
				}
				else
				{
					/*ClientScript.RegisterStartupScript(typeof(string), "jsSetPassword",
						"<script language='javascript'>" +
						"document.getElementById('LabelPWD').innerText = '" + password + "';" +
						"</script>");*/
					// JavaScript で入れるとポストバックした時に消える？のでこうした。
					LabelPWD.ForeColor = Color.FloralWhite;
					LabelPWD.Text = password;
#if SAVE_BUILDSQL_TO_SESSION
					Session["LabelPWD"] = LabelPWD.Text;
#endif
				}

				// LabelPWD を JavaScript で非表示にする。デザイン画面で Visible=false にしておくとラベル自身が作成されない為、shenexcel.aspx にパスワードを渡せない。
				ClientScript.RegisterStartupScript(typeof(string), "jsLabelPWD",
					"<script language='javascript'>" +
					"document.getElementById('LabelPWD').style.display = 'none';" +
					"</script>");
#endif

				StringBuilder jsCheckTextBox;
				int shenlongParamCount = 0;
//#if ENABLED_SUBQUERY
				if ( 2 <= verShenColumn.Major )
				{
					jsCheckTextBox = new StringBuilder();
					XmlNode fileProperty = xmlShenlongColumn.DocumentElement[cc.tagProperty];
					if ( bool.Parse(fileProperty[cc.tagSqlSelect].InnerText) )
					{
						if ( string.Compare(xmlShenlongColumn.DocumentElement[cc.tagSQL].InnerText.Trim(), 0, "SELECT", 0, 6, true) != 0 )
						{
							throw new Exception("SELECT 以外は指定できません");
						}
					}
					else
					{
						// shenlong のパラメータ箇所をテキスト コントロールとして追加する
#if false
						shenlongParamCount = AppendShenlongParamControl(xmlShenlongColumn, ref jsCheckTextBox);

						if ( (fileProperty[cc.tagSubQuery] != null) && (fileProperty[cc.tagSubQuery].InnerText.Length != 0) )
						{
							foreach ( string subQuery in fileProperty[cc.tagSubQuery].InnerText.Split(cc.SUBQUERY_SEPARATOR) )
							{
								shenlongParamCount += AppendShenlongParamControl(cc.ReadSubQueryFile(subQuery, xmlShenlongColumn.BaseURI), ref jsCheckTextBox);
							}
						}
#else
						AppendShenlongParamControlForBase(xmlShenlongColumn.BaseURI, xmlShenlongColumn, ref jsCheckTextBox, ref shenlongParamCount);
#endif
					}
				}
//#else
				else
				{
					// shenlong のパラメータ箇所をテキスト コントロールとして追加する
					shenlongParamCount = AppendShenlongParamControl(xmlShenlongColumn, debugMode, out jsCheckTextBox);
				}
//#endif

				//jsCheckTextBox.Insert(0, "if ((document.form1.RadioVoidExpression != null) && (document.form1.RadioVoidExpression.checked)) {");
				jsCheckTextBox.Insert(0, "if (document.getElementById('LabelBlankText') != null) {\r\n");
				jsCheckTextBox.Append("}\r\n");
#if UPDATE_20190314
				StringBuilder shenSubmit = new StringBuilder();
				shenSubmit.Append(jsCheckTextBox);
				shenSubmit.Append(jsOnSubmit);

				ClientScript.RegisterOnSubmitStatement(typeof(string), "ShenSubmit", shenSubmit.ToString());
#else
				form1.Attributes.Add("onsubmit", jsCheckTextBox + jsOnSubmit);
#endif

				if ( shenlongParamCount != 0 )
				{
#if USE_ORACONN_CTRL
					this.LabelParamMess.Text = "<br/>[抽出条件]<br/>";
					this.LabelParamMess.ToolTip = "日時は yyyymmdd hh24mi | yyyy/mm/dd hh24:mi 形式で";
#endif
				}
				else
				{
					this.LabelParamMess.Text = string.Empty;
					this.LabelBlankText.Visible = false;
					this.RadioUseDefault.Visible = false;
					this.RadioVoidExpression.Visible = false;
				}

				LabelExcelOption.Text = new string('　', (Request.Browser.MajorVersion == 6) ? 17 : 13);

				if ( (verShenColumn.Major < 2)/* || (shenlongParamCount == 0)*/ )
				{
					LabelExcelOption.Text = new string('　', (Request.Browser.MajorVersion == 6) ? 6 : 5);
					this.CheckWriteCondi.Visible = false;
				}

				//this.ButtonSubmit.Attributes.Add("align", "center");

				try
				{
					// Excel オプションのクッキーを読み込む
					if ( (cExcelOption = Request.Cookies[ckExcelOption]) != null )
					{
						string excelXml = cExcelOption.Values[cvExcelXml];
						if ( excelXml != null )
						{
							CheckExcelXml.Checked = bool.Parse(excelXml);
						}
						string writeCondi = cExcelOption.Values[cvWriteCondi];
						if ( writeCondi != null )
						{
							CheckWriteCondi.Checked = bool.Parse(writeCondi);
						}
					}
				}
				catch ( Exception exp )
				{
					Debug.WriteLine(exp.Message);
				}
			}
			else
			{
				// 出力形式:Excel でポストバックされた？
				// onsubmit 時に return false; していると、ポストバックしないのでココは通らない
				if ( DropDownOutputType.Text == "Excel" )
				{
					try
					{
						// Excel オプションのクッキーを保存する
						if ( (cExcelOption = Request.Cookies[ckExcelOption]) == null )
						{
							cExcelOption = new HttpCookie(ckExcelOption);
						}
						if ( cExcelOption.Values[cvExcelXml] == null )
						{
							cExcelOption.Values.Add(cvExcelXml, null);
						}
						if ( cExcelOption.Values[cvWriteCondi] == null )
						{
							cExcelOption.Values.Add(cvWriteCondi, null);
						}
						cExcelOption.Values[cvExcelXml] = CheckExcelXml.Checked.ToString();
						cExcelOption.Values[cvWriteCondi] = CheckWriteCondi.Checked.ToString();
						//cExcelOption.Expires = DateTime.Now.AddHours(24-20);	// とりあえず、IEが起動している間のみ有効とする（ie6 の時と合わせる）
						Response.AppendCookie(cExcelOption);
					}
					catch ( Exception exp )
					{
						Debug.WriteLine(exp.Message);
					}

					string redirect = "./shenlong2.aspx?" +
									  ((Request.Params[bb.pmShenDocName] != null) ? bb.pmShenDocName + "=" + Request.Params[bb.pmShenDocName] + "&" : "") +
									  ((Request.Params[bb.pmSubDir] != null) ? bb.pmSubDir + "=" + Request.Params[bb.pmSubDir] + "&" : "") +
									  (bb.pmShenFile + "=" + Request.Params[bb.pmShenFile]);
					Response.Redirect(redirect, false);	// true では Response.End() が呼び出され、ThreadAbortException が発生する
					//Response.End();
					return;
				}

				string shenFileName = HttpUtility.UrlDecode(LabelFullPath.Text);
				string buildSql/*, columnComments*/, logTableNames;

				/*XmlDocument xmlShenlongColumn = new XmlDocument();
				xmlShenlongColumn.Load(shenFileName);*/
				Version verShenColumn;
				XmlDocument xmlShenlongColumn = bb.ReadShenlongColumnFile(shenFileName, out verShenColumn);

#if SAVE_BUILDSQL_TO_SESSION
				//bool firstPostBack = string.IsNullOrEmpty((string)Session["LabelBuildSql"]);
				bool firstPostBack = (Request.Params["__EVENTTARGET"] == ButtonSubmit.ID);
#else
				bool firstPostBack = (LabelBuildSql.Text.Length == 0);
#endif

				if ( firstPostBack )
				{
//#if ENABLED_SUBQUERY
					Dictionary<string, string> selectParams = new Dictionary<string, string>();
					if ( 2 <= verShenColumn.Major )
					{
						//Dictionary<string, string> selectParams = new Dictionary<string, string>();
						foreach ( string key in Request.Params.AllKeys )
						{
							if ( !key.StartsWith(bb.pmShenlongTextID) )
								continue;
							//selectParams.Add(key, Request.Params[key]);
							string value = Request.Params[key];
							if ( (value.Length == 0) && RadioUseDefault.Checked )
								continue;
							selectParams.Add(key, value);
						}
					}
//#else
					else
					{
						// ポストバックされた shenlong のパラメータをクエリー項目ファイルにセットする
						SetShenlongParam(ref xmlShenlongColumn);
					}
//#endif

					// クエリー項目から SQL を構築する
					List<string> fromTableNames = new List<string>();
					bool result;
//#if ENABLED_SUBQUERY
					if ( 2 <= verShenColumn.Major )
					{
						if ( IsSqlSelect(xmlShenlongColumn.DocumentElement[cc.tagProperty]) )
						{
							buildSql = xmlShenlongColumn.DocumentElement[cc.tagSQL].InnerText;
							buildSql = buildSql.Replace("<br>", "\r\n"/*" "*/);
							fromTableNames = cc.GetTableNameInSQL(buildSql, true, true);
							result = true;
						}
						else
						{
							result = cc.BuildQueryColumnSQL(xmlShenlongColumn, selectParams, true, ASP.global_asax.maxRowNum, out buildSql, out columnComments, ref fromTableNames, 0);
						}
					}
//#else
					else
					{
						result = bb.BuildQueryColumnSQL(xmlShenlongColumn, ASP.global_asax.maxRowNum, out buildSql, out columnComments, ref fromTableNames, 0);
					}
//#endif
					if ( !result )
					{
						Response.Clear();
						Response.Write(columnComments);
						Response.End();
					}

					logTableNames = bb.GetLogTableNames(fromTableNames);
#if SAVE_BUILDSQL_TO_SESSION
					Session["LabelBuildSql"] = HttpUtility.UrlEncode(buildSql);
					Session["LabelColumnComments"] = HttpUtility.UrlEncode(columnComments);
					Session["LabelLogTableNames"] = HttpUtility.UrlEncode(logTableNames);
#else
					LabelBuildSql.Text = HttpUtility.UrlEncode(buildSql);
					LabelColumnComments.Text = HttpUtility.UrlEncode(columnComments);
					LabelLogTableNames.Text = HttpUtility.UrlEncode(logTableNames);
#endif
#if USE_ORACONN_CTRL
					//if ( Request.Params[pmTextPWD] != null )
					//{
					LabelPWD.Text = HttpUtility.UrlEncode(Request.Params[cc.pmTextPWD]);
					//}
#endif
				}
				else
				{
#if SAVE_BUILDSQL_TO_SESSION
					buildSql = HttpUtility.UrlDecode((string)Session["LabelBuildSql"]);
					columnComments = (!string.IsNullOrEmpty((string)Session["LabelColumnComments"])) ? HttpUtility.UrlDecode((string)Session["LabelColumnComments"]) : null;
					logTableNames = HttpUtility.UrlDecode((string)Session["LabelLogTableNames"]);
#else
					buildSql = HttpUtility.UrlDecode(LabelBuildSql.Text);
					columnComments = (LabelColumnComments.Text.Length != 0) ? HttpUtility.UrlDecode(LabelColumnComments.Text) : null;
					logTableNames = HttpUtility.UrlDecode(LabelLogTableNames.Text);
#endif
				}

				// クエリーを実行して GridView を設定する
				ExecuteQuerySetGridView(xmlShenlongColumn, verShenColumn, shenFileName, buildSql, firstPostBack, devGroupUser, remoteUser, logTableNames);
			}
		}
		catch ( ThreadAbortException exp )
		{
			Debug.WriteLine(exp.Message);
		}
		catch ( Exception exp )
		{
			this.PanelHeader.Visible = false;
			this.PanelParam.Visible = false;
			this.PanelOutputType.Visible = false;
			this.PanelSubmit.Visible = false;

			StringBuilder message = new StringBuilder();
			message.Append("<span style=\"color:Red;font-weight:bold;\">" + exp.Message.ToString() + "</span>");
			if ( devGroupUser )
			{
#if SAVE_BUILDSQL_TO_SESSION
				string buildSql = (string)Session["LabelBuildSql"];
#else
				string buildSql = LabelBuildSql.Text;
#endif
				if ( !string.IsNullOrEmpty(buildSql) )
				{
					message.Append("<p/>" + HttpUtility.UrlDecode(buildSql));
				}
			}

			Response.Write(message);
		}
		finally
		{
#if false
			try
			{
				if ( gridView.DataSource != null )
				{
					//((DataTable)(gridView.DataSource)).Clear();
					((DataTable)(gridView.DataSource)).Dispose();
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}
#endif
			GC.Collect();
		}
	}

	/// <summary>
	/// タマゴへのアクセスが許可された端末か否かを確認する
	/// </summary>
	/// <param name="eggPermission"></param>
	/// <param name="message"></param>
	/// <returns></returns>
	private bool PermittedRemoteUser(string eggPermission, out string message)
	{
		message = string.Empty;

		try
		{
			string[] permissions = eggPermission.Split(',');
			bool inJudge = true;
			bool success = false;

			if ( permissions[0].StartsWith("NOT ", StringComparison.CurrentCultureIgnoreCase) )
			{
				permissions[0] = permissions[0].Substring(4);
				inJudge = false;
			}

			string remoteUser = Request.Params["REMOTE_USER"];
			string remoteAddr = Request.Params["REMOTE_ADDR"];

			foreach ( string _permission in permissions )
			{
				string permission = _permission.Trim();

				if ( permission.IndexOf('\\') != -1 )		// \\ドメイン名\ユーザーID指定？
				{
					string[] perDomainUser = permission.Trim('\\').Split('\\');
					string[] remoteDomainUser = remoteUser.Trim('\\').Split('\\');
					for ( int i = 0; i < 2; i++ )
					{
						if ( !(success = ComparePermission(perDomainUser[i], remoteDomainUser[i])) )
							break;
					}

					message += (success ? "[OK]" : "[NG]") + remoteUser + " ";
				}
				else if ( Char.IsLetter(permission[0]) )	// パソコン名指定？
				{
					string remotePcName = bb.GetRemoteUserName(remoteUser, remoteAddr);
					success = ComparePermission(permission, remotePcName);

					message += (success ? "[OK]" : "[NG]") + remotePcName + " ";
				}
				else if ( Char.IsDigit(permission[0]) )		// IPアドレス指定？
				{
					string[] ipAddresses = permission.Split('-');
					byte[] low = System.Net.IPAddress.Parse(ipAddresses[0]).GetAddressBytes();
					byte[] high = System.Net.IPAddress.Parse(ipAddresses[1]).GetAddressBytes();

					byte[] remote = System.Net.IPAddress.Parse(remoteAddr).GetAddressBytes();

					for ( int i = 0; i < remote.Length; i++ )
					{
						if ( !(success = (low[i] <= remote[i] && remote[i] <= high[i])) )
							break;
					}

					message += (success ? "[OK]" : "[NG]") + remoteAddr + " ";
				}

				if ( success )	// 条件を満たした？
				{
					return (inJudge) ? true : false;
				}
			}

			// 全ての条件を満たさなかった
			return (inJudge) ? false : true;
		}
		catch ( Exception exp )
		{
			message = exp.Message;
			return false;
		}
	}

	/// <summary>
	/// ComparePermission
	/// </summary>
	/// <param name="permission"></param>
	/// <param name="remote"></param>
	/// <returns></returns>
	private bool ComparePermission(string permission, string remote)
	{
		if ( permission.EndsWith("*") )
		{
			string _permission = permission.Substring(0, permission.Length - 1);
			return remote.StartsWith(_permission, StringComparison.CurrentCultureIgnoreCase);
		}

		return (string.Compare(permission, remote, true) == 0);
	}

	/// <summary>
	/// [SQL] タブの SELECT 文で抽出するか否か
	/// </summary>
	/// <param name="fileProperty"></param>
	/// <returns></returns>
	bool IsSqlSelect(XmlNode fileProperty)
	{
		return bool.Parse(fileProperty[cc.tagSqlSelect].InnerText);
	}

	/// <summary>
	/// ダイレクトに呼び出された時の各パラメータを管理する
	/// </summary>
	void ShenlongDirectManager(bool devGroupUser, string remoteUser)
	{
		SetShenDocFolderSession(Request.Params[bb.pmShenDocName]);
		string shenFileName = (string)Session[bb.ssShenlongDocFolder] + (string)Session[bb.ssSubDirectory] + HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]);
		if ( !File.Exists(shenFileName) )
		{
			throw new Exception(HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]) + " が見つかりません.");
		}

		Session[bb.ssShenFileName] = HttpUtility.UrlDecode(Request.Params[bb.pmShenFile]);
		LabelFullPath.Text = HttpUtility.UrlEncode(shenFileName);
		LabelPWD.Text = string.Empty;
#if SAVE_BUILDSQL_TO_SESSION
		Session["LabelBuildSql"] = string.Empty;
		Session["LabelColumnComments"] = string.Empty;
		Session["LabelLogTableNames"] = string.Empty;
		Session["LabelPWD"] = string.Empty;
#else
		LabelBuildSql.Text = string.Empty;
		LabelColumnComments.Text = string.Empty;
		LabelLogTableNames.Text = string.Empty;
#endif

		string buildSql/*, columnComments*/, logTableNames;

		/*XmlDocument xmlShenlongColumn = new XmlDocument();
		xmlShenlongColumn.Load(shenFileName);*/
		Version verShenColumn;
		XmlDocument xmlShenlongColumn = bb.ReadShenlongColumnFile(shenFileName, out verShenColumn);

		LabelSID.Text = xmlShenlongColumn.DocumentElement.Attributes[cc.attrSID].Value;
		LabelUID.Text = xmlShenlongColumn.DocumentElement.Attributes[cc.attrUserName].Value;
		LabelPWD.Text = GetLogOnPassword();

//#if ENABLED_SUBQUERY
		Dictionary<string, string> selectParams = new Dictionary<string, string>();
		if ( 2 <= verShenColumn.Major )
		{
			// ダイレクトに渡された値をセットする
			string[] shenDirects = Request.Params[bb.pmShenDirect].Split(',');	// 転送先のテーブル.カラム名:値
			for ( int i = 0; i < shenDirects.Length; i++ )
			{
				string[] columnValues = shenDirects[i].Split(':');
				string[] tableFieldName = columnValues[0].Split('.');
				string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[@" + cc.attrTableName + "='" + tableFieldName[0] + "']" + "[" + cc.qc.fieldName.ToString() + "='" + tableFieldName[1] + "']";
				XmlNode column = xmlShenlongColumn.SelectSingleNode(xpath);
				if ( column == null )
				{
					Response.Clear();
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "転送先のカラムが見つかりません<br>" + shenDirects[i] + "</span>");
					Response.End();
				}
				if ( column[cc.qc.expression.ToString()].InnerText.Length == 0 )
				{
					Response.Clear();
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "転送先のカラムは条件式ではありません<br>" + shenDirects[i] + "</span>");
					Response.End();
				}

				string baseURI = cc.ToWebAppName(Path.GetFileNameWithoutExtension(xmlShenlongColumn.BaseURI));
				string plainTableFieldName = cc.ToWebAppName(columnValues[0]);
				string sameParamNo = "0";
				selectParams.Add(bb.pmShenlongTextID + baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo, columnValues[1]);
			}
		}
//#else
		else
		{
			// ダイレクトに渡された値をセットする
			string[] shenDirects = Request.Params[bb.pmShenDirect].Split(',');	// 転送先のテーブル.カラム名:値
			for ( int i = 0; i < shenDirects.Length; i++ )
			{
				string[] columnValues = shenDirects[i].Split(':');
				string[] tableFieldName = columnValues[0].Split('.');
				string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[@" + cc.attrTableName + "='" + tableFieldName[0] + "']" + "[" + cc.qc.fieldName.ToString() + "='" + tableFieldName[1] + "']";
				XmlNode column = xmlShenlongColumn.SelectSingleNode(xpath);
				if ( column == null )
				{
					Response.Clear();
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "転送先のカラムが見つかりません<br>" + shenDirects[i] + "</span>");
					Response.End();
				}
				if ( column[cc.qc.expression.ToString()].InnerText.Length == 0 )
				{
					Response.Clear();
					Response.Write("<span style=\"color:Red;font-weight:bold;\">" + "転送先のカラムは条件式ではありません<br>" + shenDirects[i] + "</span>");
					Response.End();
				}
				column[cc.qc.value1.ToString()].InnerText = columnValues[1];
			}
		}
//#endif

		// クエリー項目から SQL を構築する
		List<string> fromTableNames = new List<string>();
		bool result;
//#if ENABLED_SUBQUERY
		if ( 2 <= verShenColumn.Major )
		{
			result = cc.BuildQueryColumnSQL(xmlShenlongColumn, selectParams, true, ASP.global_asax.maxRowNum, out buildSql, out columnComments, ref fromTableNames, 0);
		}
//#else
		else
		{
			result = bb.BuildQueryColumnSQL(xmlShenlongColumn, ASP.global_asax.maxRowNum, out buildSql, out columnComments, ref fromTableNames, 0);
		}
//#endif
		if ( !result )
		{
			Response.Clear();
			Response.Write(columnComments);
			Response.End();
		}

		logTableNames = bb.GetLogTableNames(fromTableNames);
#if SAVE_BUILDSQL_TO_SESSION
		Session["LabelBuildSql"] = HttpUtility.UrlEncode(buildSql);
		Session["LabelColumnComments"] = HttpUtility.UrlEncode(columnComments);
		Session["LabelLogTableNames"] = HttpUtility.UrlEncode(logTableNames);
		Session["LabelPWD"] = LabelPWD.Text;
#else
		LabelBuildSql.Text = HttpUtility.UrlEncode(buildSql);
		LabelColumnComments.Text = HttpUtility.UrlEncode(columnComments);
		LabelLogTableNames.Text = HttpUtility.UrlEncode(logTableNames);
#endif

		// クエリーを実行して GridView を設定する
		ExecuteQuerySetGridView(xmlShenlongColumn, verShenColumn, shenFileName, buildSql, true, devGroupUser, remoteUser, logTableNames);
	}

	/// <summary>
	/// パラメータに shendocnm があれば（直リンクされた時）
	/// shenlongDocFolder, subDirectory セッションをセットする
	/// </summary>
	void SetShenDocFolderSession(string shenDocName)
	{
		if ( string.IsNullOrEmpty(shenDocName) )
			return;

		XmlDocument xmlShenlongDocFolder = new XmlDocument();
		xmlShenlongDocFolder.Load(appDataPathName + "ShenlongDocFolder.xml");
		string xpath = "/" + "shenlong" + "/" + "document" + "[@" + "name" + "='" + shenDocName + "']";
		XmlNode shenDoc = xmlShenlongDocFolder.SelectSingleNode(xpath);
		if ( shenDoc != null )
		{
			Session[bb.ssShenlongDocFolder] = shenDoc.Attributes["folder"].Value + "\\";
		}

		Session[bb.ssSubDirectory] = (Request.Params[bb.pmSubDir] != null) ? HttpUtility.UrlDecode(Request.Params[bb.pmSubDir] + "\\") : "";
		/*if ( ((string)Session[cc.ssSubDirectory] == "") && (Request.Params[cc.ssSubDirectory] != null) )	// 互換性のため
		{
			Session[cc.ssSubDirectory] = HttpUtility.UrlDecode(Request.Params[cc.ssSubDirectory] + "\\");
		}*/

		Session[bb.ssUrlReferer] = (Page.Request.UrlReferrer != null) ? Page.Request.UrlReferrer.ToString() : "(none)";
	}

	/// <summary>
	/// ホームの戻り先をセットする
	/// </summary>
	void SetHyperLinkHome(HyperLink hyperLinkHome, bool noLinkMyself)
	{
		hyperLinkHome.Visible = true;

		if ( Session[bb.ssUrlReferer] != null )	// 直リンクされた？
		{
#if true
			string navigateUrl = null;
			if ( (string)Session[bb.ssUrlReferer] != "(none)" )
			{
				navigateUrl = (string)Session[bb.ssUrlReferer];
			}
			else if ( Page.Request.UrlReferrer != null )
			{
				navigateUrl = Page.Request.UrlReferrer.ToString();
			}

			if ( navigateUrl == null )
			{
				hyperLinkHome.Visible = false;
			}
			else
			{
				hyperLinkHome.NavigateUrl = navigateUrl;
				if ( noLinkMyself && (navigateUrl.IndexOf("shenlong2.aspx") != -1) )
				{
					hyperLinkHome.Visible = false;
				}
			}
#else
			if ( (string)Session[cc.ssUrlReferer] != "(none)" )
			{
				HyperLinkHome.NavigateUrl = (string)Session[cc.ssUrlReferer];
			}
			else
			{
				HyperLinkHome.Visible = false;
			}
#endif
		}
		else
		{
			//hyperLinkHome.NavigateUrl = "./shenlong2.aspx?" + cc.pmShenFile + "=" + Request.Params[cc.pmShenFile];
			if ( Request.Params[bb.pmShenDirect] != null )	// ダイレクトに呼び出された？
			{
				HyperLinkHome.Visible = false;
			}
			else
			{
				string pmShenDocName = (Session[bb.pmShenDocName] != null) ? bb.pmShenDocName + "=" + Session[bb.pmShenDocName] + "&" : "";
				if ( Session[bb.ssHyperLinkHome] == null )
				{
					string pmDevelop = (Session[bb.pmDevelop] != null && bool.Parse((string)Session[bb.pmDevelop])) ? bb.pmDevelop + "=" + "true" : "";
					hyperLinkHome.NavigateUrl = "./Default.aspx" + "?" + pmShenDocName + pmDevelop;
				}
				else if ( (string)Session[bb.ssHyperLinkHome] == "DefaultFrame2.aspx" )
				{
					string ssSubDirectory = (Session[bb.ssSubDirectory] != null) ? bb.ssSubDirectory + "=" + HttpUtility.UrlEncode((string)Session[bb.ssSubDirectory]) : "";
					hyperLinkHome.NavigateUrl = "./DefaultFrame2.aspx" + "?" + pmShenDocName + ssSubDirectory;
				}
			}
		}
	}

	/// <summary>
	/// クエリーを実行して GridView を設定する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="shenFileName"></param>
	/// <param name="buildSql"></param>
	/// <param name="firstPostBack"></param>
	/// <param name="devGroupUser"></param>
	/// <param name="remoteUser"></param>
	/// <param name="logTableNames"></param>
	void ExecuteQuerySetGridView(XmlDocument xmlShenlongColumn, Version verShenColumn, string shenFileName, string buildSql, bool firstPostBack, bool devGroupUser, string remoteUser, string logTableNames)
	{
		//Response.Clear();

		string fileComment = xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagComment].InnerText;
		//this.LabelParamMess.Text = "[" + Path.GetFileNameWithoutExtension(shenFileName) + ((fileComment.Length != 0) ? " : " + fileComment : "") + "]"/* + "<p/>"*/;
		//this.LabelParamMess.ToolTip = string.Empty;
		this.LabelParamMess.Font.Bold = true;

		// クエリーを実行する
		//StringBuilder queryOutput;
		//string[] dataTypeName;
		DataTable/*DataView*/ dataView;
		bb.ot outType = (DropDownOutputType.Text == "HTML") ? bb.ot.html : bb.ot.excel;
		//bb.ExecuteQuery(LabelSID.Text, LabelUID.Text, HttpUtility.UrlDecode(LabelPWD.Text), buildSql, columnComments, outType, out queryOutput, out dataTypeName, out dataView);
		bb.ExecuteQuery(LabelSID.Text, LabelUID.Text, HttpUtility.UrlDecode(LabelPWD.Text), buildSql, out dataTypeName, out dataView);

		this.PanelHeader.Visible = false;
		this.PanelOutputType.Visible = false;
		this.PanelSubmit.Visible = false;
		this.LabelParamMess.Visible = true;

		if ( outType == bb.ot.html )
		{
			SetHyperLinkHome(HyperLinkHome, false);
			HyperLinkShenExcel.Visible = true;
			this.PanelOptions.Visible = true;

			PagerPosition pagerSettingsPosition = (RadioTop.Checked) ? PagerPosition.Top : (RadioBottom.Checked) ? PagerPosition.Bottom : PagerPosition.TopAndBottom;
#if true
			try
			{
				HttpCookie cookie;
				if ( (cookie = Request.Cookies["bubbles"]) == null )
				{
					cookie = new HttpCookie("bubbles");
				}
				cookie.Expires = DateTime.MaxValue;

				if ( cookie.Values["pagerPosition"] == null )
				{
					cookie.Values.Add("pagerPosition", "");
				}
				else
				{
					if ( firstPostBack )
					{
						pagerSettingsPosition = (cookie.Values["pagerPosition"] == PagerPosition.Top.ToString()) ? PagerPosition.Top : (cookie.Values["pagerPosition"] == PagerPosition.Bottom.ToString()) ? PagerPosition.Bottom : PagerPosition.TopAndBottom;

						RadioTop.Checked = RadioBottom.Checked = RadioTopAndBottom.Checked = false;
						if ( pagerSettingsPosition == PagerPosition.Top ) RadioTop.Checked = true;
						else if ( pagerSettingsPosition == PagerPosition.Bottom ) RadioBottom.Checked = true;
						else if ( pagerSettingsPosition == PagerPosition.TopAndBottom ) RadioTopAndBottom.Checked = true;
					}
				}
				cookie.Values["pagerPosition"] = pagerSettingsPosition.ToString();

				Response.AppendCookie(cookie);
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}
#endif

#if true
			if ( (2 <= verShenColumn.Major) && IsSqlSelect(xmlShenlongColumn.DocumentElement[cc.tagProperty]) )
			{
				hyperLink = null;
				classify = null;
			}
			else
			{
				hyperLink = new string[0];
				classify = new string[0];
				foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(cc.tagColumn) )
				{
					if ( !bool.Parse(column[cc.qc.showField.ToString()].InnerText) )
						continue;

					int j = hyperLink.Length;
					Array.Resize(ref hyperLink, j + 1);
					XmlNode bubbles = column[cc.qc.property.ToString()][cc.prop.bubbles.ToString()];
					if ( (bubbles != null) && (bubbles[cc.bubbSet.hyperLink.ToString()] != null) )
					{
						hyperLink[j] = bubbles[cc.bubbSet.hyperLink.ToString()].InnerText;
					}

					j = classify.Length;
					Array.Resize(ref classify, j + 1);
					if ( (bubbles != null) && (bubbles[cc.bubbSet.classify.ToString()] != null) )
					{
						classify[j] = bubbles[cc.bubbSet.classify.ToString()].InnerText;
					}
				}
			}
#endif

			if ( firstPostBack && !devGroupUser )
			{
				bb.WriteAccessLog(ASP.global_asax.writeLogDsnUidPwd, Path.GetFileNameWithoutExtension(shenFileName), logTableNames, LabelSID.Text, LabelUID.Text, remoteUser, bb.ot.html, cc.pno.bubbles);
			}

			//GridView gridView = new GridView();
			//gridView.AllowPaging = true;
			gridView.PagerSettings.Mode = PagerButtons.NumericFirstLast;
			gridView.PageSize = int.Parse(DropDownRowCountPage.Text);
			gridView.DataSource = dataView;

			//gridView.Attributes.Add("border", "1");
			//gridView.Attributes.Add("cellspacing", "0");
			//gridView.Attributes.Add("cellpadding", "3");
			//gridView.Attributes.Add("bordercolordark", "#777777"/*"#ffffff"*/);
			gridView.Attributes.Add("bordercolor", "#777777");
			//gridView.Attributes.Add("bgcolor", "white");

			gridView.PagerSettings.Position = pagerSettingsPosition;

			DataBind();

			int maxRowNum = ASP.global_asax.maxRowNum;
			// ROWNUM の最大指定あり？
			if ( cc.HasMaxRowNum(xmlShenlongColumn) )
			{
				maxRowNum = int.Parse(xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagMaxRowNum].InnerText);
			}
			bool maxRowLimit = (dataView.Rows.Count == maxRowNum);
			//this.LabelParamMess.Text += ("　" + (maxRowLimit ? "<span style=\"color:Red;\">" : "") + "(総数: " + dataView.Rows.Count + "件)" + (maxRowLimit ? "</span>" : ""));
			this.LabelParamMess.Text = "[" + Path.GetFileNameWithoutExtension(shenFileName) + "]　" +
									   (maxRowLimit ? "<span style=\"color:Red;\">" : "") + "(総数: " + dataView.Rows.Count + "件" + (maxRowLimit ? "以上" : "") + ")" + (maxRowLimit ? "</span>" : "")/* + "　" +
									   ((fileComment.Length != 0) ? "(" + fileComment + ")" : "")*/;
			this.LabelParamMess.ToolTip = fileComment;
		}
		/*else
		{
			Response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(Path.GetFileNameWithoutExtension(shenFileName)) + ".xls");
			//Encoding sjisEnc = Encoding.GetEncoding("shift_jis");
			//byte[] excelType = sjisEnc.GetBytes(queryOutput.ToString());
			//Response.Write(sjisEnc.GetString(excelType));
			Response.Write(queryOutput);
			Response.End();
			//Response.Redirect("./shenlong2.aspx?" + pmShenFile + "=" + Request.Params[pmShenFile]);
		}*/

		/*if ( firstPostBack && !devGroupUser )
		{
			cc.WriteAccessLog(Path.GetFileNameWithoutExtension(shenFileName), remoteUser);
		}*/
	}

	/// <summary>
	/// gridView_PreRender
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	void gridView_PreRender(object sender, EventArgs e)
	{
		//if ( Page.IsPostBack )
		{
			this.LabelParamMess.Text += ("　" + (gridView.PageIndex + 1) + "/" + gridView.PageCount + "ページ" + "</p>");

			if ( gridView.PageCount <= 1 )
			{
				PanelOptions.Visible = false;
			}
		}
	}

	/// <summary>
	/// gridView_RowDataBound
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void gridView_RowDataBound(object sender, GridViewRowEventArgs e)
	{
		try
		{
			if ( e.Row.RowType == DataControlRowType.Header )
			{
				if ( columnComments != null )
				{
					string[] colComments = columnComments.Split(cc.sepOutput[0]);
					for ( int i = 0; i < e.Row.Cells.Count; i++ )
					{
						e.Row.Cells[i].ToolTip = colComments[i];
					}
				}

				if ( hyperLink != null )	// ハイパーリンクの転送元のカラム名を設定する
				{
					for ( int i = 0; i < hyperLink.Length; i++ )
					{
						try
						{
							if ( string.IsNullOrEmpty(hyperLink[i]) )
								continue;

							string[] eggTransmit = hyperLink[i].Split(':');	// シェンロンの卵 + ":" + 転送元のカラム名 + ">" + 転送先のテーブル.カラム名 + ("," + 転送元…)
							string[] transmit = eggTransmit[1].Split(',');

							for ( int j = 0; j < transmit.Length; j++ )
							{
								string[] sourceDest = transmit[j].Split('>');
#if true
								if ( sourceDest[0][0] == '\"' )	// 転送元は直接データ指定？
									continue;
#endif
								for ( int k = 0; k < e.Row.Cells.Count; k++ )
								{
									string headerText = (e.Row.Cells[i].HasControls() ? ((LinkButton)e.Row.Cells[k].Controls[0]).Text : e.Row.Cells[k].Text);
									if ( sourceDest[0] == headerText )
									{
										sourceDest[0] = k.ToString();	// 転送元のカラム名を検索して、何番目にあたるかと入れ替える
										break;
									}
								}
								transmit[j] = sourceDest[0] + ">" + sourceDest[1];
							}
							hyperLink[i] = eggTransmit[0] + ":" + string.Join(",", transmit);
						}
						catch ( Exception exp )
						{
							if ( e.Row.Cells[i].HasControls() )
								((LinkButton)e.Row.Cells[i].Controls[0]).Text = exp.Message;
							else
								e.Row.Cells[i].Text = exp.Message;
							Debug.WriteLine(exp.Message);
						}
					}
				}
			}
			else if ( e.Row.RowType == DataControlRowType.DataRow )
			{
				for ( int i = 0; i < e.Row.Cells.Count; i++ )
				{
					try
					{
						// ハイパーリンクを設定する
						if ( (hyperLink != null && i < hyperLink.Length) && !string.IsNullOrEmpty(hyperLink[i]) )
						{
							string[] eggTransmit = hyperLink[i].Split(':');		// シェンロンの卵 + ":" + 転送情報
							string shenfile = HttpUtility.UrlEncode(eggTransmit[0]) + ((eggTransmit[0].IndexOf(".xml") == -1) ? ".xml" : "");
							StringBuilder navigateUrl = new StringBuilder("./shenlong2.aspx?" + bb.pmShenFile + "=" + shenfile + "&");

							string[] transmit = eggTransmit[1].Split(',');
							navigateUrl.Append(bb.pmShenDirect + "=");

							for ( int j = 0; j < transmit.Length; j++ )
							{
								string[] sourceDest = transmit[j].Split('>');	// 転送元 + ">" + 転送先
#if false
								if ( !Char.IsDigit(sourceDest[0][0]) )
									throw new Exception(transmit[j]);
								navigateUrl.Append(HttpUtility.UrlEncode(sourceDest[1]) + ":" + e.Row.Cells[int.Parse(sourceDest[0])].Text + ((j != transmit.Length - 1) ? "," : ""));
#else
								string dest = HttpUtility.UrlEncode(sourceDest[1]);
								string source;
								if ( !Char.IsDigit(sourceDest[0][0]) )
								{	
									// 転送元は直接データ指定ではない？
									if ( (sourceDest[0][0] != '\"') || (sourceDest[0][sourceDest[0].Length - 1] != '\"') )
										throw new Exception(transmit[j]);
									source = sourceDest[0].Substring(1, sourceDest[0].Length - 2);
								}
								else
								{
									source = e.Row.Cells[int.Parse(sourceDest[0])].Text;
								}
								navigateUrl.Append(dest + ":" + source + ((j != transmit.Length - 1) ? "," : ""));
#endif
							}

							//e.Row.Cells[i].Text = "<a href=\"./shenlong2.aspx?" + cc.pmShenFile + "=" + shenfile + "&" + cc.pmShenDirect + "=" + tableFieldName + ":" + e.Row.Cells[i].Text + "\" target=\"_blank\">" + e.Row.Cells[i].Text + "</a>";
							//e.Row.Cells[i].Text = "<a href=\"" + navigateUrl + "\" target=\"_blank\">" + e.Row.Cells[i].Text + "</a>";
							HyperLink hyperlink = new HyperLink();
							hyperlink.Text = e.Row.Cells[i].Text;
							hyperlink.NavigateUrl = navigateUrl.ToString();
							hyperlink.Target = "_blank"/*"linkwindow"*/;
							e.Row.Cells[i].Controls.Add(hyperlink);
						}

						// 色分けを設定する
						if ( (classify != null && i < classify.Length) && !string.IsNullOrEmpty(classify[i]) )
						{
							//char [] compChar = new char []{'>', '<', '='};
							string[] classifies = classify[i].Split(',');	// 比較演算子 + 比較値 + ":" + 背景色 + "/" + 文字色 + "/" + オプション + ("," + 比較演算子…)

							for ( int j = 0; j < classifies.Length; j++ )
							{
								int k = 0, value = -1, cologne = -1;
								for ( ; k < 2 && !Char.IsLetterOrDigit(classifies[j][k]); k++ ) ;
								value = k;
								cologne = classifies[j].IndexOf(':', value);
								if ( /*value == -1 ||*/ cologne == -1 )
									throw new Exception(classifies[j]);

								string compOperator = classifies[j].Substring(0, value).TrimEnd();
								string[] colors = classifies[j].Substring(cologne + 1).Split('/');	// 背景色 + "/" + 文字色 + "/" + オプション

								int res = string.Compare(e.Row.Cells[i].Text, classifies[j].Substring(value, cologne - value));
								if ( (compOperator == ">=" && res >= 0) || (compOperator == ">" && res > 0) ||
									 (compOperator == "=" && res == 0) || (compOperator == "<>" && res != 0) ||
									 (compOperator == "<=" && res <= 0) || (compOperator == "<" && res < 0) )
								{
									bool fullRow = ((3 <= colors.Length) && (colors[2] == "FULLROW"));
									if ( colors[0].Length != 0 )	// 背景色の指定あり？
									{
										if ( fullRow )
											e.Row.BackColor = Color.FromName(colors[0]);
										else
											e.Row.Cells[i].BackColor = Color.FromName(colors[0]);
									}
									if ( (2 <= colors.Length) && (colors[1].Length != 0) )		// 文字色の指定あり？
									{
										if ( fullRow )
											e.Row.ForeColor = Color.FromName(colors[1]);
										else
											e.Row.Cells[i].ForeColor = Color.FromName(colors[1]);
									}
									break;
								}
							}
						}
					}
					catch ( Exception exp )
					{
						e.Row.Cells[i].Text = exp.Message/*HttpUtility.UrlEncode(exp.Message)*/;
						Debug.WriteLine(exp.Message);
					}

					if ( !cc.IsCharColumn(dataTypeName[i]) && !dataTypeName[i].StartsWith("DATE") )
					{
						e.Row.Cells[i].HorizontalAlign = HorizontalAlign.Right;
					}
				}
				////  UnitPrice および QuantityTotal をそれぞれの累計用変数に加算します。
				//priceTotal += Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "UnitPrice"));
				//quantityTotal += Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "Quantity"));
			}
			/*else if ( e.Row.RowType == DataControlRowType.Footer )
			{
				e.Row.Cells[0].Text = "Totals:";
				// フッターに、累計を表示します。
				e.Row.Cells[1].Text = priceTotal.ToString("c");
				e.Row.Cells[2].Text = quantityTotal.ToString("d");

				e.Row.Cells[1].HorizontalAlign = e.Row.Cells[2].HorizontalAlign = HorizontalAlign.Right;
				e.Row.Font.Bold = true;
			}*/
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
		}
	}

	/// <summary>
	/// gridView_RowCreated
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void gridView_RowCreated(object sender, GridViewRowEventArgs e)
	{
		try
		{
			if ( e.Row.RowType == DataControlRowType.Pager )
			{
				Control pageControl = e.Row.Cells[0].Controls[0].Controls[0];
				foreach ( Control control in pageControl.Controls )
				{
					if ( !(control is TableCell) )
						continue;
					foreach ( Control _control in ((TableCell)control).Controls )
					{
						if ( _control is Label )
						{
							Label label = (Label)_control;
							label.Font.Underline = true;
							/*if ( Char.IsDigit(label.Text[0]) )
							{
								label.Text = "0" + label.Text;
							}*/
						}
						/*else if ( _control is LinkButton )
						{
							LinkButton linkButton = (LinkButton)_control;
							if ( Char.IsDigit(linkButton.Text[0]) && (linkButton.Text.Length == 1) )	// 数字１桁のページ番号？
							{
								linkButton.Text = "0" + linkButton.Text;
							}
						}*/
					}
				}

				// 前のページ
				LinkButton lbp = new LinkButton();
				lbp.CommandName = "Page";
				lbp.CommandArgument = "Prev";
				lbp.Text = "<&nbsp;";
				TableCell tc2 = new TableCell();
				tc2.Enabled = (gridView.PageIndex != 0);
				tc2.Controls.Add(lbp);
				pageControl.Controls.AddAt(0, tc2);

				// 次のページ
				LinkButton lbn = new LinkButton();
				lbn.CommandName = "Page";
				lbn.CommandArgument = "Next";
				lbn.Text = "&nbsp;>";
				TableCell tc1 = new TableCell();
				tc1.Enabled = (gridView.PageIndex != gridView.PageCount - 1);
				tc1.Controls.Add(lbn);
				pageControl.Controls.Add(tc1);
			}
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
		}
	}

	/// <summary>
	/// GridView_PageIndexChanging
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void GridView_PageIndexChanging(object sender, GridViewPageEventArgs e)
	{
#if ALLOW_SORTING
		gridView.DataSource = SortDataTable(gridView.DataSource as DataTable, true);
		gridView.PageIndex = e.NewPageIndex;
		gridView.DataBind();
#else
		((GridView)sender).PageIndex = e.NewPageIndex;
		DataBind();
#endif
	}

#if ALLOW_SORTING
	/// <summary>
	/// gridView_Sorting
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void gridView_Sorting(object sender, GridViewSortEventArgs e)
	{
#if true
		GridViewSortExpression = e.SortExpression;
		int pageIndex = gridView.PageIndex;
		gridView.DataSource = SortDataTable(gridView.DataSource as DataTable, false);
		gridView.DataBind();
		gridView.PageIndex = pageIndex;
#else
		StringBuilder queryOutput;
		DataView dataView;
		cc.ot outType = cc.ot.html;
		string buildSql = HttpUtility.UrlDecode(LabelBuildSql.Text);
		int idxOrderBy = buildSql.IndexOf("ORDER BY");
		if ( idxOrderBy != -1 )
		{
			//int idxEndOrderBy = buildSql.IndexOf("\r\n", idxOrderBy);
			buildSql = buildSql.Substring(0, idxOrderBy)/* + buildSql.Substring(idxEndOrderBy + 2)*/;
		}
		buildSql += "ORDER BY " + e.SortExpression + "\r\n";
		string columnComments = HttpUtility.UrlDecode(LabelColumnComments.Text);
		cc.ExecuteQuery(LabelSID.Text, LabelUID.Text, HttpUtility.UrlDecode(LabelPWD.Text), buildSql, columnComments, outType, out queryOutput, out dataTypeName, out dataView);
		gridView.DataSource = dataView;
		DataBind();
#endif
	}

	// C# GridView Sorting/Paging w/o a DataSourceControl DataSource
	// http://community.strongcoders.com/content/CSGridViewSortingPaging.aspx

	private string GridViewSortDirection
	{
		get { return ViewState["SortDirection"] as string ?? "DESC"/*"ASC"*/; }
		set { ViewState["SortDirection"] = value; }
	}

	private string GridViewSortExpression
	{
		get { return ViewState["SortExpression"] as string ?? string.Empty; }
		set { ViewState["SortExpression"] = value; }
	}

	private string GetSortDirection()
	{
		switch ( GridViewSortDirection )
		{
			case "ASC":
				GridViewSortDirection = "DESC";
				break;
			case "DESC":
				GridViewSortDirection = "ASC";
				break;
		}

		return GridViewSortDirection;
	}

	protected DataView SortDataTable(DataTable dataTable, bool isPageIndexChanging)
	{
		if ( dataTable != null )
		{
			DataView dataView = new DataView(dataTable);

			if ( GridViewSortExpression != string.Empty )
			{
				if ( isPageIndexChanging )
				{
					dataView.Sort = string.Format("{0} {1}", GridViewSortExpression, GridViewSortDirection);
				}
				else
				{
					dataView.Sort = string.Format("{0} {1}", GridViewSortExpression, GetSortDirection());
				}
			}

			return dataView;
		}
		else
		{
			return new DataView();
		}
	}
#endif

	/// <summary>
	/// LabelSID/LabelUID に対するパスワードを取得する
	/// </summary>
	/// <returns></returns>
	private string GetLogOnPassword()
	{
		string password = null;

		try
		{
			XmlDocument xmlLogOn = new XmlDocument();
			xmlLogOn.Load(appDataPathName + @"\LogOn.xml");
#if false
			string xpath = "/" + "root" + "/" + "logOn" + "[@" + "sid" + "='" + LabelSID.Text + "'][" + "userName" + "='" + LabelUID.Text + "']";
			XmlNode logOnNode = xmlLogOn.SelectSingleNode(xpath);
			if ( logOnNode != null )
			{
				password = cc.DecodePassword(logOnNode["password"].InnerText);
			}
#else
			foreach ( XmlNode logOnNode in xmlLogOn.DocumentElement.ChildNodes )
			{
				if ( (string.Compare(LabelSID.Text, logOnNode.Attributes["sid"].Value, true) == 0) &&
					 (string.Compare(LabelUID.Text, logOnNode["userName"].InnerText, true) == 0) )	// 履歴に存在しているログオン情報？
				{
					password = bb.DecodePassword(logOnNode["password"].InnerText);
					break;
				}
			}
#endif
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
		}

		return password;
	}

//#if ENABLED_SUBQUERY
	/// <summary>
	/// AppendShenlongParamControlForBase
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	private void AppendShenlongParamControlForBase(string shenColumnBaseURI, XmlDocument xmlShenlongColumn, ref StringBuilder jsCheckTextBox, ref int shenlongParamCount)
	{
		shenlongParamCount += AppendShenlongParamControl(xmlShenlongColumn, ref jsCheckTextBox);

#if ENABLED_SUBQUERY
		XmlNode fileProperty = xmlShenlongColumn.DocumentElement[cc.tagProperty];

		if ( fileProperty == null )
			return;

		if ( (fileProperty[cc.tagSubQuery] == null) || (fileProperty[cc.tagSubQuery].InnerText.Length == 0) )
			return;

		foreach ( string subQuery in fileProperty[cc.tagSubQuery].InnerText.Split(cc.SUBQUERY_SEPARATOR) )
		{
			xmlShenlongColumn = cc.ReadSubQueryFile(subQuery, shenColumnBaseURI);
			AppendShenlongParamControlForBase(shenColumnBaseURI, xmlShenlongColumn, ref jsCheckTextBox, ref shenlongParamCount);
		}
#endif
	}

	/// <summary>
	/// shenlong のパラメータ箇所をテキスト コントロールとして追加する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="debugMode"></param>
	/// <returns></returns>
	private int AppendShenlongParamControl(XmlDocument xmlShenlongColumn, ref StringBuilder jsCheckTextBox)
	{
		bool debugMode = false;
		Dictionary<string, int> paramNames = new Dictionary<string, int>();
		int shenlongParamCount = 0;
		Label lastLabelRColOp = null;

		//string baseURI = Path.GetFileNameWithoutExtension(xmlShenlongColumn.BaseURI).Replace('-', '―');
		string baseURI = cc.ToWebAppName(Path.GetFileNameWithoutExtension(xmlShenlongColumn.BaseURI));

		// TableTextInput テーブルの設定
		//TableTextInput.Caption = "<br>";
		TableTextInput.HorizontalAlign = HorizontalAlign.Center;
		TableTextInput.Attributes.Add("border", "0");
		TableTextInput.Attributes.Add("cellspacing", "0");
		TableTextInput.Attributes.Add("cellpadding", "4");

		TableRow row = null;
		TableCell cell = null;

		XmlNode fileProperty = xmlShenlongColumn.DocumentElement[cc.tagProperty];

		string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[" + cc.qc.expression.ToString() + "!='']";
		XmlNodeList columnWithExpression = xmlShenlongColumn.SelectNodes(xpath);

		foreach ( XmlNode column in columnWithExpression/*xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn)*/ )
		{
			string expression = column[cc.qc.expression.ToString()].InnerText;
			/*if ( expression.Length == 0 )
				continue;*/

			string tableName = cc.GetTableName(column.Attributes[cc.attrTableName].Value, false);
			string fieldName = column[cc.qc.fieldName.ToString()].InnerText;
			string comment = column[cc.qc.property.ToString()][cc.prop.comment.ToString()].InnerText;
			bool withComment = (comment != cc.propNoComment);
			XmlNode bubbles = column[cc.qc.property.ToString()][cc.prop.bubbles.ToString()];
			cc.bubbCtrl bubbCtrl = cc.bubbCtrl.textBox;

			if ( bubbles != null )
			{
				string control = bubbles.Attributes[cc.bubbSet.control.ToString()].Value;
				if ( control == cc.bubbCtrl.noVisible.ToString() )
					continue;
				else if ( control == cc.bubbCtrl.textBox.ToString() )
					bubbCtrl = ((bubbles[cc.bubbSet.dropDownList.ToString()] != null && bubbles[cc.bubbSet.dropDownList.ToString()].InnerText.Length == 0) || (bubbles["dropDownSql"] != null && bubbles["dropDownSql"].InnerText.Length == 0)) ? cc.bubbCtrl.textBox : cc.bubbCtrl.dropDownList;
				else if ( control == cc.bubbCtrl.label.ToString() )
					bubbCtrl = cc.bubbCtrl.label;
			}

#if NEW_GETPLAINTABLEFIELDNAME
			string fieldAliasName;
			string plainFieldName = cc.GetPlainTableFieldName(fieldName, out fieldAliasName);
#else
			int fieldAsIndex;
			string plainFieldName = cc.GetPlainTableFieldName(fieldName, out fieldAsIndex);
#endif
			/*string plainTableFieldName = tableName + bb.pmShenlongTextIdJoin + plainFieldName;
			plainTableFieldName = plainTableFieldName.Replace('.', bb.pmShenlongTextIdJoin[0]);*/
			string plainTableFieldName = tableName + "." + plainFieldName;
			plainTableFieldName = cc.ToWebAppName(plainTableFieldName);

			int sameParamNo = 0;
			if ( !paramNames.TryGetValue(plainTableFieldName, out sameParamNo) )
			{
				paramNames[plainTableFieldName] = sameParamNo;
			}
			else
			{
				sameParamNo = ++paramNames[plainTableFieldName];
			}

			row = new TableRow();

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Right;
			cell.VerticalAlign = VerticalAlign.Middle;

			//string labelText = ((fieldAsIndex != -1) ? fieldName.Substring(fieldAsIndex + 4).Trim() : ((withComment) ? comment : /*tableName + "." + */plainFieldName/*tableFieldName*/));
			string labelText = plainFieldName;
#if NEW_GETPLAINTABLEFIELDNAME
			if ( fieldAliasName != null )
			{
				labelText = fieldAliasName.Trim(" \"".ToCharArray());
#else
			if ( fieldAsIndex != -1 )
			{
				labelText = fieldName.Substring(fieldAsIndex + 4).Trim(" \"".ToCharArray());
#endif
			}
			else
			{
				XmlNode alias = column[cc.qc.property.ToString()][cc.prop.alias.ToString()];
				if ( alias != null )
				{
#if NEW_GETPLAINTABLEFIELDNAME
					fieldAliasName = alias.InnerText;
					labelText = fieldAliasName;
#else
					labelText = alias.InnerText;
#endif
				}
				else
				{
					if ( withComment )
					{
						labelText = comment;
					}
				}
			}
					
			string value1 = column[cc.qc.value1.ToString()].InnerText.Trim();
			string value2 = column[cc.qc.value2.ToString()].InnerText.Trim();

			Label label;
			string usersRoundBlanket = cc.GetUsersRoundBlanket(ref value2);
			string _text = "<br>" +
						   (usersRoundBlanket != null && usersRoundBlanket[0] == '(' ? "<font color=\"lightgray\">" + usersRoundBlanket + "</font>" : "") +
						   labelText + "　" + expression + "　";
			string _id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			string _tooltip = (withComment || (fieldAliasName != null)/*(fieldAsIndex != -1)*/) ? tableName + "." + plainFieldName/*tableFieldName*/: "";
			label = MakeLabel(bb.pmShenlongLabelID + _id, _text, SystemColors.WindowText, _tooltip);
			cell.Controls.Add(label);
			row.Cells.Add(cell);

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Left;
			cell.VerticalAlign = VerticalAlign.Bottom;

			TextBox textBox = null;
			//string value1 = column[cc.qc.value1.ToString()].InnerText.Trim();
			bool necessary = false;

			if ( value1.Length != 0 )
			{
				bool setValue = false;

				if ( bubbCtrl == cc.bubbCtrl.textBox )
				{
					if ( fileProperty[cc.tagSetValue] != null )
					{
						setValue = bool.Parse(fileProperty[cc.tagSetValue].InnerText);
					}
					if ( (!setValue) && (bubbles != null) && (bubbles.Attributes[cc.bubbSet.setValue.ToString()] != null) )
					{
						setValue = bool.Parse(bubbles.Attributes[cc.bubbSet.setValue.ToString()].Value);
					}

					_text = (setValue) ? value1 : string.Empty;
					_tooltip = (debugMode) ? string.Empty : value1;
					_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
					textBox = MakeTextBox(_id, _text, _tooltip, column);
					cell.Controls.Add(textBox);

					CheckTextBoxInputNecessary(bubbles, textBox, labelText, ref jsCheckTextBox, ref necessary);

#if WITH_CALENDAR_CONTROL
					string type = column[cc.qc.fieldName.ToString()].Attributes[cc.prop.type.ToString()].Value;
					if ( type == "DATE" )
					{
						_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
						XmlNode dateFormat = column[cc.qc.property.ToString()][cc.prop.dateFormat.ToString()];
						MakeDatePickerLink(_id, cell, (dateFormat == null) ? cc.sqlDateFormat : dateFormat.InnerText);
					}
#endif
				}
				else if ( bubbCtrl == cc.bubbCtrl.label )
				{
					_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
					label = MakeLabel(bb.pmShenlongFixTextID + _id, value1, SystemColors.WindowText, "");
					cell.Controls.Add(label);
				}
				else if ( bubbCtrl == cc.bubbCtrl.dropDownList )
				{
					string _sql = (bubbles[cc.bubbSet.dropDownList.ToString()] != null) ? bubbles[cc.bubbSet.dropDownList.ToString()].InnerText : bubbles["dropDownSql"].InnerText;
					_tooltip = value1;
					_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
					DropDownList dropDownList = MakeDropDownList(_id, _sql, _tooltip);
					cell.Controls.Add(dropDownList);
				}

				if ( expression == "BETWEEN" )
				{
					if ( bubbCtrl == cc.bubbCtrl.dropDownList )
					{
						label.Text = label.Text.Replace("　BETWEEN　", "　＝　");
					}
					else
					{
						label = MakeLabel(bb.pmShenlongLabelID, "　" + "AND" + "　", SystemColors.WindowText, "");
						cell.Controls.Add(label);
					}

					//string value2 = column[cc.qc.value2.ToString()].InnerText.Trim();

					if ( bubbCtrl == cc.bubbCtrl.textBox )
					{
						_text = (setValue) ? value2 : string.Empty;
						_tooltip = (debugMode) ? string.Empty : value2;
						_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo + "HI";
						textBox = MakeTextBox(_id, _text, _tooltip, column);
						cell.Controls.Add(textBox);

						CheckTextBoxInputNecessary(bubbles, textBox, labelText, ref jsCheckTextBox, ref necessary);

#if WITH_CALENDAR_CONTROL
						string type = column[cc.qc.fieldName.ToString()].Attributes[cc.prop.type.ToString()].Value;
						if ( type == "DATE" )
						{
							_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo + "HI";
							XmlNode dateFormat = column[cc.qc.property.ToString()][cc.prop.dateFormat.ToString()];
							MakeDatePickerLink(_id, cell, (dateFormat == null) ? cc.sqlDateFormat : dateFormat.InnerText);
						}
#endif
					}
					else if ( bubbCtrl == cc.bubbCtrl.label )
					{
						_id = baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo + "HI";
						label = MakeLabel(bb.pmShenlongFixTextID + _id, value2, SystemColors.WindowText, "");
						cell.Controls.Add(label);
					}
				}

				shenlongParamCount++;
			}
			else
			{
				/**/
				if ( sameParamNo == 0 )
				{
					paramNames.Remove(plainTableFieldName);
				}
				else
				{
					paramNames[plainTableFieldName]--;
				}

				if ( expression.IndexOf("NULL") == -1 )			// IS [NOT] NULL 以外？
				{
					row.Cells.Remove(cell);
					continue;
				}
				/**/
			}

			string rColOp = column[cc.qc.rColOp.ToString()].InnerText;
			_text = (necessary ? "<font size=\"1\" color=\"red\">&nbsp;*</font>" : "") + 
					(usersRoundBlanket != null && usersRoundBlanket[0] == ')' ? "<font color=\"lightgray\">" + usersRoundBlanket + "</font>" : "") +
					"　" + ((rColOp.Length != 0) ? rColOp : "AND");
			_id = "LabelRColOp" + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			label = MakeLabel(_id, _text, Color.LightGray/*Color.Gray*/, "");
			cell.Controls.Add(label);
			lastLabelRColOp = label;

			row.Cells.Add(cell);
			TableTextInput.Rows.Add(row);
		}

		if ( lastLabelRColOp != null )	// 最後にラベル化された右列連結がある？
		{
			/*this.PanelParam.Controls.Remove(lastLabelRColOp);*/
			/*cell.Controls.Remove(lastLabelRColOp);*/
			string text = lastLabelRColOp.Text;
			int length = text.Length;
			if ( text.EndsWith("　AND") )
				lastLabelRColOp.Text = text.Substring(0, text.Length - 4);
			else if ( text.EndsWith("　OR") )
				lastLabelRColOp.Text = text.Substring(0, text.Length - 3);
		}

		return shenlongParamCount;
	}

	/// <summary>
	/// Label を作成する
	/// </summary>
	/// <param name="id"></param>
	/// <param name="text"></param>
	/// <param name="foreColor"></param>
	/// <param name="tooltip"></param>
	/// <returns></returns>
	private Label MakeLabel(string id, string text, Color foreColor, string tooltip)
	{
		Label label = new Label();
		label.Text = text;
		label.ID = id;
		label.ForeColor = foreColor;
		label.ToolTip = tooltip;
		return label;
	}

#if true
	/// <summary>
	/// TextBox を作成する
	/// </summary>
	/// <param name="id"></param>
	/// <param name="text"></param>
	/// <param name="tooltip"></param>
	/// <param name="column"></param>
	/// <returns></returns>
	private TextBox MakeTextBox(string id, string text, string tooltip, XmlNode column)
	{
		TextBox textBox = new TextBox();

		string type = column[cc.qc.fieldName.ToString()].Attributes[cc.prop.type.ToString()].Value;

		if ( !string.IsNullOrEmpty(text) && (type == "DATE") )
		{
			XmlNode dateFormat = column[cc.qc.property.ToString()][cc.prop.dateFormat.ToString()];
			text = TextToDateTime(text, (dateFormat == null) ? cc.sqlDateFormat : dateFormat.InnerText);
		}

		textBox.Text = text;
		textBox.ID = bb.pmShenlongTextID + id;
		textBox.ToolTip = tooltip;

		return textBox;
	}

	/// <summary>
	/// テキストを日時へ変換する
	/// </summary>
	/// <param name="value"></param>
	/// <param name="dateFormat"></param>
	/// <returns></returns>
	private string TextToDateTime(string text, string dateFormat)
	{
		try
		{
			string value = text;

			if ( value.IndexOf("sysdate", StringComparison.CurrentCultureIgnoreCase) != -1 )
			{
				OracleConnection oleConn = null;
				OracleCommand oleCmd = null;
				OracleDataReader oleReader = null;

				try
				{
					string password = GetLogOnPassword();
					if ( password == null )
						return text;

					oleConn = OpenOracle(LabelSID.Text, LabelUID.Text, password);
					string toChar = (value[0] == '(') ? "to_char" : "";
					string dateQuote = (Char.IsDigit(value[0])) ? "'" : "";
					string _value = toChar + dateQuote + value + dateQuote;
					string sql = "SELECT " + _value + " FROM DUAL";
					oleCmd = new OracleCommand(sql, oleConn);
					oleReader = oleCmd.ExecuteReader();
					if ( oleReader.Read() )
					{
						value = oleReader[0].ToString();
					}
				}
				finally
				{
					CloseOracle(ref oleConn, ref oleCmd, ref oleReader);
				}
			}
			if ( value[0] == '@' )
			{
				string slash = (dateFormat.IndexOf("/") == -1) ? "" : "/";
				string colon = (dateFormat.IndexOf(":") == -1) ? "" : ":";

				if ( value == "@TODAY" )
				{
					value = DateTime.Today.ToString("yyyy" + slash + "MM" + slash + "dd");
				}
				else if ( value == "@NOW" )
				{
					value = DateTime.Now.ToString("yyyy" + slash + "MM" + slash + "dd HH" + colon + "mm" + (dateFormat.IndexOf("ss") == -1 ? "" : colon + "ss"));
				}
			}

			return value;
		}
		catch ( Exception exp )
		{
			Debug.WriteLine(exp.Message);
			return text;
		}
	}
#else
	/// <summary>
	/// TextBox を作成する
	/// </summary>
	/// <param name="id"></param>
	/// <param name="text"></param>
	/// <param name="tooltip"></param>
	/// <returns></returns>
	private TextBox MakeTextBox(string id, string text, string tooltip, XmlNode column)
	{
		TextBox textBox = new TextBox();
		textBox.Text = text;
		textBox.ID = bb.pmShenlongTextID + id;
		textBox.ToolTip = tooltip;

		if ( text.IndexOf("sysdate", StringComparison.CurrentCultureIgnoreCase) != -1 )
		{
			OleDbConnection oleConn = null;
			OleDbCommand oleCmd = null;
			OleDbDataReader oleReader = null;

			try
			{
				string password = GetLogOnPassword();
				if ( password == null )
					return textBox;

				oleConn = OpenOracle(LabelSID.Text, LabelUID.Text, password);

				string toChar = (text[0] == '(') ? "to_char" : "";
				string dateQuote = (Char.IsDigit(text[0])) ? "'" : "";
				//text = "to_date(" + toChar + dateQuote + text + dateQuote + ",'" + cc.sqlDateFormat + "')";
				text = toChar + dateQuote + text + dateQuote + ",'" + cc.sqlDateFormat + "'";
				string sql = "SELECT " + text + " FROM DUAL";
				oleCmd = new OleDbCommand(sql, oleConn);
				oleReader = oleCmd.ExecuteReader();
				if ( oleReader.Read() )
				{
					text = oleReader[0].ToString();
					//if ( text.EndsWith(" 0:00:00") || text.EndsWith("00:00:00") )
					/*if ( text.EndsWith(" 0000") )
					{
						text = text.Split(' ')[0];
					}*/
				}
			}
			finally
			{
				CloseOracle(ref oleConn, ref oleCmd, ref oleReader);
			}
		}
		else if ( text == "00000000" )
		{
			text = DateTime.Today.ToString("yyyyMMdd");
		}

		textBox.Text = text.Replace("/", "").Replace(":", "");

		return textBox;
	}
#endif

	/// <summary>
	/// 必須入力のテキストボックスであれば、JavaScript でチェックするようにする
	/// </summary>
	/// <param name="bubbles"></param>
	/// <param name="textBox"></param>
	/// <param name="labelText"></param>
	/// <param name="jsCheckTextBox"></param>
	/// <param name="necessary"></param>
	private void CheckTextBoxInputNecessary(XmlNode bubbles, TextBox textBox, string labelText, ref StringBuilder jsCheckTextBox, ref bool necessary)
	{
		if ( (bubbles == null) || (bubbles.Attributes[cc.bubbSet.input.ToString()] == null) ||
			 (bubbles.Attributes[cc.bubbSet.input.ToString()].Value != cc.bubbInput.necessary.ToString()) )
			return;

		jsCheckTextBox.Append("if (document.getElementById('" + textBox.ID + "').value == \"\") {\r\n");
		jsCheckTextBox.Append("  window.alert('" + labelText + " は必須入力です');\r\n");
		jsCheckTextBox.Append("  document.getElementById('" + textBox.ID + "').focus();\r\n");
		jsCheckTextBox.Append("  return(false);\r\n");
		jsCheckTextBox.Append("}\r\n");
		necessary = true;
	}

	/// <summary>
	/// DatePicker のリンクを作成する
	/// </summary>
	/// <param name="id"></param>
	/// <param name="cell"></param>
	/// <param name="dateFormat"></param>
	/// <returns></returns>
	private void MakeDatePickerLink(string id, TableCell cell, string dateFormat)
	{
		string textBoxID = bb.pmShenlongTextID + id;

		Label label = new Label();
		label.Text = " ";
		cell.Controls.Add(label);

		string _dateFormat = dateFormat.Split(' ')[0];
		_dateFormat = _dateFormat.Replace("mm", "MM");

		HyperLink hyperLink = new HyperLink();
		hyperLink.ID = "_Hyper" + id;
		hyperLink.Text = ">>";
		hyperLink.ImageUrl = "./images/calendar.gif";
		hyperLink.ToolTip = "カレンダーから選択する"/*"Pick Date from Calendar"*/;
		hyperLink.NavigateUrl = "javascript:;";
		hyperLink.Attributes.Add("onclick",
								 "CalendarPicker(" +
								 "'" + this.form1.Name + "." + HttpUtility.UrlEncode(textBoxID) + "'" + "," +
								 this.form1.Name + "." + textBoxID + ".value" + "," +
								 "'" + _dateFormat/*"yyyyMMdd"*/ + "'" + "," +
								 "'" + hyperLink.ID + "'" + ")");
		cell.Controls.Add(hyperLink);
	}

	/// <summary>
	/// DropDownList を作成する
	/// </summary>
	/// <param name="id"></param>
	/// <param name="sql"></param>
	/// <param name="tooltip"></param>
	/// <returns></returns>
	private DropDownList MakeDropDownList(string id, string sql, string tooltip)
	{
		DropDownList dropDownList;
		SetDropDownList(out dropDownList, sql);
		dropDownList.ID = bb.pmShenlongTextID + id;
		dropDownList.ToolTip = tooltip;
		return dropDownList;
	}
//#else
	/// <summary>
	/// shenlong のパラメータ箇所をテキスト コントロールとして追加する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="debugMode"></param>
	/// <returns></returns>
	private int AppendShenlongParamControl(XmlDocument xmlShenlongColumn, bool debugMode, out StringBuilder jsCheckTextBox)
	{
		debugMode = false;
		Dictionary<string, int> paramNames = new Dictionary<string, int>();
		int shenlongParamCount = 0;
		Label lastLabelRColOp = null;
		jsCheckTextBox = new StringBuilder();

		// TableTextInput テーブルの設定
		//TableTextInput.Caption = "<br>";
		TableTextInput.HorizontalAlign = HorizontalAlign.Center;
		TableTextInput.Attributes.Add("border", "0");
		TableTextInput.Attributes.Add("cellspacing", "0");
		TableTextInput.Attributes.Add("cellpadding", "4");

		TableRow row = null;
		TableCell cell = null;

		string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[" + cc.qc.expression.ToString() + "!='']";
		XmlNodeList columnWithExpression = xmlShenlongColumn.SelectNodes(xpath);

		foreach ( XmlNode column in columnWithExpression/*xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn)*/ )
		{
			string expression = column[cc.qc.expression.ToString()].InnerText;
			/*if ( expression.Length == 0 )
				continue;*/

			string tableName = cc.GetTableName(column.Attributes[cc.attrTableName].Value, false);
			string fieldName = column[cc.qc.fieldName.ToString()].InnerText;
			string comment = column[cc.qc.property.ToString()][cc.prop.comment.ToString()].InnerText;
			bool withComment = (comment != cc.propNoComment);
			XmlNode bubbles = column[cc.qc.property.ToString()][cc.prop.bubbles.ToString()];
			cc.bubbCtrl bubbCtrl = cc.bubbCtrl.textBox;

			if ( bubbles != null )
			{
				string control = bubbles.Attributes[cc.bubbSet.control.ToString()].Value;
				if ( control == cc.bubbCtrl.noVisible.ToString() )
					continue;
				else if ( control == cc.bubbCtrl.textBox.ToString() )
					bubbCtrl = ((bubbles[cc.bubbSet.dropDownList.ToString()] != null && bubbles[cc.bubbSet.dropDownList.ToString()].InnerText.Length == 0) || (bubbles["dropDownSql"] != null && bubbles["dropDownSql"].InnerText.Length == 0)) ? cc.bubbCtrl.textBox : cc.bubbCtrl.dropDownList;
				else if ( control == cc.bubbCtrl.label.ToString() )
					bubbCtrl = cc.bubbCtrl.label;
			}

			int fieldAsIndex;
			string plainFieldName = bb/*cc*/.GetPlainTableFieldName(fieldName, out fieldAsIndex);
			string plainTableFieldName = tableName + bb.pmShenlongTextIdJoin + plainFieldName;

			int sameParamNo = 0;
			if ( !paramNames.TryGetValue(plainTableFieldName, out sameParamNo) )
			{
				paramNames[plainTableFieldName] = sameParamNo;
			}
			else
			{
				sameParamNo = ++paramNames[plainTableFieldName];
			}

			row = new TableRow();

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Right;
			cell.VerticalAlign = VerticalAlign.Middle;

			//string labelText = ((fieldAsIndex != -1) ? fieldName.Substring(fieldAsIndex + 4).Trim() : ((withComment) ? comment : /*tableName + "." + */plainFieldName/*tableFieldName*/));
			string labelText = plainFieldName;
			if ( fieldAsIndex != -1 )
			{
				labelText = fieldName.Substring(fieldAsIndex + 4).Trim(" \"".ToCharArray());
			}
			else
			{
				XmlNode alias = column[cc.qc.property.ToString()][cc.prop.alias.ToString()];
				if ( alias != null )
				{
					labelText = alias.InnerText;
				}
				else
				{
					if ( withComment )
					{
						labelText = comment;
					}
				}
			}

			Label label = new Label();
			label.Text = "<br>" + labelText + "　" + expression + "　";
			label.ID = "Label" + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			if ( withComment || (fieldAsIndex != -1) )
			{
				label.ToolTip = tableName + "." + plainFieldName/*tableFieldName*/;
			}
			/*//this.form1.Controls.Add(label);
			this.PanelParam.Controls.Add(label);*/
			cell.Controls.Add(label);
			row.Cells.Add(cell);

			cell = new TableCell();
			cell.HorizontalAlign = HorizontalAlign.Left;
			cell.VerticalAlign = VerticalAlign.Bottom;

			TextBox textBox = null;
			string value1 = column[cc.qc.value1.ToString()].InnerText;
			bool necessary = false;

			if ( value1.Length != 0 )
			{
				if ( bubbCtrl == cc.bubbCtrl.textBox )
				{
					textBox = new TextBox();
					textBox.Text = (debugMode) ? value1 : string.Empty;
					textBox.ToolTip = (debugMode) ? string.Empty : value1;
					textBox.ID = bb.pmShenlongTextID + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
					/*textBox.Width = Unit.Percentage(30);
					//this.form1.Controls.Add(textBox);
					this.PanelParam.Controls.Add(textBox);*/
					cell.Controls.Add(textBox);

					/*if ( (bubbles != null) && (bubbles.Attributes[cc.bubbSet.input.ToString()] != null) &&
						 (bubbles.Attributes[cc.bubbSet.input.ToString()].Value == cc.bubbInput.necessary.ToString()) )
					{
						jsCheckTextBox.Append("if (document.getElementById('" + textBox.ID + "').value == \"\") {\r\n");
						jsCheckTextBox.Append("  window.alert('" + labelText + " は必須入力です');\r\n");
						jsCheckTextBox.Append("  document.getElementById('" + textBox.ID + "').focus();\r\n");
						jsCheckTextBox.Append("  return(false);\r\n");
						jsCheckTextBox.Append("}\r\n");
						necessary = true;
					}*/
					CheckTextBoxInputNecessary(bubbles, textBox, labelText, ref jsCheckTextBox, ref necessary);

#if WITH_CALENDAR_CONTROL
					string type = column[cc.qc.fieldName.ToString()].Attributes[cc.prop.type.ToString()].Value;
					if ( type == "DATE" )
					{
						string _id = plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
						MakeDatePickerLink(_id, cell, cc.sqlDateFormat);
					}
#endif
				}
				else if ( bubbCtrl == cc.bubbCtrl.label )
				{
					label = new Label();
					label.Text = value1;
					/*this.PanelParam.Controls.Add(label);*/
					cell.Controls.Add(label);
				}
				else if ( bubbCtrl == cc.bubbCtrl.dropDownList )
				{
					//label.Text = "<p/>" + label.Text.Substring(4);
					DropDownList dropDownList;
					SetDropDownList(out dropDownList, (bubbles[cc.bubbSet.dropDownList.ToString()] != null) ? bubbles[cc.bubbSet.dropDownList.ToString()].InnerText : bubbles["dropDownSql"].InnerText);
					dropDownList.ToolTip = value1;
					dropDownList.ID = bb.pmShenlongTextID + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
					/*this.PanelParam.Controls.Add(dropDownList);*/
					cell.Controls.Add(dropDownList);
				}

				if ( expression == "BETWEEN" )
				{
					if ( bubbCtrl == cc.bubbCtrl.dropDownList )
					{
						label.Text = label.Text.Replace("　BETWEEN　", "　＝　");
					}
					else
					{
						label = new Label();
						label.Text = "　" + "AND" + "　";
						/*this.PanelParam.Controls.Add(label);*/
						cell.Controls.Add(label);
					}

					string value2 = column[cc.qc.value2.ToString()].InnerText;

					if ( bubbCtrl == cc.bubbCtrl.textBox )
					{
						textBox = new TextBox();
						textBox.Text = (debugMode) ? value2 : string.Empty;
						textBox.ToolTip = (debugMode) ? string.Empty : value2;
						textBox.ID = bb.pmShenlongTextID + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo + "HI";
						/*textBox.Width = Unit.Percentage(30);
						this.PanelParam.Controls.Add(textBox);*/
						cell.Controls.Add(textBox);

						CheckTextBoxInputNecessary(bubbles, textBox, labelText, ref jsCheckTextBox, ref necessary);

#if WITH_CALENDAR_CONTROL
						string type = column[cc.qc.fieldName.ToString()].Attributes[cc.prop.type.ToString()].Value;
						if ( type == "DATE" )
						{
							string _id = plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo + "HI";
							MakeDatePickerLink(_id, cell, cc.sqlDateFormat);
						}
#endif
					}
					else if ( bubbCtrl == cc.bubbCtrl.label )
					{
						label = new Label();
						label.Text = value2;
						/*this.PanelParam.Controls.Add(label);*/
						cell.Controls.Add(label);
					}
				}

				shenlongParamCount++;
			}

			string rColOp = column[cc.qc.rColOp.ToString()].InnerText;
			label = new Label();
			label.Text = (necessary ? "<font size=\"1\" color=\"red\">&nbsp;*</font>" : "") + "　" + ((rColOp.Length != 0) ? rColOp : "AND");
			label.ID = "LabelRColOp" + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			label.ForeColor = Color.LightGray/*Color.Gray*/;
			/*this.PanelParam.Controls.Add(label);*/
			cell.Controls.Add(label);
			lastLabelRColOp = label;

			row.Cells.Add(cell);
			TableTextInput.Rows.Add(row);
		}

		if ( lastLabelRColOp != null )
		{
			/*this.PanelParam.Controls.Remove(lastLabelRColOp);*/
			/*cell.Controls.Remove(lastLabelRColOp);*/
			string text = lastLabelRColOp.Text;
			int length = text.Length;
			if ( text.Substring(length - 4) == "　AND" )
				lastLabelRColOp.Text = text.Substring(0, length - 4);
			else if ( text.Substring(length - 3) == "　OR" )
				lastLabelRColOp.Text = text.Substring(0, length - 3);
		}

		return shenlongParamCount;
	}
//#endif

	/// <summary>
	/// オラクルの接続を開く
	/// </summary>
	/// <param name="sid"></param>
	/// <param name="uid"></param>
	/// <param name="pwd"></param>
	/// <returns></returns>
	private OracleConnection OpenOracle(string sid, string uid, string pwd)
	{
		OracleConnection oleConn = null;
		oleConn = new OracleConnection("Data Source=" + sid + ";" +
									  "user id=" + uid + ";password=" + pwd + ";" +
									  "persist security info=false;");
		oleConn.Open();
		return oleConn;
	}

	/// <summary>
	/// オラクルの接続を閉じる
	/// </summary>
	/// <param name="oleConn"></param>
	/// <param name="oleCmd"></param>
	/// <param name="oleReader"></param>
	private void CloseOracle(ref OracleConnection oleConn, ref OracleCommand oleCmd, ref OracleDataReader oleReader)
	{
		if ( oleReader != null )
		{
			oleReader.Close();
			oleReader.Dispose();
			oleReader = null;
		}

		if ( oleCmd != null )
		{
			oleCmd.Dispose();
			oleCmd = null;
		}

		if ( oleConn != null )
		{
			if ( oleConn.State == ConnectionState.Open )
			{
				oleConn.Close();
			}
			oleConn.Dispose();
			oleConn = null;
		}
	}

	/// <summary>
	/// SetDropDownList
	/// </summary>
	/// <param name="dropDownList"></param>
	/// <param name="sql"></param>
	private void SetDropDownList(out DropDownList dropDownList, string sql)
	{
		OracleConnection oleConn = null;
		OracleCommand oleCmd = null;
		OracleDataReader oleReader = null;

		dropDownList = new DropDownList();

		try
		{
			if ( sql.StartsWith("SELECT", true, null) )
			{
				sql = sql.Replace("<br>", " ");
				string password = GetLogOnPassword();
				if ( password == null )
				{
					dropDownList.Items.Add("[ERROR]パスワード未登録");
					return;
				}

				oleConn = OpenOracle(LabelSID.Text, LabelUID.Text, password);
				oleCmd = new OracleCommand(sql, oleConn);
				oleReader = oleCmd.ExecuteReader();

				while ( oleReader.Read() )								// １行ずつ読み込む
				{
					string text = oleReader[0].ToString();
					string value = (2 <= oleReader.FieldCount) ? oleReader[1].ToString() : text;
					ListItem listItem = new ListItem(text, value);
					dropDownList.Items.Add(listItem);
				}
			}
			else
			{
				string[] items = sql.Replace("<br>", "").Split('|');
				for ( int i = 0; i < items.Length; i++ )
				{
					string[] item = items[i].Split(',');
					string text = item[0];
					string value = (item.Length == 1) ? text : string.Join(",", item, 1, item.Length - 1);
					ListItem listItem = new ListItem(text, value);
					dropDownList.Items.Add(listItem);
				}
			}
		}
		catch ( Exception exp )
		{
			dropDownList.Items.Add(exp.Message);
		}
		finally
		{
			CloseOracle(ref oleConn, ref oleCmd, ref oleReader);
		}
	}

//#if !ENABLED_SUBQUERY
	/// <summary>
	/// ポストバックされた shenlong のパラメータをクエリー項目ファイルにセットする
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	private void SetShenlongParam(ref XmlDocument xmlShenlongColumn)
	{
		Dictionary<string, int> paramNames = new Dictionary<string, int>();

		string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[" + cc.qc.expression.ToString() + "!='']";
		XmlNodeList columnWithExpression = xmlShenlongColumn.SelectNodes(xpath);

		foreach ( XmlNode column in columnWithExpression/*xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn)*/ )
		{
			string expression = column[cc.qc.expression.ToString()].InnerText;
			/*if ( expression.Length == 0 )
				continue;*/

			XmlNode bubbles = column[cc.qc.property.ToString()][cc.prop.bubbles.ToString()];
			if ( bubbles != null )
			{
				if ( bubbles.Attributes[cc.bubbSet.control.ToString()].Value == cc.bubbCtrl.noVisible.ToString() )
					continue;
			}

			string tableFieldName = cc.GetTableName(column.Attributes[cc.attrTableName].Value, false) + bb.pmShenlongTextIdJoin + column[cc.qc.fieldName.ToString()].InnerText;
			string plainTableFieldName = bb/*cc*/.GetPlainTableFieldName(tableFieldName);

			int sameParamNo = 0;
			if ( !paramNames.TryGetValue(plainTableFieldName, out sameParamNo) )
			{
				paramNames[plainTableFieldName] = sameParamNo;
			}
			else
			{
				sameParamNo = ++paramNames[plainTableFieldName];
			}

			string paramName = bb.pmShenlongTextID + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			if ( Request.Params[paramName] == null )
				continue;
			if ( Request.Params[paramName].Length != 0 )
			{
				column[cc.qc.value1.ToString()].InnerText = Request.Params[paramName];
			}
			else
			{
				if ( RadioVoidExpression.Checked )
				{
					column[cc.qc.expression.ToString()].InnerText = string.Empty;
					column[cc.qc.expression.ToString()].IsEmpty = true;
					continue;
				}
			}

			if ( expression == "BETWEEN" )
			{
				paramName += "HI";
				if ( (Request.Params[paramName] != null) && (Request.Params[paramName].Length != 0) )
				{
					column[cc.qc.value2.ToString()].InnerText = Request.Params[paramName];
				}
				else
				{
					int index = column[cc.qc.value1.ToString()].InnerText.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase);
					if ( index != -1 )
					{
						column[cc.qc.value2.ToString()].InnerText = column[cc.qc.value1.ToString()].InnerText.Substring(index + 5).TrimStart();
						column[cc.qc.value1.ToString()].InnerText = column[cc.qc.value1.ToString()].InnerText.Substring(0, index).TrimEnd();
					}
				}
			}
		}
	}
//#endif
}
