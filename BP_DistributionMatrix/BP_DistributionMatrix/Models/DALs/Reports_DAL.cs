using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Configuration;

/// <summary>
/// Summary description for DocTypes_Dal
/// </summary>
public class Reports_DAL
{
    private readonly string _connectionString;

    public Reports_DAL()
    {
        _connectionString = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
    }

    public List<string> GetReportDetails(string disc_code, string doc_code, string contractor_code)
    {
        string query = @"SELECT TOP 1 
                            dt.disc_desc, 
                            dt.doc_desc, 
                            cpy.Company
                        FROM ddm.DocTypes dt
                        JOIN ddm.User_Rels ur ON dt.id = ur.doc_type_disc_id
                        JOIN ddm.Companies cpy ON ur.company_id = cpy.id
                        WHERE dt.disc_code = @disc_code
                          AND dt.doc_code = @doc_code
                          AND cpy.contractorCode = @contractor_code;";

        List<string> res = new List<string>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@disc_code", disc_code);
                cmd.Parameters.AddWithValue("@doc_code", doc_code);
                cmd.Parameters.AddWithValue("@contractor_code", contractor_code);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(reader["Company"].ToString());
                        res.Add(reader["disc_desc"].ToString());
                        res.Add(reader["doc_desc"].ToString());
                    }
                }

            }
        }

        return res;
    }

    public List<Report_Model> GetReportActions(string disc_code, string doc_code, string contractor_code)
    {
        string query = @"SELECT CONCAT(ufl.firstname, ' ', ufl.lastname, ' (', IJV, ')') as username,
                        dt.disc_desc, dt.doc_desc, cpy.Company, cpy.contractorCode, value
                        FROM ddm.DocTypes dt
                        JOIN ddm.User_Rels ur ON dt.id = ur.doc_type_disc_id
                        JOIN ddm.Users_FL ufl ON ur.user_id = ufl.id
                        JOIN ddm.Users_PD upd ON upd.userEmail = ufl.email
                        JOIN ddm.Companies cpy ON ur.company_id = cpy.id
                        WHERE dt.disc_code = @disc_code
                          AND dt.doc_code = @doc_code
                          AND cpy.contractorCode = @contractor_code
                        ;
 
 
                        SELECT CONCAT(ufl.firstname, ' ', ufl.lastname) as username,
                        upd.IJV AS scope,
                        dt.disc_desc, dt.doc_desc, cpy.Company, cpy.contractorCode, value
                        FROM ddm.DocTypes dt
                        JOIN ddm.User_Rels ur ON dt.id = ur.doc_type_disc_id
                        JOIN ddm.Users_FL ufl ON ur.user_id = ufl.id
                        JOIN ddm.Users_PD upd ON upd.userEmail = ufl.email
                        JOIN ddm.Companies cpy ON ur.company_id = cpy.id
                        WHERE dt.disc_code = @disc_code
                          AND dt.doc_code = @doc_code
                          AND cpy.contractorCode = @contractor_code
                        ;";

        List<Report_Model> res = new List<Report_Model>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@disc_code", disc_code);
                cmd.Parameters.AddWithValue("@doc_code", doc_code);
                cmd.Parameters.AddWithValue("@contractor_code", contractor_code);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Report_Model model = new Report_Model
                        {
                            Username = reader["username"].ToString(),
                            Company = reader["Company"].ToString(),
                            Action = reader["Value"].ToString(),
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }
}