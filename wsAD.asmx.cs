using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.DirectoryServices;
using System.Collections;
using System.Configuration;
using DataAcessObject;
using System.Data.SqlClient;
using System.Data;
/// <summary>
/// Summary description for wsAD
/// </summary>
[WebService(Namespace = "http://tempuri.org/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
// To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
// [System.Web.Script.Services.ScriptService]
[ScriptService]

public class wsAD : System.Web.Services.WebService
{

    [WebMethod()]
    [ScriptMethod(ResponseFormat = ResponseFormat.Json)]

    public string findUser(string prefixText)
    {
        DirectoryEntry directory = new DirectoryEntry("LDAP://RootDSE");

        string defaultNamingContext = directory.Properties["defaultNamingContext"].Value.ToString();
        //DirectoryEntry pattern = new DirectoryEntry("LDAP://" + defaultNamingContext);
        DirectoryEntry pattern = new DirectoryEntry("LDAP://" + ConfigurationManager.AppSettings["diretorio"]);


        //string filter = "(&(|(mail=*" + prefixText + "*)(cn=*" + prefixText + "*))(mail=*@*))";
        string filter = "(&(ObjectClass=user)(&(|(mail=*" + prefixText + "*)(cn=*" + prefixText + "*))(mail=*@*)))";

        string[] strCats = { "cn", "mail", "samaccountname", "objectGuid" };
        List<string> items = new List<string>();
        DirectorySearcher dirComp = new DirectorySearcher(pattern, filter, strCats, SearchScope.Subtree);
        SearchResultCollection results = dirComp.FindAll();

        // Create a multidimensional jagged array
        string[][] JaggedArray = new string[results.Count][];
        //SortedList Lista = new SortedList();
        string cn, mail, samaccountname, objectGuid = "";
        int i = 0;

        foreach (SearchResult result in results)
        {
            DirectoryEntry entrada = result.GetDirectoryEntry();

            cn = (entrada.Properties["cn"].Value != null) ? entrada.Properties["cn"].Value.ToString() : "";
            mail = (entrada.Properties["mail"].Value != null) ? entrada.Properties["mail"].Value.ToString() : "";
            samaccountname = (entrada.Properties["samaccountname"].Value != null) ? entrada.Properties["samaccountname"].Value.ToString() : "";

            byte[] uid = (byte[])entrada.Properties["objectGuid"][0];
            objectGuid = (uid != null) ? uid[0].ToString() : "";

            JaggedArray[i] = new string[] { cn, samaccountname, mail, objectGuid };
            /*
            Lista.Add("cn", cn);
            Lista.Add("samaccountname", samaccountname);
            Lista.Add("mail", mail);
            */
            i++;
        }
        results.Dispose();
        // Return JSON data
        JavaScriptSerializer js = new JavaScriptSerializer();
        string strJSON = js.Serialize(JaggedArray);
        //string strJSON = js.Serialize(Lista);

        directory.Dispose();
        directory = null;
        pattern.Dispose();
        pattern = null;
        dirComp.Dispose();
        dirComp = null;
        return strJSON;
    }

    public string findUserBD(string prefixText)
    {
        SqlConnection conn = new SqlConnection(Conexao.strconn);
        SqlCommand comando = new SqlCommand();
        comando.Connection = conn;
        conn.Open();

        comando.CommandType = CommandType.Text;
        comando.CommandText = "Select * from usuario where loginusuario like '%' + @login + '%'";
        comando.Parameters.Add(new SqlParameter("@login", SqlDbType.VarChar, 25)).Value = prefixText;
        comando.Prepare();
        SqlDataAdapter adapter = new SqlDataAdapter(comando);
        DataSet ds = new DataSet();
        adapter.Fill(ds, "consulta");
        int i = 0;
        string[][] JaggedArray = new string[ds.Tables[0].Rows.Count][];

        foreach (DataRow row in ds.Tables[0].Rows)
        {
            string login = row["loginusuario"].ToString();
            string id = row["idusuario"].ToString();
            string nome = row["nome"].ToString();
            JaggedArray[i] = new string[] { id, login, nome };
            i++;
        }

        // Return JSON data
        JavaScriptSerializer js = new JavaScriptSerializer();
        string strJSON = js.Serialize(JaggedArray);
        //string strJSON = js.Serialize(Lista);
        ds = null;
        conn.Close();
        conn = null;
        return strJSON;
    }

    /*
    [WebMethod]
    public string HelloWorld() {
        return "Hello World";
    }
    */
}
