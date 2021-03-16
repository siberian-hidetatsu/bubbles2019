using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.IO;

public partial class DefaultFrame : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
		try
		{
			string pmShenDocName = (Request.Params[bb.pmShenDocName] == null) ? "" : bb.pmShenDocName + "=" + Request.Params[bb.pmShenDocName] + "&";
			string pmDevelop = (Request.Params[bb.pmDevelop] == null) ? "" : bb.pmDevelop + "=" + Request.Params[bb.pmDevelop];

			Response.Write("<head>\r\n" +
						   "<title>bubbles</title>\r\n" +
						   "<link rel=\"shortcut icon\" href=\"./favicon.ico\" />\r\n" +
						   "</head>\r\n" +
						   //"<frameset cols=\"20%,80%\">\r\n" +
						   "<frameset cols=\"150,*\">\r\n" +
						   "<frame name=\"frame1\" src=\"./DefaultFrame1.aspx" + "?" + pmShenDocName + pmDevelop + "\">\r\n" +
						   //"<frame name=\"frame2\" src=\"./blank.html\">\r\n" +
						   "<frame name=\"frame2\">\r\n" +
						   "<noframes>\r\n" +
						   " <body></body>\r\n" +
						   "</noframes>\r\n" +
						   "</frameset>");
			Response.End();
		}
		catch ( Exception exp )
		{
			System.Diagnostics.Debug.WriteLine(exp.Message);
		}
    }
}
