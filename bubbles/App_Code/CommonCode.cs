#define	TABLE_NAME_HAS_ALIAS		// テーブル名が別名を持つ事がある
#define	COLLECT_OUTER_JOIN			// 正しい外部結合のSQLを構築する
#define	ENABLED_SUBQUERY			// サブクエリのロジックを有効にする
//#define	WITHIN_SHENGLOBAL
using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Text;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml;
using System.Collections.Generic;
//using System.Data.OleDb;
using System.IO;
#if !WITHIN_SHENGLOBAL
using Shenlong;
#endif
using Oracle.ManagedDataAccess.Client;

/// <summary>
/// bubbles CommonCode の概要の説明です
/// </summary>
public class bb
{
	public const string ssShenlongDocFolder = "shenlongDocFolder";
	public const string ssSubDirectory = "subDirectory";
	public const string ssShenFileName = "shenFileName";
	public const string ssUrlReferer = "urlReferer";
	public const string ssHyperLinkHome = "hyperLinkHome";

	public const string pmDevelop = "develop";
	public const string pmShenDocName = "shendocnm";
	public const string pmDownload = "download";
	public const string pmShenFile = "shenfile";
	public const string pmCacheability = "cacheability";
	public const string pmTextPWD = "TextPWD";
	public const string pmShenlongTextID = "_Text";
	public const string pmShenlongTextIdJoin = "_";
	public const string pmShenlongTextIdNo = "_";
	public const string pmShenlongLabelID = "_Label";
	public const string pmShenlongFixTextID = "_FixText";
	public const string pmValues = "values";
	public const string pmBlankValue = "blankValue";
	public const string pmExcelXml = "excelXml";
	public const string pmWriteCondi = "writeCondi";
	public const string pmShenDirect = "shenDirect";
	public const string pmSubDir = "subdir";

#if WITHIN_SHENGLOBAL
	public static string sqlDateFormat = "yyyymmdd hh24mi";		// SQL 日付の条件書式

	public const string tagShenlong = "shenlong";
	public const string attrSID = "sid";
	public const string attrUserName = "userName";
	private const string attrVer = "ver";
	public const string tagColumn = "column";
	public const string attrTableName = "tableName";
	private const string attrWidth = "width";
	private const string tagTableJoin = "tableJoin";
	private const string tagSQL = "sql";
	private const string tagBuildedSQL = "buildedSql";
	public const string tagProperty = "property";
	public const string tagComment = "comment";
	public const string tagAuthor = "author";
	public const string tagDownload = "download";
	public const string tagMaxRowNum = "maxRowNum";
	public const string tagSetValue = "setValue";
#if ENABLED_SUBQUERY
	public const string tagSubQuery = "subQuery";

	public const char SUBQUERY_SEPARATOR = ';';
	public const string SUBQUERY_RELATIVE_PATH = ".";
#endif

	public enum prop { type, length, nullable, comment, alias, bubbles, count };// カラムのプロパティ（兼タグ名）

	public const string propNoComment = "n/c";					// NO COMMENT

	public enum bubbSet { control, input, setValue, dropDownList, hyperLink, classify };	// バブルスの設定値（兼属性|タグ名）
	public const Char sepBubbSet = '&';									// バブルスの設定値の区切り
	public enum bubbCtrl { textBox, label, noVisible, dropDownList };	// バブルスのコントロール設定（兼属性名）
	public enum bubbInput { noAppoint, necessary };						// バブルスの入力条件設定（兼属性名）

	public enum qc {											// クエリー項目のアイテム（兼タグ名）
		fieldName, showField, expression, value1, value2, rColOp, orderBy, groupFunc, property };

	public enum tabJoin { leftTabCol, way, rightTabCol };		// [テーブル結合] のサブアイテム（兼タグ名）

	public enum authorize { permit, deny };						// 権限を許可するか否か（兼値）

	private const string withoutTableName = "::";

	public const string sepOutput = "\t";						// クエリー出力の区切り
#endif

	public enum ot { html, excel };

	public const string EXCEL_FORMAT_SYLK = "SYLK:";
	public const string EXCEL_FORMAT_XML = "XML:";

	public bb()
	{
		//
		// TODO: コンストラクタ ロジックをここに追加します
		//
	}

	/// <summary>
	/// リモートユーザー名を取得する
	/// ※実際にはリモートのホスト名を返している
	/// </summary>
	/// <param name="remoteUser"></param>
	/// <param name="remoteAddr"></param>
	/// <returns></returns>
	public static string GetRemoteUserName(string remoteUser, string remoteAddr)
	{
		/*remoteUser = (remoteAddr != null && remoteAddr.Length != 0) ? "ip" + remoteAddr : "anonymous";*/
		System.Net.IPHostEntry ipHostEntry;

		try
		{
#pragma warning disable 0618
			ipHostEntry = System.Net.Dns.GetHostByAddress(remoteAddr);	// GetHostEntry だと例外が発生する？ので、先に GetHostByAddress をやってみる
#pragma warning restore 0618
			remoteUser = ipHostEntry.HostName;
		}
		catch ( Exception )
		{
			try
			{
				ipHostEntry = System.Net.Dns.GetHostEntry(remoteAddr);
				remoteUser = ipHostEntry.HostName;
			}
			catch ( Exception )
			{
				remoteUser = (remoteAddr != null && remoteAddr.Length != 0) ? "ip" + remoteAddr : "anonymous";
			}
		}

		bool isLetter = false;
		foreach ( char c in remoteUser )
		{
			if ( char.IsLetter(c) )
			{
				isLetter = true;
				break;
			}
		}

		if ( isLetter/*char.IsLetter(remoteUser[0])*/ )
		{
			int index = remoteUser.IndexOf('.');
			if ( index != -1 )
			{
				remoteUser = remoteUser.Substring(0, index);
			}

			if ( char.IsDigit(remoteUser[0]) )
			{
				remoteUser = "pc" + remoteUser;
			}
		}
		else
		{
			remoteUser = (remoteAddr != null && remoteAddr.Length != 0) ? "ip" + remoteAddr : "anonymous";
		}

		return remoteUser.ToLower();
	}

	/// <summary>
	/// 接続されたホスト名は開発者グループである？
	/// </summary>
	/// <param name="devGroupUsers"></param>
	/// <param name="remoteUser"></param>
	/// <param name="userIdentityName"></param>
	/// <returns></returns>
	public static bool IsDevGroupUser(string[] devGroupUsers, string remoteUser, string userIdentityName)
	{
		bool devGroupUser = false;

		try
		{
			if ( devGroupUsers == null )
				return devGroupUser;

			/*int i;
			for ( i = 0; i < devGroupUsers.Length && (String.Compare(remoteUser, devGroupUsers[i], true) != 0); i++ ) ;
			devGroupUser = (i != devGroupUsers.Length);*/
			for ( int i = 0; i < devGroupUsers.Length; i++ )
			{
				if ( (string.Compare(remoteUser, devGroupUsers[i], true) == 0) || (string.Compare(userIdentityName, devGroupUsers[i], true) == 0) )
				{
					devGroupUser = true;
					break;
				}
			}
		}
		catch ( Exception )
		{
		}

		return devGroupUser;
	}

	/// <summary>
	/// 開発者用のロジックを通るようにする？
	/// </summary>
	/// <param name="pmDevelop"></param>
	/// <param name="ssDevelop"></param>
	/// <returns></returns>
	public static bool IsDevelopMode(string pmDevelop, string ssDevelop)
	{
		if ( !string.IsNullOrEmpty(pmDevelop) )
		{
			return bool.Parse(pmDevelop);
		}
		else if ( !string.IsNullOrEmpty(ssDevelop) )
		{
			return bool.Parse(ssDevelop);
		}

		return false;
	}

	/// <summary>
	/// パスワードを暗号化する。
	/// 文字列を一字ずつ１６進数にして、その上位と下位を入れ替える。
	/// 偶数番目の上位と下位の間にダミー キャラクタを挿入する。
	/// ABCDEFG -> 1x4243x4445x4647x4
	/// </summary>
	public static string EncodePassword(string decodePassword)
	{
		byte[] bytePass = Encoding.Default.GetBytes(decodePassword);
		char[] chPass = new char[decodePassword.Length * 3];
		Random random = new Random();
		int dest = 0;

		for ( int i = 0; i < decodePassword.Length; i++ )
		{
			string hex = bytePass[i].ToString("X");
			chPass[dest++] = hex[1];
			if ( i % 2 == 0 )
				chPass[dest++] = random.Next(0, 9).ToString()[0];	// ダミー キャラクタ
			chPass[dest++] = hex[0];
		}
		//return new string(chPass);
		return new string(chPass, 0, dest);		// 2007/09/04 末尾に余分な '\0' が入るのを防ぐ為
	}

	/// <summary>
	/// 暗号化されたパスワードを復元する。
	/// 偶数番目の上位と下位の間のダミー キャラクタは読み飛ばす。
	/// 1x4243x4445x4647x4 -> ABCDEFG
	/// </summary>
	public static string DecodePassword(string encodePassword)
	{
		try
		{
			byte[] bytePass = new byte[encodePassword.Length];
			char[] chPass = new char[2];
			int i, src = 0;

			for ( i = 0; src < encodePassword.Length; i++ )
			{
				chPass[1] = encodePassword[src++];
				if ( i % 2 == 0 )
					src++;
				chPass[0] = encodePassword[src++];
				bytePass[i] = Byte.Parse(new string(chPass), System.Globalization.NumberStyles.HexNumber);
			}
			return new string(Encoding.Default.GetChars(bytePass, 0, i));
		}
		catch ( Exception )
		{
			return null;
		}
	}

	/// <summary>
	/// クエリー項目ファイルを読み込む
	/// </summary>
	/// <param name="fileName"></param>
	/// <returns></returns>
	public static XmlDocument ReadShenlongColumnFile(string fileName, out Version verShenColumn)
	{
		XmlDocument xmlShenlongColumn = new XmlDocument();
		xmlShenlongColumn.Load(fileName);

		verShenColumn = GetShenColumnVer(xmlShenlongColumn.DocumentElement.Attributes[ShenGlobal.attrVer]);

#if COLLECT_OUTER_JOIN
		foreach ( XmlNode tableJoin in xmlShenlongColumn.DocumentElement.SelectNodes(ShenGlobal.tagTableJoin) )
		{
			if ( verShenColumn <= new Version(1, 13) )	// Version 1.13 以前の外部結合は逆向きにする
			{
				string way = tableJoin.Attributes[ShenGlobal.tabJoin.way.ToString()].Value;
				tableJoin.Attributes[ShenGlobal.tabJoin.way.ToString()].Value = (way == "<=") ? ">=" : ((way == ">=") ? "<=" : way);
			}
		}
#endif

		return xmlShenlongColumn;
	}

	/// <summary>
	/// クエリー項目ファイルのバージョンを取得する
	/// </summary>
	/// <param name="ver"></param>
	/// <returns></returns>
	public static Version GetShenColumnVer(XmlAttribute ver)
	{
		Version verShenColumn = new Version(0, 0);
		if ( ver != null )
		{
			verShenColumn = new Version(ver.Value);
		}

		return verShenColumn;
	}

//#if ENABLED_SUBQUERY
#if WITHIN_SHENGLOBAL
	/// <summary>
	/// バブルス設定を文字列に変換する
	/// </summary>
	/// <param name="bubbles"></param>
	/// <returns></returns>
	public static string BubblesSettingToString(XmlNode bubbles)
	{
		StringBuilder setting = new StringBuilder();			// enum bubbSet の順に取り出して格納する

		setting.Append(bubbles.Attributes[bubbSet.control.ToString()].Value);
		setting.Append(sepBubbSet);

		setting.Append((bubbles.Attributes[bubbSet.input.ToString()] != null) ? bubbles.Attributes[bubbSet.input.ToString()].Value : bubbInput.noAppoint.ToString());
		setting.Append(sepBubbSet);

		setting.Append((bubbles.Attributes[bubbSet.setValue.ToString()] != null) ? bubbles.Attributes[bubbSet.setValue.ToString()].Value : false.ToString());
		setting.Append(sepBubbSet);

		setting.Append((bubbles[bubbSet.dropDownList.ToString()] != null) ? bubbles[bubbSet.dropDownList.ToString()].InnerText : /*(bubbles["dropDownSql"] != null ? bubbles["dropDownSql"].InnerText : */string.Empty/*)*/);
		setting.Append(sepBubbSet);

		setting.Append((bubbles[bubbSet.hyperLink.ToString()] != null) ? bubbles[bubbSet.hyperLink.ToString()].InnerText : string.Empty);
		setting.Append(sepBubbSet);

		setting.Append((bubbles[bubbSet.classify.ToString()] != null) ? bubbles[bubbSet.classify.ToString()].InnerText : string.Empty);

		return setting.ToString();
	}

	/// <summary>
	/// クエリー項目(xml)から SQL を構築する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="selectParams"></param>
	/// <param name="maxRowNum"></param>
	/// <param name="buildedSql"></param>
	/// <param name="columnComments"></param>
	/// <param name="fromTableNames"></param>
	/// <param name="indentCnt"></param>
	/// <returns></returns>
	public static bool BuildQueryColumnSQL(XmlDocument xmlShenlongColumn, Dictionary<string, string> selectParams, int maxRowNum, out string buildedSql, out string columnComments, ref List<string> fromTableNames, int indentCnt)
	{
		buildedSql = null;
		columnComments = null;

		try
		{
			string indent = new string(' ', indentCnt);
			StringBuilder select = new StringBuilder("SELECT\r\n");
			StringBuilder from = new StringBuilder("\r\n" + indent + "FROM\r\n");
			StringBuilder where = new StringBuilder("\r\n" + indent + "WHERE\r\n");
			StringBuilder groupBy = new StringBuilder("\r\n" + indent + "GROUP BY\r\n");
			StringBuilder orderBy = new StringBuilder("\r\n" + indent + "ORDER BY\r\n");
			int defSelect = select.Length;
			int defWhereLen = where.Length;
			int defGroupByLen = groupBy.Length;
			int defOrderByLen = orderBy.Length;

			string[] _sqlDateFormat = { "yyyymmdd hh24mi", "yyyy/mm/dd hh24:mi" };
			List<string> _queryTableNames = new List<string>();		// 選択済みのテーブル名（現在の状態）
			List<string> _fileSubQuery = new List<string>();

			XmlNode fileProperty = xmlShenlongColumn.DocumentElement[tagProperty];
			if ( fileProperty != null )
			{
				if ( (fileProperty[tagSubQuery] != null) && (fileProperty[tagSubQuery].InnerText.Length != 0) )
				{
					foreach ( string subQuery in fileProperty[tagSubQuery].InnerText.Split(SUBQUERY_SEPARATOR) )
					{
						if ( _fileSubQuery.IndexOf(subQuery) == -1 )
						{
							_fileSubQuery.Add(subQuery);
						}
					}
				}
			}

			Dictionary<string, int> paramNames = new Dictionary<string, int>();
			List<string> orders = new List<string>();
			StringBuilder colComments = new StringBuilder();
			int colCommentsCount = 0;
			int groupFuncCount = 0;
			bool cameOR = false;
			indent += " ";

			foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn) )
			{
				string _tableName = column.Attributes[attrTableName].Value;
				if ( _queryTableNames.IndexOf(_tableName) == -1 )
				{
					_queryTableNames.Add(_tableName);
				}

#if TABLE_NAME_HAS_ALIAS
				string tableName = GetTableName(column.Attributes[attrTableName].Value, false);			// テーブル名
#else
				string tableName = column.Attributes[attrTableName].Value;		// テーブル名
#endif
				string fieldName = column[qc.fieldName.ToString()].InnerText;	// フィールド名
				string[] property = new string[(int)prop.count];				// プロパティ
				property[(int)prop.type] = column[qc.fieldName.ToString()].Attributes[prop.type.ToString()].Value;
				property[(int)prop.length] = column[qc.fieldName.ToString()].Attributes[prop.length.ToString()].Value;
				property[(int)prop.nullable] = column[qc.fieldName.ToString()].Attributes[prop.nullable.ToString()].Value;
				property[(int)prop.comment] = column[qc.property.ToString()][prop.comment.ToString()].InnerText;
				string tableFieldName = (!fieldName.StartsWith(withoutTableName)) ? (tableName + "." + fieldName) : fieldName.Substring(withoutTableName.Length);

				int asFieldName;
				string plainTableFieldName = GetPlainTableFieldName(tableFieldName, out asFieldName);

#if true
				XmlNode alias = column[qc.property.ToString()][prop.alias.ToString()];
				property[(int)prop.alias] = (alias == null) ? string.Empty : "\"" + alias.InnerText + "\"";
				if ( (property[(int)prop.alias].Length != 0) && (asFieldName == -1) )	// プロパティでの別名があり、直接の別名指定は無い？
				{
					tableFieldName += " AS " + property[(int)prop.alias];
					plainTableFieldName = GetPlainTableFieldName(tableFieldName, out asFieldName);
				}
#endif

				if ( bool.Parse(column[qc.showField.ToString()].InnerText) )
				{
					string groupFunc = column[qc.groupFunc.ToString()].InnerText;
					if ( !string.IsNullOrEmpty(groupFunc) )
					{
						tableFieldName = groupFunc + "(" + tableFieldName + ")";
						groupFuncCount++;
					}

#if true
					if ( (property[(int)prop.type] == "DATE") && !tableFieldName.StartsWith("to_char(", StringComparison.OrdinalIgnoreCase) )
					{
						//select.Append(indent + "to_char(" + tableFieldName + ",'YYYY/MM/DD HH24:MI:SS') " + fieldName + ",\r\n");
						select.Append(indent + "to_char(" + plainTableFieldName + ",'YYYY/MM/DD HH24:MI:SS') ");
						select.Append((asFieldName != -1) ? tableFieldName.Substring(asFieldName/* + 4*/).Trim() : fieldName);
						select.Append(",\r\n");
					}
					else
					{
						select.Append(indent + tableFieldName + ",\r\n");
					}
#else
					select.Append(indent + tableFieldName + ",\r\n");
#endif

					colComments.Append(property[(int)prop.comment] + sepOutput);
					if ( property[(int)prop.comment] != propNoComment )
					{
						colCommentsCount++;
					}
				}

				tableFieldName = plainTableFieldName;

				// 条件式
				string expression = column[qc.expression.ToString()].InnerText;
				string value1 = column[qc.value1.ToString()].InnerText.Trim();
				string value2 = column[qc.value2.ToString()].InnerText.Trim();
				string rColOp = column[qc.rColOp.ToString()].InnerText;
				string leftRndBkt = "(", rightRndBkt = ")";

				string bubbles = string.Empty;
				XmlNode bubblesNode = column[qc.property.ToString()][prop.bubbles.ToString()];
				if ( bubblesNode != null )
				{
					bubbles = BubblesSettingToString(bubblesNode);
				}
				SetShenlongParam(selectParams, xmlShenlongColumn.BaseURI.Replace('-','―'), bubbles, plainTableFieldName, ref paramNames, expression, ref value1, ref value2);

				//string quotation = (property[(int)prop.type].StartsWith("VARCHAR")) ? "'" : "";
				string quotation = IsCharColumn(property[(int)prop.type]) ? "'" : "";

				if ( !string.IsNullOrEmpty(value1) && (property[(int)prop.type] == "DATE") )	// 日付の条件指定あり？
				{
					int dtfmt = value1.IndexOf('/') == -1 ? 0 : 1;
					string toChar = (value1[0] == '(') ? "to_char" : "";
					string dateQuote = (Char.IsDigit(value1[0])) ? "'" : "";
					value1 = "to_date(" + toChar + dateQuote + value1 + dateQuote + ",'" + _sqlDateFormat[dtfmt] + "')";
				}

				if ( rColOp.Length == 0 )
				{
					rColOp = "AND";
				}

				if ( rColOp == "OR" )
				{
					leftRndBkt += (!cameOR) ? "(" : "";
					cameOR = true;
				}
				else/* if ( rColOp == "AND" )*/
				{
					//rightRndBkt += (cameOR) ? ")" : "";
					if ( cameOR )
					{
						if ( expression.Length != 0 )
						{
							rightRndBkt += ")";
						}
						else
						{
							where.Insert(where.Length - 5, ')');	// OR の括弧が閉じられていないので、強制的に右括弧で閉じる 5:" OR\r\n"
						}
					}
					cameOR = false;
				}

				// =, NOT =, >=, <=, >, <
				if ( (expression.IndexOf('=') != -1 || expression == "<" || expression == ">") && !string.IsNullOrEmpty(value1) )
				{
					expression = (expression == "NOT =") ? "<>" : expression;
					where.Append(indent + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// BETWEEN, NOT BETWEEN
				else if ( (expression.IndexOf("BETWEEN") != -1) && (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2)) )
				{
					if ( !string.IsNullOrEmpty(value2) && (property[(int)prop.type] == "DATE") )	// 日付の条件指定あり？
					{
						int dtfmt = value2.IndexOf('/') == -1 ? 0 : 1;
						string toChar = (value2[0] == '(') ? "to_char" : "";
						string dateQuote = (Char.IsDigit(value2[0])) ? "'" : "";
						value2 = "to_date(" + toChar + dateQuote + value2 + dateQuote + ",'" + _sqlDateFormat[dtfmt] + "')";
					}
					where.Append(indent + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + " AND " + quotation + value2 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IN, NOT IN
				else if ( (expression.IndexOf("IN") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string[] values = value1.Split(',');
					where.Append(indent + leftRndBkt + tableFieldName + " " + expression + " (");
					for ( int k = 0; k < values.Length; k++ )
					{
						where.Append(quotation + values[k] + quotation + ((k != values.Length - 1) ? "," : ""));
					}
					where.Append(")" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// LIKE, NOT LIKE
				else if ( (expression.IndexOf("LIKE") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string wildCard = (value1.IndexOfAny(new char[] { '%', '_' }) == -1) ? "%" : "";
					where.Append(indent + leftRndBkt + tableFieldName + " " + expression + " '" + value1 + wildCard + "'" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IS NULL, IS NOT NULL
				else if ( (expression.IndexOf("NULL") != -1) && string.IsNullOrEmpty(value1) )
				{
					where.Append(indent + leftRndBkt + tableFieldName + " " + expression + rightRndBkt + " " + rColOp + "\r\n");
				}

				// ソート順
				string order = column[qc.orderBy.ToString()].InnerText.Trim();
				if ( !string.IsNullOrEmpty(order) )
				{
					int k, number;
					for ( k = 0; k < order.Length && Char.IsDigit(order[k]); k++ ) ;
					number = (Char.IsDigit(order[0])) ? int.Parse(order.Substring(0, k)) : 999;
					string desc = (order.IndexOf("DESC", k, StringComparison.CurrentCultureIgnoreCase) != -1) ? " DESC" : "";
#if true
					string orderTableFieldName = (property[(int)prop.alias].Length == 0) ? tableFieldName : property[(int)prop.alias];
					orders.Add(number.ToString("D3") + "\t" + orderTableFieldName + desc);
#else
					orders.Add(number.ToString("D3") + "\t" + tableFieldName + desc);
#endif
				}
			}

			if ( select.Length == defSelect )
			{
				columnComments = "表示する項目が１つ以上必要です";
				return false;
			}

			// FROM テーブル名
			foreach ( string tableName in _queryTableNames )
			{
				if ( _fileSubQuery.Find(delegate(string s) { return s.IndexOf(tableName) != -1; }) != null )
					continue;
				from.Append(indent + tableName + ",\r\n");
				fromTableNames.Add(tableName);
			}
			// サブクエリ
			foreach ( string subQuery in _fileSubQuery )
			{
				XmlDocument _xmlShenlongColumn = ReadSubQueryFile(subQuery, xmlShenlongColumn.BaseURI/*GetSubQueryBaseURI(subQuery, xmlShenlongColumn.BaseURI)*/);
				string _buildedSql, _columnComments;
				if ( !BuildQueryColumnSQL(_xmlShenlongColumn, selectParams, -1/*maxRowNum*/, out _buildedSql, out _columnComments, ref fromTableNames, indentCnt + 2) )
				{
					columnComments = _columnComments;
					return false;
				}
				from.Append(indent + "(" + _buildedSql + indent + ") " + Path.GetFileNameWithoutExtension(subQuery) + ",\r\n");
			}

			if ( groupFuncCount != 0 )	// グループ関数の指定あり？
			{
				// GROUP BY
				List<string> groupFields = new List<string>();
				foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn) )
				{
					if ( !bool.Parse(column[qc.showField.ToString()].InnerText) )
						continue;
					if ( !string.IsNullOrEmpty(column[qc.groupFunc.ToString()].InnerText) )
						continue;

#if TABLE_NAME_HAS_ALIAS
					string tableFieldName = GetPlainTableFieldName(GetTableName(column.Attributes[attrTableName].Value, false) + "." + column[qc.fieldName.ToString()].InnerText);
#else
					string tableFieldName = GetPlainTableFieldName(column.Attributes[attrTableName].Value + "." + column[qc.fieldName.ToString()].InnerText);
#endif
					if ( groupFields.IndexOf(tableFieldName) != -1 )
						continue;
					groupFields.Add(tableFieldName);
				}

				if ( groupFields.Count != 0 )
				{
					foreach ( string groupField in groupFields )
					{
						groupBy.Append(indent + groupField + ",\r\n");
					}
					groupBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
				}
			}

			if ( orders.Count != 0 )	// ソートの指定あり？
			{
				// ORDER BY
				orders.Sort();
				foreach ( string order in orders )
				{
					orderBy.Append(indent + order.Substring(3 + 1) + ",\r\n");	// (3 + 1):ソート順\t
				}
				orderBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
			}

			// WHERE
			if ( groupFuncCount == 0 )
			{
				if ( maxRowNum != -1 )	// -1 の時は ROWNUM の条件は設定しない
				{
					// ROWNUM の最大指定あり？
					if ( HasMaxRowNum(xmlShenlongColumn) )
					{
						maxRowNum = int.Parse(xmlShenlongColumn.DocumentElement[tagProperty][tagMaxRowNum].InnerText);
					}
					if ( 0 < maxRowNum )
					{
						where.Append(indent + "(ROWNUM <= " + maxRowNum + ") AND\r\n");
					}
				}
			}
			if ( defWhereLen < where.Length )
			{
				//where.Insert(defWhereLen + 1, '(');
				where.Insert(defWhereLen, indent + "(");
				where.Remove(defWhereLen + indent.Length + 1, indent.Length);
				int lastSpace;
				for ( lastSpace = where.Length - 1; where[lastSpace] != ' '; lastSpace-- ) ;
				where.Remove(lastSpace + 1, where.Length - lastSpace - 1);		// "AND|OR\r\n" を削除する
				if ( cameOR )
				{
					where.Insert(lastSpace++, ')');
					cameOR = false;
				}
				where.Insert(lastSpace, ')');

				/*if ( groupFuncCount != 0 )
				{
					// HAVING
					groupBy.Append("\r\nHAVING\r\n" + where.ToString().Substring(defWhereLen));
					where = new StringBuilder("\r\nWHERE\r\n");
				}*/
			}

			// テーブル結合
			XmlNodeList tableJoins = xmlShenlongColumn.DocumentElement.SelectNodes(tagTableJoin);
			for ( int i = 0; i < tableJoins.Count; i++ )
			{
				XmlNode tableJoin = tableJoins[i];
				if ( (i == 0) && (defWhereLen != where.Length) )
				{
					where.Append("AND\r\n");
				}

#if TABLE_NAME_HAS_ALIAS
				string leftTableName, leftColumnName, leftTableColumn;
				SplitTableFieldName(tableJoin.Attributes[tabJoin.leftTabCol.ToString()].Value, out leftTableName, out leftColumnName, false);
				//leftTableColumn = GetPlainTableFieldName(leftTableName + "." + leftColumnName);
				leftTableColumn = GetPlainTableFieldName(!leftColumnName.StartsWith(withoutTableName) ? (leftTableName + "." + leftColumnName) : leftColumnName.Substring(withoutTableName.Length));

				string rightTableName, rightColumnName, rightTableColumn;
				SplitTableFieldName(tableJoin.Attributes[tabJoin.rightTabCol.ToString()].Value, out rightTableName, out rightColumnName, false);
				//rightTableColumn = GetPlainTableFieldName(rightTableName + "." + rightColumnName);
				rightTableColumn = GetPlainTableFieldName(!rightColumnName.StartsWith(withoutTableName) ? (rightTableName + "." + rightColumnName) : rightColumnName.Substring(withoutTableName.Length));

				where.Append(indent + "(");
#if COLLECT_OUTER_JOIN
				where.Append(leftTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=") ? "(+)" : ""));	// 右外部結合(RIGHT [OUTER] JOIN)
				where.Append(" = ");
				where.Append(rightTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=") ? "(+)" : ""));	// 左外部結合(LEFT [OUTER] JOIN)
#else
				where.Append(leftTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=") ? " (+)" : ""));
				where.Append(" = ");
				where.Append(rightTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=") ? " (+)" : ""));
#endif
				where.Append(") ");
#else
				where.Append(" (" + GetPlainTableFieldName(tableJoin.Attributes[tabJoin.leftTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=" ? " (+)" : ""));
				where.Append(" = ");
				where.Append(GetPlainTableFieldName(tableJoin.Attributes[tabJoin.rightTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=" ? " (+)" : ""));
				where.Append(") ");
#endif

				if ( i != tableJoins.Count - 1 )
				{
					where.Append("AND\r\n");
				}
			}

			buildedSql = select.ToString(0, select.Length - (1 + 2)) + " " +	// (1 + 2):",\r\n"
						 from.ToString(0, from.Length - (1 + 2)) + " " +		// (1 + 2):",\r\n"
						 ((where.Length == defWhereLen) ? "" : where.ToString()) +
						 ((groupBy.Length == defGroupByLen) ? "" : groupBy.ToString()) +
						 ((orderBy.Length == defOrderByLen) ? "" : orderBy.ToString()) +
						 "\r\n";

			if ( colCommentsCount != 0 )
			{
				columnComments = colComments.ToString();
			}

			return true;
		}
		catch ( Exception exp )
		{
			columnComments = exp.Message;
			return false;
		}
	}

	/// <summary>
	/// サブクエリ ファイルを読み込む
	/// </summary>
	/// <param name="subQuery"></param>
	/// <returns></returns>
	public static XmlDocument ReadSubQueryFile(string subQuery, string shenColumnBaseURI)
	{
		string _xmlShenlongColumnFileName = subQuery;

		if ( subQuery.StartsWith(SUBQUERY_RELATIVE_PATH) )
		{
			// 相対パスを絶対パスに変換する
			_xmlShenlongColumnFileName = Path.GetDirectoryName(shenColumnBaseURI) + subQuery.Substring(SUBQUERY_RELATIVE_PATH.Length);
		}

		// ファイル名の '□' を ' ' に戻す
		_xmlShenlongColumnFileName = Path.GetDirectoryName(_xmlShenlongColumnFileName) + "\\" + Path.GetFileName(_xmlShenlongColumnFileName).Replace('□', ' ');

		XmlDocument _xmlShenlongColumn = new XmlDocument();
		_xmlShenlongColumn.Load(_xmlShenlongColumnFileName);

		return _xmlShenlongColumn;
	}

	/// <summary>
	/// 入力された抽出条件があればクエリー項目にセットする
	/// </summary>
	/// <param name="selectParams"></param>
	/// <param name="bubbles"></param>
	/// <param name="plainTableFieldName"></param>
	/// <param name="paramNames"></param>
	/// <param name="expression"></param>
	/// <param name="value1"></param>
	/// <param name="value2"></param>
	public static void SetShenlongParam(Dictionary<string, string> selectParams, string baseURI, string bubbles, string plainTableFieldName, ref Dictionary<string, int> paramNames, string expression, ref string value1, ref string value2)
	{
		if ( (selectParams == null) || (expression.Length == 0) )
			return;

		if ( bubbles.Length != 0 )
		{
			string[] setting = bubbles.Split(sepBubbSet);
			if ( setting[(int)bubbSet.control] == bubbCtrl.noVisible.ToString() )
				return;
		}

		int sameParamNo = 0;
		plainTableFieldName = plainTableFieldName.Replace('.', pmShenlongTextIdJoin[0]);
		if ( !paramNames.TryGetValue(plainTableFieldName, out sameParamNo) )
		{
			paramNames[plainTableFieldName] = sameParamNo;
		}
		else
		{
			sameParamNo = ++paramNames[plainTableFieldName];
		}

		string _baseURI = Path.GetFileNameWithoutExtension(baseURI);
		string paramName = pmShenlongTextID + _baseURI + pmShenlongTextIdJoin + plainTableFieldName + pmShenlongTextIdNo + sameParamNo;
		string _value;
		if ( !selectParams.TryGetValue(paramName, out _value) )
			return;

		value1 = _value;

		if ( expression == "BETWEEN" )
		{
			paramName += "HI";
			if ( selectParams.TryGetValue(paramName, out _value) )
			{
				value2 = _value;
			}
			else
			{
				int index = value1.IndexOf(" AND ", StringComparison.OrdinalIgnoreCase);
				if ( index != -1 )
				{
					value2 = value1.Substring(index + 5).TrimStart();
					value1 = value1.Substring(0, index).TrimEnd();
				}
			}
		}
	}
#endif
//#else
#if TABLE_NAME_HAS_ALIAS
	/// <summary>
	/// クエリー項目から SQL を構築する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="maxRowNum"></param>
	/// <param name="buildedSql"></param>
	/// <param name="columnComments"></param>
	/// <param name="fromTableNames"></param>
	/// <returns></returns>
	public static bool BuildQueryColumnSQL(XmlDocument xmlShenlongColumn, int maxRowNum, out string buildedSql, out string columnComments, ref List<string> fromTableNames, int dummy)
	{
		buildedSql = null;
		columnComments = null;

		try
		{
			StringBuilder select = new StringBuilder("SELECT\r\n");
			StringBuilder from = new StringBuilder("\r\nFROM\r\n");
			StringBuilder where = new StringBuilder("\r\nWHERE\r\n");
			StringBuilder groupBy = new StringBuilder("\r\nGROUP BY\r\n");
			StringBuilder orderBy = new StringBuilder("\r\nORDER BY\r\n");
			int defSelect = select.Length;
			int defWhereLen = where.Length;
			int defGroupByLen = groupBy.Length;
			int defOrderByLen = orderBy.Length;

			string[] sqlDateFormat = { "yyyymmdd hh24mi", "yyyy/mm/dd hh24:mi" };
			List<string> queryTableNames = new List<string>();		// 選択済みのテーブル名（現在の状態）

			List<string> orders = new List<string>();
			StringBuilder colComments = new StringBuilder();
			int colCommentsCount = 0;
			int groupFuncCount = 0;
			bool cameOR = false;

			foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(ShenGlobal.tagColumn) )
			{
				string _tableName = column.Attributes[ShenGlobal.attrTableName].Value;
				if ( queryTableNames.IndexOf(_tableName) == -1 )
				{
					queryTableNames.Add(_tableName);
				}

#if TABLE_NAME_HAS_ALIAS
				string tableName = ShenGlobal.GetTableName(column.Attributes[ShenGlobal.attrTableName].Value, false);			// テーブル名
#else
				string tableName = column.Attributes[attrTableName].Value;		// テーブル名
#endif
				string fieldName = column[ShenGlobal.qc.fieldName.ToString()].InnerText;	// フィールド名
				string[] property = new string[(int)ShenGlobal.prop.count];				// プロパティ
				property[(int)ShenGlobal.prop.type] = column[ShenGlobal.qc.fieldName.ToString()].Attributes[ShenGlobal.prop.type.ToString()].Value;
				property[(int)ShenGlobal.prop.length] = column[ShenGlobal.qc.fieldName.ToString()].Attributes[ShenGlobal.prop.length.ToString()].Value;
				property[(int)ShenGlobal.prop.nullable] = column[ShenGlobal.qc.fieldName.ToString()].Attributes[ShenGlobal.prop.nullable.ToString()].Value;
				property[(int)ShenGlobal.prop.comment] = column[ShenGlobal.qc.property.ToString()][ShenGlobal.prop.comment.ToString()].InnerText;
				string tableFieldName = (!fieldName.StartsWith(ShenGlobal.withoutTableName)) ? (tableName + "." + fieldName) : fieldName.Substring(ShenGlobal.withoutTableName.Length);

				int asFieldName;
				string plainTableFieldName = /*ShenGlobal.*/GetPlainTableFieldName(tableFieldName, out asFieldName);

#if true
				XmlNode alias = column[ShenGlobal.qc.property.ToString()][ShenGlobal.prop.alias.ToString()];
				property[(int)ShenGlobal.prop.alias] = (alias == null) ? string.Empty : "\"" + alias.InnerText + "\"";
				if ( (property[(int)ShenGlobal.prop.alias].Length != 0) && (asFieldName == -1) )	// プロパティでの別名があり、直接の別名指定は無い？
				{
					tableFieldName += " AS " + property[(int)ShenGlobal.prop.alias];
					plainTableFieldName = /*ShenGlobal.*/GetPlainTableFieldName(tableFieldName, out asFieldName);
				}
#endif

				if ( bool.Parse(column[ShenGlobal.qc.showField.ToString()].InnerText) )
				{
					string groupFunc = column[ShenGlobal.qc.groupFunc.ToString()].InnerText;
					if ( !string.IsNullOrEmpty(groupFunc) )
					{
						tableFieldName = groupFunc + "(" + tableFieldName + ")";
						groupFuncCount++;
					}

#if true
					if ( (property[(int)ShenGlobal.prop.type] == "DATE") && !tableFieldName.StartsWith("to_char(", StringComparison.OrdinalIgnoreCase) )
					{
						//select.Append(" " + "to_char(" + tableFieldName + ",'YYYY/MM/DD HH24:MI:SS') " + fieldName + ",\r\n");
						select.Append(" " + "to_char(" + plainTableFieldName + ",'YYYY/MM/DD HH24:MI:SS') ");
						select.Append((asFieldName != -1) ? tableFieldName.Substring(asFieldName/* + 4*/).Trim() : fieldName);
						select.Append(",\r\n");
					}
					else
					{
						select.Append(" " + tableFieldName + ",\r\n");
					}
#else
					select.Append(" " + tableFieldName + ",\r\n");
#endif

					colComments.Append(property[(int)ShenGlobal.prop.comment] + ShenGlobal.sepOutput);
					if ( property[(int)ShenGlobal.prop.comment] != ShenGlobal.propNoComment )
					{
						colCommentsCount++;
					}
				}

				tableFieldName = plainTableFieldName;

				// 条件式
				string expression = column[ShenGlobal.qc.expression.ToString()].InnerText;
				string value1 = column[ShenGlobal.qc.value1.ToString()].InnerText.Trim();
				string value2 = column[ShenGlobal.qc.value2.ToString()].InnerText.Trim();
				string rColOp = column[ShenGlobal.qc.rColOp.ToString()].InnerText;
				string leftRndBkt = "(", rightRndBkt = ")";

				//string quotation = (property[(int)prop.type].StartsWith("VARCHAR")) ? "'" : "";
				string quotation = ShenGlobal.IsCharColumn(property[(int)ShenGlobal.prop.type]) ? "'" : "";

				if ( !string.IsNullOrEmpty(value1) && (property[(int)ShenGlobal.prop.type] == "DATE") )	// 日付の条件指定あり？
				{
					int dtfmt = value1.IndexOf('/') == -1 ? 0 : 1;
					string toChar = (value1[0] == '(') ? "to_char" : "";
					string dateQuote = (Char.IsDigit(value1[0])) ? "'" : "";
					value1 = "to_date(" + toChar + dateQuote + value1 + dateQuote + ",'" + sqlDateFormat[dtfmt] + "')";
				}

				if ( rColOp.Length == 0 )
				{
					rColOp = "AND";
				}

				if ( rColOp == "OR" )
				{
					leftRndBkt += (!cameOR) ? "(" : "";
					cameOR = true;
				}
				else/* if ( rColOp == "AND" )*/
				{
					//rightRndBkt += (cameOR) ? ")" : "";
					if ( cameOR )
					{
						if ( expression.Length != 0 )
						{
							rightRndBkt += ")";
						}
						else
						{
							where.Insert(where.Length - 5, ')');	// OR の括弧が閉じられていないので、強制的に右括弧で閉じる 5:" OR\r\n"
						}
					}
					cameOR = false;
				}

				// =, NOT =, >=, <=, >, <
				if ( (expression.IndexOf('=') != -1 || expression == "<" || expression == ">") && !string.IsNullOrEmpty(value1) )
				{
					expression = (expression == "NOT =") ? "<>" : expression;
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// BETWEEN, NOT BETWEEN
				else if ( (expression.IndexOf("BETWEEN") != -1) && (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2)) )
				{
					if ( !string.IsNullOrEmpty(value2) && (property[(int)ShenGlobal.prop.type] == "DATE") )	// 日付の条件指定あり？
					{
						int dtfmt = value2.IndexOf('/') == -1 ? 0 : 1;
						string toChar = (value2[0] == '(') ? "to_char" : "";
						string dateQuote = (Char.IsDigit(value2[0])) ? "'" : "";
						value2 = "to_date(" + toChar + dateQuote + value2 + dateQuote + ",'" + sqlDateFormat[dtfmt] + "')";
					}
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + " AND " + quotation + value2 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IN, NOT IN
				else if ( (expression.IndexOf("IN") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string[] values = value1.Split(',');
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " (");
					for ( int k = 0; k < values.Length; k++ )
					{
						where.Append(quotation + values[k] + quotation + ((k != values.Length - 1) ? "," : ""));
					}
					where.Append(")" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// LIKE, NOT LIKE
				else if ( (expression.IndexOf("LIKE") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string wildCard = (value1.IndexOfAny(new char[] { '%', '_' }) == -1) ? "%" : "";
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " '" + value1 + wildCard + "'" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IS NULL, IS NOT NULL
				else if ( (expression.IndexOf("NULL") != -1) && string.IsNullOrEmpty(value1) )
				{
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + rightRndBkt + " " + rColOp + "\r\n");
				}

				// ソート順
				string order = column[ShenGlobal.qc.orderBy.ToString()].InnerText.Trim();
				if ( !string.IsNullOrEmpty(order) )
				{
					int k, number;
					for ( k = 0; k < order.Length && Char.IsDigit(order[k]); k++ ) ;
					number = (Char.IsDigit(order[0])) ? int.Parse(order.Substring(0, k)) : 999;
					string desc = (order.IndexOf("DESC", k, StringComparison.CurrentCultureIgnoreCase) != -1) ? " DESC" : "";
#if true
					string orderTableFieldName = (property[(int)ShenGlobal.prop.alias].Length == 0) ? tableFieldName : property[(int)ShenGlobal.prop.alias];
					orders.Add(number.ToString("D3") + "\t" + orderTableFieldName + desc);
#else
					orders.Add(number.ToString("D3") + "\t" + tableFieldName + desc);
#endif
				}
			}

			if ( select.Length == defSelect )
			{
				columnComments = "表示する項目が１つ以上必要です";
				return false;
			}

			// FROM テーブル名
			foreach ( string tableName in queryTableNames )
			{
				from.Append(" " + tableName + ",\r\n");
				fromTableNames.Add(tableName);
			}

			if ( groupFuncCount != 0 )	// グループ関数の指定あり？
			{
				// GROUP BY
				List<string> groupFields = new List<string>();
				foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(ShenGlobal.tagColumn) )
				{
					if ( !bool.Parse(column[ShenGlobal.qc.showField.ToString()].InnerText) )
						continue;
					if ( !string.IsNullOrEmpty(column[ShenGlobal.qc.groupFunc.ToString()].InnerText) )
						continue;

#if TABLE_NAME_HAS_ALIAS
					string tableFieldName = /*ShenGlobal.*/GetPlainTableFieldName(ShenGlobal.GetTableName(column.Attributes[ShenGlobal.attrTableName].Value, false) + "." + column[ShenGlobal.qc.fieldName.ToString()].InnerText);
#else
					string tableFieldName = GetPlainTableFieldName(column.Attributes[attrTableName].Value + "." + column[qc.fieldName.ToString()].InnerText);
#endif
					if ( groupFields.IndexOf(tableFieldName) != -1 )
						continue;
					groupFields.Add(tableFieldName);
				}

				if ( groupFields.Count != 0 )
				{
					foreach ( string groupField in groupFields )
					{
						groupBy.Append(" " + groupField + ",\r\n");
					}
					groupBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
				}
			}

			if ( orders.Count != 0 )	// ソートの指定あり？
			{
				// ORDER BY
				orders.Sort();
				foreach ( string order in orders )
				{
					orderBy.Append(" " + order.Substring(3 + 1) + ",\r\n");	// (3 + 1):ソート順\t
				}
				orderBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
			}

			// WHERE
#if true
			if ( groupFuncCount == 0 )
			{
				// ROWNUM の最大指定あり？
				if ( ShenGlobal.HasMaxRowNum(xmlShenlongColumn) )
				{
					maxRowNum = int.Parse(xmlShenlongColumn.DocumentElement[ShenGlobal.tagProperty][ShenGlobal.tagMaxRowNum].InnerText);
				}
				if ( 0 < maxRowNum )
				{
					where.Append(" (ROWNUM <= " + maxRowNum + ") AND\r\n");
				}
			}
#endif
			if ( defWhereLen < where.Length )
			{
				where.Insert(defWhereLen + 1, '(');
				int lastSpace;
				for ( lastSpace = where.Length - 1; where[lastSpace] != ' '; lastSpace-- ) ;
				where.Remove(lastSpace + 1, where.Length - lastSpace - 1);		// "AND|OR\r\n" を削除する
				if ( cameOR )
				{
					where.Insert(lastSpace++, ')');
					cameOR = false;
				}
				where.Insert(lastSpace, ')');

				/*if ( groupFuncCount != 0 )
				{
					// HAVING
					groupBy.Append("\r\nHAVING\r\n" + where.ToString().Substring(defWhereLen));
					where = new StringBuilder("\r\nWHERE\r\n");
				}*/
			}

			// テーブル結合
			XmlNodeList tableJoins = xmlShenlongColumn.DocumentElement.SelectNodes(ShenGlobal.tagTableJoin);
			for ( int i = 0; i < tableJoins.Count; i++ )
			{
				XmlNode tableJoin = tableJoins[i];
				if ( (i == 0) && (defWhereLen != where.Length) )
				{
					where.Append("AND\r\n");
				}

#if TABLE_NAME_HAS_ALIAS
				string leftTableName, leftColumnName, leftTableColumn;
				/*ShenGlobal.*/SplitTableFieldName(tableJoin.Attributes[ShenGlobal.tabJoin.leftTabCol.ToString()].Value, out leftTableName, out leftColumnName, false);
				//leftTableColumn = GetPlainTableFieldName(leftTableName + "." + leftColumnName);
				leftTableColumn = /*ShenGlobal.*/GetPlainTableFieldName(!leftColumnName.StartsWith(ShenGlobal.withoutTableName) ? (leftTableName + "." + leftColumnName) : leftColumnName.Substring(ShenGlobal.withoutTableName.Length));

				string rightTableName, rightColumnName, rightTableColumn;
				/*ShenGlobal.*/SplitTableFieldName(tableJoin.Attributes[ShenGlobal.tabJoin.rightTabCol.ToString()].Value, out rightTableName, out rightColumnName, false);
				//rightTableColumn = GetPlainTableFieldName(rightTableName + "." + rightColumnName);
				rightTableColumn = /*ShenGlobal.*/GetPlainTableFieldName(!rightColumnName.StartsWith(ShenGlobal.withoutTableName) ? (rightTableName + "." + rightColumnName) : rightColumnName.Substring(ShenGlobal.withoutTableName.Length));

				where.Append(" (");
#if COLLECT_OUTER_JOIN
				where.Append(leftTableColumn + ((tableJoin.Attributes[ShenGlobal.tabJoin.way.ToString()].Value == "<=") ? "(+)" : ""));	// 右外部結合(RIGHT [OUTER] JOIN)
				where.Append(" = ");
				where.Append(rightTableColumn + ((tableJoin.Attributes[ShenGlobal.tabJoin.way.ToString()].Value == ">=") ? "(+)" : ""));	// 左外部結合(LEFT [OUTER] JOIN)
#else
				where.Append(leftTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=") ? " (+)" : ""));
				where.Append(" = ");
				where.Append(rightTableColumn + ((tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=") ? " (+)" : ""));
#endif
				where.Append(") ");
#else
				where.Append(" (" + GetPlainTableFieldName(tableJoin.Attributes[tabJoin.leftTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=" ? " (+)" : ""));
				where.Append(" = ");
				where.Append(GetPlainTableFieldName(tableJoin.Attributes[tabJoin.rightTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=" ? " (+)" : ""));
				where.Append(") ");
#endif

				if ( i != tableJoins.Count - 1 )
				{
					where.Append("AND\r\n");
				}
			}

			buildedSql = select.ToString(0, select.Length - (1 + 2)) + " " +	// (1 + 2):",\r\n"
						 from.ToString(0, from.Length - (1 + 2)) + " " +		// (1 + 2):",\r\n"
						 ((where.Length == defWhereLen) ? "" : where.ToString()) +
						 ((groupBy.Length == defGroupByLen) ? "" : groupBy.ToString()) +
						 ((orderBy.Length == defOrderByLen) ? "" : orderBy.ToString()) +
						 "\r\n";

			if ( colCommentsCount != 0 )
			{
				columnComments = colComments.ToString();
			}

			return true;
		}
		catch ( Exception exp )
		{
			columnComments = exp.Message;
			return false;
		}
	}
#else
	/// <summary>
	/// クエリー項目から SQL を構築する
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <param name="maxRowNum"></param>
	/// <param name="buldedSql"></param>
	/// <param name="columnComments"></param>
	/// <returns></returns>
	public static bool BuildQueryColumnSQL(XmlDocument xmlShenlongColumn, int maxRowNum, out string buldedSql, out string columnComments)
	{
		buldedSql = null;
		columnComments = null;

		try
		{
			StringBuilder select = new StringBuilder("SELECT\r\n");
			StringBuilder from = new StringBuilder("\r\nFROM\r\n");
			StringBuilder groupBy = new StringBuilder("\r\nGROUP BY\r\n");
			StringBuilder where = new StringBuilder("\r\nWHERE\r\n");
			StringBuilder orderBy = new StringBuilder("\r\nORDER BY\r\n");
			int defSelect = select.Length;
			int defGroupByLen = groupBy.Length;
			int defWhereLen = where.Length;
			int defOrderByLen = orderBy.Length;

			string[] sqlDateFormat = { "yyyymmdd hh24mi", "yyyy/mm/dd hh24:mi" };
			ArrayList queryTableNames = new ArrayList();		// 選択済みのテーブル名（現在の状態）

			ArrayList orders = new ArrayList();
			StringBuilder colComments = new StringBuilder();
			int colCommentsCount = 0;
			int groupFuncCount = 0;
			bool cameOR = false;

			foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn) )
			{
				string tableName = column.Attributes[attrTableName].Value;		// テーブル名
				string fieldName = column[qc.fieldName.ToString()].InnerText;	// フィールド名
				string[] property = new string[(int)prop.count];				// プロパティ
				property[(int)prop.type] = column[qc.fieldName.ToString()].Attributes[prop.type.ToString()].Value;
				property[(int)prop.length] = column[qc.fieldName.ToString()].Attributes[prop.length.ToString()].Value;
				property[(int)prop.nullable] = column[qc.fieldName.ToString()].Attributes[prop.nullable.ToString()].Value;
				property[(int)prop.comment] = column[qc.property.ToString()][prop.comment.ToString()].InnerText;
				string tableFieldName = (!fieldName.StartsWith(withoutTableName)) ? (tableName + "." + fieldName) : fieldName.Substring(withoutTableName.Length);

				if ( queryTableNames.IndexOf(tableName) == -1 )
				{
					queryTableNames.Add(tableName);
				}

				int asFieldName;
				string plainTableFieldName = GetPlainTableFieldName(tableFieldName, out asFieldName);

#if true
				XmlNode alias = column[cc.qc.property.ToString()][cc.prop.alias.ToString()];
				property[(int)prop.alias] = (alias == null) ? string.Empty : alias.InnerText;
				if ( (property[(int)prop.alias].Length != 0) && (asFieldName == -1) )	// プロパティでの別名があり、直接の別名指定は無い？
				{
					tableFieldName += " AS " + property[(int)prop.alias];
					plainTableFieldName = GetPlainTableFieldName(tableFieldName, out asFieldName);
				}
#endif

				if ( bool.Parse(column[qc.showField.ToString()].InnerText) )
				{
					string groupFunc = column[qc.groupFunc.ToString()].InnerText;
					if ( !string.IsNullOrEmpty(groupFunc) )
					{
						tableFieldName = groupFunc + "(" + tableFieldName + ")";
						groupFuncCount++;
					}

#if true
					if ( (property[(int)prop.type] == "DATE") && !tableFieldName.StartsWith("to_char(", StringComparison.OrdinalIgnoreCase) )
					{
						//select.Append(" " + "to_char(" + tableFieldName + ",'YYYY/MM/DD HH24:MI:SS') " + fieldName + ",\r\n");
						select.Append(" " + "to_char(" + plainTableFieldName + ",'YYYY/MM/DD HH24:MI:SS') ");
						select.Append((asFieldName != -1) ? tableFieldName.Substring(asFieldName/* + 4*/).Trim() : fieldName);
						select.Append(",\r\n");
					}
					else
					{
						select.Append(" " + tableFieldName + ",\r\n");
					}
#else
					select.Append(" " + tableFieldName + ",\r\n");
#endif

					colComments.Append(property[(int)prop.comment] + sepOutput);
					if ( property[(int)prop.comment] != propNoComment )
					{
						colCommentsCount++;
					}
				}

				tableFieldName = plainTableFieldName;

				// 条件式
				string expression = column[qc.expression.ToString()].InnerText;
				string value1 = column[qc.value1.ToString()].InnerText.Trim();
				string value2 = column[qc.value2.ToString()].InnerText.Trim();
				string rColOp = column[qc.rColOp.ToString()].InnerText;
				string leftRndBkt = "(", rightRndBkt = ")";

				string quotation = (property[(int)prop.type].StartsWith("VARCHAR")) ? "'" : "";

				if ( !string.IsNullOrEmpty(value1) && (property[(int)prop.type] == "DATE") )	// 日付の条件指定あり？
				{
					int dtfmt = value1.IndexOf('/') == -1 ? 0 : 1;
					string toChar = (value1[0] == '(') ? "to_char" : "";
					string dateQuote = (Char.IsDigit(value1[0])) ? "'" : "";
					value1 = "to_date(" + toChar + dateQuote + value1 + dateQuote + ",'" + sqlDateFormat[dtfmt] + "')";
				}

				if ( rColOp.Length == 0 )
				{
					rColOp = "AND";
				}

				if ( rColOp == "OR" )
				{
					leftRndBkt += (!cameOR) ? "(" : "";
					cameOR = true;
				}
				else/* if ( rColOp == "AND" )*/
				{
					//rightRndBkt += (cameOR) ? ")" : "";
					if ( cameOR )
					{
						if ( expression.Length != 0 )
						{
							rightRndBkt += ")";
						}
						else
						{
							where.Insert(where.Length - 5, ')');	// OR の括弧が閉じられていないので、強制的に右括弧で閉じる 5:" OR\r\n"
						}
					}
					cameOR = false;
				}

				// =, NOT =, >=, <=, >, <
				if ( (expression.IndexOf('=') != -1 || expression == "<" || expression == ">") && !string.IsNullOrEmpty(value1) )
				{
					expression = (expression == "NOT =") ? "<>" : expression;
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// BETWEEN, NOT BETWEEN
				else if ( (expression.IndexOf("BETWEEN") != -1) && (!string.IsNullOrEmpty(value1) && !string.IsNullOrEmpty(value2)) )
				{
					if ( !string.IsNullOrEmpty(value2) && (property[(int)prop.type] == "DATE") )	// 日付の条件指定あり？
					{
						int dtfmt = value2.IndexOf('/') == -1 ? 0 : 1;
						string toChar = (value2[0] == '(') ? "to_char" : "";
						string dateQuote = (Char.IsDigit(value2[0])) ? "'" : "";
						value2 = "to_date(" + toChar + dateQuote + value2 + dateQuote + ",'" + sqlDateFormat[dtfmt] + "')";
					}
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " " + quotation + value1 + quotation + " AND " + quotation + value2 + quotation + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IN, NOT IN
				else if ( (expression.IndexOf("IN") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string[] values = value1.Split(',');
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " (");
					for ( int k = 0; k < values.Length; k++ )
					{
						where.Append(quotation + values[k] + quotation + ((k != values.Length - 1) ? "," : ""));
					}
					where.Append(")" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// LIKE, NOT LIKE
				else if ( (expression.IndexOf("LIKE") != -1) && !string.IsNullOrEmpty(value1) )
				{
					string wildCard = (value1.IndexOfAny(new char[] { '%', '_' }) == -1) ? "%" : "";
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + " '" + value1 + wildCard + "'" + rightRndBkt + " " + rColOp + "\r\n");
				}
				// IS NULL, IS NOT NULL
				else if ( (expression.IndexOf("NULL") != -1) && string.IsNullOrEmpty(value1) )
				{
					where.Append(" " + leftRndBkt + tableFieldName + " " + expression + rightRndBkt + " " + rColOp + "\r\n");
				}

				// ソート順
				string order = column[qc.orderBy.ToString()].InnerText.Trim();
				if ( !string.IsNullOrEmpty(order) )
				{
					int k, number;
					for ( k = 0; k < order.Length && Char.IsDigit(order[k]); k++ ) ;
					number = (Char.IsDigit(order[0])) ? int.Parse(order.Substring(0, k)) : 999;
					string desc = (order.IndexOf("DESC", k, StringComparison.CurrentCultureIgnoreCase) != -1) ? " DESC" : "";
					orders.Add(number.ToString("D3") + "\t" + tableFieldName + desc);
				}
			}

			if ( select.Length == defSelect )
			{
				columnComments = "表示する項目が１つ以上必要です";
				return false;
			}

			// FROM テーブル名
			foreach ( string tableName in queryTableNames )
			{
				from.Append(" " + tableName + ",\r\n");
			}

			if ( groupFuncCount != 0 )	// グループ関数の指定あり？
			{
				// GROUP BY
				ArrayList groupFields = new ArrayList();
				foreach ( XmlNode column in xmlShenlongColumn.DocumentElement.SelectNodes(tagColumn) )
				{
					if ( !string.IsNullOrEmpty(column[qc.groupFunc.ToString()].InnerText) )
						continue;
					string tableFieldName = GetPlainTableFieldName(column.Attributes[attrTableName].Value + "." + column[qc.fieldName.ToString()].InnerText);
					if ( groupFields.IndexOf(tableFieldName) != -1 )
						continue;
					groupFields.Add(tableFieldName);
				}
				if ( groupFields.Count != 0 )
				{
					foreach ( string groupField in groupFields )
					{
						groupBy.Append(" " + groupField + ",\r\n");
					}
					groupBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
				}
			}

			if ( orders.Count != 0 )	// ソートの指定あり？
			{
				// ORDER BY
				orders.Sort();
				foreach ( string order in orders )
				{
					orderBy.Append(" " + order.Substring(3 + 1) + ",\r\n");	// (3 + 1):ソート順\t
				}
				orderBy.Length -= (1 + 2);	// (1 + 2):",\r\n"
			}

			// WHERE
			if ( groupFuncCount == 0 )
			{
				// ROWNUM の最大指定あり？
				if ( HasMaxRowNum(xmlShenlongColumn) )
				{
					maxRowNum = int.Parse(xmlShenlongColumn.DocumentElement[tagProperty][tagMaxRowNum].InnerText);
				}
				if ( 0 < maxRowNum )
				{
					where.Append(" (ROWNUM <= " + maxRowNum + ") AND\r\n");
				}
			}
			if ( defWhereLen < where.Length )
			{
				where.Insert(defWhereLen + 1, '(');
				int lastSpace;
				for ( lastSpace = where.Length - 1; where[lastSpace] != ' '; lastSpace-- ) ;
				where.Remove(lastSpace + 1, where.Length - lastSpace - 1);		// "AND|OR\r\n" を削除する
				if ( cameOR )
				{
					where.Insert(lastSpace++, ')');
					cameOR = false;
				}
				where.Insert(lastSpace, ')');

				if ( groupFuncCount != 0 )
				{
					// HAVING
					groupBy.Append("\r\nHAVING\r\n" + where.ToString().Substring(defWhereLen));
					where = new StringBuilder("\r\nWHERE\r\n");
				}
			}

			// テーブル結合
			XmlNodeList tableJoins = xmlShenlongColumn.DocumentElement.SelectNodes(tagTableJoin);
			for ( int i = 0; i < tableJoins.Count; i++ )
			{
				XmlNode tableJoin = tableJoins[i];
				if ( (i == 0) && (defWhereLen != where.Length) )
				{
					where.Append("AND\r\n");
				}
				where.Append(" (" + GetPlainTableFieldName(tableJoin.Attributes[tabJoin.leftTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == ">=" ? " (+)" : ""));
				where.Append(" = ");
				where.Append(GetPlainTableFieldName(tableJoin.Attributes[tabJoin.rightTabCol.ToString()].Value) + (tableJoin.Attributes[tabJoin.way.ToString()].Value == "<=" ? " (+)" : ""));
				where.Append(") ");
				if ( i != tableJoins.Count - 1 )
				{
					where.Append("AND\r\n");
				}
			}

			buldedSql = select.ToString(0, select.Length - (1 + 2)) + " " +	// (1 + 2):",\r\n"
						from.ToString(0, from.Length - (1 + 2)) + " " +		// (1 + 2):",\r\n"
						((groupBy.Length == defGroupByLen) ? "" : groupBy.ToString()) +
						((where.Length == defWhereLen) ? "" : where.ToString()) +
						((orderBy.Length == defOrderByLen) ? "" : orderBy.ToString()) +
						"\r\n";

			if ( colCommentsCount != 0 )
			{
				columnComments = colComments.ToString();
			}

			return true;
		}
		catch ( Exception exp )
		{
			columnComments = exp.Message;
			return false;
		}
	}
#endif
//#endif

#if true
	/// <summary>
	/// テーブル名.カラム名を分割する
	/// ShenGlobal.SplitTableFieldName が OWNER.TNAME に対応したので、互換性を保つ為にとりあえずこれを使う 2010/03/29
	/// </summary>
	/// <param name="tableFieldName"></param>
	/// <param name="tableName"></param>
	/// <param name="fieldName"></param>
	private static bool SplitTableFieldName(string tableFieldName, out string tableName, out string fieldName, bool? plainTblName)
	{
		int dot = tableFieldName.IndexOf('.');
		if ( dot == -1 )
		{
			tableName = fieldName = string.Empty;
			return false;
		}

		if ( plainTblName == null )
		{
			tableName = tableFieldName.Substring(0, dot);
		}
		else
		{
			tableName = ShenGlobal.GetTableName(tableFieldName.Substring(0, dot), (bool)plainTblName);
		}

		fieldName = tableFieldName.Substring(dot + 1);

		return true;
	}
#endif

#if WITHIN_SHENGLOBAL
	/// <summary>
	/// カラムがCHAR型か否か
	/// </summary>
	/// <param name="colType"></param>
	/// <returns></returns>
	public static bool IsCharColumn(string colType)
	{
		return (colType.StartsWith("VARCHAR") || colType.StartsWith("CHAR"));
	}
#endif

//#if WITHIN_SHENGLOBAL
	/// <summary>
	/// 別名を除いたテーブル名.カラム名を抽出する
	/// </summary>
	/// <param name="tableFieldName"></param>
	/// <param name="asFieldName"></param>
	/// <returns></returns>
	public static string GetPlainTableFieldName(/*ref */string tableFieldName, out int asFieldName)
	{
		asFieldName = -1;
		string plainTableFieldName = tableFieldName;

		try
		{
			if ( (asFieldName = tableFieldName.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase)) != -1 )
			{
				plainTableFieldName = tableFieldName.Substring(0, asFieldName).TrimEnd();
				//tableFieldName = tableFieldName.Replace('(', '（').Replace(')', '）');
			}
		}
		catch ( Exception exp )
		{
			System.Diagnostics.Debug.WriteLine(exp.Message);
		}

		return plainTableFieldName;
	}

	public static string GetPlainTableFieldName(string tableFieldName)
	{
		int asFieldName;
		return GetPlainTableFieldName(tableFieldName, out asFieldName);
	}
//#endif

#if WITHIN_SHENGLOBAL
	/// <summary>
	/// 最大抽出行数の設定がある？
	/// </summary>
	/// <param name="xmlShenlongColumn"></param>
	/// <returns></returns>
	public static bool HasMaxRowNum(XmlDocument xmlShenlongColumn)
	{
		return ((xmlShenlongColumn.DocumentElement[ShenGlobal.tagProperty][ShenGlobal.tagMaxRowNum] != null) &&
				(xmlShenlongColumn.DocumentElement[ShenGlobal.tagProperty][ShenGlobal.tagMaxRowNum].InnerText.Length != 0));
	}
#endif

#if WITHIN_SHENGLOBAL
#if TABLE_NAME_HAS_ALIAS
	/// <summary>
	/// テーブル名（またはその別名）を取得する
	/// </summary>
	/// <param name="tableName"></param>
	/// <param name="plainTblName"></param>
	/// <returns></returns>
	public static string GetTableName(string tableName, bool plainTblName)
	{
		string _tableName;
		string _alias;

		int index = tableName.IndexOf(' ');
		if ( index == -1 )
		{
			_tableName = tableName;
			_alias = null;
		}
		else
		{
			_tableName = tableName.Substring(0, index);
			_alias = tableName.Substring(index).Trim();
		}

		return (plainTblName || (_alias == null)) ? _tableName : _alias;
	}

	/// <summary>
	/// テーブル名.カラム名を分割する
	/// </summary>
	/// <param name="tableFieldName"></param>
	/// <param name="tableName"></param>
	/// <param name="fieldName"></param>
	private static bool SplitTableFieldName(string tableFieldName, out string tableName, out string fieldName, bool? plainTblName)
	{
		int dot = tableFieldName.IndexOf('.');
		if ( dot == -1 )
		{
			tableName = fieldName = string.Empty;
			return false;
		}

		if ( plainTblName == null )
		{
			tableName = tableFieldName.Substring(0, dot);
		}
		else
		{
			tableName = GetTableName(tableFieldName.Substring(0, dot), (bool)plainTblName);
		}

		fieldName = tableFieldName.Substring(dot + 1);

		return true;
	}
#endif
#endif

#if true
	/// <summary>
	/// クエリーを実行する
	/// HTML 用
	/// </summary>
	/// <param name="dataSource"></param>
	/// <param name="userId"></param>
	/// <param name="password"></param>
	/// <param name="sql"></param>
	/// <param name="dataTypeName"></param>
	/// <param name="dataTable"></param>
	public static void ExecuteQuery(string dataSource, string userId, string password, string sql, out string[] dataTypeName, out DataTable dataTable)
	{
		OracleConnection oleConn = null;
		OracleDataAdapter oleDataAdapter = null;
		dataTypeName = null;
		dataTable = null;

		try
		{
			oleConn = new OracleConnection("Data Source=" + dataSource + ";" +
										  "user id=" + userId + ";password=" + password + ";" +
										  "persist security info=false;");
			oleConn.Open();
			oleDataAdapter = new OracleDataAdapter(sql, oleConn);
			dataTable = new DataTable();
			oleDataAdapter.Fill(dataTable);

			dataTypeName = new string[dataTable.Columns.Count];

			for ( int i = 0; i < dataTypeName.Length; i++ )		// ソースデータ型を取得
			{
				dataTypeName[i] = GetOraDataTypeName(dataTable.Columns[i].DataType);
			}
		}
		finally
		{
			if ( oleDataAdapter != null )
			{
				oleDataAdapter.Dispose();
				oleDataAdapter = null;
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
	}

	/// <summary>
	/// クエリーを実行する
	/// Excel 用
	/// </summary>
	/// <param name="dataSource"></param>
	/// <param name="userId"></param>
	/// <param name="password"></param>
	/// <param name="sql"></param>
	/// <param name="format"></param>
	/// <param name="headerOutput"></param>
	/// <param name="columnComments"></param>
	/// <param name="conditions"></param>
	/// <param name="queryOutput"></param>
	public static void ExecuteQuery(string dataSource, string userId, string password, string sql, string format, int headerOutput, string columnComments, string conditions, out StringBuilder queryOutput)
	{
		OracleConnection oleConn = null;
		OracleDataAdapter oleDataAdapter = null;
		queryOutput = new StringBuilder();
		DataTable dataTable = null;

		try
		{
			oleConn = new OracleConnection("Data Source=" + dataSource + ";" +
										  "user id=" + userId + ";password=" + password + ";" +
										  "persist security info=false;");
			oleConn.Open();
			oleDataAdapter = new OracleDataAdapter(sql, oleConn);
			dataTable = new DataTable();
			oleDataAdapter.Fill(dataTable);

			string[] columnNames = new string[dataTable.Columns.Count];
			string[] dataTypeName = new string[dataTable.Columns.Count];

			for ( int i = 0; i < dataTypeName.Length; i++ )		// ソースデータ型を取得
			{
				columnNames[i] = dataTable.Columns[i].ColumnName;
				dataTypeName[i] = GetOraDataTypeName(dataTable.Columns[i].DataType);
			}

			bool outputColumnName = ((headerOutput & (int)ShenGlobal.header.columnName) != 0) ||
									(((headerOutput & (int)ShenGlobal.header.comment) != 0) && (columnComments == null));

			if ( format.StartsWith(EXCEL_FORMAT_SYLK) )
			{
				int rowBegin = 1/*2*/;
				int colBegin = 1/*2*/;

				queryOutput.Append("ID;BUBBLES;N;E\r\nP;PGeneral\r\nP;P#,##0\r\nP;P#,##0.00\r\n");

				if ( !string.IsNullOrEmpty(conditions) )
				{
					queryOutput.Append(";\r\n; 抽出条件\r\n;\r\n");
					queryOutput.Append("C;Y" + rowBegin + ";" + "X" + colBegin + ";K");
					queryOutput.Append("\"" + conditions + "\"\r\n");
					rowBegin++;
					queryOutput.Append("C;Y" + rowBegin + ";" + "X" + colBegin + ";K");
					queryOutput.Append("\"\"\r\n");
					rowBegin++;
				}

				StringBuilder globalFormatting = null, titleRow = null;
				globalFormatting = new StringBuilder(";\r\n; Global Formatting\r\n;\r\n"/* + "F;C1;FG0R;SM1\r\n"*/);

				titleRow = new StringBuilder(";\r\n; Title Row\r\n;\r\n");
				for ( int i = 0, x = colBegin; i < dataTable.Columns.Count; i++, x++ )		// フィールド名を取得
				{
					if ( outputColumnName )
					{
						titleRow.Append("C;" + ((i == 0) ? "Y" + rowBegin + ";" : "") + "X" + x + ";K\"" + columnNames[i] + "\"\r\n");
					}
					globalFormatting.Append("F;C" + (i + colBegin).ToString() + ";" + (ShenGlobal.IsCharColumn(dataTypeName[i]) || dataTypeName[i] == "DATE" ? "FG0L;" : "FG0R;") + "SM0\r\n");
				}

				queryOutput.Append(globalFormatting);
				if ( outputColumnName )
				{
					queryOutput.Append(titleRow);
					rowBegin++;
				}

				if ( ((headerOutput & (int)ShenGlobal.header.comment) != 0) && !string.IsNullOrEmpty(columnComments) )
				{
					StringBuilder commentRow = new StringBuilder(";\r\n; Comment Row\r\n;\r\n");
					string[] colComments = columnComments.Split(ShenGlobal.sepOutput[0]);
					for ( int i = 0, x = colBegin; i < colComments.Length; i++, x++ )
					{
						string comment = (outputColumnName || (colComments[i] != ShenGlobal.propNoComment)) ? colComments[i] : columnNames[i];
						commentRow.Append("C;" + ((i == 0) ? "Y" + rowBegin + ";" : "") + "X" + x + ";K\"" + comment + "\"\r\n");
					}
					queryOutput.Append(commentRow);
					rowBegin++;
				}

				for ( int i = 0; i < dataTable.Rows.Count; i++ )
				{
					queryOutput.Append(";\r\n; Row " + (i + 1) + "\r\n;\r\n");
					queryOutput.Append("C;Y" + (rowBegin + (i + 0)).ToString() + "\r\n");

					for ( int j = 0, x = colBegin; j < dataTable.Columns.Count; j++, x++ )
					{
						bool number = (dataTypeName[j] == "NUMERIC");
						queryOutput.Append("C;X" + x + ";K");
						queryOutput.Append((number ? "" : "\"") + dataTable.Rows[i][j] + (number ? "" : "\"") + "\r\n");
					}
				}

				//queryOutput.Append(";\r\n; Format Column Widths\r\n;\r\nF;W1 1 3\r\n");
				queryOutput.Append("E\r\n");
			}
			else if ( format.StartsWith(EXCEL_FORMAT_XML) )
			{
				queryOutput.AppendLine("<?xml version=\"1.0\"?>");
				queryOutput.AppendLine("<?mso-application progid=\"Excel.Sheet\"?>");
				queryOutput.AppendLine("<Workbook xmlns=\"urn:schemas-microsoft-com:office:spreadsheet\"");
				queryOutput.AppendLine(" xmlns:o=\"urn:schemas-microsoft-com:office:office\"");
				queryOutput.AppendLine(" xmlns:x=\"urn:schemas-microsoft-com:office:excel\"");
				queryOutput.AppendLine(" xmlns:ss=\"urn:schemas-microsoft-com:office:spreadsheet\"");
				queryOutput.AppendLine(" xmlns:html=\"http://www.w3.org/TR/REC-html40\">");
				queryOutput.AppendLine(" <Styles>");
				queryOutput.AppendLine("  <Style ss:ID=\"s21\">");
				queryOutput.AppendLine("   <Alignment ss:Horizontal=\"Right\" ss:Vertical=\"Center\"/>");
				queryOutput.AppendLine("  </Style>");
				queryOutput.AppendLine(" </Styles>");

				string sheetName = format.Substring(4);
				queryOutput.AppendLine(" <Worksheet ss:Name=\"" + sheetName + "\">");
				queryOutput.AppendLine("  <Table>");

				if ( !string.IsNullOrEmpty(conditions) )
				{
					queryOutput.AppendLine("   <Row>");
					queryOutput.AppendLine("    <Cell><Data ss:Type=\"String\">" + conditions + "</Data></Cell>");
					queryOutput.AppendLine("   </Row>");
					queryOutput.AppendLine("   <Row>");
					queryOutput.AppendLine("    <Cell><Data ss:Type=\"String\">" + "" + "</Data></Cell>");
					queryOutput.AppendLine("   </Row>");
				}

				if ( outputColumnName )
				{
					queryOutput.AppendLine("   <Row>");
					for ( int i = 0; i < dataTable.Columns.Count; i++ )		// フィールド名を取得
					{
						string styleID = (ShenGlobal.IsCharColumn(dataTypeName[i]) || dataTypeName[i] == "DATE") ? "" : " ss:StyleID=\"s21\"";
						queryOutput.AppendLine("    <Cell" + styleID + "><Data ss:Type=\"String\">" + columnNames[i] + "</Data></Cell>");
					}
					queryOutput.AppendLine("   </Row>");
				}

				if ( ((headerOutput & (int)ShenGlobal.header.comment) != 0) && !string.IsNullOrEmpty(columnComments) )
				{
					queryOutput.AppendLine("   <Row>");
					string[] colComments = columnComments.Split(ShenGlobal.sepOutput[0]);
					for ( int i = 0; i < colComments.Length; i++ )
					{
						string comment = (outputColumnName || (colComments[i] != ShenGlobal.propNoComment)) ? colComments[i] : columnNames[i];
						queryOutput.AppendLine("    <Cell><Data ss:Type=\"String\">" + comment + "</Data></Cell>");
					}
					queryOutput.AppendLine("   </Row>");
				}

				for ( int i = 0; i < dataTable.Rows.Count; i++ )
				{
					queryOutput.AppendLine("   <Row>");
					for ( int j = 0; j < dataTable.Columns.Count; j++ )
					{
						string type = (dataTypeName[j] == "NUMERIC") ? "Number" : "String";
						queryOutput.AppendLine("    <Cell><Data ss:Type=\"" + type + "\">" + dataTable.Rows[i][j] + "</Data></Cell>");
					}
					queryOutput.AppendLine("   </Row>");
				}

				queryOutput.AppendLine("  </Table>");
				queryOutput.AppendLine(" </Worksheet>");
				queryOutput.AppendLine("</Workbook>");
			}
		}
		finally
		{
			if ( dataTable != null )
			{
				dataTable.Clear();
				dataTable.Dispose();
				dataTable = null;
			}

			if ( oleDataAdapter != null )
			{
				oleDataAdapter.Dispose();
				oleDataAdapter = null;
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
	}

	/// <summary>
	/// .net の型をオラクルの型に変換する
	/// 日付型は SELECT 文で to_char にしているので文字列となっている
	/// </summary>
	/// <param name="dataType"></param>
	/// <returns></returns>
	private static string GetOraDataTypeName(Type dataType)
	{
		if ( dataType == typeof(String) )
		{
			return "VARCHAR2";
		}
		else if ( dataType == typeof(DateTime) )
		{
			return "DATE";
		}
		else if ( (dataType == typeof(Decimal)) || (dataType == typeof(Double)) )
		{
			return "NUMERIC";
		}

		return dataType.Name;
	}
#else
#if true
	/// <summary>
	/// クエリーを実行する
	/// </summary>
	/// <param name="dataSource"></param>
	/// <param name="userId"></param>
	/// <param name="password"></param>
	/// <param name="sql"></param>
	/// <param name="columnComments"></param>
	/// <param name="outType"></param>
	/// <param name="queryOutput"></param>
	/// <param name="dataTypeName"></param>
	/// <param name="dataTable"></param>
	public static void ExecuteQuery(string dataSource, string userId, string password, string sql, string columnComments, ot outType, out StringBuilder queryOutput, out string[] dataTypeName, out DataTable dataTable)
	{
		OleDbConnection oleConn = null;
		OleDbDataAdapter oleDataAdapter = null;
		queryOutput = new StringBuilder();
		dataTypeName = null;
		dataTable = null;

		try
		{
			oleConn = new OleDbConnection("Provider=MSDAORA;Data Source=" + dataSource + ";" +
										  "user id=" + userId + ";password=" + password + ";" +
										  "persist security info=false;");
			oleConn.Open();
			oleDataAdapter = new OleDbDataAdapter(sql, oleConn);
			dataTable = new DataTable();
			oleDataAdapter.Fill(dataTable);

			dataTypeName = new string[dataTable.Columns.Count];

			for ( int i = 0; i < dataTypeName.Length; i++ )		// ソースデータ型を取得
			{
				dataTypeName[i] = GetOraDataTypeName(dataTable.Columns[i].DataType);
			}

			if ( outType == ot.html )
				return;

			StringBuilder globalFormatting = null, titleRow = null;

			queryOutput.Append("ID;BUBBLES;N;E\r\nP;PGeneral\r\nP;P#,##0\r\nP;P#,##0.00\r\n");
			globalFormatting = new StringBuilder(";\r\n; Global Formatting\r\n;\r\n"/* + "F;C1;FG0R;SM1\r\n"*/);
			titleRow = new StringBuilder(";\r\n; Title Row\r\n;\r\n");

			int colBegin = 1/*2*/;

			for ( int i = 0, x = colBegin; i < dataTable.Columns.Count; i++, x++ )		// フィールド名を取得
			{
				titleRow.Append("C;" + ((i == 0) ? "Y1;" : "") + "X" + x + ";K\"" + dataTable.Columns[i].ColumnName + "\"\r\n");
				globalFormatting.Append("F;C" + (i + colBegin).ToString() + ";" + (ShenGlobal.IsCharColumn(dataTypeName[i]) || dataTypeName[i] == "DATE" ? "FG0L;" : "FG0R;") + "SM0\r\n");
			}

			queryOutput.Append(globalFormatting);
			queryOutput.Append(titleRow);

			int rowBegin = 1/*2*/;

			if ( columnComments != null )
			{
				StringBuilder commentRow = new StringBuilder(";\r\n; Comment Row\r\n;\r\n");
				string[] colComments = columnComments.Split(ShenGlobal.sepOutput[0]);
				for ( int i = 0, x = colBegin; i < colComments.Length; i++, x++ )
				{
					commentRow.Append("C;" + ((i == 0) ? "Y2;" : "") + "X" + x + ";K\"" + colComments[i] + "\"\r\n");
				}
				queryOutput.Append(commentRow);
				rowBegin++;
			}

			for ( int i = 0; i < dataTable.Rows.Count; i++ )
			{
				queryOutput.Append(";\r\n; Row " + (i + 1) + "\r\n;\r\n");
				queryOutput.Append("C;Y" + (rowBegin + (i + 1)).ToString() + "\r\n");

				for ( int j = 0, x = colBegin; j < dataTable.Columns.Count; j++, x++ )
				{
					bool number = (dataTypeName[j] == "NUMERIC");
					queryOutput.Append("C;X" + x + ";K");
					queryOutput.Append((number ? "" : "\"") + dataTable.Rows[i][j] + (number ? "" : "\"") + "\r\n");
				}
			}

			//queryOutput.Append(";\r\n; Format Column Widths\r\n;\r\nF;W1 1 3\r\n");
			queryOutput.Append("E\r\n");
		}
		finally
		{
			if ( (outType == ot.excel) && (dataTable != null) )
			{
				dataTable.Clear();
				dataTable.Dispose();
				dataTable = null;
			}

			if ( oleDataAdapter != null )
			{
				oleDataAdapter.Dispose();
				oleDataAdapter = null;
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
	}

	/// <summary>
	/// .net の型をオラクルの型に変換する
	/// 日付型は SELECT 文で to_char にしているので文字列となっている
	/// </summary>
	/// <param name="dataType"></param>
	/// <returns></returns>
	private static string GetOraDataTypeName(Type dataType)
	{
		if ( dataType == typeof(String) )
		{
			return "VARCHAR2";
		}
		else if ( dataType == typeof(DateTime) )
		{
			return "DATE";
		}
		else if ( dataType == typeof(Decimal) )
		{
			return "NUMERIC";
		}

		return dataType.Name;
	}
#else
	/// <summary>
	/// クエリーを実行する
	/// </summary>
	/// <param name="dataSource"></param>
	/// <param name="userId"></param>
	/// <param name="password"></param>
	/// <param name="sql"></param>
	/// <param name="columnComments"></param>
	/// <param name="outType"></param>
	/// <param name="queryOutput"></param>
	/// <param name="dataTypeName"></param>
	/// <param name="dataTable"></param>
	public static void ExecuteQuery(string dataSource, string userId, string password, string sql, string columnComments, ot outType, out StringBuilder queryOutput, out string[] dataTypeName, out DataTable dataTable)
	{
		OleDbConnection oleConn = null;
		OleDbCommand oleCmd = null;
		OleDbDataReader oleReader = null;
		queryOutput = new StringBuilder();
		dataTypeName = null;
		dataTable = null;

		try
		{
			oleConn = new OleDbConnection("Provider=MSDAORA;Data Source=" + dataSource + ";" +
										  "user id=" + userId + ";password=" + password + ";" +
										  "persist security info=false;");
			oleConn.Open();
			oleCmd = new OleDbCommand(sql, oleConn);
			oleReader = oleCmd.ExecuteReader();

			dataTypeName = new string[oleReader.FieldCount];

			//DataTable dataTable = null;
			DataRow dataRow = null;
			StringBuilder globalFormatting = null, titleRow = null;

			if ( outType == ot.html )
			{
				dataTable = new DataTable();
			}
			else
			{
				queryOutput.Append("ID;ORACLE;N;E\r\nP;PGeneral\r\nP;P#,##0\r\nP;P#,##0.00\r\n");
				globalFormatting = new StringBuilder(";\r\n; Global Formatting\r\n;\r\nF;C1;FG0R;SM1\r\n");
				titleRow = new StringBuilder(";\r\n; Title Row\r\n;\r\n");
			}

			for ( int i = 0, x = 2; i < oleReader.FieldCount; i++, x++ )		// フィールド名を取得
			{
				dataTypeName[i] = GetOraDataTypeName(oleReader.GetDataTypeName(i));

				if ( outType == ot.html )
				{
					dataTable.Columns.Add(oleReader.GetName(i));
				}
				else
				{
					titleRow.Append("C;" + ((i == 0) ? "Y1;" : "") + "X" + x + ";K\"" + oleReader.GetName(i) + "\"\r\n");
					globalFormatting.Append("F;C" + (i + 2).ToString() + ";" + (dataTypeName[i].StartsWith("VARCHAR") || dataTypeName[i] == "DATE" ? "FG0L;" : "FG0R;") + "SM0\r\n");
				}
			}

			if ( outType == ot.html )
			{
			}
			else
			{
				queryOutput.Append(globalFormatting);
				queryOutput.Append(titleRow);
			}

			int rowBegin = 2, rowCount = 1;	// for excel

			if ( columnComments != null )
			{
				if ( outType == ot.html )
				{
				}
				else
				{
					StringBuilder commentRow = new StringBuilder(";\r\n; Comment Row\r\n;\r\n");
					string[] colComments = columnComments.Split('\t');
					for ( int i = 0, x = 2; i < colComments.Length; i++, x++ )
					{
						commentRow.Append("C;" + ((i == 0) ? "Y2;" : "") + "X" + x + ";K\"" + colComments[i] + "\"\r\n");
					}
					queryOutput.Append(commentRow);
					rowBegin++;
				}
			}

			Object[] values = new Object[oleReader.FieldCount];

			while ( oleReader.Read() )								// １行ずつ読み込む
			{
				if ( outType == ot.html )
				{
					dataRow = dataTable.NewRow();
				}
				else
				{
					queryOutput.Append(";\r\n; Row " + rowCount + "\r\n;\r\n");
					queryOutput.Append("C;Y" + (rowBegin + rowCount).ToString() + "\r\n");
				}

				oleReader.GetValues(values);
				int x = 2, i = 0;
				foreach ( Object value in values )
				{
					string strValue = value.ToString();

					if ( outType == ot.html )
					{
						dataRow[i++] = strValue;
					}
					else
					{
						bool number = (dataTypeName[i++] == "NUMERIC");
						//queryOutput.Append("C;X" + x + ";K\"" + strValue + "\"\r\n");
						queryOutput.Append("C;X" + x + ";K");
						queryOutput.Append((number ? "" : "\"") + strValue + (number ? "" : "\"") + "\r\n");
						x++;
					}
				}

				if ( outType == ot.html )
				{
					dataTable.Rows.Add(dataRow);
				}
				else
				{
					rowCount++;
				}
			}

			if ( outType == ot.html )
			{
				//dataView = new DataView(dataTable);
			}
			else
			{
				queryOutput.Append(";\r\n; Format Column Widths\r\n;\r\nF;W1 1 3\r\n");
				queryOutput.Append("E\r\n");
			}
		}
		finally
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
	}

	/// <summary>
	/// OleDb の型をオラクルの型に変換する
	/// </summary>
	/// <param name="dataTypeName"></param>
	/// <returns></returns>
	private static string GetOraDataTypeName(string dataTypeName)
	{
		switch ( dataTypeName )
		{
			case "DBTYPE_VARCHAR":
				return ("VARCHAR2");
			case "DBTYPE_DBTIMESTAMP":
				return ("DATE");
			default:
				if ( dataTypeName.StartsWith("DBTYPE_") )
				{
					return dataTypeName.Substring(7);
				}
				return (dataTypeName);
		}
	}
#endif
#endif

	/// <summary>
	/// ログに保存するテーブル名を CSV 文字列にする
	/// </summary>
	/// <param name="fromTableNames"></param>
	/// <returns></returns>
	public static string GetLogTableNames(List<string> fromTableNames)
	{
		try
		{
			List<string> logTableNames = new List<string>();
			StringBuilder logTableName = new StringBuilder();

			foreach ( string table in fromTableNames )
			{
#if TABLE_NAME_HAS_ALIAS
				string tableName = ShenGlobal.GetTableName(table, true);
#else
				string tableName = table;
#endif
				if ( logTableNames.IndexOf(tableName) == -1 )
				{
					logTableNames.Add(tableName);
					logTableName.Append(tableName + ",");
				}
			}

			if ( logTableName.Length != 0 )
			{
				logTableName.Length--;
			}

			return logTableName.ToString();
		}
		catch ( Exception exp )
		{
			System.Diagnostics.Debug.WriteLine(exp.Message);
			return string.Empty;
		}
	}

	/// <summary>
	/// アクセス ログをテーブルに保存する
	/// </summary>
	/// <param name="writeLogDsnUidPwd"></param>
	/// <param name="eggName"></param>
	/// <param name="tableNames"></param>
	/// <param name="serviceName"></param>
	/// <param name="userName"></param>
	/// <param name="pcName"></param>
	/// <param name="outType"></param>
	/// <param name="progNo"></param>
	public static void WriteAccessLog(string[] writeLogDsnUidPwd, string eggName, string tableNames, string serviceName, string userName, string pcName, ot outType, ShenGlobal.pno progNo)
	{
		OracleConnection oleInfoPub = null;
		OracleCommand oleCmd = null;

		try
		{
			if ( writeLogDsnUidPwd[0] == null )
				return;

			string connectString = "" +
								   "Data Source=" + writeLogDsnUidPwd[0] + ";" +
								   "user id=" + writeLogDsnUidPwd[1] + ";" +
								   "password=" + writeLogDsnUidPwd[2] + ";" +
								   "persist security info=false;";
			oleInfoPub = new OracleConnection(connectString);
			oleInfoPub.Open();		// 情報公開サーバに接続する

			string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");	// ACCESS_DATE

			string sql = "INSERT INTO T_LOG_BUBBLES " +
						 "(ACCESS_DATE,EGG_NAME,PC_NAME,OUT_TYPE) " +
						 "VALUES(" +
						 " TO_DATE('" + now + "','yyyy/mm/dd hh24:mi:ss')" + "," +
						 " '" + eggName + "'" + "," +
						 " '" + pcName + "'," + "" +
						 " '" + ((int)outType + 1) + "'" +
						 ")";
			oleCmd = new OracleCommand(sql, oleInfoPub);
			oleCmd.ExecuteNonQuery();
			oleCmd.Dispose();
			oleCmd = null;
#if false
			sql = "DELETE T_LOG_BUBBLES " +
				  "WHERE PC_NAME='" + pcName + "'";
			oleCmd = new OleDbCommand(sql, oleInfoPub);
			int rows = oleCmd.ExecuteNonQuery();
			oleCmd.Dispose();
			oleCmd = null;
#endif

			foreach ( string tableName in tableNames.Split(',') )
			{
				sql = "INSERT INTO T_LOG_SHENLONG " +
					  "(ACCESS_DATE,SERVICE_NAME,USER_NAME,TABLE_NAME,PC_NAME,PROG_NO) " +
					  "VALUES(" +
					  " TO_DATE('" + now + "','yyyy/mm/dd hh24:mi:ss')" + "," +
					  " '" + serviceName + "'" + "," +
					  " '" + userName + "'" + "," +
					  " '" + tableName + "'" + "," +
					  " '" + pcName + "'" + "," +
					  " '" + (int)progNo + "'" +
					  ")";
				oleCmd = new OracleCommand(sql, oleInfoPub);
				oleCmd.ExecuteNonQuery();
				oleCmd.Dispose();
				oleCmd = null;
			}
#if false
			{
				sql = "DELETE T_LOG_SHENLONG " +
					  "WHERE USER_NAME='" + logUserName + "' AND PC_NAME='" + pcName + "'";
				oleCmd = new OleDbCommand(sql, oleInfoPub);
				rows = oleCmd.ExecuteNonQuery();
				oleCmd.Dispose();
				oleCmd = null;
			}
#endif
		}
		catch ( Exception exp )
		{
			System.Diagnostics.Debug.WriteLine(exp.Message);
		}
		finally
		{
			if ( oleCmd != null )
			{
				oleCmd.Dispose();
				oleCmd = null;
			}

			if ( oleInfoPub != null )
			{
				oleInfoPub.Close();
				oleInfoPub.Dispose();
				oleInfoPub = null;
			}
		}
	}
}
