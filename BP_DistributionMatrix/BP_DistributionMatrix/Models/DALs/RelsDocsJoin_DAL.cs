using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;

public class RelsDocsJoin_DAL
{
    private readonly string _connectionString;

    public RelsDocsJoin_DAL()
    {
        _connectionString = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
    }

    public List<Companies_Model> GetCompanies() {
        string query = @"SELECT [id]
                              ,[Company]
                          FROM ddm.Companies
                          WHERE inUse = 'TRUE';";

        List<Companies_Model> companies = new List<Companies_Model>();
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Companies_Model company = new Companies_Model
                        {
                            id = int.Parse(reader["id"].ToString()),
                            Company = reader["Company"].ToString(),
                        };

                        companies.Add(company);
                    }
                }

            }
        }

        return companies;

    }

    public string GetUserName(int userId)
    {
        string query = @"SELECT firstname, lastname
                        FROM ddm.Users_FL
                        WHERE id = @UserId;";


        string username = "";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@UserId", userId);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        username += reader["firstname"].ToString();
                        username += " ";
                        username += reader["lastname"].ToString();
                    }
                }

            }
        }

        return username;

    }

    public bool CheckCompanyId(int CompanyID)
    {
        string query = @"SELECT 1
                        FROM ddm.User_Rels
                        WHERE company_id = @CompanyId;";


        bool res = false;

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@CompanyId", CompanyID);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    res = reader.HasRows;
                }

            }
        }

        return res;

    }

    public Companies_Model GetCompany(string folderId)
    {
        string query = @"SELECT [id]
                              ,[Company]
                          FROM ddm.Companies
                          WHERE ID = @FolderId";

        Companies_Model company = new Companies_Model();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@FolderId", folderId);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        company = new Companies_Model
                        {
                            id = int.Parse(reader["id"].ToString()),
                            Company = reader["Company"].ToString(),
                        };

                    }
                }

            }
        }

        return company;

    }

    public List<RelsDocsJoin_Model> GetAllRows(int userid, string company_id)
    {
        string query = @"SELECT  
    COALESCE(GlobalCounts.I, 0) AS I, -- Overall count of 'I' for all users  
    COALESCE(GlobalCounts.R, 0) AS R, -- Overall count of 'R' for all users  
    COALESCE(GlobalCounts.A, 0) AS A, -- Overall count of 'A' for all users  
    t2.id AS doc_type_disc_id,  
    t2.disc_code,  
    t2.disc_desc,  
    t2.doc_code,  
    t2.doc_desc,  
    COALESCE(t1.user_id, @UserId) AS user_id,  
    COALESCE(t1.value, '') AS value,  
    t1.company_id,  
    -- Comma-separated list of first names & last names of users with 'A' for the same doc_type_disc_id & company_id
    ConsolidatorData.consolidators  
FROM  
    ddm.DocType_Comp_Rels dtcr  -- Step 1: Filter doc types for the company  
JOIN  
    ddm.DocTypes t2  -- Step 2: Get document details  
    ON dtcr.doc_type_id = t2.id  
    AND dtcr.company_id = @CompanyId  
LEFT JOIN (  
    -- Step 3: Calculate IRA counts  
    SELECT  
        doc_type_disc_id,  
        SUM(CASE WHEN value = 'I' THEN 1 ELSE 0 END) AS I,  
        SUM(CASE WHEN value = 'R' THEN 1 ELSE 0 END) AS R,  
        SUM(CASE WHEN value = 'A' THEN 1 ELSE 0 END) AS A  
    FROM  
        ddm.User_Rels  
    WHERE  
        company_id = @CompanyId  
    GROUP BY  
        doc_type_disc_id  
) GlobalCounts  
ON t2.id = GlobalCounts.doc_type_disc_id  
LEFT JOIN (  
    -- Step 4: Retrieve consolidators for each doc type  
    SELECT  
        sub.doc_type_disc_id,  
        STUFF((  
            SELECT ', ' + CONCAT(subUser.firstname, ' ', subUser.lastname)  
            FROM ddm.User_Rels AS subInner  
            JOIN ddm.Users_FL AS subUser  
                ON subInner.user_id = subUser.id  
            WHERE subInner.doc_type_disc_id = sub.doc_type_disc_id  -- Correct reference  
            AND subInner.company_id = @CompanyId  
            AND subInner.value = 'A'  -- Ensure only 'A' actions are considered  
            FOR XML PATH(''), TYPE).value('.', 'NVARCHAR(MAX)'), 1, 2, '') AS consolidators  
    FROM ddm.User_Rels sub  
    WHERE sub.company_id = @CompanyId  -- Ensure filtering by company  
    GROUP BY sub.doc_type_disc_id  
) ConsolidatorData  
ON t2.id = ConsolidatorData.doc_type_disc_id   
LEFT JOIN  
    ddm.User_Rels t1  -- Step 5: Finally, get the user's action  
    ON t1.doc_type_disc_id = t2.id  
    AND t1.company_id = @CompanyId  
    AND t1.user_id = @UserId  
GROUP BY  
    t2.id,  
    t2.disc_code,  
    t2.disc_desc,  
    t2.doc_code,  
    t2.doc_desc,  
    t1.user_id,  
    t1.value,  
    GlobalCounts.I,  
    GlobalCounts.R,  
    GlobalCounts.A,  
    t1.company_id,  
    ConsolidatorData.consolidators;";

        List<RelsDocsJoin_Model> res = new List<RelsDocsJoin_Model>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@UserId", userid);
                cmd.Parameters.AddWithValue("@CompanyId", company_id);

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {

                        RelsDocsJoin_Model model = new RelsDocsJoin_Model
                        {
                            I = reader["I"].ToString(),
                            R = reader["R"].ToString(),
                            A = reader["A"].ToString(),
                            doc_type_disc_id = reader["doc_type_disc_id"].ToString(),
                            disc_code = reader["disc_code"].ToString(),
                            disc_desc = reader["disc_desc"].ToString(),
                            doc_code = reader["doc_code"].ToString(),
                            doc_desc = reader["doc_desc"].ToString(),
                            user_id = reader["user_id"].ToString(),
                            value = reader["value"].ToString(),
                            company = reader["company_id"].ToString(),
                            consolidators = reader["consolidators"].ToString(),
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }

    public List<int> GetTotalIRACount(string company_id)
    {
        string query = @"SELECT
                            SUM(CASE WHEN value = 'I' THEN 1 ELSE 0 END) AS I,
                            SUM(CASE WHEN value = 'R' THEN 1 ELSE 0 END) AS R,
                            SUM(CASE WHEN value = 'A' THEN 1 ELSE 0 END) AS A
                        FROM
                            ddm.User_Rels
                        WHERE
                            company_id = @CompanyId;";

        List<int> res = new List<int>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@CompanyId", company_id);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res.Add(int.Parse(reader["R"].ToString()));
                        res.Add(int.Parse(reader["I"].ToString()));
                        res.Add(int.Parse(reader["A"].ToString()));
                    }
                }

            }
        }

        return res;
    }

    public List<Tuple<int, string>> GetTeamMembers(int userid, int team_id)
    {
        if(team_id == -1) return new List<Tuple<int, string>>();

        string query = @"SELECT [team_id], [team_name], concat(firstname, ' ', lastname) AS Name
                          ,[user_id]
                          ,[role]
                      FROM ddm.TeamUsers tu
                      inner join ddm.Teams t ON tu.team_id = t.id
                      inner join ddm.Users_FL ufl ON tu.user_id = ufl.id
                      WHERE team_id = @TeamId
                      order by name;";

        List<Tuple<int, string>> res = new List<Tuple<int, string>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@TeamId", team_id);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int curuser_id = int.Parse(reader["user_Id"].ToString());
                        string username = reader["name"].ToString();

                        if (curuser_id == userid) continue; // Skip main user

                        Tuple<int, string> data = new Tuple<int, string>(curuser_id, username);
                        res.Add(data);
                    }
                }

            }
        }

        return res;
    }

    public List<Tuple<int, string>> GetAllUsers(int userid)
    {
        string query = @"SELECT id, CONCAT(firstname, ' ', lastname) AS name 
                        FROM ddm.Users_FL fl
                        JOIN ddm.Users_PD pd On fl.email = pd.userEmail
                        where fl.lockedstatus = 'Unlocked'
                        order by name;";

        List<Tuple<int, string>> res = new List<Tuple<int, string>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int curuser_id = int.Parse(reader["id"].ToString());
                        string username = reader["name"].ToString();

                        if (curuser_id == userid) continue; // Skip main user

                        Tuple<int, string> data = new Tuple<int, string>(curuser_id, username);
                        res.Add(data);
                    }
                }

            }
        }

        return res;
    }

    public Dictionary<string, string> GetTeamActions(List<Tuple<int, string>> userids, string company_Id)
    {
        if (userids == null || !userids.Any()) 
        {
            return new Dictionary<string, string>();
        }

        string query = @"SELECT [id]
                              ,[company_id]
                              ,[doc_type_disc_id]
                              ,[user_id]
                              ,[value]
                          FROM ddm.User_Rels
                          WHERE user_id IN ({0})
                          AND company_id = @CompanyId;";


        // Extract just the IDs for the IN clause
        string inClause = string.Join(", ", userids.Select((userId, index) => "@UserId" + index));

        query = string.Format(query, inClause);

        Dictionary<string, string> res = new Dictionary<string, string>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                for (int i = 0; i < userids.Count; i++)
                {
                    cmd.Parameters.AddWithValue("@UserId" + i, userids[i].Item1);
                }

                cmd.Parameters.AddWithValue("@CompanyId", company_Id);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string concat_id = reader["doc_type_disc_id"].ToString() + reader["user_id"].ToString();
                        res[concat_id] = reader["value"].ToString();
                    }
                }

            }
        }

        return res;
    }

    public List<Tuple<int, string>> GetUserTeams(int userid)
    {
        string query = @"SELECT 
                            tu.[team_id],
                            tu.[user_id],
                            tu.[role],
                            t.[team_name]
                        FROM ddm.TeamUsers tu
                        INNER JOIN ddm.Teams t
                            ON tu.[team_id] = t.[id]
	                        WHERE user_id = @UserId;";

        List<Tuple<int, string>> res = new List<Tuple<int, string>>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@UserId", userid);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int teamid = int.Parse(reader["team_id"].ToString());
                        string teamName = reader["team_name"].ToString();
                        Tuple<int, string> curData = new Tuple<int, string>(teamid, teamName);
                        res.Add(curData);
                    }
                }

            }
        }

        return res;
    }

    public void SaveActions(List<UserRels_Model> rows)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            foreach (UserRels_Model row in rows)
            {
                string query = @"MERGE INTO ddm.User_Rels AS target
                                USING (SELECT @DocTypeDiscId AS doc_type_disc_id, @UserId AS user_id, @CompanyID AS company_id, @Value AS value) AS source
                                ON target.doc_type_disc_id = source.doc_type_disc_id
                                AND target.user_id = source.user_id
                                AND target.company_id = source.company_id
                                WHEN MATCHED THEN
                                    UPDATE SET value = source.value
                                WHEN NOT MATCHED THEN
                                    INSERT (doc_type_disc_id, user_id, company_id, value)
                                    VALUES (source.doc_type_disc_id, source.user_id, source.company_id, source.value);";

                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@DocTypeDiscId", row.doc_type_disc_id);
                    cmd.Parameters.AddWithValue("@CompanyID", row.company_id);
                    cmd.Parameters.AddWithValue("@UserId", row.user_id);
                    cmd.Parameters.AddWithValue("@Value", row.value ?? (object)DBNull.Value);

                    cmd.ExecuteNonQuery(); 
                }
            }
        }

        Debug.WriteLine("Save Attempt Complete");
    }
}