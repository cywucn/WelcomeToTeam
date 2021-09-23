using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;


namespace WelcomeToTeam
{
    public static class SimpleDatabase
    {
        public static readonly SqliteConnection _sqliteConnection = new SqliteConnection("Data Source=WelcomeToTeam.db");

        public static void ExecuteNonQuery(string commandText)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = commandText;
            command.ExecuteNonQuery();
        }
        static SimpleDatabase()
        {
            _sqliteConnection.Open();

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS selfIntro (teamId TEXT NOT NULL, userId TEXT NOT NULL, content TEXT NOT NULL, PRIMARY KEY (teamId, userId))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS selfIntroConversationActivityId (teamId TEXT NOT NULL, userId TEXT NOT NULL, conversationId TEXT NOT NULL, activityId TEXT NOT NULL, PRIMARY KEY (teamId, userId))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS teamUser (teamId TEXT NOT NULL, userId TEXT NOT NULL, isOwner INTEGER NOT NULL, PRIMARY KEY (teamId, userId))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS teamInfo (teamId TEXT NOT NULL PRIMARY KEY, teamTeamsId TEXT NOT NULL, teamName TEXT NOT NULL)");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS comment (teamId TEXT NOT NULL, userId TEXT NOT NULL, commentUserId TEXT NOT NULL, time INTEGER NOT NULL, content TEXT NOT NULL, PRIMARY KEY (teamId, userId, commentUserId))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS toDo (teamId TEXT NOT NULL, value INTEGER NOT NULL, title TEXT NOT NULL, content TEXT NOT NULL, PRIMARY KEY (teamId, value))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS toDoUser (teamId TEXT NOT NULL, userId TEXT NOT NULL, value INTEGER NOT NULL, PRIMARY KEY (teamId, userId, value))");

            ExecuteNonQuery("CREATE TABLE IF NOT EXISTS sso (ssoConversationId TEXT NOT NULL, ssoUserTeamsId TEXT NOT NULL)");
        }

        public static void UpdateSelfIntro(string teamId, string userId, string submission)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO selfIntro (teamId, userId, content) VALUES ($teamId, $userId, $submission)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$submission", submission);
            command.ExecuteNonQuery();
        }

        public static string GetSelfIntro(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT content FROM selfIntro WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            string selfIntro = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    selfIntro = reader.GetString(0);
                }
            }
            return selfIntro;
        }


        public static void UpdateSelfIntroConversationActivityId(string teamId, string userId, string conversationId, string activityId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO selfIntroConversationActivityId (teamId, userId, conversationId, activityId) VALUES ($teamId, $userId, $conversationId, $activityId)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$conversationId", conversationId);
            command.Parameters.AddWithValue("$activityId", activityId);
            command.ExecuteNonQuery();
        }





        public static (string ConversationId, string ActivityId) GetSelfIntroConversationActivityId(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT conversationId, activityId FROM selfIntroConversationActivityId WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            string conversationId = null;
            string activityId = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    conversationId = reader.GetString(0);
                    activityId = reader.GetString(1);
                }
            }
            return (conversationId, activityId);
        }

        public static void UpdateTeamInfo(string teamId, string teamTeamsId, string teamName)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO teamInfo (teamId, teamTeamsId, teamName) VALUES ($teamId, $teamTeamsId, $teamName)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$teamTeamsId", teamTeamsId);
            command.Parameters.AddWithValue("$teamName", teamName);
            command.ExecuteNonQuery();
        }
        public static string GetTeamName(string teamId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamName from teamInfo WHERE teamId=$teamId";
            command.Parameters.AddWithValue("$teamId", teamId);
            string teamName = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    teamName = reader.GetString(0);
                }
            }
            return teamName;
        }

        public static string GetTeamTeamsId(string teamId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamTeamsId from teamInfo WHERE teamId=$teamId";
            command.Parameters.AddWithValue("$teamId", teamId);
            string teamTeamsId = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    teamTeamsId = reader.GetString(0);
                }
            }
            return teamTeamsId;
        }

        public static void UpdateTeamUser(string teamId, string userId, bool isOwner)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO teamUser (teamId, userId, isOwner) VALUES ($teamId, $userId, $isOwner)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$isOwner", isOwner);
            command.ExecuteNonQuery();
        }

        public static bool IsUserInTeam(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT * FROM teamUser WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return true;
                }
            }
            return false;
        }



        public static List<(string teamId, string userId, string content)> GetSelfIntroList(string userId)
        {
            List<(string teamId, string userId, string content)> selfIntroList = new();
            SqliteCommand command;
            command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamUser.teamId, selfIntro.userId, selfIntro.content FROM teamUser, selfIntro WHERE teamUser.userId=$userId AND teamUser.teamId=selfIntro.teamId ORDER BY teamUser.teamId";
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string teamId = reader.GetString(0);
                    string introUserId = reader.GetString(1);
                    string content = reader.GetString(2);
                    selfIntroList.Add((teamId, introUserId, content));
                }
            }
            return selfIntroList;
        }





        public static void UpdateComment(string teamId, string userId, string commentUserId, string submission)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO comment (teamId, userId, commentUserId, time, content) VALUES ($teamId, $userId, $commentUserId, $time, $submission)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$commentUserId", commentUserId);
            command.Parameters.AddWithValue("$time", DateTime.UtcNow.Ticks);
            command.Parameters.AddWithValue("$submission", submission);
            command.ExecuteNonQuery();
        }


        public static List<(string commentUserId, long time, string comment)> GetCommentList(string teamId, string userId)
        {
            List<(string commentUserId, long time, string comment)> commentList = new();
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT commentUserId, time, content FROM comment WHERE teamId=$teamId AND userId=$userId ORDER BY time ASC";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string commentUserId = reader.GetString(0);
                    long time = reader.GetInt64(1);
                    string comment = reader.GetString(2);
                    commentList.Add((commentUserId, time, comment));
                }
            }
            return commentList;
        }

        public static List<(string teamId, int value, string title, string content)> GetToDoList(string userId)
        {
            List<(string teamId, int value, string title, string content)> toDoList = new();
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamUser.teamId, toDo.value, toDo.title, toDo.content FROM teamUser, toDo WHERE teamUser.userId=$userId AND teamUser.teamId=toDo.teamId ORDER BY teamUser.teamId, toDo.value ASC";
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string teamId = reader.GetString(0);
                    int value = reader.GetInt32(1);
                    string title = reader.GetString(2);
                    string content = reader.GetString(3);
                    toDoList.Add((teamId, value, title, content));
                }
            }
            return toDoList;
        }

        public static void UpdateToDoList(string teamId, int value, string title, string content)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO toDo (teamId, value, title, content) VALUES ($teamId, $value, $title, $content)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$value", value);
            command.Parameters.AddWithValue("$title", title);
            command.Parameters.AddWithValue("$content", content);
            command.ExecuteNonQuery();
        }

        public static void UpdateToDoList(string teamId, string title, string content)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT value FROM toDo WHERE teamId=$teamId ORDER BY value DESC LIMIT 1";
            command.Parameters.AddWithValue("$teamId", teamId);
            int value = 0;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    value = reader.GetInt32(0);
                    ++value;
                }
            }

            UpdateToDoList(teamId, value, title, content);
        }



        public static List<(string teamId, int value)> GetToDoUserValueList(string userId)
        {
            List<(string teamId, int value)> toDoUserValueList = new();
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamId, value FROM toDoUser WHERE userId=$userId";
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string teamId = reader.GetString(0);
                    int value = reader.GetInt32(1);
                    toDoUserValueList.Add((teamId, value));
                }
            }
            return toDoUserValueList;
        }

        public static void UpdateToDoUserValue(string teamId, string userId, int value, bool flag)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            if (flag)
            {
                command.CommandText = "REPLACE INTO toDoUser (teamId, userId, value) VALUES ($teamId, $userId, $value)";
            }
            else
            {
                command.CommandText = "DELETE FROM toDoUser WHERE teamId=$teamId AND userId=$userId AND value=$value";
            }
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.Parameters.AddWithValue("$value", value);
            command.ExecuteNonQuery();
        }

        public static void ClearToDoList(string teamId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM toDo WHERE teamId=$teamId AND value<>0";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.ExecuteNonQuery();
            command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM toDoUser WHERE teamId=$teamId AND value<>0";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.ExecuteNonQuery();
        }
        public static List<string> GetOwnedTeamIdList(string userId)
        {
            List<string> ownedTeamIdList = new();
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT teamId FROM teamUser WHERE userId=$userId AND isOwner=1";
            command.Parameters.AddWithValue("$userId", userId);
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    string teamId = reader.GetString(0);
                    ownedTeamIdList.Add(teamId);
                }
            }
            return ownedTeamIdList;
        }

        public static void DeleteUserFromTeamSelfIntro(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM selfIntro WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public static void DeleteUserFromTeamSelfIntroConversationActivityId(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM selfIntroConversationActivityId WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public static void DeleteUserFromTeamTeamUser(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM teamUser WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public static void DeleteUserFromTeamComment(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM comment WHERE teamId=$teamId AND (userId=$userId OR commentUserId=$userId)";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public static void DeleteUserFromTeamToDoUser(string teamId, string userId)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "DELETE FROM toDoUser WHERE teamId=$teamId AND userId=$userId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.Parameters.AddWithValue("$userId", userId);
            command.ExecuteNonQuery();
        }

        public static void DeleteUserFromTeam(string teamId, string userId)
        {
            DeleteUserFromTeamSelfIntro(teamId, userId);
            DeleteUserFromTeamSelfIntroConversationActivityId(teamId, userId);
            DeleteUserFromTeamTeamUser(teamId, userId);
            DeleteUserFromTeamComment(teamId, userId);
            DeleteUserFromTeamToDoUser(teamId, userId);
        }

        public static void DeleteTeam(string teamId, string tableName)
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = $"DELETE FROM {tableName} WHERE teamId=$teamId";
            command.Parameters.AddWithValue("$teamId", teamId);
            command.ExecuteNonQuery();
        }

        public static void DeleteTeam(string teamId)
        {
            DeleteTeam(teamId, "selfIntro");
            DeleteTeam(teamId, "selfIntroConversationActivityId");
            DeleteTeam(teamId, "teamUser");
            DeleteTeam(teamId, "teamInfo");
            DeleteTeam(teamId, "comment");
            DeleteTeam(teamId, "toDo");
            DeleteTeam(teamId, "toDoUser");
        }

        public static (string ssoConversationId, string ssoUserTeamsId) GetSSOConversationIdUserTeamsId()
        {
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "SELECT ssoConversationId, ssoUserTeamsId FROM sso";
            string ssoConversationId = null;
            string ssoUserTeamsId = null;
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    ssoConversationId = reader.GetString(0);
                    ssoUserTeamsId = reader.GetString(1);
                }
            }
            return (ssoConversationId, ssoUserTeamsId);
        }

        public static void UpdateSSOConversationIdUserTeamsId(string ssoConversationId, string ssoUserTeamsId)
        {
            ExecuteNonQuery("DELETE FROM sso");
            SqliteCommand command = _sqliteConnection.CreateCommand();
            command.CommandText = "REPLACE INTO sso (ssoConversationId, ssoUserTeamsId) VALUES ($ssoConversationId, $ssoUserTeamsId)";
            command.Parameters.AddWithValue("$ssoConversationId", ssoConversationId);
            command.Parameters.AddWithValue("$ssoUserTeamsId", ssoUserTeamsId);
            command.ExecuteNonQuery();
        }
    }
}
