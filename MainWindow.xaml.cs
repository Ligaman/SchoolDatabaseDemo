using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SchoolDatabaseDemo;

public partial class MainWindow : Window
{
    // Целият SQL е в DatabaseService – тук само го използваме.
    private readonly DatabaseService _db = new DatabaseService();

    // Id на избрания ученик (null = няма избран ред)
    private long? _selectedId;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // При стартиране гарантираме, че базата и таблиците съществуват.
            _db.CreateDatabase();
            LoadStudents();
            StatusTextBlock.Text = "Базата е заредена.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Урок 11 ----------------

    private void CreateDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _db.CreateDatabase();
            LoadStudents();
            StatusTextBlock.Text = "Базата е готова.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // Урок 11, задача 3
    private void CheckDatabaseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            string report = _db.CheckDatabase();
            StatusTextBlock.Text = "Проверката е направена.";
            MessageBox.Show(report, "Проверка на базата",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Урок 12: показване ----------------

    private void LoadStudents()
    {
        string search = SearchBox.Text;
        var students = _db.GetStudents(search);
        StudentsGrid.ItemsSource = students;

        // Урок 12, задача 3 – брой на записите
        long total = _db.GetStudentCount();
        CountTextBlock.Text = string.IsNullOrWhiteSpace(search)
            ? $"Брой записи: {total}"
            : $"Намерени: {students.Count} от {total}";
    }

    // Урок 12, задача 2 – търсене по име (филтрира при всяка промяна)
    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        try
        {
            LoadStudents();
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Избор на ред ----------------

    private void StudentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (StudentsGrid.SelectedItem is Student student)
        {
            _selectedId = student.Id;
            NameBox.Text = student.Name;
            ClassBox.Text = student.ClassName;
            GradeBox.Text = student.AverageGrade.ToString("0.00", CultureInfo.InvariantCulture);
            EmailBox.Text = student.Email;
        }
        else
        {
            _selectedId = null;
        }

        // Урок 12, задача 4 – без избран ред „Обнови“ и „Изтрий“ са забранени
        UpdateButton.IsEnabled = _selectedId != null;
        DeleteButton.IsEnabled = _selectedId != null;
    }

    // ---------------- Добавяне ----------------

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var student = ReadStudentFromFields();
            if (student == null) return;

            _db.AddStudent(student);
            ClearFields();
            LoadStudents();
            StatusTextBlock.Text = "Ученикът е добавен.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Редактиране ----------------

    private void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedId == null)
        {
            StatusTextBlock.Text = "Първо изберете ученик от таблицата.";
            return;
        }

        try
        {
            var student = ReadStudentFromFields();
            if (student == null) return;

            student.Id = _selectedId.Value;
            _db.UpdateStudent(student);
            ClearFields();
            LoadStudents();
            StatusTextBlock.Text = "Ученикът е обновен.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Изтриване ----------------

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        // Урок 12, задача 4 – забрана за изтриване без избран ред
        if (_selectedId == null)
        {
            StatusTextBlock.Text = "Първо изберете ученик от таблицата.";
            return;
        }

        var answer = MessageBox.Show(
            $"Сигурни ли сте, че искате да изтриете „{NameBox.Text}“?",
            "Потвърждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;

        try
        {
            _db.DeleteStudent(_selectedId.Value);
            ClearFields();
            LoadStudents();
            StatusTextBlock.Text = "Ученикът е изтрит.";
        }
        catch (Exception ex)
        {
            StatusTextBlock.Text = "Грешка: " + ex.Message;
        }
    }

    // ---------------- Урок 12, задача 1 – „Изчисти полетата“ ----------------

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        ClearFields();
        StatusTextBlock.Text = "Полетата са изчистени.";
    }

    private void ClearFields()
    {
        NameBox.Text = "";
        ClassBox.Text = "";
        GradeBox.Text = "";
        EmailBox.Text = "";
        StudentsGrid.SelectedItem = null;   // това нулира и _selectedId
        _selectedId = null;
        UpdateButton.IsEnabled = false;
        DeleteButton.IsEnabled = false;
    }

    // ---------------- Проверка на въведените данни ----------------

    // Връща ученик или null, ако има грешка (грешката се показва в StatusTextBlock).
    private Student? ReadStudentFromFields()
    {
        string name = NameBox.Text.Trim();
        string className = ClassBox.Text.Trim();
        string gradeText = GradeBox.Text.Trim().Replace(',', '.'); // приема и 5,50, и 5.50
        string email = EmailBox.Text.Trim();

        if (name == "" || className == "")
        {
            StatusTextBlock.Text = "Името и класът са задължителни.";
            return null;
        }

        if (!double.TryParse(gradeText, NumberStyles.Float, CultureInfo.InvariantCulture, out double grade)
            || grade < 2 || grade > 6)
        {
            StatusTextBlock.Text = "Успехът трябва да е число между 2 и 6.";
            return null;
        }

        if (email != "" && (!email.Contains('@') || !email.Contains('.')))
        {
            StatusTextBlock.Text = "Невалиден имейл адрес.";
            return null;
        }

        return new Student
        {
            Name = name,
            ClassName = className,
            AverageGrade = grade,
            Email = email
        };
    }
}
