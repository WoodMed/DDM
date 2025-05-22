using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Configuration;

/// <summary>
/// Summary description for DocTypes_Dal
/// </summary>
public class Teams_Dal
{
    private readonly string _connectionString;

    public Teams_Dal()
    {
        _connectionString = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
    }

    public string GetTeamName(int teamId)
    {
        string query = @"SELECT team_name FROM ddm.Teams WHERE id = @TeamID";

        string res = "";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@TeamId", teamId);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res = reader["team_name"].ToString();
                    }
                }

            }
        }

        return res;
    }

    public string GetRole(int userid,int teamId)
    {
        string query = @"SELECT * FROM ddm.TeamUsers
                        WHERE user_id = @UserId AND team_id = @TeamId";

        string res = "";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.Parameters.AddWithValue("@UserId", userid);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        res = reader["role"].ToString();
                    }
                }

            }
        }

        return res;
    }

    public List<TeamUser_Model> GetTeamUsers(int teamId, int userId)
    {
        string query = @"SELECT 
                            tu.user_id, 
                            tu.role, 
                            CONCAT(u.firstname, ' ', u.lastname, ' (', u.email, ')') AS name
                        FROM ddm.TeamUsers tu
                        JOIN ddm.Users_FL u ON tu.user_id = u.id
                        WHERE tu.team_id = @TeamId;";

        List<TeamUser_Model> res = new List<TeamUser_Model>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                cmd.Parameters.AddWithValue("@TeamId", teamId);
                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int user_id = int.Parse(reader["user_id"].ToString());
                        if (user_id == userId) continue;
                        string username = reader["name"].ToString();

                        TeamUser_Model model = new TeamUser_Model()
                        {
                            Id = user_id,
                            Name = username,
                            Role = reader["role"].ToString()
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }

    public List<TeamUser_Model> GetAllUsers(int userid)
    {
        string query = @"SELECT id, CONCAT(firstname, ' ', lastname) AS name, fl.email
                        FROM ddm.Users_FL fl
                        JOIN ddm.Users_PD pd On fl.email = pd.userEmail
                        where fl.lockedstatus = 'Unlocked'
                        order by name;";

        List<TeamUser_Model> res = new List<TeamUser_Model>();

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                connection.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int user_id = int.Parse(reader["id"].ToString());
                        if (user_id == userid) continue; // skip main user
                        string username = reader["name"].ToString();
                        string email = reader["email"].ToString();
                        string popupdisplay = username + " (" + email + ")";

                        TeamUser_Model model = new TeamUser_Model()
                        {
                            Id = user_id,
                            Name = username,
                            Email = email,
                            Role = "Member",
                            PopupDisplay = popupdisplay
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }

    public List<Teams_Model> GetAllTeams(string userId)
    {
        string query = @"SELECT 
                            t.id, 
                            t.team_name, 
                            tu.role, 
                            (SELECT COUNT(*) FROM ddm.TeamUsers WHERE team_id = t.id) AS membercount
                        FROM ddm.Teams t
                        INNER JOIN ddm.TeamUsers tu ON t.id = tu.team_id
                        WHERE tu.user_id = @UserId
                        ORDER BY t.team_name;";

        List<Teams_Model> res = new List<Teams_Model>();

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
                        Teams_Model model = new Teams_Model
                        {
                            Id = int.Parse( reader["id"].ToString() ),
                            Name = reader["team_name"].ToString(),
                            Role = reader["role"].ToString(),
                            MemberCount = int.Parse( reader["memberCount"].ToString() ),
                        };

                        res.Add(model);
                    }
                }

            }
        }

        return res;
    }

    public bool CreateTeam(List<Tuple<int, string>> TeamMembers, string teamName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string checkQuery = @"SELECT id FROM ddm.Teams WHERE team_name = @TeamName";
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@TeamName", teamName);
                object existingTeamId = checkCmd.ExecuteScalar();

                if (existingTeamId != null)
                {
                    Debug.WriteLine("Team already exists. Exiting operation.");
                    return false;
                }
            }

            string CreateQuery = @"INSERT INTO ddm.Teams (team_name, created_by, created_on)
                            SELECT @TeamName, @UserID, GETDATE()
                            WHERE NOT EXISTS (SELECT 1 FROM ddm.Teams WHERE team_name = @TeamName);
                            SELECT SCOPE_IDENTITY();";

            int newTeamId = 0;

            using (SqlCommand cmd = new SqlCommand(CreateQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TeamName", teamName);
                cmd.Parameters.AddWithValue("@UserID", TeamMembers[0].Item1);

                newTeamId = Convert.ToInt32(cmd.ExecuteScalar()); // Gets ID
                Debug.WriteLine("");
            }

            string MemberQuery = @" INSERT INTO ddm.TeamUsers(team_id, user_id, role)
                                    VALUES(@TeamId, @UserId, @UserRole);";

            using (SqlCommand cmd = new SqlCommand(MemberQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TeamId", newTeamId);
                foreach (var member in TeamMembers)
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@TeamId", newTeamId);
                    cmd.Parameters.AddWithValue("@UserId", member.Item1);
                    cmd.Parameters.AddWithValue("@UserRole", member.Item2);

                    cmd.ExecuteNonQuery();
                }

            }
        }

        Debug.WriteLine("Team Created!");
        return true;
    }

    public void UpdateTeam(int teamId, List<Tuple<int, string>> TeamMembers, string teamName)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            // Step 1: Delete existing team members
            string deleteQuery = @"DELETE FROM ddm.TeamUsers WHERE team_id = @TeamId;";
            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, connection))
            {
                deleteCmd.Parameters.AddWithValue("@TeamId", teamId);
                deleteCmd.ExecuteNonQuery();
            }

            // Step 2: Insert updated team members
            string insertQuery = @"INSERT INTO ddm.TeamUsers (team_id, user_id, role)
                               VALUES (@TeamId, @UserId, @Role);";

            foreach (var member in TeamMembers)
            {
                using (SqlCommand insertCmd = new SqlCommand(insertQuery, connection))
                {
                    insertCmd.Parameters.AddWithValue("@TeamId", teamId);
                    insertCmd.Parameters.AddWithValue("@UserId", member.Item1);
                    insertCmd.Parameters.AddWithValue("@Role", member.Item2);
                    insertCmd.ExecuteNonQuery();
                }
            }

            string updateTeamsQuery = @"UPDATE ddm.Teams
                    SET team_name = @NewTeamName
                    WHERE id = @TeamId;";

            using (SqlCommand insertCmd = new SqlCommand(updateTeamsQuery, connection))
            {
                insertCmd.Parameters.AddWithValue("@TeamId", teamId);
                insertCmd.Parameters.AddWithValue("@NewTeamName", teamName);
                insertCmd.ExecuteNonQuery();
            }
        }
    }

    public void LeaveTeam(int userId,int teamId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string TeamUsersDeleteQuery = @"DELETE FROM ddm.TeamUsers WHERE team_id = @TeamId AND user_id = @UserId";

            using (SqlCommand cmd = new SqlCommand(TeamUsersDeleteQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.ExecuteNonQuery();
            }
        }

        Debug.WriteLine("Team Deleted!");
    }


    public void DeleteTeam(int teamId)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            string TeamUsersDeleteQuery = @"DELETE FROM ddm.TeamUsers WHERE team_id = @TeamId";

            using (SqlCommand cmd = new SqlCommand(TeamUsersDeleteQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.ExecuteNonQuery();
            }

            string TeamDeleteQuery = @"DELETE FROM ddm.Teams WHERE id = @TeamId";

            using (SqlCommand cmd = new SqlCommand(TeamDeleteQuery, connection))
            {
                cmd.Parameters.AddWithValue("@TeamId", teamId);
                cmd.ExecuteNonQuery();

            }
        }

        Debug.WriteLine("Team Deleted!");
    }

}