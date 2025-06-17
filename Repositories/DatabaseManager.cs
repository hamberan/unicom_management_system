using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;

namespace UMS.Repositories
{
    public static class DatabaseManager
    {
        private static readonly string dbFile = "unicomtic.db";
        private static readonly string connectionString = $"Data Source={dbFile};Version=3;";

        // Initialize the database and tables
        public static void InitializeDatabase()
        {
            if (!File.Exists(dbFile))
            {
                SQLiteConnection.CreateFile(dbFile);
            }

            using (var connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                CreateTable(connection, "Users",
                    @"CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT,
                        Password TEXT,
                        Role TEXT
                    );");

                CreateTable(connection, "Courses",
                    @"CREATE TABLE IF NOT EXISTS Courses (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CourseName TEXT
                    );");

                CreateTable(connection, "Subjects",
                    @"CREATE TABLE IF NOT EXISTS Subjects (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SubjectName TEXT,
                        CourseId INTEGER,
                        FOREIGN KEY(CourseId) REFERENCES Courses(Id)
                    );");

                CreateTable(connection, "Students",
                    @"CREATE TABLE IF NOT EXISTS Students (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT,
                        CourseId INTEGER,
                        FOREIGN KEY(CourseId) REFERENCES Courses(Id)
                    );");

                CreateTable(connection, "Exams",
                    @"CREATE TABLE IF NOT EXISTS Exams (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ExamName TEXT,
                        SubjectId INTEGER,
                        ExamDate TEXT,
                        FOREIGN KEY(SubjectId) REFERENCES Subjects(Id)
                    );");

                CreateTable(connection, "Marks",
                    @"CREATE TABLE IF NOT EXISTS Marks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        StudentId INTEGER,
                        ExamId INTEGER,
                        Score INTEGER,
                        FOREIGN KEY(StudentId) REFERENCES Students(Id),
                        FOREIGN KEY(ExamId) REFERENCES Exams(Id)
                    );");

                CreateTable(connection, "Rooms",
                    @"CREATE TABLE IF NOT EXISTS Rooms (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        RoomName TEXT,
                        RoomType TEXT
                    );");

                CreateTable(connection, "Timetables",
                    @"CREATE TABLE IF NOT EXISTS Timetables (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        SubjectId INTEGER,
                        Lecturer TEXT,
                        RoomId INTEGER,
                        TimeSlot TEXT,
                        FOREIGN KEY(SubjectId) REFERENCES Subjects(Id),
                        FOREIGN KEY(RoomId) REFERENCES Rooms(Id)
                    );");
            }
        }

        private static void CreateTable(SQLiteConnection connection, string tableName, string createQuery)
        {
            using (var command = new SQLiteCommand(createQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Add / Edit / Delete (similar logic)
        public static async Task ExecuteNonQueryAsync(string query, Dictionary<string, object> parameters)
        {
            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(query, connection))
                {
                    foreach (var param in parameters)
                        command.Parameters.AddWithValue(param.Key, param.Value);

                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        public static Task AddAsync(string query, Dictionary<string, object> parameters) =>
            ExecuteNonQueryAsync(query, parameters);

        public static Task EditAsync(string query, Dictionary<string, object> parameters) =>
            ExecuteNonQueryAsync(query, parameters);

        public static Task DeleteAsync(string query, Dictionary<string, object> parameters) =>
            ExecuteNonQueryAsync(query, parameters);

        // View data
        public static async Task<List<Dictionary<string, object>>> ViewAsync(string query)
        {
            var results = new List<Dictionary<string, object>>();

            using (var connection = new SQLiteConnection(connectionString))
            {
                await connection.OpenAsync();
                using (var command = new SQLiteCommand(query, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var row = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                            row[reader.GetName(i)] = reader.GetValue(i);
                        results.Add(row);
                    }
                }
            }

            return results;
        }
    }
}
