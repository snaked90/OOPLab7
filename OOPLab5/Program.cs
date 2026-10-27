namespace OOPLab7;

using System.Runtime.CompilerServices;
using System.Text.Json;

public class Program
{
    static List<Satellite> arr = new List<Satellite>();
    static int maxCount;
    static Random rnd = new Random();

    static void Main(string[] args)
    {

        maxCount = ReadValidInt("Введіть максимальну кількість супутників: ", 1, int.MaxValue);

        bool isRunning = true;
        while (isRunning)
        {
            Console.WriteLine("1 – Додати об'єкт\n" +
                              "2 – Переглянути всі об'єкти\n" +
                              "3 – Знайти об'єкт\n" +
                              "4 – Продемонструвати поведінку\n" +
                              "5 – Видалити об'єкт\n" +
                              "6 – Продемонструвати static-методи\n" +
                              "7 – Зберегти колекцію об'єктів у файлі\n" +
                              "8 – Зчитати колекцію об'єктів з файлу\n" +
                              "9 – Очистити колекцію об'єктів\n" +
                              "0 – Вийти з програми");
            Console.Write("Виберіть пункт меню: ");

            string choice = Console.ReadLine();
            Console.WriteLine();

            switch (choice)
            {
                case "1": AddSatellite(); break;
                case "2": DisplaySatellites(arr); break;
                case "3": SearchSatellites(); break;
                case "4": DemonstrateSatellites(); break;
                case "5": DeleteSatellite(); break;
                case "6": DemonstrateStaticMethods(); break;
                case "7": SaveCollection(); break;
                case "8": LoadCollection(); break;
                case "9": ClearCollection(); break;
                case "0":
                    Console.WriteLine("Вихід з програми");
                    isRunning = false;
                    break;
                default:
                    Console.WriteLine("Введіть знов");
                    break;
            }
        }
    }

    static int ReadValidInt(string message, int min, int max)
    {
        while (true)
        {
            Console.Write(message);
            if (int.TryParse(Console.ReadLine(), out int res) && res >= min && res <= max)
                return res;
            Console.WriteLine($"Введіть ціле число в діапазоні {min} - {max}");
        }
    }

    static double ReadValidDouble(string message, double min, double max)
    {
        while (true)
        {
            Console.Write(message);
            if (double.TryParse(Console.ReadLine(), out double res) && res >= min && res <= max)
                return res;
            Console.WriteLine($"Введіть дійсне число в діапазоні {min} - {max}");
        }
    }

    public static void AddSatelliteToList(List<Satellite> list, Satellite satellite, int max)
    {
        if (list == null)
            throw new Exception("Список не існує");
        if (satellite == null)
            throw new Exception("Супутник не може бути порожнім");
        if (list.Count >= max)
            throw new Exception($"Досягнуто максимальної кількості супутників ({max}). Додавання неможливе");

        list.Add(satellite);
    }

    public static Satellite CreateDefaultSatellite()
    {
        return new Satellite
        {
            Name = $"Experimental {Satellite.ExpCount}",
            Type = SType.Communication,
            OrbitAltitude = 400,
            LaunchDate = DateTime.Now
        };
    }

    public static List<Satellite> FindByType(List<Satellite> list, SType type)
    {
        if (!Enum.IsDefined(typeof(SType), type))
            throw new Exception("Вказано неіснуючий тип супутника");

        return list.FindAll(s => s.Type == type);
    }

    public static List<Satellite> FindByName(List<Satellite> list, string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new Exception("Запит для пошуку не може бути порожнім");

        string lower = query.ToLower();
        return list.FindAll(s => s.Name.ToLower().Contains(lower));
    }

    public static string RemoveByIndex(List<Satellite> list, int position)
    {
        if (position < 1 || position > list.Count)
            throw new Exception($"Номер має бути в межах 1 - {list.Count}");

        string removedName = list[position - 1].Name;
        list.RemoveAt(position - 1);
        return removedName;
    }

    public static int RemoveByType(List<Satellite> list, SType type)
    {
        if (!Enum.IsDefined(typeof(SType), type))
            throw new Exception("Вказано неіснуючий тип супутника");

        return list.RemoveAll(s => s.Type == type);
    }

    public static string FormatTable(List<Satellite> list)
    {
        if (list == null || list.Count == 0)
            return "Жодного супутника не знайдено";

        string line = new string('-', 110);
        string result = line + "\n" +
                        $"| {"№",-3} | {"Назва",-12} | {"Тип",-13} | {"Орбіта (км)",-12} " +
                        $"| {"Дата запуску",-12} | {"Пальне",-8} | {"Стан",-6} | {"Категорія",-15} |" + "\n" +
                        line + "\n";

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            result += $"| {i + 1,-3} | {s.Name,-12} | {s.Type,-13} | {s.OrbitAltitude,-12} " +
                      $"| {s.LaunchDate,-12:dd.MM.yyyy} | {s.FuelLevel,-8} | {(s.IsActive ? "On" : "Off"),-6} | {s.OrbitCategory,-15} |" + "\n";
        }

        result += line;
        return result;
    }

    public static string FormatStatistics()
    {
        return $"Всього супутників: {Satellite.Count}\n" +
               $"Всього експерементальних супутників: {Satellite.ExpCount}\n" +
               $"Всього витрачено палива: {Satellite.UsedFuel} %";
    }

    static void AddSatelliteHandle()
    {
        string name;
        while (true)
        {
            Console.Write("Введіть назву (3-15 символів): ");
            name = Console.ReadLine();
            if (name.Length >= 3 && name.Length <= 15)
                break;
            Console.WriteLine("Назва має містити від 3 до 15 символів");
        }

        Console.WriteLine("Виберіть тип супутника: 1-Communication, 2-Weather, 3-Scientific, 4-Military");
        int typeVal = ReadValidInt("Введіть номер типу (1-4): ", 1, 4);
        SType type = (SType)typeVal;

        double altitude = ReadValidDouble("Введіть висоту орбіти у км: ", 160.0, 40000.0);

        DateTime launchDate;
        DateTime minDate = new DateTime(1957, 10, 4);
        while (true)
        {
            Console.Write("Введіть дату запуску (формат dd.MM.yyyy): ");
            if (DateTime.TryParseExact(Console.ReadLine(), "dd.MM.yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out launchDate)
                && launchDate >= minDate && launchDate <= DateTime.Now)
                break;
            Console.WriteLine($"Введіть дату у форматі dd.MM.yyyy в діапазоні {minDate:dd.MM.yyyy} - {DateTime.Now:dd.MM.yyyy}");
        }

        Satellite s = new Satellite(name, type, altitude, launchDate);
        AddSatelliteToList(arr, s, maxCount);
        Console.WriteLine("Супутник успішно додано\n");
    }

    static void AddSatellite()
    {
        if (arr.Count >= maxCount)
        {
            Console.WriteLine($"Досягнуто максимальної кількості супутників ({maxCount}). Додавання неможливе.\n");
            return;
        }

        Console.WriteLine("Виберіть режим додавання: \n" +
                          "1 - Ввести дані вручну (повний конструктор) \n" +
                          "2 - Створити тестовий супутник за замовченням\n" +
                          "3 - Випадковий конструктор\n" +
                          "4 - Ввести рядком (TryParse)");
        Console.Write("Виберіть пункт меню: ");
        string choice = Console.ReadLine();

        if (choice == "1")
        {
            AddSatelliteHandle();
        }

        if (choice == "2")
        {
            AddSatelliteToList(arr, CreateDefaultSatellite(), maxCount);
            Console.WriteLine("Супутник за замовченням додано\n");
            return;
        }

        if (choice == "3")
        {
            int num = rnd.Next(1, 4);
            switch (num)
            {
                case 1:
                    Console.WriteLine("Використано повний конструктор");
                    AddSatelliteHandle();
                    break;
                case 2:
                    Console.WriteLine("Використано конструктор за замовчуванням");
                    AddSatelliteToList(arr, CreateDefaultSatellite(), maxCount);
                    break;
                case 3:
                    Console.WriteLine("Використано неповний конструктор");
                    string name;
                    while (true)
                    {
                        Console.Write("Введіть назву (3-15 символів): ");
                        name = Console.ReadLine();
                        if (name.Length >= 3 && name.Length <= 15)
                            break;
                        Console.WriteLine("Назва має містити від 3 до 15 символів");
                    }

                    Console.WriteLine("Виберіть тип супутника: 1-Communication, 2-Weather, 3-Scientific, 4-Military");
                    int typeVal = ReadValidInt("Введіть номер типу (1-4): ", 1, 4);
                    SType type = (SType)typeVal;
                    AddSatelliteToList(arr, new Satellite(name, type), maxCount);
                    break;
            }
        }
        if (choice == "4")
        {
            Console.Write("Введіть дані супутника (назва; тип; висота; дата): ");
            string input = Console.ReadLine();

            if (Satellite.TryParse(input, out Satellite s))
            {
                AddSatelliteToList(arr, s, maxCount);
                Console.WriteLine("Супутник успішно додано\n");
            }
            else
            {
                Console.WriteLine("Некоректний формат або значення — супутник не додано\n");
            }
        }
    }

    static void DisplaySatellites(List<Satellite> list)
    {
        if (list.Count == 0)
        {
            Console.WriteLine("Жодного супутника не знайдено");
            return;
        }

        Console.WriteLine(FormatStatistics());
        Console.WriteLine(FormatTable(list));
    }

    static void SearchSatellites()
    {
        if (arr.Count == 0) { Console.WriteLine("Список порожній"); return; }
        Console.WriteLine("Виберіть характеристику для пошуку: \n1 - За типом супутника \n2 - За назвою");
        Console.Write("Виберіть пункт меню: ");
        string opt = Console.ReadLine();
        List<Satellite> results = new List<Satellite>();

        if (opt == "1")
        {
            int typeVal = ReadValidInt("Введіть тип (1-4): ", 1, 4);
            SType type = (SType)typeVal;
            results = FindByType(arr, type);
        }
        else if (opt == "2")
        {
            Console.Write("Введіть назву (або її частину): ");
            string query = Console.ReadLine();
            try
            {
                results = FindByName(arr, query);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }
        else
        {
            Console.WriteLine("Некоректний вибір характеристики");
            return;
        }

        Console.WriteLine("\nРезультати пошуку");
        DisplaySatellites(results);
    }

    static void DemonstrateSatellites()
    {
        if (arr.Count == 0) { Console.WriteLine("Список порожній"); return; }
        DisplaySatellites(arr);
        int index = ReadValidInt("Введіть номер супутника для взаємодії: ", 1, arr.Count);
        Satellite selected = arr[index - 1];

        Console.WriteLine($"\nКерування супутником '{selected.Name}'");
        Console.WriteLine("1 - Передати дані\n2 - Перемкнути живлення\n3 - Змінити висоту орбіти\n4 - Симуляція відхилення по днях");
        Console.Write("Виберіть пункт меню: ");
        string subChoice = Console.ReadLine();
        Console.WriteLine();

        switch (subChoice)
        {
            case "1":
                try { Console.WriteLine(selected.GetData()); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                break;
            case "2":
                try { Console.WriteLine(selected.SwitchPower()); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                break;
            case "3":
                int choice = ReadValidInt("1 - Звичайна \n2 - Термінова \n3 - За замовчуванням \nВибір: ", 1, 3);
                Console.WriteLine();
                switch (choice)
                {
                    case 1:
                        double change = ReadValidDouble("Введіть зміну орбіти у км (від -500 до 500): ", -500, 500);
                        try { Console.WriteLine(selected.AdjustAltitude(change)); }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case 2:
                        double changee = ReadValidDouble("Введіть зміну орбіти у км (від -500 до 500): ", -500, 500);
                        try { Console.WriteLine(selected.AdjustAltitude(changee, true)); }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                    case 3:
                        try { Console.WriteLine(selected.AdjustAltitude()); }
                        catch (Exception ex) { Console.WriteLine(ex.Message); }
                        break;
                }
                break;
            case "4":
                int days = ReadValidInt("Введіть кількість днів для симуляції (від 0 до 300): ", 0, 300);
                try { Console.WriteLine(selected.LeaveTimePassing(days)); }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
                break;
        }
    }

    static void DeleteSatellite()
    {
        if (arr.Count == 0) { Console.WriteLine("Список порожній"); return; }
        Console.WriteLine("Виберіть спосіб видалення:\n1 - За порядковим номером у таблиці\n2 - За характеристикою (тип)\n");
        string opt = Console.ReadLine();

        if (opt == "1")
        {
            DisplaySatellites(arr);
            int position = ReadValidInt("Введіть номер об'єкта для видалення: ", 1, arr.Count);
            string removedName = RemoveByIndex(arr, position);
            Console.WriteLine($"Об'єкт '{removedName}' успішно видалено");
        }
        else if (opt == "2")
        {
            int typeVal = ReadValidInt("Введіть тип (1-4): ", 1, 4);
            SType targetType = (SType)typeVal;

            int removedCount = RemoveByType(arr, targetType);

            if (removedCount > 0)
                Console.WriteLine($"Успішно видалено {removedCount} об'єкт(ів) типу {targetType}");
            else
                Console.WriteLine("Об'єктів із вказаною характеристикою не знайдено");
        }
        else
        {
            Console.WriteLine("Некоректний вибір");
        }
    }
    static void DemonstrateStaticMethods()
    {
        Console.WriteLine($"Всього супутників: {Satellite.Count}");
        Console.WriteLine($"Всього експерементальних супутників: {Satellite.ExpCount}");
        Console.WriteLine($"Всього витрачено палива: {Satellite.UsedFuel} %");

        double from = ReadValidDouble("Введіть першу висоту (км): ", 160, 40000);
        double to = ReadValidDouble("Введіть другу висоту (км): ", 160, 40000);

        try
        {
            Console.WriteLine(Satellite.CalculateFuel(from, to));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public const string CsvHeader = "Name; Type; OrbitAltitude; LaunchDate";

    private static void CheckListAndPath(List<Satellite> list, string path)
    {
        if (list == null)
        {
            throw new Exception("Список не існує");
        }
        if(string.IsNullOrWhiteSpace(path))
            throw new Exception("Шлях до файлу не може бути порожнім");
    }

    public static void SaveToCsv(List<Satellite> list, string path)
    {
        CheckListAndPath(list, path);
        List<string> lines = new List<string>();
        lines.Add(CsvHeader);
        foreach(Satellite s in list)
        {
            lines.Add(s.ToString());
        }
        try
        {
            File.WriteAllLines(path, lines);
        }
        catch (Exception ex)
        {
            throw new Exception($"Помилка при збереженні у файл: {ex.Message}");
        }
    }

    public static int LoadFromCsv(List<Satellite> list, string path, int max)
    {
        CheckListAndPath(list, path);

        if (!File.Exists(path))
            throw new Exception("Файл не знайдено");
        
        string[] lines;

        try
        {
            lines = File.ReadAllLines(path);
        }
        catch (Exception ex)
        {
            throw new Exception($"Помилка при читанні файлу: {ex.Message}");
        }

        if (lines.Length == 0 || lines[0].Trim() != CsvHeader)
            throw new Exception("Файл не містить коректного заголовка CSV");

        int loaded = 0;
        for(int i = 1; i < lines.Length; i++)
        {
            if (list.Count >= max)
                break;
            if (Satellite.TryParse(lines[i], out Satellite s))
            {
                AddSatelliteToList(list, s, max);
                loaded++;
            }
        }
        return loaded;
    }

    public static void SaveToJson(List<Satellite> list, string path)
    {
        CheckListAndPath(list,path);

        JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
        string json = JsonSerializer.Serialize(list, options);

        try
        {
            File.WriteAllText(path, json);
        }
        catch (Exception ex) 
        {
            throw new Exception("Не вдалося записати файл: " + ex.Message);
        }
    }

    public static int LoadFromJson(List<Satellite> list, string path, int max)
    {
        CheckListAndPath(list, path);
        if (!File.Exists(path))
            throw new Exception("Файл не знайдено");
        List<Satellite> loadedList;

        try
        {
            string json = File.ReadAllText(path);
            loadedList = JsonSerializer.Deserialize<List<Satellite>>(json);
        }
        catch (Exception ex)
        {
            throw new Exception("Файл пошкоджено або неправильний формат" + ex.Message);
        }

        if (loadedList == null)
            throw new Exception("Файл порожній");

        int loaded = 0;
        foreach(Satellite s in loadedList)
        {
            if (s == null)
                continue;
            if (list.Count >= max)
                break;
            AddSatelliteToList(list, s, max);
            loaded++;
        }
        return loaded;
    }

    public static int ClearList(List<Satellite> list)
    {
        if (list == null)
            throw new Exception("Список не існує");
        int count = list.Count;
        list.Clear();
        return count;
    }

    static string GetProjectFolder([CallerFilePath] string sourceFile = "")
    {
        return Path.GetDirectoryName(sourceFile);
    }
    
    static void SaveCollection()
    {
        if (arr.Count == 0) { Console.WriteLine("Список порожній, зберігати нічого"); return; }
        Console.WriteLine("1 - Зберегти у файл *.csv\n2 - Зберегти у файл *.json");
        Console.Write("Виберіть пункт меню: ");
        string opt = Console.ReadLine();

        if (opt != "1" && opt != "2")
        {
            Console.WriteLine("Некоректний вибір");
            return;
        }

        string fileName = opt == "1" ? "satellites.csv" : "satellites.json";
        string defaultPath = Path.Combine(GetProjectFolder(), fileName);
        Console.Write($"Введіть шлях до файлу (Enter - {defaultPath}): ");
        string path = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(path))
            path = defaultPath;

        try
        {
            if (opt == "1")
                SaveToCsv(arr, path);
            else
                SaveToJson(arr, path);
            Console.WriteLine($"Збережено об'єктів: {arr.Count}. Файл: {path}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void LoadCollection()
    {
        if (arr.Count >= maxCount)
        {
            Console.WriteLine($"Досягнуто максимальної кількості супутників ({maxCount}). Зчитування неможливе.\n");
            return;
        }

        Console.WriteLine("1 - Зчитати з файлу *.csv\n2 - Зчитати з файлу *.json");
        Console.Write("Виберіть пункт меню: ");
        string opt = Console.ReadLine();

        if (opt != "1" && opt != "2")
        {
            Console.WriteLine("Некоректний вибір");
            return;
        }

        string fileName = opt == "1" ? "satellites.csv" : "satellites.json";
        string defaultPath = Path.Combine(GetProjectFolder(), fileName);
        Console.Write($"Введіть шлях до файлу (Enter - {defaultPath}): ");
        string path = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(path))
            path = defaultPath;

        try
        {
            int loaded;
            if (opt == "1")
                loaded = LoadFromCsv(arr, path, maxCount);
            else
                loaded = LoadFromJson(arr, path, maxCount);
            Console.WriteLine($"Десеріалізовано елементів колекції: {loaded}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static void ClearCollection()
    {
        int removed = ClearList(arr);
        Console.WriteLine($"Колекцію очищено. Видалено об'єктів: {removed}\n");
    }

}