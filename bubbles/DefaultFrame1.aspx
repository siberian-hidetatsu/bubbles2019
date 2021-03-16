<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DefaultFrame1.aspx.cs" Inherits="DefaultFrame1" %>

<!--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">-->
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>bubbles</title>
    <link href="bubbstyle.css" rel="stylesheet" type="text/css" />
</head>
<body class ="normal">
    <form id="form1" runat="server">
    <div>
    
        <asp:Label ID="LabelSubTitle" runat="server" Text="[SubTitle]<br/>" Font-Bold="True" 
            ForeColor="Gray"></asp:Label>
        <asp:DropDownList ID="DropDownSubDirectory" runat="server" AutoPostBack="True"></asp:DropDownList>
        <asp:Panel ID="PanelCategory" runat="server">
        </asp:Panel>
    
    </div>
    </form>
</body>
</html>
