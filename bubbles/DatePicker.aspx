<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DatePicker.aspx.cs" Inherits="DatePicker" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>DatePicker</title>
    <link href="bubbstyle.css" type="text/css" rel="stylesheet" />
	<meta name="GENERATOR" content="Microsoft Visual Studio .NET 7.1">
	<meta name="CODE_LANGUAGE" content="C#">
	<meta name="vs_defaultClientScript" content="JavaScript">
	<meta name="vs_targetSchema" content="http://schemas.microsoft.com/intellisense/ie5">
	<style type="text/css">
	BODY { PADDING-RIGHT: 0px; PADDING-LEFT: 0px; PADDING-BOTTOM: 0px; MARGIN: 4px; PADDING-TOP: 0px }
	BODY { FONT-SIZE: 9pt; FONT-FAMILY: Verdana, Geneva, Sans-Serif }
	TABLE { FONT-SIZE: 9pt; FONT-FAMILY: Verdana, Geneva, Sans-Serif }
	TR { FONT-SIZE: 9pt; FONT-FAMILY: Verdana, Geneva, Sans-Serif }
	TD { FONT-SIZE: 9pt; FONT-FAMILY: Verdana, Geneva, Sans-Serif }
	</style>
</head>
	<body onblur="this.window.focus();">
		<form id="form1" method="post" runat="server">
			<div align="center">
				<asp:calendar id="Calendar" runat="server" bordercolor="#FFCC66" 
                    BackColor="#FFFFCC" DayNameFormat="Shortest" Font-Names="Verdana" 
                    Font-Size="8pt" ForeColor="#663399" Height="200px" Width="220px" 
                    BorderWidth="1px" ShowGridLines="True">
					<todaydaystyle forecolor="White" backcolor="#FFCC66"></todaydaystyle>
					<selectorstyle backcolor="#FFCC66"></selectorstyle>
					<nextprevstyle Font-Size="9pt" ForeColor="#FFFFCC"></nextprevstyle>
					<dayheaderstyle backcolor="#FFCC66" Font-Bold="True" Height="1px"></dayheaderstyle>
					<selecteddaystyle font-bold="True" backcolor="#CCCCFF"></selecteddaystyle>
					<titlestyle font-bold="True" backcolor="#990000" Font-Size="9pt" 
                        ForeColor="#FFFFCC"></titlestyle>
					<WeekendDayStyle ForeColor="Green" />
					<othermonthdaystyle forecolor="#CC9966"></othermonthdaystyle>
				</asp:calendar>
			</div>
		</form>
	</body>
</html>
