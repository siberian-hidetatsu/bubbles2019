<%@ Page Language="C#" AutoEventWireup="true" CodeFile="MailToDeveloper.aspx.cs" Inherits="MailToDeveloper" %>

<!--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">-->
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
  <title>メール送信フォーム</title>
  <LINK href="bubbstyle.css" type="text/css" rel="stylesheet">
  <link rel="shortcut icon" href="./images/flower.ico" />
</head>
<body class="normal">
  <form id="form" runat="server">
    <asp:Label ID="Label4" runat="server" Font-Bold="True" ForeColor="Navy" Text="開発担当者へメールで問い合わせる"></asp:Label><br />
    <br />
    <asp:Label ID="Label1" runat="server" Text="送信者："></asp:Label><br />
    <asp:TextBox ID="TextFrom" runat="server" Width="180px"></asp:TextBox><br />
    <br />
    <asp:Label ID="Label2" runat="server" Text="件名："></asp:Label><br />
    <asp:TextBox ID="TextSubject" runat="server" Width="420px"></asp:TextBox><br />
    <br />
    <asp:Label ID="Label3" runat="server" Text="内容："></asp:Label><br />
    <asp:TextBox ID="TextBody" runat="server" Height="160px" TextMode="MultiLine" Width="420px"></asp:TextBox><br />
    <br />
    <asp:Button ID="ButtonSend" runat="server" Text="送信" OnClick="ButtonSend_Click" Height="35px" Width="80px" /><br />
    <br />
    <asp:Label ID="LabelDebugOutput" runat="server" ForeColor="FloralWhite" Text="ForDebugOutput"></asp:Label>
  </form>
</body>
</html>
