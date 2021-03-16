#define	FOR_WINDOW_OPEN
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Threading;
using System.Text;
using System.Diagnostics;

public partial class DatePicker : System.Web.UI.Page
{
	public const string pmField = "field";
	public const string pmDate = "date";
	public const string pmEventtarget = "EVENTTARGET";
	public const string pmFormat = "format";

	private const string defaultFormat = "yyyyMMdd";

	private string time = "";

	/// <summary>
	/// Page_Load
	/// ※ カレンダーの"先月|来月へ移動"はポストバックで処理されている
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			// Put user code to initialize the page here
			this.Calendar.Attributes.Add("title", "");

			string date = Request.QueryString[pmDate];		// 呼び出し元の日付

			int index = date.IndexOf(' ');
			if ( index != -1 )
			{
				time = date.Substring(index);
				date = date.Substring(0, index);
			}

			if ( (date.Length == 4) && (new Regex(@"\d{4}")).IsMatch(date) )		// YYYY ?
			{
				date += "0101";
			}
			else if ( (date.Length == 6) && (new Regex(@"\d{6}")).IsMatch(date) )	// YYYYMM ?
			{
				date += "01";
			}

			string format = string.IsNullOrEmpty(Request.QueryString[pmFormat]) ? defaultFormat : Request.QueryString[pmFormat];

			DateTime dateTime;
			if ( DateTime.TryParseExact(date, format, DateTimeFormatInfo.CurrentInfo, DateTimeStyles.None, out dateTime) )
			{
				Calendar.VisibleDate = dateTime;
				Calendar.SelectedDate = dateTime;
			}

#if true
			string javaScript = "\r\n" +
								//"window.onunload = OnUnload;\r\n" +
								"var __EVENTTARGET;\r\n" +
								"__EVENTTARGET = QueryString('EVENTTARGET');\r\n" +
								//"window.alert(__EVENTTARGET);\r\n" +
#if FOR_WINDOW_OPEN
								"//window.opener.document.form1.disabled = true;\r\n" +
								"window.opener.form1.style.cursor = 'wait';\r\n" +
								"if (__EVENTTARGET != null) {\r\n" +
								"  if (window.opener.document.getElementById(__EVENTTARGET) != null)\r\n" +
								"    window.opener.document.getElementById(__EVENTTARGET).style.cursor = 'wait';\r\n" +
								"}\r\n" +
								"window.opener.document.getElementById('DropDownOutputType').disabled = true;\r\n" +
								"window.opener.document.getElementById('DropDownRowCountPage').disabled = true;\r\n" +
								"window.opener.document.getElementById('ButtonSubmit').disabled = true;\r\n" +
#endif
								"// OnBeforeUnload\r\n" +
								"function window.onbeforeunload() {\r\n" +
#if FOR_WINDOW_OPEN
								"  if (__EVENTTARGET != null) {\r\n" +
								"    if (window.opener.document.getElementById(__EVENTTARGET) != null)\r\n" +
								"      window.opener.document.getElementById(__EVENTTARGET).style.cursor = 'hand';\r\n" +
								"  }\r\n" +
#endif
								//"  window.alert(event.clientX + ':' + document.body.clientWidth + ',' + event.clientY + ',' + event.altKey);\r\n" +
								//"  window.alert(window.returnValue)\r\n" +
								//"  if (((event.clientX > document.body.clientWidth) && (event.clientY < 0)) || event.altKey) {\r\n" +
								"  //先月|来月への移動？\r\n" +
								"  if (((window.returnValue == null) && (0 <= event.clientY)) && (!event.altKey)) {\r\n" +
								"    return;\r\n" +
								"  }\r\n" +
#if FOR_WINDOW_OPEN
								"  //window.opener.document.form1.disabled = false;\r\n" +
								"  window.opener.form1.style.cursor = 'default';\r\n" +
								"  window.opener.document.getElementById('DropDownOutputType').disabled = false;\r\n" +
								"  window.opener.document.getElementById('DropDownRowCountPage').disabled = false;\r\n" +
								"  window.opener.document.getElementById('ButtonSubmit').disabled = false;\r\n" +
#endif
								"}\r\n" +
#if false
								"// OnUnload\r\n" +
								"function OnUnload() {\r\n" +
								"  //window.alert('OnUnload');\r\n" +
#if FOR_WINDOW_OPEN
								"  //window.opener.document.form1.disabled = false;\r\n" +
								"  window.opener.form1.style.cursor = 'default';\r\n" +
								"  if (__EVENTTARGET != null) {\r\n" +
								"    if (window.opener.document.getElementById(__EVENTTARGET) != null)\r\n" +
								"      window.opener.document.getElementById(__EVENTTARGET).style.cursor = 'hand';\r\n" +
								"  }\r\n" +
								"  window.opener.document.getElementById('DropDownOutputType').disabled = false;\r\n" +
								"  window.opener.document.getElementById('DropDownRowCountPage').disabled = false;\r\n" +
								"  window.opener.document.getElementById('ButtonSubmit').disabled = false;\r\n" +
#endif
								"  //var field = QueryString('field');\r\n" +
								"  //if (field != null)\r\n" +
								"  //window.opener.document.getElementById(field.split('.')[1]).focus();\r\n" +
								"}\r\n" +
#endif
								"// QueryString\r\n" +
								"function QueryString(keyName) {\r\n" +
								"  /* アドレスの「?」以降の引数(パラメータ)を取得 */\r\n" +
								"  var pram=location.search;\r\n" +
								"  /* 引数がない時は処理しない */\r\n" +
								"  if (!pram) return null;\r\n" +
								"  /* 先頭の?をカット */\r\n" +
								"  pram=pram.substring(1);\r\n" +
								"  /* 「&」で引数を分割して配列に */\r\n" +
								//"  var ampersand = decodeURIComponent('%26');\r\n" +
								//"  var pair=pram.split(ampersand);\r\n" +
								"  var pair=pram.split('&');\r\n" +
								"  var i=temp='';\r\n" +
								"  var key=new Array();\r\n" +
								"  for (i=0; i<pair.length; i++) {\r\n" +
								//"  for (i=0; Math.min(i,pair.length)!=pair.length; i++) {\r\n" +
								//"  for (i=0; pair.length-i!=0; i++) {\r\n" +
								//"window.alert('i:' + i + ' ' + pair[i]);\r\n"+
#if true
								"    /* 配列の値を「=」で分割 */\r\n" +
								"    temp=pair[i].split('=');\r\n" +
								"    _keyName=temp[0];\r\n" +
								"    _keyValue=temp[1];\r\n" +
								"    /* キーと値の連想配列を生成 */\r\n" +
								"    key[_keyName]=_keyValue;\r\n" +
#endif
								"  }\r\n" +
								"  return key[keyName];\r\n" +
								//"  return null;\r\n" +
								"}\r\n" +
								"";
			/*ClientScript.RegisterStartupScript(typeof(string), "myJavaScript", javaScript);*/
			//javaScript = "var i = 0; if ( 10 - i != 0 ) window.alert(i)";
			//javaScript = "var i = 10; if ( 0 < i ) window.alert(i);";
			//javaScript = @"alert('I am in Head Element.')";
			/*HtmlGenericControl js = new HtmlGenericControl("script");
			js.Attributes["type"] = "text/javascript";
			js.Attributes["src"] = javaScript;
			Page.Header.Controls.Add(js);*/
			// 普通に HtmlGenericControl を使う
			HtmlGenericControl js = new HtmlGenericControl();
			js.TagName = "script";
			js.Attributes.Add("language", "javascript");
			js.Attributes.Add("type", @"text/javascript");
			js.InnerText = javaScript;
			js.InnerHtml = js.InnerHtml.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
			//js.Attributes.Add("src", "DatePicker.js");
			// ScriptHtmlGenericControl を使う
			/*ScriptHtmlGenericControl js = new ScriptHtmlGenericControl();
			js.InnerText = javaScript;*/
			this.Page.Header.Controls.Add(js);
#endif
		}
		catch ( ThreadAbortException )
		{
		}
		catch ( Exception exp )
		{
			StringBuilder message = new StringBuilder();
			message.Append("<span style=\"color:Red;font-weight:bold;\">" + exp.Message.ToString() + "</span>");
			Response.Write(message);
		}
	}

	override protected void OnInit(EventArgs e)
	{
		this.Calendar.DayRender += new DayRenderEventHandler(this.Calendar_DayRender);
		base.OnInit(e);
	}

	/// <summary>
	/// Replaces the standard post-back link for each calendar day 
	/// with the javascript to set the opener window's TextBox text.
	/// Eliminates a needless round-trip to the server.
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	private void Calendar_DayRender(object sender, System.Web.UI.WebControls.DayRenderEventArgs e)
	{
		try
		{
			// Clear the link from this day
			e.Cell.Controls.Clear();

			// Add the custom link
			HtmlGenericControl Link = new HtmlGenericControl();
			Link.TagName = "a";
			//Link.InnerText = string.Format("{0,2}", int.Parse(e.Day.DayNumberText)).Replace(" ", "　");
			Link.InnerText = e.Day.DayNumberText;
			if ( Link.InnerText.Length == 1 ) { Link.InnerText = "　" + Link.InnerText; }
			string javaScript = "JavaScript:" +
								//"if(window.opener!=null){{" +		// 親ウィンドウが閉じられたときの対策だが、結局スクリプトエラーになる？のでコメントにした。
#if FOR_WINDOW_OPEN
								"window.opener.document.{0}.value='{1}';" + /*{1:d}*/
								"window.opener.document.{0}.focus();" +
								"window.returnValue='{1}';" +
#else
								"window.returnValue='{1}';" +
#endif
								//"}}" +
								"window.close();";
			string field = Request.QueryString[pmField];
			string format = string.IsNullOrEmpty(Request.QueryString[pmFormat]) ? defaultFormat : Request.QueryString[pmFormat];
			string value = String.Format(javaScript, field, e.Day.Date.ToString(format) + time);
			Link.Attributes.Add("href", value);
			Link.Attributes.Add("title", ""/*e.Day.DayNumberText + "日"*/);

			// By default, this will highlight today's date.
			if ( e.Day.IsSelected )
			{
				Link.Attributes.Add("style", this.Calendar.SelectedDayStyle.ToString());
			}

			if ( e.Day.IsOtherMonth )
			{
				//string color = "#" + this.Calendar.OtherMonthDayStyle.ForeColor.ToArgb().ToString("X").Substring(2);
				string color = this.Calendar.OtherMonthDayStyle.ForeColor.ToArgb().ToString("X");
				Link.Style.Add("color", "#" + color.Substring(2));
			}
			else
			{
				if ( e.Day.IsWeekend )
				{
					string color = this.Calendar.WeekendDayStyle.ForeColor.Name;
					Link.Style.Add("color", color);
				}
			}

			// Now add our custom link to the page
			e.Cell.Controls.Add(Link);
		}
		catch ( Exception exp )
		{
			try
			{
				e.Cell.ToolTip = exp.Message;
			}
			catch ( Exception _exp )
			{
				Debug.WriteLine(_exp.Message);
			}
		}
	}
}

public class ScriptHtmlGenericControl : HtmlGenericControl
{
	protected override void Render(HtmlTextWriter htWriter)
	{
		//string title = this.Attributes["title"].Replace("\r\n", "&#13;&#10;");
		string innerHtml = "<script language=\"javascript\" type=\"text/javascript\">" +
						   this.InnerText.Replace("&lt;", "<").Replace("&amp;","&") +
						   "</script>";
		htWriter.Write(innerHtml);
	}
}
