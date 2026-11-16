# SchoolDatabaseDemo – обединена допълнителна задача (Урок 11 + Урок 12)

WPF · C# · .NET 10 · Microsoft.Data.Sqlite

## Какво е направено

| Урок | Задача | Къде в кода |
|------|--------|-------------|
| 11 | 1. Таблица Subjects | `DatabaseService.CreateDatabase()` (с 4 начални предмета) |
| 11 | 2. Колона Email в Students | `CreateDatabase()` + `ALTER TABLE`, ако базата е стара; поле Email в прозореца |
| 11 | 3. Бутон „Провери база“ | `CheckDatabaseButton_Click` → `DatabaseService.CheckDatabase()` |
| 11 | 4. Отделен клас DatabaseService | `DatabaseService.cs` |
| 11 | 5. Запис в Git | `.gitignore` + командите по-долу |
| 12 | 1. Бутон „Изчисти полетата“ | `ClearButton_Click`, `ClearFields()` |
| 12 | 2. Търсене по име | `SearchBox_TextChanged` → `GetStudents(search)` (работи и с кирилица, без значение от главни/малки букви) |
| 12 | 3. Брой на записите | `CountTextBlock` – „Брой записи: N“ / „Намерени: X от N“ |
| 12 | 4. Без изтриване без избран ред | „Обнови“ и „Изтрий“ са неактивни без избран ред + проверка в кода |
| 12 | 5. SQL кодът в DatabaseService | В `MainWindow.xaml.cs` няма нито един SQL ред |

Задачи 4 от Урок 11 и 5 от Урок 12 съвпадат – решени са с един клас.

## Файлове

- `SchoolDatabaseDemo.csproj` – проектът и пакетът Microsoft.Data.Sqlite
- `Student.cs` – модел (Id, Name, ClassName, AverageGrade, Email)
- `DatabaseService.cs` – целият SQL код
- `MainWindow.xaml` / `MainWindow.xaml.cs` – интерфейс и обработчици
- `App.xaml` / `App.xaml.cs`

## Стартиране

Отворете `SchoolDatabaseDemo.csproj` във Visual Studio 2026 и натиснете F5.
Ако вече имате проект от урок 11/12, копирайте файловете върху него (namespace е `SchoolDatabaseDemo`).
Стар `school.db` без колона Email се обновява автоматично.

## Проверка

1. Добавете ученик → появява се в таблицата, броят се увеличава.
2. Празно име / успех „abc“ или 7 → съобщение за грешка, нищо не се записва.
3. Изберете ред → полетата се попълват, „Обнови“ и „Изтрий“ стават активни.
4. Променете и натиснете „Обнови“.
5. „Изтрий“ → потвърждение → редът изчезва.
6. Напишете част от име в търсенето → таблицата се филтрира.
7. „Провери база“ → показва таблиците, броя записи и колоната Email.
8. Затворете и стартирайте отново → данните са запазени.

## Git (Урок 11, задача 5)

```bash
git init
git add .
git commit -m "SchoolDatabaseDemo: SQLite CRUD + допълнителни задачи от уроци 11 и 12"
```
