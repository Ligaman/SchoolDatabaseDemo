using System.IO;
using System.Text;
using Microsoft.Data.Sqlite;

namespace SchoolDatabaseDemo;

// Целият SQL код на приложението е тук (Урок 11, задача 4 и Урок 12, задача 5).
// MainWindow само извиква методите на този клас.
public class DatabaseService
{
    private const string DatabaseFile = "school.db";
    private const string ConnectionString = "Data Source=" + DatabaseFile;

    // Метод за отваряне на връзка (Урок 12)
    private SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        return connection;
    }

    // ---------------- Създаване на базата ----------------

    // Създава таблиците, ако ги няма. Може да се вика многократно.
    public void CreateDatabase()
    {
        using var connection = OpenConnection();

        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    ClassName TEXT NOT NULL,
                    AverageGrade REAL,
                    Email TEXT
                  );";
            command.ExecuteNonQuery();
        }

        // Урок 11, задача 2: ако school.db е създаден в урок 11 без Email,
        // добавяме колоната към съществуващата таблица.
        if (!ColumnExists(connection, "Students", "Email"))
        {
            using var alter = connection.CreateCommand();
            alter.CommandText = "ALTER TABLE Students ADD COLUMN Email TEXT;";
            alter.ExecuteNonQuery();
        }

        // Урок 11, задача 1: таблица Subjects
        using (var command = connection.CreateCommand())
        {
            command.CommandText =
                @"CREATE TABLE IF NOT EXISTS Subjects (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE
                  );";
            command.ExecuteNonQuery();
        }

        // Няколко начални предмета. INSERT OR IGNORE не дублира при повторно изпълнение.
        string[] subjects = { "Математика", "Български език", "Информатика", "Английски език" };
        foreach (var subject in subjects)
        {
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT OR IGNORE INTO Subjects (Name) VALUES ($name);";
            insert.Parameters.AddWithValue("$name", subject);
            insert.ExecuteNonQuery();
        }
    }

    private static bool ColumnExists(SqliteConnection connection, string table, string column)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info({table});";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            // Колона 1 в резултата на PRAGMA table_info е името на колоната
            if (string.Equals(reader.GetString(1), column, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // ---------------- Проверка на базата (Урок 11, задача 3) ----------------

    public string CheckDatabase()
    {
        // Проверяваме файла ПРЕДИ да отворим връзка, защото Open() създава празен файл.
        if (!File.Exists(DatabaseFile))
            return "Файлът school.db не съществува. Натиснете „Създай база“.";

        using var connection = OpenConnection();
        var report = new StringBuilder();
        report.AppendLine("Файл: " + Path.GetFullPath(DatabaseFile));

        foreach (var table in new[] { "Students", "Subjects" })
        {
            if (TableExists(connection, table))
                report.AppendLine($"Таблица {table}: има ({CountRows(connection, table)} записа)");
            else
                report.AppendLine($"Таблица {table}: ЛИПСВА");
        }

        if (TableExists(connection, "Students"))
        {
            report.AppendLine(ColumnExists(connection, "Students", "Email")
                ? "Колона Email: има"
                : "Колона Email: ЛИПСВА");
        }

        return report.ToString().TrimEnd();
    }

    private static bool TableExists(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name;";
        command.Parameters.AddWithValue("$name", table);
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static long CountRows(SqliteConnection connection, string table)
    {
        using var command = connection.CreateCommand();
        // Името на таблицата идва само от нашия код (Students/Subjects), не от потребителя.
        command.CommandText = $"SELECT COUNT(*) FROM {table};";
        return Convert.ToInt64(command.ExecuteScalar());
    }

    // ---------------- CRUD за Students (Урок 12) ----------------

    // SELECT. Ако search не е празно, връща само учениците, чието име го съдържа
    // (Урок 12, задача 2 – търсене по име).
    public List<Student> GetStudents(string? search = null)
    {
        var students = new List<Student>();
        using var connection = OpenConnection();

        // Вграденото LOWER() в SQLite работи само с латиница.
        // Добавяме наша функция, за да работи търсенето и с кирилица (Иван = иван).
        connection.CreateFunction("to_lower", (string? text) => text?.ToLower());

        using var command = connection.CreateCommand();
        if (string.IsNullOrWhiteSpace(search))
        {
            command.CommandText =
                @"SELECT Id, Name, ClassName, AverageGrade, Email
                  FROM Students
                  ORDER BY Id";
        }
        else
        {
            command.CommandText =
                @"SELECT Id, Name, ClassName, AverageGrade, Email
                  FROM Students
                  WHERE to_lower(Name) LIKE $search
                  ORDER BY Id";
            command.Parameters.AddWithValue("$search", "%" + search.Trim().ToLower() + "%");
        }

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            students.Add(new Student
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                ClassName = reader.GetString(2),
                AverageGrade = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                Email = reader.IsDBNull(4) ? "" : reader.GetString(4)
            });
        }
        return students;
    }

    // Урок 12, задача 3 – брой на записите
    public long GetStudentCount()
    {
        using var connection = OpenConnection();
        return CountRows(connection, "Students");
    }

    public void AddStudent(Student student)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            @"INSERT INTO Students (Name, ClassName, AverageGrade, Email)
              VALUES ($name, $class, $grade, $email)";
        AddStudentParameters(command, student);
        command.ExecuteNonQuery();
    }

    public void UpdateStudent(Student student)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText =
            @"UPDATE Students SET
                Name = $name,
                ClassName = $class,
                AverageGrade = $grade,
                Email = $email
              WHERE Id = $id";
        AddStudentParameters(command, student);
        command.Parameters.AddWithValue("$id", student.Id);
        command.ExecuteNonQuery();
    }

    public void DeleteStudent(long id)
    {
        using var connection = OpenConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Students WHERE Id = $id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void AddStudentParameters(SqliteCommand command, Student student)
    {
        command.Parameters.AddWithValue("$name", student.Name);
        command.Parameters.AddWithValue("$class", student.ClassName);
        command.Parameters.AddWithValue("$grade", student.AverageGrade);
        // Празен имейл записваме като NULL
        command.Parameters.AddWithValue("$email",
            string.IsNullOrWhiteSpace(student.Email) ? (object)DBNull.Value : student.Email);
    }
}
