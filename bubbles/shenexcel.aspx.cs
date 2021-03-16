#define	ENABLED_SUBQUERY			// サブクエリのロジックを有効にする
#define	WRITECONDI_IS_STRING
#define	UPDATE_20190809
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
using System.Xml;
using System.Text;
using System.Drawing;
using System.Data.OleDb;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Diagnostics;
#if !WITHIN_SHENGLOBAL
using cc = Shenlong.ShenGlobal;
#endif

public partial class shenexcel : System.Web.UI.Page
{
	private const string EXT_XLS = ".xls";
	private const string EXT_SLK = ".slk";
	private const string EXT_XML = ".xml";

	/// <summary>
	/// Page_Load
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			/*Debug.WriteLine("[Session]");
			foreach ( string key in Session.Keys )
			{
				Debug.WriteLine(key + ":" + Session[key]);
			}
			Debug.WriteLine("[Params]");
			foreach ( string key in Request.Params )
			{
				Debug.WriteLine(key + ":" + Request.Params[key]);
			}*/

			string remoteUser = bb.GetRemoteUserName(Request.Params["REMOTE_USER"], Request.Params["REMOTE_ADDR"]);
			bool devGroupUser = bb.IsDevGroupUser(ASP.global_asax.devGroupUsers, remoteUser, User.Identity.Name);
			devGroupUser = !devGroupUser ? bb.IsDevelopMode(Request.Params[bb.pmDevelop], (string)Session[bb.pmDevelop]) : devGroupUser;

			string excelExtension = EXT_XLS;

			if ( /*devGroupUser && */Request.Browser.Platform.Contains("WinNT") )
			{
				string windows = string.Empty;
				string userAgent = Page.Request.UserAgent;
				if ( userAgent.Contains("Windows NT 5.2") )
					windows = "Windows Server 2003";
				else if ( userAgent.Contains("Windows NT 6.0") )
					windows = "Vista";
				else if ( userAgent.Contains("Windows NT 6.1") )
					windows = "Windows 7";
#if UPDATE_20190809
				else if ( userAgent.Contains("Windows NT 10.0") )
					windows = "Windows 10";
#endif

#if UPDATE_20190809
				if ( (windows == "Windows 7") || (windows == "Windows 10") )
#else
				if ( windows == "Windows 7" )
#endif
				{
					excelExtension = EXT_SLK;
				}
			}

			bool voidExpression = (Request.Params[bb.pmBlankValue] != null) && (Request.Params[bb.pmBlankValue] == "VoidExpression");

			if ( (Request.Params[bb.pmExcelXml] != null) && (string.Compare(Request.Params[bb.pmExcelXml], "true", true) == 0) )
			{
				//string s = Request.Params[bb.pmExcelXml];
				excelExtension = EXT_XML;
			}

#if WRITECONDI_IS_STRING
			string conditions;
			if ( (Request.Params[bb.pmWriteCondi] == null) || string.IsNullOrEmpty(Request.Params[bb.pmWriteCondi]) )
			{
				conditions = null;
			}
			else
			{
				conditions = Request.Params[bb.pmWriteCondi].Replace('　', ' ');
				conditions = "抽出条件：" + conditions.Substring(0, conditions.Length - 1);	// 1:,
			}
#else
			bool writeCondi = (Request.Params[bb.pmWriteCondi] != null) && (string.Compare(Request.Params[bb.pmWriteCondi], "true", true) == 0);
			StringBuilder _conditions = new StringBuilder ();
			/*string conditions = null;
			if ( writeCondi )
			{
				StringBuilder _conditions = new StringBuilder();
				foreach ( string key in values.Keys )
				{
					if ( !key.StartsWith(bb.pmShenlongTextID) )
						continue;
					string[] keys = key.Split('．');
					string _key = keys[keys.Length - 1];
					int index = _key.LastIndexOf(bb.pmShenlongTextIdJoin[0]);
					if ( index != -1 )
					{
						_key = _key.Substring(0, index);
					}
					_conditions.Append(_key + ":" + values[key] + ",");
				}
				_conditions.Length--;	// ','
				conditions = _conditions.ToString();
			}*/
#endif

			string shenFileName = (string)Session[bb.ssShenlongDocFolder] + (string)Session[bb.ssSubDirectory] + (string)Session[bb.ssShenFileName];

			/*XmlDocument xmlShenlongColumn = new XmlDocument();
			xmlShenlongColumn.Load(shenFileName);*/
			Version verShenColumn;
			XmlDocument xmlShenlongColumn = bb.ReadShenlongColumnFile(shenFileName, out verShenColumn);

			XmlNode fileProperty = (2 <= verShenColumn.Major) ? xmlShenlongColumn.DocumentElement[cc.tagProperty] : null;
			string buildSql, columnComments = null;
			List<string> fromTableNames = new List<string>();
			Dictionary<string, string> values = new Dictionary<string, string>();

			// [抽出開始] での要求？
			if ( string.IsNullOrEmpty((string)Session["LabelBuildSql"]) )
			{
				// 親ページから渡されたパラメータ(element.name=element.value) をディクショナリに登録する
				//Dictionary<string, string> values = new Dictionary<string, string>();
				if ( Request.Params[bb.pmValues] != null )
				{
					string[] value = Request.Params[bb.pmValues].Split('\t');
					for ( int i = 0; i < value.Length; i++ )
					{
						if ( value[i].Length == 0 )
							continue;
						string[] nameValue = value[i].Split('=');
						if ( (2 <= verShenColumn.Major) && (nameValue[1].Length == 0) && !voidExpression )	// 規定値を使う？
							continue;
						values[nameValue[0]] = HttpUtility.UrlDecode(nameValue[1]);

#if !WRITECONDI_IS_STRING
						if ( (2 <= verShenColumn.Major) && (nameValue[0].StartsWith(bb.pmShenlongTextID)) )
						{
							string[] keys = nameValue[0].Split('．');
							string _key = keys[keys.Length - 1];
							_conditions.Append(_key.Substring(0, _key.LastIndexOf(bb.pmShenlongTextIdJoin[0])) + ":" + values[nameValue[0]] + ",");
						}
#endif
					}
				}

				//#if !ENABLED_SUBQUERY
				if ( verShenColumn.Major < 2 )
				{
					// ポストされた shenlong のパラメータをクエリー項目ファイルにセットする
#if WRITECONDI_IS_STRING
					SetShenlongParam(ref xmlShenlongColumn, values);
#else
					SetShenlongParam(ref xmlShenlongColumn, values, out _conditions);
#endif
				}
				//#endif

				// 最大抽出行数をExcelの最大行数にする
#if false
				if ( xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagMaxRowNum] == null )
				{
					XmlElement elem = xmlShenlongColumn.CreateElement(cc.tagMaxRowNum);
					xmlShenlongColumn.DocumentElement[cc.tagProperty].AppendChild(elem);
				}
				xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagMaxRowNum].InnerText = ((uint)0x10000 - 2).ToString();
#else
				if ( cc.HasMaxRowNum(xmlShenlongColumn) )
				{
					int maxRowNum = int.Parse(xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagMaxRowNum].InnerText);
					int excelMaxRowNum = (int)((uint)0x100000/*0x10000*/ - 2);
					if ( (maxRowNum == 0) || (excelMaxRowNum < maxRowNum) )
					{
						xmlShenlongColumn.DocumentElement[cc.tagProperty][cc.tagMaxRowNum].InnerText = excelMaxRowNum.ToString();
					}
				}
#endif

				// クエリー項目から SQL を構築する
				//XmlNode fileProperty = null;
				//string buildSql, columnComments = null;
				//List<string> fromTableNames = new List<string>();
				bool result;
				//#if ENABLED_SUBQUERY
				if ( 2 <= verShenColumn.Major )
				{
					//fileProperty = xmlShenlongColumn.DocumentElement[cc.tagProperty];
					if ( bool.Parse(fileProperty[cc.tagSqlSelect].InnerText) )
					{
						buildSql = xmlShenlongColumn.DocumentElement[cc.tagSQL].InnerText;
						buildSql = buildSql.Replace("<br>", "\r\n"/*" "*/);
						fromTableNames = cc.GetTableNameInSQL(buildSql, true, true);
						result = true;
					}
					else
					{
						result = cc.BuildQueryColumnSQL(xmlShenlongColumn, values, false, ASP.global_asax.maxRowNum, out buildSql, out columnComments, ref fromTableNames, 0);
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
			}
			// [HyperLinkShenExcel] からの要求？（SAVE_BUILDSQL_TO_SESSION@shenlong2.aspx.cs が有効の時）
			else
			{
				buildSql = HttpUtility.UrlDecode((string)Session["LabelBuildSql"]);
				columnComments = (!string.IsNullOrEmpty((string)Session["LabelColumnComments"])) ? HttpUtility.UrlDecode((string)Session["LabelColumnComments"]) : null;
				values.Add(bb.pmTextPWD, (string)Session["LabelPWD"]);
			}

			string sid = xmlShenlongColumn.DocumentElement.Attributes[cc.attrSID].Value;
			string uid = xmlShenlongColumn.DocumentElement.Attributes[cc.attrUserName].Value;

			if ( !devGroupUser )
			{
				string logTableNames = bb.GetLogTableNames(fromTableNames);
				bb.WriteAccessLog(ASP.global_asax.writeLogDsnUidPwd, Path.GetFileNameWithoutExtension(shenFileName), logTableNames, sid, uid, remoteUser, bb.ot.excel, cc.pno.bubbles);
			}

			// クエリーを実行する
			string format = (excelExtension == EXT_XLS || excelExtension == EXT_SLK) ? bb.EXCEL_FORMAT_SYLK : (bb.EXCEL_FORMAT_XML + Path.GetFileNameWithoutExtension(shenFileName));
#if !WRITECONDI_IS_STRING
			string conditions = (!writeCondi || (_conditions.Length == 0)) ? null : _conditions.ToString(0, _conditions.Length - 1);	// 1:,
#endif
			int headerOutput = ((int)cc.header.columnName | (int)cc.header.comment);
			if ( fileProperty != null )
			{
				if ( fileProperty[cc.tagHeaderOutput] != null )
				{
					headerOutput = int.Parse(fileProperty[cc.tagHeaderOutput].InnerText);
				}
			}
			StringBuilder queryOutput;
			//string[] dataTypeName;
			//DataTable/*DataView*/ dataView;
			//bb.ot outType = bb.ot.excel;
			//string sid = xmlShenlongColumn.DocumentElement.Attributes[cc.attrSID].Value;
			//string uid = xmlShenlongColumn.DocumentElement.Attributes[cc.attrUserName].Value;
			//bb.ExecuteQuery(sid, uid, values[bb.pmTextPWD], buildSql, columnComments, outType, out queryOutput, out dataTypeName, out dataView);
			bb.ExecuteQuery(sid, uid, values[bb.pmTextPWD], buildSql, format, headerOutput, columnComments, conditions, out queryOutput);

			/*if ( !devGroupUser )
			{
				cc.WriteAccessLog(Path.GetFileNameWithoutExtension(shenFileName), remoteUser);
			}*/

			//Response.Cache.SetCacheability(HttpCacheability.NoCache);	// キャッシュを無効にする

#if false
			//Response.Clear();
			//Response.ContentType = "application/octet-stream-dummy"/*"application/excel"*/;
			Response.AppendHeader("Content-Disposition", "attachment; filename=" + HttpUtility.UrlEncode(Path.GetFileNameWithoutExtension(shenFileName)) + ".xls");
#else
			// ダウンロードするファイル名が文字化けする対策
			// web.config に <globalization responseHeaderEncoding="shift_jis"/> を追加する。
			// 但しデバッグサーバにおいてはIISと同じ動作をせず文字化けする
			// 参照：ファイルをダウンロードする ASP.NET Web ページで日本語ファイル名が文字化けする(http://support.microsoft.com/default.aspx?scid=kb;ja;436616)
			//Response.AppendHeader("Content-Type", "application/vnd.ms-excel");
			Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileNameWithoutExtension(shenFileName) + excelExtension);
#endif

			//Encoding sjisEnc = Encoding.GetEncoding("shift_jis");
			//byte[] excelType = sjisEnc.GetBytes(queryOutput.ToString());
			//Response.Write(sjisEnc.GetString(excelType));
			if ( format.StartsWith(bb.EXCEL_FORMAT_SYLK) )
			{
				Response.Write(queryOutput/*buildSql*/);
			}
			else if ( format.StartsWith(bb.EXCEL_FORMAT_XML) )
			{
				Response.BinaryWrite(Encoding.UTF8.GetBytes(queryOutput.ToString()));
			}
			Response.End();
		}
		catch ( ThreadAbortException )
		{
		}
		catch ( Exception exp )
		{
			Response.Write("<span style=\"color:Red;font-weight:bold;\">" + exp.Message.ToString() + "</span>");
		}
	}

//#if !ENABLED_SUBQUERY
	/// <summary>
	/// ポストされた shenlong のパラメータをクエリー項目ファイルにセットする
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="values"></param>
#if WRITECONDI_IS_STRING
	private void SetShenlongParam(ref XmlDocument xmlShenlongColumn, Dictionary<string, string> values)
#else
	private void SetShenlongParam(ref XmlDocument xmlShenlongColumn, Dictionary<string, string> values, out StringBuilder _conditions)
#endif
	{
		Dictionary<string, int> paramNames = new Dictionary<string, int>();
#if !WRITECONDI_IS_STRING
		_conditions = new StringBuilder();
#endif
		bool voidExpression = (Request.Params[bb.pmBlankValue] != null) && (Request.Params[bb.pmBlankValue] == "VoidExpression");

		string xpath = "/" + cc.tagShenlong + "/" + cc.tagColumn + "[" + cc.qc.expression.ToString() + "!='']";
		XmlNodeList columnWithExpression = xmlShenlongColumn.SelectNodes(xpath);

		foreach ( XmlNode column in columnWithExpression/*xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn)*/ )
		{
			string expression = column[cc.qc.expression.ToString()].InnerText;
			/*if ( expression.Length == 0 )
				continue;*/

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
			string value;
			if ( !values.TryGetValue(paramName, out value) )
				continue;
			if ( value.Length != 0 )
			{
				column[cc.qc.value1.ToString()].InnerText = value;
#if !WRITECONDI_IS_STRING
				_conditions.Append(column[cc.qc.fieldName.ToString()].InnerText + ":" + value + ",");
#endif
			}
			else
			{
				if ( voidExpression )
				{
					column[cc.qc.expression.ToString()].InnerText = string.Empty;
					column[cc.qc.expression.ToString()].IsEmpty = true;
					continue;
				}
			}

			if ( expression == "BETWEEN" )
			{
				paramName += "HI";
				if ( values.TryGetValue(paramName, out value) )
				{
					if ( value.Length != 0 )
					{
						column[cc.qc.value2.ToString()].InnerText = value;
#if !WRITECONDI_IS_STRING
						_conditions.Append(column[cc.qc.fieldName.ToString()].InnerText + ":" + value + ",");
#endif
					}
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
