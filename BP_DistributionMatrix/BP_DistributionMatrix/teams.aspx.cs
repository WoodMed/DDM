using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.Configuration;
using DevExpress.Web;
using System.Diagnostics;

public partial class teams : Page
{

    private readonly ITeamRepository _repo;
    private int _userId;
    private string _loginName;
    private const string SignInUrl = "~/Account/SignIn.aspx";


    public teams()
    {
        string connStr = WebConfigurationManager.ConnectionStrings["csHCCUKDDM"].ConnectionString;
        _repo = new TeamRepository(connStr);
    }

    protected void Page_Init(object  sender, EventArgs e)
    {

        if (Session["SelectedTeamId"] != null)
        {
            BindAvailableUsers((int)Session["SelectedTeamId"]);
        }

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if(Session["SelectedTeamId"] != null)
        {
            BindAvailableUsers((int)Session["SelectedTeamId"]);
        }

        if (!IsPostBack)
        {
            HttpCookie cookieId = Request.Cookies["session_userID"];
            if (cookieId == null)
            {
                Response.Redirect(SignInUrl);
                return;
            }
            Session["UserId"] = _userId;

            HttpCookie cookieName = Request.Cookies["session_user"];
            _loginName = cookieName != null ? cookieName.Value : String.Empty;
            Session["UserName"] = _loginName;

            gridTeams.DataBind();
        }
        else
        {
            _userId = Session["UserId"] != null ? (int)Session["UserId"] : -1;
            _loginName = Session["UserName"] as string ?? String.Empty;
        }
    }

    protected void gridTeams_DataBinding(object sender, EventArgs e)
    {
        gridTeams.DataSource = _repo.GetTeamsSummary();
    }

    protected void gridTeams_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
    {

        int teamId = Convert.ToInt32(gridTeams.GetRowValues(e.VisibleIndex, "Id"));
        int intMembers = Convert.ToInt32(gridTeams.GetRowValues(e.VisibleIndex, "MemberCount"));

        gridTeams.FocusedRowIndex = e.VisibleIndex;
        if (e.ButtonID == "Delete")
        {

            if (intMembers == 0)
            {
                Session["TeamToDelete"] = teamId;
                popupConfirmDelete.ShowOnPageLoad = true;
            }

            if (!_repo.IsUserLeader(teamId, _userId))
            {
                lblMessage.Text = "You must be a leader to delete this team.";
                lblMessage.ClientVisible = true;
                return;
            }

            Session["TeamToDelete"] = teamId;
            popupConfirmDelete.ShowOnPageLoad = true;
        }
        else // Edit
        {
            PerformDetailLoad(teamId, true);
        }
    }

    protected void btnConfirmDelete_Click(object sender, EventArgs e)
    {
        if (Session["TeamToDelete"] != null)
        {
            int teamId = (int)Session["TeamToDelete"];
            _repo.DeleteTeam(teamId, _userId);
            Session.Remove("TeamToDelete");
            gridTeams.DataBind();
        }
        popupConfirmDelete.ShowOnPageLoad = false;
    }

    protected void btnCreateTeam_Click(object sender, EventArgs e)
    {
        panelDetails.Visible = true;
        txtTeamName.Enabled = true;
        txtTeamName.Text = String.Empty;
        txtTeamName.ClientEnabled = true;
        Session.Remove("SelectedTeamId");
    }

    protected void btnSaveTeam_Click(object sender, EventArgs e)
    {

        string name = txtTeamName.Text.Trim();
        if (String.IsNullOrEmpty(name)) return;

        int? selected = Session["SelectedTeamId"] as int?;
        if (!selected.HasValue)
        {
            int newId = _repo.CreateTeam(name, _userId);
            Session["SelectedTeamId"] = newId;
            txtTeamName.ClientEnabled = false;
            gridTeams.DataBind();
            PerformDetailLoad(newId, false);
        }
        else
        {
            int oldId = selected.Value;
            string oldName = _repo.GetTeamName(oldId);
            _repo.RenameTeam(oldId, name, _userId, oldName);
            txtTeamName.ClientEnabled = false;
            gridTeams.DataBind();
            PerformDetailLoad(oldId, true);
        }
    }

    // 1) Data-binding for the Team Members grid
    protected void gridTeamMembers_DataBinding(object sender, EventArgs e)
    {
        if (Session["SelectedTeamId"] != null)
            gridTeamMembers.DataSource = _repo.GetTeamMembers((int)Session["SelectedTeamId"]);
    }

    // 2) Callback for the “Add Members” panel
    protected void callbackPanelAddMembers_Callback(object sender, CallbackEventArgsBase e)
    {
        int teamId = Convert.ToInt32(e.Parameter);
        Session["SelectedTeamId"] = teamId;
        // listAvailableUsers.DataSource = _repo.GetAvailableUsers(teamId);
        // listAvailableUsers.DataBind();
    }


    //protected void popupAddMembers_Init(object sender, EventArgs e)
    //{
    //    if (Session["SelectedTeamId"] != null)
    //    {
    //        int teamId = (int)Session["SelectedTeamId"];
    //        BindAvailableUsers(teamId);
    //    }
    //}


    // 3) Callback for the “Confirm Delete” popup (if you kept OnCallback on it)
    protected void callbackPanelConfirmDelete_Callback(object sender, CallbackEventArgsBase e)
    {
        if (Session["TeamToDelete"] != null)
        {
            int teamId = (int)Session["TeamToDelete"];
            _repo.DeleteTeam(teamId, _userId);
            Session.Remove("TeamToDelete");
            gridTeams.DataBind();
        }
        // server‐side hide
        popupConfirmDelete.ShowOnPageLoad = false;
    }

    protected void callbackPanelTeamDetails_Callback(object sender, CallbackEventArgsBase e)
    {
        int teamId;
        if (!Int32.TryParse(e.Parameter, out teamId)) return;
        Session["SelectedTeamId"] = teamId;
        PerformDetailLoad(teamId, false);

        gridAuditLog.DataSource = _repo.GetAuditLog(teamId); // Your data access logic
        gridAuditLog.DataBind();
    }

    protected void callbackPanelAudit_Callback(object sender, CallbackEventArgsBase e)
    {
        int teamId;
        if (!Int32.TryParse(e.Parameter, out teamId)) return;
        {
            // Update Audit Log Grid
            gridAuditLog.DataSource = _repo.GetAuditLog(teamId); // Your data access logic
            gridAuditLog.DataBind();
        }
    }

    private void PerformDetailLoad(int teamId, bool enableEdit)
    {
        TeamDetail detail = _repo.GetTeamDetails(teamId);
        txtTeamName.Text = detail.Name;
        lblCreatedBy.Text = String.Format("Created By: {0}", detail.CreatedByName);
        lblDateCreated.Text = String.Format("Date Created: {0:dd MMM yyyy}", detail.CreatedOn);
        txtTeamName.ClientEnabled = (detail.CreatedById == _userId && enableEdit);
        panelDetails.Visible = true;

        gridTeamMembers.DataSource = _repo.GetTeamMembers(teamId);
        gridTeamMembers.DataBind();

        // refill available users
        // BindAvailableUsers(teamId);

        gridAuditLog.DataSource = _repo.GetAuditLog(teamId);
        gridAuditLog.DataBind();
    }

    protected void gridAuditLog_DataBinding(object sender, EventArgs e)
    {
        if (Session["SelectedTeamId"] != null)
            gridAuditLog.DataSource = _repo.GetAuditLog((int)Session["SelectedTeamId"]);
    }

    // The missing helper you need:
    private void BindAvailableUsers(int teamId)
    {
        listAvailableUsers.DataSource = _repo.GetAvailableUsers(teamId);
        listAvailableUsers.ValueField = "Id";
        listAvailableUsers.TextField = "FullName";
        listAvailableUsers.DataBind();
    }

    protected void btnAddSelectedMembers_Click(object sender, EventArgs e)
    {
        int teamId = (int)Session["SelectedTeamId"];
        foreach (ListEditItem item in listAvailableUsers.Items)
        {
            Debug.WriteLine(item.Text);
            if (item.Selected)
                _repo.AddMember(teamId, Convert.ToInt32(item.Value), _userId);
        }

        //PerformDetailLoad(teamId, false);

    }

    protected void gridTeamMembers_CustomButtonCallback(object sender, ASPxGridViewCustomButtonCallbackEventArgs e)
    {
        int memberId = Convert.ToInt32(gridTeamMembers.GetRowValues(e.VisibleIndex, "Id"));
        int teamId = (int)Session["SelectedTeamId"];

        if (e.ButtonID == "Promote")
        {
            _repo.PromoteMember(teamId, memberId, _userId);
        }

        if (e.ButtonID == "Remove")
        {
            if (gridTeamMembers.VisibleRowCount <= 1)
            {
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "alertMessage", "alert('There must be at least 1 team member')", true);
                return;
            }
            else
            {
                _repo.RemoveMember(teamId, memberId, _userId);
            }
        }
        PerformDetailLoad(teamId, false);
        gridAuditLog.DataBind();
    }

    // … followed by your DTOs and TeamRepository implementation as before …
}


// --- DTOs and Repository below ---

public class TeamSummary
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int MemberCount { get; set; }
}

public class TeamDetail
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class MemberDto
{
    public int Id { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
}

public class AuditDto
{
    public DateTime Timestamp { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
}

public class AvailableUserDto
{
    public int Id { get; set; }
    public string FullName { get; set; }
}


public interface ITeamRepository
{
    IList<TeamSummary> GetTeamsSummary();
    TeamDetail GetTeamDetails(int teamId);
    IList<MemberDto> GetTeamMembers(int teamId);
    IList<AvailableUserDto> GetAvailableUsers(int teamId);
    IList<AuditDto> GetAuditLog(int teamId);
    bool IsUserLeader(int teamId, int userId);
    int CreateTeam(string name, int creatorId);
    string GetTeamName(int teamId);
    void RenameTeam(int teamId, string newName, int actingUserId, string oldName);
    void DeleteTeam(int teamId, int actingUserId);
    void AddMember(int teamId, int userId, int actingUserId);
    void PromoteMember(int teamId, int userId, int actingUserId);
    void RemoveMember(int teamId, int userId, int actingUserId);
}

public class TeamRepository : ITeamRepository
{
    private readonly string _connStr;

    public TeamRepository(string connStr)
    {
        _connStr = connStr;
    }

    public IList<TeamSummary> GetTeamsSummary()
    {
        var list = new List<TeamSummary>();
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT t.id, t.team_name, COUNT(tu.user_id) AS membercount " +
            "FROM ddm.Teams t " +
            "LEFT JOIN ddm.TeamUsers tu ON t.id = tu.team_id " +
            "WHERE ISNULL(t.is_deleted,0)=0 " +
            "GROUP BY t.id, t.team_name ORDER BY t.team_name", conn))
        {
            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                {
                    list.Add(new TeamSummary
                    {
                        Id = (int)rdr["id"],
                        Name = rdr["team_name"].ToString(),
                        MemberCount = (int)rdr["membercount"]
                    });
                }
            }
        }
        return list;
    }

    public TeamDetail GetTeamDetails(int teamId)
    {
        TeamDetail detail = null;
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT team_name, created_by, created_on FROM ddm.Teams WHERE id=@id", conn))
        {
            cmd.Parameters.AddWithValue("@id", teamId);
            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                if (rdr.Read())
                    detail = new TeamDetail
                    {
                        Id = teamId,
                        Name = rdr["team_name"].ToString(),
                        CreatedById = (int)rdr["created_by"],
                        CreatedByName = GetUserName((int)rdr["created_by"], conn),
                        CreatedOn = (DateTime)rdr["created_on"]
                    };
            }
        }
        return detail;
    }

    public IList<MemberDto> GetTeamMembers(int teamId)
    {
        var list = new List<MemberDto>();
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT u.id, u.firstname + ' ' + u.lastname AS UserName, " +
            "ISNULL(tu.role,'Member') AS Role " +
            "FROM ddm.Users_FL u " +
            "INNER JOIN ddm.TeamUsers tu ON u.id = tu.user_id " +
            "WHERE tu.team_id=@tid", conn))
        {
            cmd.Parameters.AddWithValue("@tid", teamId);
            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    list.Add(new MemberDto
                    {
                        Id = (int)rdr["id"],
                        UserName = rdr["UserName"].ToString(),
                        Role = rdr["Role"].ToString()
                    });
            }
        }
        return list;
    }

    public IList<AvailableUserDto> GetAvailableUsers(int teamId)
    {
        var list = new List<AvailableUserDto>();
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT id, firstname + ' ' + lastname + ' (' + email + ')' AS FullName " +
            "FROM ddm.Users_FL WHERE id NOT IN " +
            "(SELECT user_id FROM ddm.TeamUsers WHERE team_id=@tid)", conn))
        {
            cmd.Parameters.AddWithValue("@tid", teamId);
            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    list.Add(new AvailableUserDto
                    {
                        Id = (int)rdr["id"],
                        FullName = rdr["FullName"].ToString()
                    });
            }
        }
        return list;
    }

    public IList<AuditDto> GetAuditLog(int teamId)
    {
        var list = new List<AuditDto>();
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT action_timestamp, action, details " +
            "FROM ddm.TeamAuditLog WHERE team_id=@tid " +
            "ORDER BY action_timestamp DESC", conn))
        {
            cmd.Parameters.AddWithValue("@tid", teamId);
            conn.Open();
            using (var rdr = cmd.ExecuteReader())
            {
                while (rdr.Read())
                    list.Add(new AuditDto
                    {
                        Timestamp = (DateTime)rdr["action_timestamp"],
                        Action = rdr["action"].ToString(),
                        Details = rdr["details"].ToString()
                    });
            }
        }
        return list;
    }

    public bool IsUserLeader(int teamId, int userId)
    {
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT COUNT(*) FROM ddm.TeamUsers " +
            "WHERE team_id=@tid AND user_id=@uid AND role='Leader'", conn))
        {
            cmd.Parameters.AddWithValue("@tid", teamId);
            cmd.Parameters.AddWithValue("@uid", userId);
            conn.Open();
            return (int)cmd.ExecuteScalar() > 0;
        }
    }

    public int CreateTeam(string name, int creatorId)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {

                var cmd = new SqlCommand(
                    "INSERT INTO ddm.Teams (team_name, created_by, created_on) " +
                    "OUTPUT INSERTED.id VALUES (@name,@creator,GETDATE())",
                    conn, tran);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@creator", creatorId);
                int newId = (int)cmd.ExecuteScalar();

                var addSelf = new SqlCommand(
                    "INSERT INTO ddm.TeamUsers (team_id, user_id, role) " +
                    "VALUES (@tid, @uid, 'Leader')",
                    conn, tran);
                addSelf.Parameters.AddWithValue("@tid", newId);
                addSelf.Parameters.AddWithValue("@uid", creatorId);
                addSelf.ExecuteNonQuery();

                LogAudit(conn, tran, newId, creatorId,
                         "Create Team", "Created new team '" + name + "'");

                tran.Commit();
                return newId;

            }
        }
    }

    public string GetTeamName(int teamId)
    {
        using (var conn = new SqlConnection(_connStr))
        using (var cmd = new SqlCommand(
            "SELECT team_name FROM ddm.Teams WHERE id=@id", conn))
        {
            cmd.Parameters.AddWithValue("@id", teamId);
            conn.Open();
            return cmd.ExecuteScalar() as string;
        }
    }

    public void RenameTeam(int teamId, string newName,
                           int actingUserId, string oldName)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                var cmd = new SqlCommand(
                    "UPDATE ddm.Teams SET team_name=@name WHERE id=@id",
                    conn, tran);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", teamId);
                cmd.ExecuteNonQuery();

                LogAudit(conn, tran, teamId, actingUserId,
                         "Rename Team",
                         "Renamed from '" + oldName + "' to '" + newName + "'");

                tran.Commit();
            }
        }
    }

    public void DeleteTeam(int teamId, int actingUserId)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                var cmd = new SqlCommand(
                    "UPDATE ddm.Teams SET is_deleted=1 WHERE id=@id",
                    conn, tran);
                cmd.Parameters.AddWithValue("@id", teamId);
                cmd.ExecuteNonQuery();

                LogAudit(conn, tran, teamId, actingUserId,
                         "Delete Team", "Soft-deleted team");

                tran.Commit();
            }
        }
    }

    public void AddMember(int teamId, int userId, int actingUserId)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                var cmd = new SqlCommand(
                    "IF NOT EXISTS (SELECT 1 FROM ddm.TeamUsers WHERE team_id=@tid AND user_id=@uid) " +
                    "INSERT INTO ddm.TeamUsers (team_id, user_id, role) VALUES (@tid,@uid,'Member')",
                    conn, tran);
                cmd.Parameters.AddWithValue("@tid", teamId);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();

                LogAudit(conn, tran, teamId, actingUserId,
                         "Add Member", "Added user ID " + userId);

                tran.Commit();
            }
        }
    }

    public void PromoteMember(int teamId, int userId, int actingUserId)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                var cmd = new SqlCommand(
                    "UPDATE ddm.TeamUsers SET role='Leader' WHERE team_id=@tid AND user_id=@uid",
                    conn, tran);
                cmd.Parameters.AddWithValue("@tid", teamId);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();

                LogAudit(conn, tran, teamId, actingUserId,
                         "Promote Member", "Promoted user ID " + userId);

                tran.Commit();
            }
        }
    }

    public void RemoveMember(int teamId, int userId, int actingUserId)
    {
        using (var conn = new SqlConnection(_connStr))
        {
            conn.Open();
            using (var tran = conn.BeginTransaction())
            {
                var cmd = new SqlCommand(
                    "DELETE FROM ddm.TeamUsers WHERE team_id=@tid AND user_id=@uid",
                    conn, tran);
                cmd.Parameters.AddWithValue("@tid", teamId);
                cmd.Parameters.AddWithValue("@uid", userId);
                cmd.ExecuteNonQuery();

                LogAudit(conn, tran, teamId, actingUserId,
                         "Remove Member", "Removed user ID " + userId);

                tran.Commit();
            }
        }
    }

    // Helper to write audit entries
    private void LogAudit(SqlConnection conn, SqlTransaction tran,
                          int teamId, int userId,
                          string action, string details)
    {
        var log = new SqlCommand(
            "INSERT INTO ddm.TeamAuditLog (team_id, user_id, action, action_timestamp, details) " +
            "VALUES (@tid,@uid,@act,GETDATE(),@det)",
            conn, tran);

        log.Parameters.AddWithValue("@tid", teamId);
        log.Parameters.AddWithValue("@uid", userId);
        log.Parameters.AddWithValue("@act", action);
        log.Parameters.AddWithValue("@det", details);
        log.ExecuteNonQuery();
    }

    // Helper to get a user's display name
    private string GetUserName(int userId, SqlConnection conn)
    {
        using (var cmd = new SqlCommand(
            "SELECT firstname + ' ' + lastname FROM ddm.Users_FL WHERE id=@id",
            conn))
        {
            cmd.Parameters.AddWithValue("@id", userId);
            object o = cmd.ExecuteScalar();
            return o != null ? o.ToString() : String.Empty;
        }
    }
}