<%@ Page Language="C#" AutoEventWireup="true" CodeFile="shenlong2.aspx.cs" Inherits="shenlong2" %>
<!-- 上の行に ResponseEncoding="Shift_JIS" を追加すると、GridView で２バイト文字のカラムをクリックすると例外が発生する。とりあえず、Excel の出力は別ファイルになったので問題ない。 -->

<!--<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">-->
<!DOCTYPE HTML PUBLIC "-//W3C//DTD HTML 4.0 Transitional//EN" >

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
  <title>shenlong</title>
  <link href="bubbstyle.css" type="text/css" rel="stylesheet" />
  <link rel="shortcut icon" href="./images/shenlong.ico" />
  <style type="text/css">
  </style>
</head>
<body class="normal">
    <form id="form1" runat="server">
    <div>
        <asp:Panel ID="PanelHeader" runat="server">
            <asp:Image ID="ImageTitle" runat="server" ImageUrl="~/images/title.png" />
            <br />
            <asp:HyperLink ID="HyperLinkHome0" runat="server" ImageUrl="~/images/home.gif" 
                NavigateUrl="./Default.aspx" ToolTip="ＴＯＰページに戻る" Visible="False">HyperLink</asp:HyperLink>
        <asp:Label ID="LabelComment" runat="server" Text="コメント " Font-Bold="True" 
                ForeColor="DarkBlue"></asp:Label>
        <asp:Label ID="LabelFullPath" runat="server" Text="FullPath  " Visible="False"></asp:Label>
            <asp:Label ID="LabelPWD" runat="server" Text="PWD  "></asp:Label>
        <asp:Label ID="LabelBuildSql" runat="server" Text="BuildSql  " Visible="False"></asp:Label>
        <asp:Label ID="LabelColumnComments" runat="server" Text="ColumnComments  " 
            Visible="False"></asp:Label>
            <asp:Label ID="LabelLogTableNames" runat="server" Text="LogTableNames " 
                Visible="False"></asp:Label>
            &nbsp;<asp:Panel ID="PanelOraConnCtrl" runat="server">
        <br />
        <asp:Label ID="Label1" runat="server" Text="接続先　　：" Font-Names="ＭＳ ゴシック"></asp:Label>
        <asp:Label ID="LabelSID" runat="server" Text="SID"></asp:Label><br />
        <asp:Label ID="Label2" runat="server" Text="ユーザー名：" Font-Names="ＭＳ ゴシック"></asp:Label>
        <asp:Label ID="LabelUID" runat="server" Text="UID"></asp:Label><br />
        <asp:Label ID="Label4" runat="server" Text="パスワード：" Font-Names="ＭＳ ゴシック"></asp:Label>
        <asp:TextBox ID="TextPWD" runat="server"></asp:TextBox>
        <br /><br /><hr width="95%" style="height: -12px" />
            </asp:Panel>
        </asp:Panel>
    </div>
    
    <asp:Panel ID="PanelParam" runat="server">
        <asp:HyperLink ID="HyperLinkHome" runat="server" ImageUrl="~/images/home.gif" 
            NavigateUrl="./Default.aspx" ToolTip="ＴＯＰページに戻る" Visible="False">HyperLink</asp:HyperLink>
        <asp:HyperLink ID="HyperLinkShenExcel" runat="server" 
            ImageUrl="~/images/excel.gif" NavigateUrl="./shenexcel.aspx">Excel ダウンロード</asp:HyperLink>
        <asp:Label ID="LabelParamMess" runat="server" Text="LabelParamMessage" 
            ForeColor="DarkBlue"></asp:Label>
    </asp:Panel>
    
    <asp:Panel ID="PanelOutputType" runat ="server">
        <asp:Table ID="TableTextInput" runat="server">
        </asp:Table>
        <br /><br />
        <div align="center">
        <asp:Label ID="LabelBlankText" runat="server" Text="空白の項目："></asp:Label>
            <asp:RadioButton ID="RadioVoidExpression" runat="server" Checked="True" 
                GroupName="NoInput" Text="抽出条件から除外する" ToolTip="条件項目は無視して抽出する" />
        <asp:RadioButton ID="RadioUseDefault" runat="server" 
            GroupName="NoInput" Text="規定値を使う" ToolTip="ツールチップ表示の条件で抽出する" />
        <br /><br />
        </div>
        <div align="center">
        <asp:Label ID="LabelOutputType" runat="server" Text="出力形式："></asp:Label>
        <asp:DropDownList ID="DropDownOutputType" runat="server">
            <asp:ListItem>HTML</asp:ListItem>
            <asp:ListItem>Excel</asp:ListItem>
        </asp:DropDownList>
            <asp:Panel ID="PanelExcelOption" runat="server">
                <asp:Label ID="LabelExcelOption" runat="server" Text="Label"></asp:Label>
                <asp:CheckBox ID="CheckExcelXml" runat="server" Text="XML" 
                    ToolTip="XML フォーマットで出力する" /><span lang="ja">&nbsp;</span>
                <asp:CheckBox ID="CheckWriteCondi" runat="server" Text="抽出条件付き" 
                    ToolTip="選択された抽出条件も書き込む" />
            </asp:Panel>
        <br />
        <asp:Label ID="LabelRowCountPage" runat="server" Text="１頁の行数："></asp:Label>
        <asp:DropDownList ID="DropDownRowCountPage" runat="server">
            <asp:ListItem Value="25"></asp:ListItem>
            <asp:ListItem Value="50"></asp:ListItem>
            <asp:ListItem Selected="True" Value="100"></asp:ListItem>
            <asp:ListItem Value="200"></asp:ListItem>
            <asp:ListItem Value="500"></asp:ListItem>
        </asp:DropDownList>
        </div>
        <br />
    </asp:Panel>

    <asp:Panel ID="PanelSubmit" runat ="server" >
    <div align="center">
        <asp:Button ID="ButtonSubmit" runat="server" Text="抽出開始" Height="32px" /><br />
        <asp:Label ID="LabelOneClick" runat="server" Text="クリックは１回だけ" 
            Font-Size="Smaller" ForeColor="Red" Font-Bold="True"></asp:Label>
        <input type="hidden" name="__EVENTTARGET" id="__EVENTTARGET" value="" />
    </div>
    </asp:Panel>
    
    <!--<script language='javascript'>
    //window.alert(document.form1.DropDownOutputType.value);
    </script>-->
    <asp:GridView ID="gridView" runat="server" AllowPaging="True" BackColor="White" 
        BorderColor="#DEDFDE" BorderStyle="None" BorderWidth="1px" CellPadding="2" 
        ForeColor="Black" PageSize="3">
        <FooterStyle BackColor="#CCCC99" />
        <RowStyle BackColor="#F7F7DE" CssClass="gridViewRowStyle" />
        <PagerStyle BackColor="#EEEEEE" ForeColor="Black" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <AlternatingRowStyle BackColor="White" />
    </asp:GridView>
    <asp:Panel ID="PanelOptions" runat="server" Visible="False">
        <br />
        <asp:Label ID="Label7" runat="server" Text="PagerPosition:"></asp:Label>
        <asp:RadioButton ID="RadioTop" runat="server" GroupName="PagerPosition" 
            Text="Top" />
        &nbsp;<asp:RadioButton ID="RadioBottom" runat="server" Checked="True" 
            GroupName="PagerPosition" Text="Bottom" />
        &nbsp;<asp:RadioButton ID="RadioTopAndBottom" runat="server" 
            GroupName="PagerPosition" Text="TopAndBottom" />
    </asp:Panel>
    <p>
    <asp:Label ID="LabelDebug" runat="server" ForeColor="FloralWhite" Text="Debug"></asp:Label>
    </p>
    </form>
</body>
</html>
