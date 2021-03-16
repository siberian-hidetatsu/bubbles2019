#define	TABLE_NAME_HAS_ALIAS		// テーブル名が別名を持つ事がある
#define	COLLECT_OUTER_JOIN			// 正しい外部結合のSQLを構築する
#define	ENABLED_SUBQUERY			// サブクエリのロジックを有効にする（実際にはプロジェクト プロパティの[ビルド][条件付きコンパイル定数]で設定する）
#define	NEW_GETPLAINTABLEFIELDNAME	// 新しいGetPlainTableFieldName関数を使う（実際にはプロジェクト プロパティの[ビルド][条件付きコンパイル定数]で設定する）
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
//using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Data.OleDb;

namespace Shenlong
{
#if !WITHIN_SHENGLOBAL
	public class ShenGlobal
	{
		public enum apps {
			unkown, form, console, web
		};

		public static apps app = apps.unkown;						// アプリケーションの種類（呼び出し側で設定する必要あり）
		public static string sqlDateFormat = "yyyymmdd hh24mi";		// SQL 日付の条件書式

		public const string tagShenlong = "shenlong";
		public const string attrSID = "sid";
		public const string attrUserName = "userName";
		public const string attrVer = "ver";
		public const string tagColumn = "column";
		public const string attrTableName = "tableName";
		public const string attrWidth = "width";
		public const string tagTableJoin = "tableJoin";
		public const string tagSQL = "sql";
		public const string tagBuildedSQL = "buildedSql";
		public const string tagProperty = "property";
		public const string tagComment = "comment";
		public const string tagAuthor = "author";
		public const string tagDistinct = "distinct";
		public const string tagUseJoin = "useJoin";
		public const string tagHeaderOutput = "headerOutput";
		public const string tagDownload = "download";
		public const string tagEggPermission = "eggPermission";
		public const string tagMaxRowNum = "maxRowNum";
		public const string tagSetValue = "setValue";
		public const string tagSqlSelect = "sqlSelect";
#if ENABLED_SUBQUERY
		public const string tagSubQuery = "subQuery";

		public const char SUBQUERY_SEPARATOR = ';';
		public const string SUBQUERY_RELATIVE_PATH = ".";
#endif

		public enum prop { type, length, nullable, comment, alias, dateFormat, bubbles, count };// カラムのプロパティ（兼属性|タグ名）

		public const string propNoComment = "n/c";					// NO COMMENT

		public enum bubbSet { control, input, setValue, dropDownList, hyperLink, classify };	// [bubbles] の設定値（兼属性|タグ名）
		public const Char sepBubbSet = '&';									// [bubbles] の設定値の区切り
		public enum bubbCtrl { textBox, label, noVisible, dropDownList };	// [bubbles] のコントロール設定（兼属性名）(dropDownList は bubbles のみ)
		public enum bubbInput { noAppoint, necessary };						// [bubbles] の入力条件設定（兼属性名）

		public enum qc {											// クエリー項目のアイテム（兼タグ名）
			fieldName, showField, expression, value1, value2, rColOp, orderBy, groupFunc, property };

		public enum tabJoin { leftTabCol, way, rightTabCol };		// [テーブル結合] のサブアイテム（兼タグ名）

		public enum header { columnName = 0x0001, comment = 0x0002 };	// ヘッダの出力フラグ

		public enum authority { permit, deny };						// 権限を許可するか否か（兼値）

		public const string withoutTableName = "::";

		public struct fromJoin								// テーブル結合用の構造体
		{
			public string join;
			public string way;
			public string tableName;
			public string subQuery;
			public List<string> equalColumn;

			public fromJoin(string join, string way, string tableName)
			{
				this.join = join;
				this.way = way;
				this.tableName = tableName;
				this.subQuery = null;
				this.equalColumn = new List<string>();
			}
		}

		public const string sepOutput = "\t";						// クエリー出力の区切り

		public enum pno { shenlong = 1, shencmd, bubbles };

		public static char[,] replaceForWebAppChars = null;

#if false
		public enum mout
		{
			all = 0xffff, show = 0x0001, strb = 0x0002
		}

		static private StringBuilder log = new StringBuilder();
		
		/// <summary>
		/// ログメッセージを初期化する
		/// </summary>
		static public void InitLogMessage()
		{
			log = new StringBuilder();
		}

		/// <summary>
		/// ログメッセージを表示＆保存する
		/// </summary>
		/// <param name="message"></param>
		/// <param name="_mout"></param>
		static public void LogMessage(string text, string caption, mout _mout)
		{
			try
			{
				if ( app == apps.unkown || app == apps.web )
					return;

				if ( ((uint)_mout & (uint)mout.show) != 0 )
				{
					if ( app == apps.form )
					{
						MessageBox.Show(text, caption ?? "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Stop);
					}
					else if ( app == apps.console )
					{
						Console.WriteLine(text);
					}
				}

				if ( ((uint)_mout & (uint)mout.strb) != 0 )
				{
					log.Append(text + "\r\n");
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}
		}

		static public void LogMessage(string text, mout _mout)
		{
			LogMessage(text, null, _mout);
		}

		/// <summary>
		/// ログをファイルに保存する
		/// </summary>
		/// <param name="fileName"></param>
		static public void SaveLogMessage(string fileName)
		{
			try
			{
				using ( StreamWriter swLogFile = new StreamWriter(fileName, false, Encoding.Default) )
				{
					swLogFile.Write(log.ToString());
					swLogFile.Close();
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}
		}
#endif

		/// <summary>
		/// 端末に対するタマゴ権限の設定がある？
		/// </summary>
		/// <param name="eggPermission"></param>
		/// <returns></returns>
		public static bool IsEggPermissionSet(XmlNode eggPermission)
		{
			if ( eggPermission == null )
				return false;

			if ( string.IsNullOrEmpty(eggPermission.InnerText) )
				return false;

			return true;
		}

		/// <summary>
		/// [bubbles] 設定を文字列に変換する
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
		/// [bubbles] 設定を文字列を XmlElement に変換する
		/// </summary>
		/// <param name="setting"></param>
		/// <param name="xmlShenlongColumn"></param>
		/// <returns></returns>
		public static XmlElement BubblesSettingToXml(string setting, XmlDocument xmlShenlongColumn)
		{
			string[] settings = setting.Split(sepBubbSet);

			XmlElement bubbles = xmlShenlongColumn.CreateElement(prop.bubbles.ToString());			// <bubbles>

			XmlAttribute attr = xmlShenlongColumn.CreateAttribute(bubbSet.control.ToString());		// @control
			attr.Value = settings[(int)bubbSet.control];
			bubbles.Attributes.Append(attr);

			attr = xmlShenlongColumn.CreateAttribute(bubbSet.input.ToString());						// @input
			attr.Value = settings[(int)bubbSet.input];
			bubbles.Attributes.Append(attr);

			attr = xmlShenlongColumn.CreateAttribute(bubbSet.setValue.ToString());					// @setValue
			attr.Value = settings[(int)bubbSet.setValue];
			bubbles.Attributes.Append(attr);

			XmlElement elem = xmlShenlongColumn.CreateElement(bubbSet.dropDownList.ToString());		//   <dropDownList>
			elem.InnerText = settings[(int)bubbSet.dropDownList];
			elem.IsEmpty = (elem.InnerText.Length == 0);
			bubbles.AppendChild(elem);

			elem = xmlShenlongColumn.CreateElement(bubbSet.hyperLink.ToString());					//   <hyperLink>
			elem.InnerText = settings[(int)bubbSet.hyperLink];
			elem.IsEmpty = (elem.InnerText.Length == 0);
			bubbles.AppendChild(elem);

			elem = xmlShenlongColumn.CreateElement(bubbSet.classify.ToString());					//   <classify>
			elem.InnerText = settings[(int)bubbSet.classify];
			elem.IsEmpty = (elem.InnerText.Length == 0);
			bubbles.AppendChild(elem);

			return bubbles;
		}

#if ENABLED_SUBQUERY
		/// <summary>
		/// クエリー項目(xml)から SQL を構築する
		/// </summary>
		/// <param name="xmlShenlongColumn"></param>
		/// <param name="selectParams"></param>
		/// <param name="checkHeaderOutput"></param>
		/// <param name="maxRowNum"></param>
		/// <param name="buildedSql"></param>
		/// <param name="columnComments"></param>
		/// <param name="fromTableNames"></param>
		/// <param name="indentCnt"></param>
		/// <returns></returns>
		public static bool BuildQueryColumnSQL(XmlDocument xmlShenlongColumn, Dictionary<string, string> selectParams, bool checkHeaderOutput, int maxRowNum, out string buildedSql, out string columnComments, ref List<string> fromTableNames, int indentCnt)
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

				//string[] _sqlDateFormat = { "yyyymmdd hh24mi", "yyyy/mm/dd hh24:mi" };
				List<string> _queryTableNames = new List<string>();		// 選択済みのテーブル名（現在の状態）
				bool _fileDistinct = false;
				bool _fileUseJoin = false;
				int _fileHeaderOutput = ((int)header.columnName | (int)header.comment);
				List<string> _fileSubQuery = new List<string>();

				XmlNode fileProperty = xmlShenlongColumn.DocumentElement[tagProperty];
				if ( fileProperty != null )
				{
					if ( fileProperty[tagDistinct] != null )
					{
						_fileDistinct = bool.Parse(fileProperty[tagDistinct].InnerText);
					}

					if ( fileProperty[tagUseJoin] != null )
					{
						_fileUseJoin = bool.Parse(fileProperty[tagUseJoin].InnerText);
					}

					if ( fileProperty[tagHeaderOutput] != null )
					{
						_fileHeaderOutput = int.Parse(fileProperty[tagHeaderOutput].InnerText);
					}

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
				int usersRndBktCount = 0;
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

					int fieldAsIndex;
					string fieldAliasName;
					string plainTableFieldName = GetPlainTableFieldName(tableFieldName, out fieldAsIndex, out fieldAliasName);

#if true
					if ( fieldAsIndex == -1 )	// 直接の別名指定は無い？
					{
						XmlNode alias = column[qc.property.ToString()][prop.alias.ToString()];
						property[(int)prop.alias] = (alias == null) ? string.Empty : "\"" + alias.InnerText + "\"";

						if ( property[(int)prop.alias].Length != 0 )	// プロパティでの別名指定がある？
						{
							fieldAliasName = property[(int)prop.alias];
							tableFieldName += " AS " + fieldAliasName;	// 直接指定の書式に変換しておく
							plainTableFieldName = GetPlainTableFieldName(tableFieldName, out fieldAsIndex);
						}
					}
#endif

#if true
					if ( checkHeaderOutput )
					{
						// 直接の別名指定が無く、コメントの出力フラグのみがオンで、カラムのコメントがある？
						if ( (fieldAsIndex == -1) && (_fileHeaderOutput == (int)header.comment) && (property[(int)prop.comment] != propNoComment) )
						{
							fieldAliasName = property[(int)prop.comment];
							tableFieldName += " AS " + "\"" + fieldAliasName + "\"";	// 直接指定の書式に変換しておく
							plainTableFieldName = GetPlainTableFieldName(tableFieldName, out fieldAsIndex);
						}
					}
#endif

					if ( bool.Parse(column[qc.showField.ToString()].InnerText) )
					{
						string groupFunc = column[qc.groupFunc.ToString()].InnerText;
						if ( !string.IsNullOrEmpty(groupFunc) )
						{
							//tableFieldName = groupFunc + "(" + tableFieldName + ")";
							tableFieldName = groupFunc + "(" + plainTableFieldName + ")" + ((fieldAsIndex != -1) ? tableFieldName.Substring(fieldAsIndex) : "");
							groupFuncCount++;
						}

#if true
						if ( (property[(int)prop.type] == "DATE") && !tableFieldName.StartsWith("to_char(", StringComparison.OrdinalIgnoreCase) )
						{
							//select.Append(indent + "to_char(" + tableFieldName + ",'YYYY/MM/DD HH24:MI:SS') " + fieldName + ",\r\n");
							select.Append(indent + "to_char(" + plainTableFieldName + ",'YYYY/MM/DD HH24:MI:SS') ");
							select.Append((fieldAsIndex != -1) ? tableFieldName.Substring(fieldAsIndex/* + 4*/).Trim() : fieldName);
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
					string usersRoundBlanket = GetUsersRoundBlanket(ref value2);

					if ( (expression.Length != 0)/**/ && !string.IsNullOrEmpty(value1)/**/ )
					{
						string bubbles = string.Empty;
						XmlNode bubblesNode = column[qc.property.ToString()][prop.bubbles.ToString()];
						if ( bubblesNode != null )
						{
							bubbles = BubblesSettingToString(bubblesNode);
						}

						string plainFieldName = GetPlainTableFieldName(fieldName);
						SetShenlongParam(selectParams, xmlShenlongColumn.BaseURI/*.Replace('-', '―')*/, bubbles, tableName + "." + plainFieldName/*plainTableFieldName*/, ref paramNames, expression, ref value1, ref value2);

						// 値１の指定が無くなった時、rColOp を null にして、余計なロジックを通らないようした。(2011/01/12)
						rColOp = (value1.Length == 0) ? null : rColOp;

						// ユーザー定義の開き括弧があれば設定する
						SetUsersRoundBlanket(usersRoundBlanket, indent, ref where, ref usersRndBktCount);
					}

					//string quotation = (property[(int)prop.type].StartsWith("VARCHAR")) ? "'" : "";
					string quotation = IsCharColumn(property[(int)prop.type]) ? "'" : "";

					if ( !string.IsNullOrEmpty(value1) && (property[(int)prop.type] == "DATE") )	// 日付の条件指定あり？
					{
						XmlNode dateFormat = column[qc.property.ToString()][prop.dateFormat.ToString()];
						property[(int)prop.dateFormat] = (dateFormat == null) ? string.Empty : dateFormat.InnerText;

						/*int dtfmt = value1.IndexOf('/') == -1 ? 0 : 1;
						string toChar = (value1[0] == '(') ? "to_char" : "";
						string dateQuote = (Char.IsDigit(value1[0])) ? "'" : "";
						value1 = "to_date(" + toChar + dateQuote + value1 + dateQuote + ",'" + _sqlDateFormat[dtfmt] + "')";*/
						value1 = ValueToDateFormat(value1, property[(int)prop.dateFormat]);
					}

					if ( rColOp != null )	// 有効な条件式？
					{
						if ( rColOp.Length == 0 )
						{
							rColOp = "AND";
						}

						// 連続した OR 条件の開き|閉じ括弧をセットする
						SetOrRoundBlanket(rColOp, expression, ref leftRndBkt, ref rightRndBkt, ref cameOR, ref where);
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
							/*int dtfmt = value2.IndexOf('/') == -1 ? 0 : 1;
							string toChar = (value2[0] == '(') ? "to_char" : "";
							string dateQuote = (Char.IsDigit(value2[0])) ? "'" : "";
							value2 = "to_date(" + toChar + dateQuote + value2 + dateQuote + ",'" + _sqlDateFormat[dtfmt] + "')";*/
							value2 = ValueToDateFormat(value2, property[(int)prop.dateFormat]);
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

					// ユーザー定義の閉じ括弧があれば設定する
					SetUsersRoundBlanket(usersRoundBlanket, null, ref where, ref usersRndBktCount);

					// OR の途中でユーザー定義の閉じ括弧が設定された？
					if ( cameOR && ((usersRoundBlanket != null) && (where[where.Length - (1 + 1 + rColOp.Length + 2)] == '」')) )
					{
						// OR 条件を閉じ括弧でターミネートする
						TerminateOrRoundBlanket(ref cameOR, where.Length - (1 + 1 + rColOp.Length + 2), ref where);
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
						//string orderTableFieldName = (property[(int)prop.alias].Length == 0) ? tableFieldName : property[(int)prop.alias];
						string orderTableFieldName = fieldAliasName ?? tableFieldName;
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

				if ( _fileDistinct )
				{
					select.Insert(6, " DISTINCT");	// 6:SELECT
				}

				List<ShenGlobal.fromJoin> fromJoins = null;

				if ( _fileUseJoin )
				{
					// JOIN でテーブルを結合する
					fromJoins = new List<fromJoin>();

					XmlNodeList tableJoins = xmlShenlongColumn.DocumentElement.SelectNodes(tagTableJoin);
					for ( int i = 0; i < tableJoins.Count; i++ )
					{
						XmlNode tableJoin = tableJoins[i];

						string leftTableName, leftColumnName, leftTableColumn;
						SplitTableFieldName(tableJoin.Attributes[tabJoin.leftTabCol.ToString()].Value, out leftTableName, out leftColumnName, null/*false*/);
						leftTableColumn = GetPlainTableFieldName(!leftColumnName.StartsWith(withoutTableName) ? (GetTableName(leftTableName, false)/*leftTableName*/ + "." + leftColumnName) : leftColumnName.Substring(withoutTableName.Length));

						string way = tableJoin.Attributes[tabJoin.way.ToString()].Value;

						string rightTableName, rightColumnName, rightTableColumn;
						SplitTableFieldName(tableJoin.Attributes[tabJoin.rightTabCol.ToString()].Value, out rightTableName, out rightColumnName, null/*false*/);
						rightTableColumn = GetPlainTableFieldName(!rightColumnName.StartsWith(withoutTableName) ? (GetTableName(rightTableName, false)/*rightTableName*/ + "." + rightColumnName) : rightColumnName.Substring(withoutTableName.Length));

						int j;
						// 新規の核となるテーブル？
						if ( (j = GetIndexOfJoinTableName(fromJoins, leftTableName, null)) == fromJoins.Count )
						{
							ShenGlobal.fromJoin fromJoin = new ShenGlobal.fromJoin("", "", leftTableName);
							fromJoins.Add(fromJoin);
						}

						// 新規の結合するテーブル？
						if ( (j = GetIndexOfJoinTableName(fromJoins, rightTableName, way)) == fromJoins.Count )
						{
							// 結合するテーブルを追加する
							string join = (way == "=" ? "INNER" : (way == "<=" ? "RIGHT OUTER" : (way == ">=" ? "LEFT OUTER" : (way == "><" ? "FULL OUTER" : "")))) + " JOIN ";
							string subQuery;
							Dictionary<string, string> _subQueryAlias = new Dictionary<string, string>();
							if ( (subQuery = IsTableNameSubQuery(rightTableName, _fileSubQuery, ref _subQueryAlias)) != null )
							{
								XmlDocument _xmlShenlongColumn = ReadSubQueryFile(subQuery, xmlShenlongColumn.BaseURI/*GetSubQueryBaseURI(subQuery, xmlShenlongColumnFileName ?? GetLatestBaseURI())*/);
								string _buildedSql, _columnComments;
								if ( !BuildQueryColumnSQL(_xmlShenlongColumn, selectParams, false, -1, out _buildedSql, out _columnComments, ref fromTableNames, indentCnt + 2) )
									return false;
								subQuery = "(" + _buildedSql + indent + ") " + GetSubQueryName(subQuery, _subQueryAlias);
							}
							fromJoin fromJoin = new fromJoin(join, way, rightTableName);
							fromJoin.subQuery = subQuery;
							fromJoins.Add(fromJoin);
						}

						// 結合するカラムを追加する
						fromJoins[j].equalColumn.Add(leftTableColumn + " = " + rightTableColumn);
					}

					// JOIN 句を使った SQL を構築する
					StringBuilder fromJoinSql = BuildFromJoinSql(fromJoins, indent, ref fromTableNames);

					if ( fromJoinSql.Length != 0 )	// JOIN するテーブルがある？
					{
						fromJoinSql.Insert(fromJoinSql.Length - 2, ",");	// 2:"\r\n"
						from.Append(fromJoinSql);
					}
				}

				// FROM テーブル名
				Dictionary<string, string> subQueryAlias = new Dictionary<string, string>();
				foreach ( string tableName in _queryTableNames )
				{
					if ( fromJoins != null )
					{
						int j;
						for ( j = 0; (j < fromJoins.Count) && (tableName != fromJoins[j].tableName); j++ ) ;
						if ( j != fromJoins.Count )						// JOIN されたテーブル名？
							continue;
					}

					/*if ( _fileSubQuery.Find(delegate(string s) { return s.IndexOf(tableName) != -1; }) != null )
						continue;*/
					/* サブクエリの別名対応 */
					if ( IsTableNameSubQuery(tableName, _fileSubQuery, ref subQueryAlias) != null )
						continue;
					from.Append(indent + tableName + ",\r\n");
					fromTableNames.Add(tableName);
				}
				// サブクエリ
				foreach ( string subQuery in _fileSubQuery )
				{
					if ( fromJoins != null )
					{
						string _subQuery = Path.GetFileNameWithoutExtension(subQuery);
						int j;
						for ( j = 0; (j < fromJoins.Count) && (_subQuery != ShenGlobal.GetTableName(fromJoins[j].tableName, true)); j++ ) ;
						if ( j != fromJoins.Count )						// JOIN されたサブクエリ？
							continue;
					}

					XmlDocument _xmlShenlongColumn = ReadSubQueryFile(subQuery, xmlShenlongColumn.BaseURI/*GetSubQueryBaseURI(subQuery, xmlShenlongColumn.BaseURI)*/);
					string _buildedSql, _columnComments;
					if ( !BuildQueryColumnSQL(_xmlShenlongColumn, selectParams, false, -1/*maxRowNum*/, out _buildedSql, out _columnComments, ref fromTableNames, indentCnt + 2) )
					{
						columnComments = _columnComments;
						return false;
					}
					/*from.Append(indent + "(" + _buildedSql + indent + ") " + Path.GetFileNameWithoutExtension(subQuery) + ",\r\n");*/
					/* サブクエリの別名対応 */
					from.Append(indent + "(" + _buildedSql + indent + ") ");
					from.Append(GetSubQueryName(subQuery, subQueryAlias));
					from.Append(",\r\n");
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
							if ( (where[where.Length - 4] == 'O') && (where[where.Length - 3] == 'R') )
							{
								where.Remove(where.Length - 4, 2);
								where.Insert(where.Length - 2, "AND");
							}
							where.Append(indent + "(ROWNUM <= " + maxRowNum + ") AND\r\n");
						}
					}
				}
				if ( defWhereLen < where.Length )
				{
					if ( !_fileUseJoin )
					{
						where.Insert(defWhereLen, indent + "(");
						where.Remove(defWhereLen + indent.Length + 1, indent.Length);
					}
					int lastSpace;
					for ( lastSpace = where.Length - 1; where[lastSpace] != ' '; lastSpace-- ) ;
					where.Remove(lastSpace + 1, where.Length - lastSpace - 1);		// "AND|OR\r\n" を削除する
					if ( cameOR )
					{
						// OR 条件を閉じ括弧でターミネートする
						ShenGlobal.TerminateOrRoundBlanket(ref cameOR, lastSpace++, ref where);
					}
					if ( !_fileUseJoin )
					{
						where.Insert(lastSpace, ")");
					}

					// エンコードされたユーザー定義の括弧をデコードする
					DecodeUsersRoundBlanket(usersRndBktCount, ref where);

					/*if ( groupFuncCount != 0 )
					{
						// HAVING
						groupBy.Append("\r\nHAVING\r\n" + where.ToString().Substring(defWhereLen));
						where = new StringBuilder("\r\nWHERE\r\n");
					}*/
				}

				if ( !_fileUseJoin )
				{
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
				}

				buildedSql = select.ToString(0, select.Length - (1 + 2)) + " " +	// (1 + 2):",\r\n"
							 from.ToString(0, from.Length - (1 + 2)) + " " +		// (1 + 2):",\r\n"
							 ((where.Length == defWhereLen) ? "" : where.ToString()) +
							 ((groupBy.Length == defGroupByLen) ? "" : groupBy.ToString()) +
							 ((orderBy.Length == defOrderByLen) ? "" : orderBy.ToString()) +
							 "\r\n";

				if ( colCommentsCount != 0 )
				{
					if ( !checkHeaderOutput || ((_fileHeaderOutput & (int)header.columnName) != 0) )
					{
						colComments.Length--;
						columnComments = colComments.ToString();
					}
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
		/// 結合するテーブル名を検索してインデックスを取得する
		/// </summary>
		/// <param name="fromJoins"></param>
		/// <param name="tableName"></param>
		/// <param name="way"></param>
		/// <returns></returns>
		public static int GetIndexOfJoinTableName(List<ShenGlobal.fromJoin> fromJoins, string tableName, string way)
		{
			int j;

			for ( j = 0; j < fromJoins.Count; j++ )
			{
				if ( fromJoins[j].tableName == tableName )
				{
					if ( (way == null) ||
						 ((way != null) && (fromJoins[j].way == way)) )
						break;
				}
			}

			return j;
		}

		/// <summary>
		/// JOIN 句を使った SQL を構築する
		/// </summary>
		/// <param name="fromJoins"></param>
		/// <param name="indent"></param>
		/// <param name="fromTableNames"></param>
		/// <returns></returns>
		public static StringBuilder BuildFromJoinSql(List<ShenGlobal.fromJoin> fromJoins, string indent, ref List<string> fromTableNames)
		{
			StringBuilder fromJoinSql = new StringBuilder();

			for ( int j = 0; j < fromJoins.Count; j++ )
			{
				fromJoin fromJoin = fromJoins[j];

				if ( 2 <= j )
				{
					/* GetTableNameInSQL での処理がややこしくなるので、とりあえず、囲まないようにしておく */
					// 一つ前の結合を括弧で囲む
					//fromJoinTable.Insert(indent.Length, '(');
					//fromJoinTable.Insert(fromJoinTable.Length - 2, ')');	// 2:"\r\n"
				}

				string crlf = "\r\n";

				// 字下げ
				fromJoinSql.Append(indent);

				// JOIN
				fromJoinSql.Append(fromJoin.join);
				if ( string.IsNullOrEmpty(fromJoin.join) )	// 核となるテーブル？
				{
					crlf = "";

					if ( 2 <= j )
					{
						fromJoinSql.Insert(fromJoinSql.Length - indent.Length - 2, ",");	// 2:"\r\n"
					}
				}

				// テーブル名（またはサブクエリ）
				if ( fromJoin.subQuery == null )
				{
					fromJoinSql.Append(fromJoin.tableName);
				}
				else
				{
					fromJoinSql.Append("\r\n" + indent + fromJoin.subQuery);
				}

				// 結合するカラム
				if ( fromJoin.equalColumn.Count != 0 )
				{
					fromJoinSql.Append(" ON ");
					foreach ( string equalColumn in fromJoin.equalColumn )
					{
						fromJoinSql.Append("(" + equalColumn + ")");
						fromJoinSql.Append(" AND ");
					}
					fromJoinSql.Length -= 5;	// 5:" AND "
				}

				fromJoinSql.Append(crlf);

				if ( fromJoin.subQuery == null )	// サブクエリではない？
				{
					fromTableNames.Add(fromJoin.tableName);
				}
			}

			return fromJoinSql;
		}

		/// <summary>
		/// 値を to_date 日付書式へ変換する
		/// </summary>
		/// <param name="value"></param>
		/// <param name="dateFormat"></param>
		/// <returns></returns>
		public static string ValueToDateFormat(string value, string dateFormat)
		{
			//if ( value.StartsWith("to_date(", StringComparison.CurrentCultureIgnoreCase) )
			//	return value;

			string toChar = (value[0] == '(') ? "to_char" : "";

			string dateQuote = (Char.IsDigit(value[0])) ? "'" : "";

			//string _sqlDateFormat = (string.IsNullOrEmpty(dateFormat)) ? ShenGlobal.sqlDateFormat : dateFormat;
			string _sqlDateFormat;
			if ( string.IsNullOrEmpty(dateFormat) )	// 日付書式の指定は無い？
			{
				_sqlDateFormat = (value.IndexOf('/') == -1) ? ShenGlobal.sqlDateFormat : "yyyy/mm/dd hh24:mi";
			}
			else
			{
				_sqlDateFormat = dateFormat;
			}

			return "to_date(" + toChar + dateQuote + value + dateQuote + ",'" + _sqlDateFormat + "')";
		}

		/// <summary>
		/// 連続した OR 条件の開き|閉じ括弧をセットする
		/// </summary>
		/// <param name="rColOp"></param>
		/// <param name="expression"></param>
		/// <param name="leftRndBkt"></param>
		/// <param name="rightRndBkt"></param>
		/// <param name="cameOR"></param>
		/// <param name="where"></param>
		public static void SetOrRoundBlanket(string rColOp, string expression, ref string leftRndBkt, ref string rightRndBkt, ref bool cameOR, ref StringBuilder where)
		{
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
						// OR の括弧が閉じられていないので、強制的に右括弧で閉じる 5:" OR\r\n"
						//where.Insert(where.Length - 5, ')');
						int endOfOR;
						for ( endOfOR = where.Length - 1; (0 <= endOfOR) && (where[endOfOR] != '\n'); endOfOR-- ) ;	// '\n' を探す
						where.Insert(endOfOR - 4, ')');
					}
				}
				cameOR = false;
			}
		}

		/// <summary>
		/// OR 条件を閉じ括弧でターミネートする
		/// </summary>
		/// <param name="cameOR"></param>
		/// <param name="index"></param>
		/// <param name="where"></param>
		public static void TerminateOrRoundBlanket(ref bool cameOR, int index, ref StringBuilder where)
		{
			for ( ; where[index - 1] == '」'; index-- ) ;
			where.Insert(index, ')');
			cameOR = false;
		}

		/// <summary>
		/// ユーザーで設定した括弧を取得する
		/// </summary>
		/// <param name="value2"></param>
		/// <returns></returns>
		public static string GetUsersRoundBlanket(ref string value2)
		{
			string usersRoundBlanket = null;

			if ( value2.Length != 0 )
			{
				int endOfRndBkt = value2.IndexOf('"', 1);
				if ( (value2[0] == '"') && (endOfRndBkt != -1) )	// ユーザーの括弧["("|")"]指定がある？
				{
					usersRoundBlanket = value2.Substring(1, endOfRndBkt - 1);
					value2 = value2.Substring(endOfRndBkt + 1);
				}
			}

			return usersRoundBlanket;
		}

		/// <summary>
		/// ユーザー定義の括弧があれば WHERE 句へセットする
		/// </summary>
		/// <param name="usersRoundBlanket"></param>
		/// <param name="indent"></param>
		/// <param name="where"></param>
		/// <param name="usersRndBktCount"></param>
		public static void SetUsersRoundBlanket(string usersRoundBlanket, string indent, ref StringBuilder where, ref int usersRndBktCount)
		{
			if ( usersRoundBlanket == null )
				return;

			if ( (indent != null) && (usersRoundBlanket[0] == '(') )		// 開き括弧？
			{
				where.Append(indent + new string('「', usersRoundBlanket.Length));
				usersRndBktCount += usersRoundBlanket.Length;
			}
			else if ( (indent == null) && (usersRoundBlanket[0] == ')') )	// 閉じ括弧？
			{
				for ( int i = 0; i < usersRoundBlanket.Length; i++ )
				{
					// ユーザーで指定した括弧内の条件項目は全てスキップされた？
					if ( where[where.Length - 1] == '「' )
					{
						// '「' を削除する
						where.Length--;
						usersRndBktCount--;

						// '「' のインデントを削除する
						int j;
						for ( j = where.Length - 1; (0 <= j) && (where[j] == ' '); j-- ) ;
						where.Remove(j + 1, where.Length - 1 - j);
					}
					else
					{
						// AND|OR の前のスペースを探して閉じ括弧をインサートする
						for ( int j = where.Length - 1; 0 <= j; j-- )
						{
							if ( where[j] == ' ' )
							{
								where.Insert(j, '」');
								break;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// エンコードされたユーザー定義の括弧をデコードする
		/// </summary>
		/// <param name="usersRndBktCount"></param>
		/// <param name="where"></param>
		public static void DecodeUsersRoundBlanket(int usersRndBktCount, ref StringBuilder where)
		{
			if ( usersRndBktCount == 0 )
				return;

			for ( int i = 0; i < where.Length; i++ )
			{
				if ( where[i] == '「' )
				{
					where[i] = '(';
					if ( where[i + 1] == ' ' )
					{
						// '「' から '(' までのスペースを削除する
						int j;
						for ( j = ++i; (j < where.Length) && (where[j] == ' '); j++ ) ;
						where.Remove(i, j - i);
						i--;
					}
				}
				else if ( where[i] == '」' )
				{
					where[i] = ')';
				}
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

#if true
			if ( shenColumnBaseURI.StartsWith("file:") )
			{
				shenColumnBaseURI = shenColumnBaseURI.Substring(5);	// 5:file:
				if ( shenColumnBaseURI.StartsWith("///") )
				{
					shenColumnBaseURI = shenColumnBaseURI.Substring(3);
				}
			}
			//shenColumnBaseURI = System.Web.HttpUtility.UrlDecode(shenColumnBaseURI);
#endif

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
		/// テーブル名がサブクエリか否かチェックする
		/// サブクエリであった場合、別名を格納する
		/// </summary>
		/// <param name="tableName"></param>
		/// <param name="fileSubQuery"></param>
		/// <param name="subQueryAlias"></param>
		/// <returns></returns>
		public static string IsTableNameSubQuery(string tableName, List<string> fileSubQuery, ref Dictionary<string, string> subQueryAlias)
		{
			string _tableName, _aliasName;
			_tableName = GetPlainTableFieldName(tableName, out _aliasName);

			string subQuery = fileSubQuery.Find(delegate(string fileName) { return Path.GetFileNameWithoutExtension(fileName) == _tableName; });

			if ( subQuery != null )
			{
				if ( !string.IsNullOrEmpty(_aliasName) )
				{
					if ( !subQueryAlias.ContainsKey(_tableName) )
					{
						subQueryAlias.Add(_tableName, _aliasName);
					}
				}
			}

			return subQuery;
		}

		/// <summary>
		/// サブクエリ名を取得する
		/// 別名があれば別名を返す
		/// </summary>
		/// <param name="subQuery"></param>
		/// <param name="subQueryAlias"></param>
		/// <returns></returns>
		public static string GetSubQueryName(string subQuery, Dictionary<string, string> subQueryAlias)
		{
			string _tableName, _alialName;
			_tableName = Path.GetFileNameWithoutExtension(subQuery);

			if ( subQueryAlias.TryGetValue(_tableName, out _alialName) )
				return _alialName;

			return _tableName;
		}
#endif

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
			if ( (selectParams == null)/* || (expression.Length == 0)*/ )
				return;

			if ( bubbles.Length != 0 )
			{
				string[] setting = bubbles.Split(sepBubbSet);
				if ( setting[(int)bubbSet.control] == bubbCtrl.noVisible.ToString() )
					return;
			}

			int sameParamNo = 0;
			//plainTableFieldName = plainTableFieldName.Replace('.', bb.pmShenlongTextIdJoin[0]);
			plainTableFieldName = ToWebAppName(plainTableFieldName);
			if ( !paramNames.TryGetValue(plainTableFieldName, out sameParamNo) )
			{
				paramNames[plainTableFieldName] = sameParamNo;
			}
			else
			{
				sameParamNo = ++paramNames[plainTableFieldName];
			}

			//string _baseURI = Path.GetFileNameWithoutExtension(baseURI);
			string _baseURI = ToWebAppName(Path.GetFileNameWithoutExtension(baseURI));
			string paramName = bb.pmShenlongTextID + _baseURI + bb.pmShenlongTextIdJoin + plainTableFieldName + bb.pmShenlongTextIdNo + sameParamNo;
			string _value;
			if ( !selectParams.TryGetValue(paramName, out _value) )
				return;

			value1 = _value;

#if false
			const string ex = "&%EX%";
			int exIndex = value1.IndexOf(ex);
			if ( exIndex != -1 )				// 式の差し替え指定あり？
			{
				expression = value1.Substring(exIndex + ex.Length);
				value1 = value1.Substring(0, exIndex);
			}
#endif

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

		/// <summary>
		/// カラムがCHAR型か否か
		/// </summary>
		/// <param name="colType"></param>
		/// <returns></returns>
		public static bool IsCharColumn(string colType)
		{
			// 値に関数が含まれている時は、@VARCHAR 等となっているので注意すること
			return (colType.StartsWith("VARCHAR") || colType.StartsWith("CHAR"));
		}

#if NEW_GETPLAINTABLEFIELDNAME
		/// <summary>
		/// 別名を除いたテーブル名.カラム名を抽出する
		/// </summary>
		/// <param name="tableFieldName"></param>
		/// <param name="fieldAsIndex"></param>
		/// <param name="aliasName"></param>
		/// <returns></returns>
		public static string GetPlainTableFieldName(string tableFieldName, out int fieldAsIndex, out string aliasName)
		{
			fieldAsIndex = -1;
			aliasName = null;

			try
			{
				int quotationStart = tableFieldName.Length;
				int spaceIndex;

				if ( tableFieldName.EndsWith("\"") )	// "別名" ?
				{
					quotationStart = tableFieldName.LastIndexOf("\"", tableFieldName.Length - 1 - 1);
					spaceIndex = tableFieldName.LastIndexOf(' ', quotationStart);
				}
				else
				{
					spaceIndex = tableFieldName.LastIndexOf(' ');
				}

				if ( spaceIndex != -1 )						// 空白で区切られている？
				{
					int count = quotationStart - spaceIndex;
					int rightRoundBracket = tableFieldName.IndexOf(')', spaceIndex, count);	// 例：(sysdate-1, 'yyyymmdd')
					int dot = tableFieldName.IndexOf('.', spaceIndex, count);				// 例：TNAME TT.COLUMN

					if ( (rightRoundBracket == -1) && (dot == -1) )	// 関数の中の空白でも別名付きのテーブルでもない？
					{
						const int aslen = 3;	// 3:" AS"
						aliasName = tableFieldName.Substring(spaceIndex + 1);
						for ( int i = spaceIndex - 1; 0 <= i && tableFieldName[i] == ' '; i--, spaceIndex-- ) ;	// 左の空白を探す
						if ( (aslen <= spaceIndex) && (string.Compare(tableFieldName, spaceIndex - aslen, " AS", 0, aslen, true) == 0) )
						{
							spaceIndex -= aslen;
							for ( int i = spaceIndex - 1; 0 <= i && tableFieldName[i] == ' '; i--, spaceIndex-- ) ;	// 左の空白を探す
						}
						fieldAsIndex = spaceIndex;
						tableFieldName = tableFieldName.Substring(0, fieldAsIndex);
					}
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}

			return tableFieldName;
		}

		public static string GetPlainTableFieldName(string tableFieldName, out int fieldAsIndex)
		{
			string aliasName;

			return GetPlainTableFieldName(tableFieldName, out fieldAsIndex, out aliasName);
		}

		public static string GetPlainTableFieldName(string tableFieldName, out string aliasName)
		{
			int fieldAsIndex;

			return GetPlainTableFieldName(tableFieldName, out fieldAsIndex, out aliasName);
		}

		public static string GetPlainTableFieldName(string tableFieldName)
		{
			int fieldAsIndex;
			string aliasName;

			return GetPlainTableFieldName(tableFieldName, out fieldAsIndex, out aliasName);
		}
#else
		/// <summary>
		/// 別名を除いたテーブル名.カラム名を抽出する
		/// </summary>
		/// <param name="tableFieldName"></param>
		/// <param name="fieldAsIndex"></param>
		/// <returns></returns>
		public static string GetPlainTableFieldName(/*ref */string tableFieldName, out int fieldAsIndex)
		{
			fieldAsIndex = -1;
			string plainTableFieldName = tableFieldName;

			try
			{
				if ( (fieldAsIndex = tableFieldName.IndexOf(" AS ", StringComparison.OrdinalIgnoreCase)) != -1 )
				{
					plainTableFieldName = tableFieldName.Substring(0, fieldAsIndex).TrimEnd();
					//tableFieldName = tableFieldName.Replace('(', '（').Replace(')', '）');
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
			}

			return plainTableFieldName;
		}

		public static string GetPlainTableFieldName(string tableFieldName)
		{
			int fieldAsIndex;
			return GetPlainTableFieldName(tableFieldName, out fieldAsIndex);
		}

		public static string GetPlainTableFieldName(string tableFieldName, out int fieldAsIndex, out string aliasName)
		{
			string plainTableFieldName = GetPlainTableFieldName(tableFieldName, out fieldAsIndex);
			aliasName = null;

			if ( fieldAsIndex != -1 )
			{
				int alias = tableFieldName.IndexOf("AS", fieldAsIndex, StringComparison.CurrentCultureIgnoreCase);
				aliasName = tableFieldName.Substring(alias + 2).Trim();
			}

			return plainTableFieldName;
		}
#endif
		/// <summary>
		/// 最大抽出行数の設定がある？
		/// </summary>
		/// <param name="xmlShenlongColumn"></param>
		/// <returns></returns>
		public static bool HasMaxRowNum(XmlDocument xmlShenlongColumn)
		{
			return ((xmlShenlongColumn.DocumentElement[tagProperty][tagMaxRowNum] != null) &&
					(xmlShenlongColumn.DocumentElement[tagProperty][tagMaxRowNum].InnerText.Length != 0));
		}

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
		public static bool SplitTableFieldName(string tableFieldName, out string tableName, out string fieldName, bool? plainTblName)
		{
#if false
			int dot = tableFieldName.IndexOf('.');
#else
			//int dot = tableFieldName.LastIndexOf('.');	// テーブル名に OWNER が付いている場合の対策 2010/03/24
			string _tableFieldName = GetPlainTableFieldName(tableFieldName);	// テーブルの別名に'.'が含まれている場合の対策 2010/03/29
			int dot = _tableFieldName.LastIndexOf('.');
			while ( dot != -1 )
			{
				if ( _tableFieldName.LastIndexOf('(', dot) == -1 )				// さらに、関数内の'.'でなければいい
					break;
				dot = _tableFieldName.LastIndexOf('.', dot - 1);
			}
#endif

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

		#region SQL 解析関数
		/// <summary>
		/// SQL 文の SELECT するカラムを抜き出す
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="plain"></param>
		/// <returns></returns>
		public static List<string> GetSelectColumnInSQL(string sql, bool plain)
		{
			List<string> columns = new List<string>();

			try
			{
				int from = GetIndexOfWord(sql, "FROM");
				if ( from == -1 )
					return null;

				sql = sql.Substring(6 + 1, from - (6 + 1));		// (6 + 1):"SELECT "

				int distinct = GetIndexOfWord(sql, "DISTINCT");
				if ( distinct != -1 )
				{
					sql = sql.Substring(distinct + 8);	// 8:"DISTINCT"
				}

				string column = string.Empty;

				for ( int i = 0; i < sql.Length; i++ )
				{
					if ( sql[i] == ',' )
					{
						AddSqlColumn(ref columns, ref column, plain);
						continue;
					}
					else if ( sql[i] == '\r' )
					{
						continue;
					}
					else if ( sql[i] == '\n' )
					{
						continue;
					}
					else if ( sql[i] == '(' )	// 関数？
					{
						int j = IndexOfRightRoundBracket(sql, i);

						column += sql.Substring(i, j - i + 1);

						i = j;
						continue;
					}

					column += sql[i];
				}

				AddSqlColumn(ref columns, ref column, plain);
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
				columns = null;
			}

			return columns ?? null;
		}

		private static void AddSqlColumn(ref List<string> columns, ref string column, bool plain)
		{
			string _column = column.Trim();

			if ( _column.Length != 0 )
			{
				if ( plain )
				{
					_column = GetPlainTableFieldName(_column);
				}

				columns.Add(_column);
			}

			column = string.Empty;
		}

		/// <summary>
		/// SQL 文の FROM 以降のテーブル名を抜き出す
		/// </summary>
		/// <param name="sql"></param>
		/// <param name="plain"></param>
		/// <param name="distinct"></param>
		/// <param name="untilFromIndex"></param>
		/// <returns></returns>
		public static List<string> GetTableNameInSQL(string sql, bool plain, bool distinct, out int untilFromIndex)
		{
			List<string> tableNames = new List<string>();
			untilFromIndex = -1;

			try
			{
				int from = GetIndexOfWord(sql, "FROM");
				if ( from == -1 )
					return null;
				Debug.WriteLine("from: " + from);

				int startTableName;
				//for ( startTableName = from + 4; !Char.IsLetter(sql[startTableName]); startTableName++ ) ;
				for ( startTableName = from + 4; Char.IsControl(sql[startTableName]) || Char.IsWhiteSpace(sql[startTableName]); startTableName++ ) ;

				string tableName = string.Empty;
				string[] endOfFrom = { "WHERE", "GROUP BY", "ORDER BY" };

				if ( sql.IndexOf(" JOIN", startTableName) == -1 )
				{
					for ( int i = startTableName; i < sql.Length; i++ )
					{
						Debug.Write(sql[i]);
						if ( sql[i] == ',' )
						{
							AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
							continue;
						}
						else if ( sql[i] == '\r' )
						{
							continue;
						}
						else if ( sql[i] == '\n' || sql[i] == ' ' )
						{
							char _c = sql[i];

							for ( ; i + 1 < sql.Length && sql[i + 1] == ' '; i++ ) ;	// 空白をスキップする

							if ( (i + 1 < sql.Length) && (CompareAnyString(sql, i + 1, endOfFrom) != -1) )
							{
								AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
								untilFromIndex = i;
								break;
							}

							tableName += (Char.IsControl(_c) ? "" : _c.ToString());
							continue;
						}
						else if ( sql[i] == '(' )	// サブクエリ？
						{
							int j = IndexOfRightRoundBracket(sql, i);
							j++;

							for ( ; j < sql.Length && sql[j] == ' '; j++ ) ;				// 空白をスキップする

							for ( ; j < sql.Length && Char.IsLetterOrDigit(sql[j]); j++ ) ;	// サブクエリの別名をスキップする

							string subQuery = sql.Substring(i, j - i);

							if ( plain )
							{
								List<string> _subTableNames = GetTableNameInSQL(subQuery, true, distinct);
								for ( int k = 0; k < _subTableNames.Count; k++ )
								{
									string _subTableName = _subTableNames[k];
									AddSqlTableName(ref tableNames, ref _subTableName, true, distinct);
								}
							}
							else
							{
								AddSqlTableName(ref tableNames, ref subQuery, plain, distinct);
							}

							i = j;
							continue;
						}

						tableName += sql[i];
					}

					AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
				}
				else
				{
					string[] beginOfJoin = { "INNER JOIN", "LEFT OUTER JOIN", "LEFT JOIN", "RIGHT OUTER JOIN", "RIGHT JOIN", "FULL OUTER JOIN" };

					for ( int i = startTableName; i < sql.Length; i++ )
					{
						Debug.Write(sql[i]);
						if ( sql[i] == ',' )
						{
							AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
							continue;
						}
						else if ( sql[i] == '\r' )
						{
							continue;
						}
						else if ( (sql[i] == '\n') || (sql[i] == ' ') )
						{
							char _c = sql[i];

							for ( ; i + 1 < sql.Length && sql[i + 1] == ' '; i++ ) ;	// 空白をスキップする

							int j;
							// [LEFT|RIGHT|FULL] [INNER|OUTER] JOIN ?
							if ( (i + 1 < sql.Length) && (j = CompareAnyString(sql, i + 1, beginOfJoin)) != -1 )
							{
								Debug.Write(sql.Substring(i, beginOfJoin[j].Length + 1));
								AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
								i += beginOfJoin[j].Length;
								continue;
							}

							SkipJoinColumn(ref i, sql, ref tableNames, ref tableName, plain, distinct);

							tableName += (Char.IsControl(_c) ? "" : _c.ToString());
							continue;
						}
						else if ( sql[i] == '(' )	// サブクエリ？
						{
							int j = IndexOfRightRoundBracket(sql, i);
							j++;

							for ( ; j < sql.Length && sql[j] == ' '; j++ ) ;				// 空白をスキップする

							for ( ; j < sql.Length && (Char.IsLetterOrDigit(sql[j]) || (sql[j] == '_')); j++ ) ;	// サブクエリの別名をスキップする

							string subQuery = sql.Substring(i, j - i);
							Debug.Write(subQuery.Substring(1));

							if ( plain )
							{
								List<string> _subTableNames = GetTableNameInSQL(subQuery, true, distinct);
								for ( int k = 0; k < _subTableNames.Count; k++ )
								{
									string _subTableName = _subTableNames[k];
									AddSqlTableName(ref tableNames, ref _subTableName, true, distinct);
								}
							}
							else
							{
								AddSqlTableName(ref tableNames, ref subQuery, plain, distinct);
							}

							i = j;

							SkipJoinColumn(ref i, sql, ref tableNames, ref tableName, plain, distinct);
							continue;
						}
						else if ( Char.IsLetter(sql[i]) )
						{
							if ( CompareAnyString(sql, i, endOfFrom) != -1 )
							{
								AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
								untilFromIndex = i - 1;
								break;
							}
						}

						tableName += sql[i];
					}

					AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
				}
			}
			catch ( Exception exp )
			{
				Debug.WriteLine(exp.Message);
				tableNames = null;
			}

#if (DEBUG)
			Debug.WriteLine("[" + MethodBase.GetCurrentMethod().Name + "]");
			Debug.WriteLine(sql);
			Debug.WriteLine("plain:" + plain);
			Debug.WriteLine("distinct:" + distinct);
			foreach ( string tableName in tableNames )
			{
				Debug.WriteLine(tableName);
			}
			Debug.WriteLine("untilFromIndex: " + untilFromIndex);
			Debug.WriteLine("");
#endif

			return tableNames ?? null;
		}

		public static List<string> GetTableNameInSQL(string sql, bool plain, bool distinct)
		{
			int untilFromIndex;

			return GetTableNameInSQL(sql, plain, distinct, out untilFromIndex);
		}

		/// <summary>
		/// ON (～) AND (～) をスキップする 
		/// </summary>
		/// <param name="i"></param>
		/// <param name="sql"></param>
		/// <param name="tableNames"></param>
		/// <param name="tableName"></param>
		/// <param name="plain"></param>
		/// <param name="distinct"></param>
		private static void SkipJoinColumn(ref int i, string sql, ref List<string> tableNames, ref string tableName, bool plain, bool distinct)
		{
			if ( sql.Length <= i + 1 )
				return;

			string[] onAnd = { "ON ", "AND " };
			int j;

			while ( (j = CompareAnyString(sql, i + 1, onAnd)) != -1 )
			{
				Debug.Write(sql.Substring(i, onAnd[j].Length + 1));
				AddSqlTableName(ref tableNames, ref tableName, plain, distinct);
				i += onAnd[j].Length;

				for ( ; i < sql.Length && sql[i] == ' '; i++ ) ;	// 空白をスキップする

				if ( sql[i] == '(' )
				{
					j = IndexOfRightRoundBracket(sql, i);
					Debug.Write(sql.Substring(i, j - i + 1));
					i = ++j;
				}
			}
		}

		/// <summary>
		/// SQL 内のテーブル名をリストに追加する
		/// </summary>
		/// <param name="tableNames"></param>
		/// <param name="tableName"></param>
		/// <param name="plain"></param>
		/// <param name="distinct"></param>
		private static void AddSqlTableName(ref List<string> tableNames, ref string tableName, bool plain, bool distinct)
		{
			string _tableName = tableName.Trim();
			tableName = string.Empty;

			if ( _tableName.Length == 0 )
				return;

			if ( plain )
			{
				_tableName = GetTableName(_tableName, true);
			}

			if ( distinct )
			{
				if ( tableNames.IndexOf(_tableName) != -1 )
				{
					return;
				}
			}

			tableNames.Add(_tableName);
		}
		#endregion

		/// <summary>
		/// 文字列から単語を検索してインデックスを取得する
		/// </summary>
		/// <param name="str"></param>
		/// <param name="word"></param>
		/// <returns></returns>
		public static int GetIndexOfWord(string str, string word)
		{
			int index = str.IndexOf(word, StringComparison.CurrentCultureIgnoreCase);

			while ( index != -1 )
			{
				int j = index - 1;
				if ( (index == 0) || (str[j] == ' ' || str[j] == '\n') )	// 文字列の最初か、単語の左隣が ' ' か '\n' ？
				{
					j = index + word.Length;
					if ( (j == str.Length) || (str[j] == ' ' || str[j] == '\r') )	// 文字列の最後か、単語の左隣が ' ' か '\r' ？
					{
						break;
					}
				}

				index = str.IndexOf(word, index + 1, StringComparison.CurrentCultureIgnoreCase);
			}

			return index;
		}

		/// <summary>
		/// 文字列と複数の文字列を比較する
		/// </summary>
		/// <param name="strA"></param>
		/// <param name="indexA"></param>
		/// <param name="strB"></param>
		/// <returns></returns>
		public static int CompareAnyString(string strA, int indexA, string[] strB)
		{
			int result = -1;

			for ( int i = 0; i < strB.Length; i++ )
			{
				if ( string.Compare(strA, indexA, strB[i], 0, strB[i].Length, true) == 0 )
				{
					result = i;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// 文字列中の '(' に対する ')' のインデックスを取得する
		/// </summary>
		/// <param name="str"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static int IndexOfRightRoundBracket(string str, int index)
		{
			int leftRoundBracket = 1;
			int j;

			for ( j = index + 1; j < str.Length; j++ )
			{
				if ( str[j] == ')' )
				{
					if ( --leftRoundBracket == 0 )
						break;
				}
				else if ( str[j] == '(' )
				{
					leftRoundBracket++;
				}
			}

			return j;
		}

#if false
		/// <summary>
		/// アクセス ログをテーブルに保存する
		/// </summary>
		/// <param name="logServiceName"></param>
		/// <param name="logUserName"></param>
		/// <param name="logTableNames"></param>
		public static void WriteAccessLog(string logServiceName, string logUserName, List<string> logTableNames)
		{
			OleDbConnection oraInfoPub = null;
			OleDbCommand oraCmd = null;

			try
			{
				if ( logTableNames == null )
					return;

				string infoPubSID = "sid", infoPubUser = "uid", infoPubPwd = "pwd";

				/*try
				{
					string xmlLogOnFileName = Application.StartupPath + LogOnDlg.LOGON_FILE_NAME;
					XmlDocument xmlLogOn = new XmlDocument();
					xmlLogOn.Load(xmlLogOnFileName);
					string xpath = "/" + LogOnDlg.tagRoot + "/" + LogOnDlg.tagLogOn + "[@" + LogOnDlg.attrSID + "='" + infoPubSID + "']" + "[" + LogOnDlg.tagUserName + "='" + infoPubUser + "']";
					XmlNode logOnNode = xmlLogOn.SelectSingleNode(xpath);
					if ( logOnNode != null )
					{
						// LogOn.xml に登録されているパスワードを優先する
						infoPubSID = logOnNode.Attributes[LogOnDlg.attrSID].Value;
						infoPubUser = logOnNode[LogOnDlg.tagUserName].InnerText;
						infoPubPwd = common.DecodePassword(logOnNode[LogOnDlg.tagPassword].InnerText);
					}
				}
				catch ( Exception exp )
				{
					Debug.WriteLine("[" + MethodBase.GetCurrentMethod().Name + "] " + exp.Message);
				}*/

				string conStr = "Provider=MSDAORA;Data Source=" + infoPubSID + ";" +
								"user id=" + infoPubUser + ";password=" + infoPubPwd + ";" +
								"persist security info=false;";
				oraInfoPub = new OleDbConnection(conStr);
				oraInfoPub.Open();							// 情報公開サーバに接続する

				string now = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");	// ACCESS_DATE

				string pcName;
				try
				{
					pcName = System.Net.Dns.GetHostName().ToLower();// PC_NAME
				}
				catch ( Exception exp )
				{
					pcName = exp.Message;
				}

				foreach ( string tableName in logTableNames )
				{
					string sql = "INSERT INTO T_LOG_SHENLONG (ACCESS_DATE,SERVICE_NAME,USER_NAME,TABLE_NAME,PC_NAME) " +
								 "VALUES(" + "TO_DATE('" + now + "','yyyy/mm/dd hh24:mi:ss')" + ",'" + logServiceName + "','" + logUserName + "','" + tableName + "','" + pcName + "')";
					oraCmd = new OleDbCommand(sql, oraInfoPub);
					oraCmd.ExecuteNonQuery();
					oraCmd.Dispose();
					oraCmd = null;
				}
#if (DEBUG)
#if false
				{
					string sql = "DELETE T_LOG_SHENLONG " +
								 "WHERE USER_NAME='" + logUserName + "' AND PC_NAME='" + pcName + "'";
					oraCmd = new OracleCommand(sql, oraInfoPub);
					int rows = oraCmd.ExecuteNonQuery();
					oraCmd.Dispose();
					oraCmd = null;
				}
#endif
#endif
			}
			catch ( Exception exp )
			{
				Debug.WriteLine("[" + MethodBase.GetCurrentMethod().Name + "] " + exp.Message);
			}
			finally
			{
				if ( oraCmd != null )
				{
					oraCmd.Dispose();
					oraCmd = null;
				}

				if ( oraInfoPub != null )
				{
					oraInfoPub.Close();
					oraInfoPub.Dispose();
					oraInfoPub = null;
				}
			}
		}
#endif

		/// <summary>
		/// Web アプリケーションに適したキャラクタに変換する
		/// JavaScript でのエラー対策など
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static string ToWebAppName(string name)
		{
			/*name = name.Replace('-', '―');
			name = name.Replace('.', '．');
			name = name.Replace(' ', '□');*/
			if ( replaceForWebAppChars != null )
			{
				for ( int i = 0; i < replaceForWebAppChars.GetLength(0); i++ )
				{
					name = name.Replace(replaceForWebAppChars[i, 0], replaceForWebAppChars[i, 1]);
				}
			}
			return name;
		}
	}
#endif
}
