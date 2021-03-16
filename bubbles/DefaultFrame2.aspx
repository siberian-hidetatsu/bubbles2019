<%@ Page Language="C#" AutoEventWireup="true" CodeFile="DefaultFrame2.aspx.cs" Inherits="DefaultFrame2" %>

<!--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">-->
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>bubbles</title>
    <link href="bubbstyle.css" type="text/css" rel="stylesheet" />
    <script type="text/javascript" language="JavaScript">
      function WaitCursor() {
        document.body.style.cursor = "wait";
        //alert("ビッケ");
      }
      function InitPanelOption() {
        var panelOption = document.getElementById('PanelOption');
        panelOption.style.display = 'none';
        //var toggleOption = document.getElementById('ToggleOption');
        //toggleOption.title = toggleOption.title.replace("[br]", "\n");
      }
      function TogglePanelOption() {
        var panelOption = document.getElementById('PanelOption');
        if (panelOption.style.display == 'none') {
          panelOption.style.display = 'block';
          document.getElementById('ToggleOption').innerText = '-オプション';
          panelOption.focus();
        }
        else {
          panelOption.style.display = 'none';
          document.getElementById('ToggleOption').innerText = '+オプション';
        }
      }
    </script>
</head>
<body class="normal">
    <form id="form1" runat="server">
    <div>
        <asp:Image ID="ImageTitle" runat="server" ImageUrl="~/images/title.png" />
        <br />
        <asp:Label ID="LabelCategory" runat="server" Text="Category" Font-Bold="False"></asp:Label>
        <asp:Table ID="TableShenlongFiles" runat="server" Height="16px">
        </asp:Table>
    </div>
    <br/>
    
    <br/>
    <div align="right">
        <asp:Label ID="LabelCounter" runat="server" Font-Size="Smaller">00000</asp:Label>
    </div>
    <asp:HyperLink ID="ToggleOption" runat="server" NavigateUrl="javascript:;">+オプション</asp:HyperLink>
    <asp:Panel ID="PanelOption" runat="server" BorderColor="#777777" 
        BorderStyle="Solid" BorderWidth="1px" Height="46px" Width="268px" 
        BackColor="#E8F5FF">
        <asp:Label ID="LabelDefaultOutput" runat="server" Text="デフォルトの出力形式:"></asp:Label>
        <asp:RadioButton ID="RadioOutputHTML" runat="server" GroupName="DefaultOutput" 
            Text="HTML" />
        <asp:RadioButton ID="RadioOutputExcel" runat="server" GroupName="DefaultOutput" 
            Text="Excel" />
        <br />
        <asp:CheckBox ID="CheckEnableCache" runat="server" Text="キャッシュを有効にする" 
            Checked="True" />
        <asp:Label ID="LabelExcelGetBorder" runat="server" Text="<br/>ExcelGetBorder:"></asp:Label>
        <asp:TextBox ID="TextExcelGetBorder" runat="server" Width="60px"></asp:TextBox>
        <asp:Label ID="LabelExcelFrameVisible" runat="server" Text="<br/>ExcelFrameVisible:"></asp:Label>
        <asp:TextBox ID="TextExcelFrameVisible" runat="server" Width="80px"></asp:TextBox>
        <div align="center"><asp:Button ID="ButtonApply" runat="server" Text="適用" 
            ToolTip="オプションの設定を有効にする" /></div>
    </asp:Panel>
    <p/>
    <asp:Label ID="LabelDebug" runat="server" ForeColor="FloralWhite" Text="Debug"></asp:Label>
    <!--<script language='javascript'>
      InitPanelOption();
    </script>-->
    </form>
</body>
</html>
