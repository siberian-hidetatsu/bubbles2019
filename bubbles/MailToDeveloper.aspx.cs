using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Net.Mail;
using System.Threading;

public partial class MailToDeveloper : System.Web.UI.Page
{
	static string homePage = null;

	protected void Page_Load(object sender, EventArgs e)
	{
		try
		{
			if ( !Page.IsPostBack )
			{
				string javaScript =
					"<script type='text/javascript'>\r\n" +
					"var message = 'こちらにデータを入力して下さい。';\r\n" +	//クリックする前に表示する内容
					"flag = 0;\r\n" +											//削除チェックフラグ
					"function BeforeData(){\r\n" +
					"  document.form.TextBody.value = message;\r\n" +			//クリック前に表示する内容
					"}\r\n" +
					"function ClearData(){\r\n" +
					"  if(flag == 0){\r\n" +									//削除チェックフラグしていない場合
					"    document.form.TextBody.value = '';\r\n" +				//内容を空にする
					"    flag = 1;\r\n" +										//削除チェックON
					"  }\r\n" +
					"}\r\n" +
					//"if(!document.layers){window.onload = BeforeData;}" +
					"</script>";
				ClientScript.RegisterStartupScript(typeof(string), "startupJavaScript", javaScript);

				string jsOnSubmit =
					"document.body.style.cursor = 'wait';\r\n" +
					"window.form.ButtonSend.disabled = true;\r\n" +
					"return true;\r\n";
				form.Attributes.Add("onsubmit", jsOnSubmit);

				string developerMailAddress = ConfigurationManager.AppSettings["developerMailAddress"];
				if ( !string.IsNullOrEmpty(developerMailAddress) )
				{
					int domain = developerMailAddress.IndexOf("@");
					TextFrom.Text = (domain != -1) ? developerMailAddress.Substring(domain) : "";
				}

				TextSubject.Text = "[" + (Request.Params["subject"] ?? "問い合わせ") + "]";
				TextBody.Attributes["OnClick"] = "ClearData()";
				TextBody.Text = "ここに問い合わせ内容、名前、内線番号等を記入して\r\n" +
								"[送信]ボタンを押して下さい。\r\n" +
								"またメールアドレスがあれば[送信者]に記入して下さい。";

				homePage = Request.Params["HTTP_REFERER"];
				LabelDebugOutput.Text = homePage;
			}
			else
			{
				// form.Attributes.Add("onsubmit", jsOnSubmit); の時
				SendMail();
			}
		}
		catch ( ThreadAbortException )
		{
		}
		catch ( Exception exp )
		{
			Response.Write(exp.Message.ToString());
		}
	}

	/// <summary>
	/// [送信] ボタンが押された
	/// form.Attributes.Add("onsubmit", jsOnSubmit); の時はここは呼ばれない
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	protected void ButtonSend_Click(object sender, EventArgs e)
	{
		SendMail();
	}

	/// <summary>
	/// メールを送信する
	/// </summary>
	private void SendMail()
	{
		try
		{
			string smtpServerAddress = ConfigurationManager.AppSettings["smtpServerAddress"];
			string developerMailAddress = ConfigurationManager.AppSettings["developerMailAddress"];

			string senderAddress = (string.IsNullOrEmpty(TextFrom.Text) || TextFrom.Text[0] == '@') ? developerMailAddress : TextFrom.Text;

			SmtpClient smtpClient = new SmtpClient(smtpServerAddress);
			MailAddress from = new MailAddress(senderAddress);
			MailAddress to = new MailAddress(developerMailAddress);
			MailMessage message = new MailMessage(from, to);
			message.Subject = TextSubject.Text;
			message.Body = TextBody.Text;
			message.IsBodyHtml = false;
			smtpClient.Send(message);
			message.Dispose();

			string reply;

			if ( homePage != null )
			{
				reply = "<html>" +
						"<head>" +
						"  <title>メール送信</title>" +
						"  <link href=\"bubbstyle.css\" type=\"text/css\" rel=\"stylesheet\" />" +
						"  <link rel=\"shortcut icon\" href=\"./images/flower.ico\" />" +
						"  <meta HTTP-EQUIV=\"Refresh\" CONTENT=\"3;URL=" + homePage + "\">" +
						"</head>" +
						"<body class=\"normal\">" +
						"ありがとうございます。<br>" +
						"このページは数秒後にホームに戻ります。<br>" +
						"もし戻らない場合は、「戻る」ボタンで戻して下さい。<br>" +
						"<span style='color:FloralWhite;'>" + homePage + "</span>" +
						"</body>" +
						"</html>";
			}
			else
			{
				reply = "<html>" +
						"<head>" +
						"  <title>メール送信</title>" +
						"  <link href=\"bubbstyle.css\" type=\"text/css\" rel=\"stylesheet\" />" +
						"  <link rel=\"shortcut icon\" href=\"./images/flower.ico\" />" +
						"</head>" +
						"<body class=\"normal\" onLoad=\"setTimeout('window.close()',3000)\">" +
						"ありがとうございます。<br>" +
						"このページは３秒後に自動的に閉じられます。" +
						"</body>" +
						"</html>";
			}

			Response.Write(reply);
			Response.End();
		}
		catch ( ThreadAbortException )
		{
		}
		catch ( Exception exp )
		{
			Response.Write(exp.Message.ToString());
			Response.End();
		}
	}
}
