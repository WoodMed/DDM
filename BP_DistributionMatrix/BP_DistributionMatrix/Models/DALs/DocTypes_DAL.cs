using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

/// <summary>
/// Summary description for DocTypes_Dal
/// </summary>
public class DocTypes_DAL
{
    private readonly string _connectionString;

    public DocTypes_DAL()
    {
        _connectionString = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
    }

    public List<DocTypes_Model> GetAllRows()
    {
        string query = @"SELECT id, disc_code, disc_desc, doc_code, doc_desc, doc_type_discipline
                     FROM ddm.DocTypes;";

        List<DocTypes_Model> res = new List<DocTypes_Model>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        DocTypes_Model model = new DocTypes_Model
                        {
                            id = reader["id"].ToString(),
                            disc_code = reader["disc_code"].ToString(),
                            disc_desc = reader["disc_desc"].ToString(),
                            doc_code = reader["doc_code"].ToString(),
                            doc_desc = reader["doc_desc"].ToString(),
                            doc_type_discipline = reader["doc_type_discipline"].ToString()
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }


    public void InsertRow(DocTypes_Model model)
    {
        string query = @"INSERT INTO TestTable (Name, CreatedDate)
                         VALUES (@Name, @CreatedDate);";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}