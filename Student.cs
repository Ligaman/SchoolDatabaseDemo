namespace SchoolDatabaseDemo;

// Модел на един ред от таблица Students
public class Student
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string ClassName { get; set; } = "";
    public double AverageGrade { get; set; }
    public string Email { get; set; } = "";   // Урок 11, задача 2
}
