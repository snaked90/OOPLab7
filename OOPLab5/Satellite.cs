using System.Globalization;
using System.Text.Json.Serialization;

namespace OOPLab7;

public class Satellite
{
    public static int Count { get; private set; }
    public static int ExpCount { get; private set; }
    public static double UsedFuel { get; private set; }
    
    private string _name;
    private SType _type;
    private double _orbitAltitude; 
    private DateTime _launchDate;
    
    [JsonConstructor]
    public Satellite(string name, SType type, double orbitAltitude, DateTime launchDate)
    {
        Name = name;
        Type = type;
        OrbitAltitude = orbitAltitude;
        LaunchDate = launchDate;
        Count++;
    }
    
    public Satellite()
    {
        Count++;
        ExpCount++;
    }

    public Satellite(string name, SType type) : this(name, type, 500, new DateTime(2001, 02, 03)) { }

    [JsonPropertyName("satellite_name")]
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            if (value == null || value.Length < 3 || value.Length > 15)
                throw new Exception("Назва має містити від 3 до 15 символів");
            _name = value;
        }
    }

    [JsonPropertyName("satellite_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public SType Type
    {
        get
        {
            return _type;
        }
        set
        {
            if (!Enum.IsDefined(typeof(SType), value))
                throw new Exception("Вказано неіснуючий тип супутника");
            _type = value;
        }
    }

    [JsonPropertyName("altitude_km")]
    public double OrbitAltitude
    {
        get
        {
            return _orbitAltitude;
        }
        set
        {
            if (value < 160 || value > 40000)
                throw new Exception("Висота орбіти має бути в межах 160 - 40000 км");
            _orbitAltitude = value;
        }
    }

    [JsonPropertyName("launch_date")]
    public DateTime LaunchDate
    {
        get
        {
            return _launchDate;
        }
        set
        {
            DateTime minDate = new DateTime(1957, 10, 4);
            if (value < minDate || value > DateTime.Now)
                throw new Exception($"Дата запуску має бути в межах {minDate:dd.MM.yyyy} - {DateTime.Now:dd.MM.yyyy}");
            _launchDate = value;
        }
    }

    [JsonIgnore]
    public bool IsActive { get; private set; } = true;
    [JsonIgnore]
    public double FuelLevel { get; private set; } = 100;
    [JsonIgnore]
    public int TransmissionPower { get; set; } = 20;
    [JsonIgnore]
    public string OrbitCategory
    {
        get
        {
            if (OrbitAltitude < 2000)
                return "LEO (низька)";
            if (OrbitAltitude < 35786)
                return "MEO (середня)";
            return "GEO (геостаціонарна)";
        }
    }

    public static string CalculateFuel(double one, double two)
    {
        if (one > two)
            throw new Exception("Перша точка має бути менше ніж друга");
        double distance = two - one;
        double fuel = distance * 0.1;
        return $"Відстань між точками {distance}, для подолання такої відстані потрібно {fuel} відсотків пальнього";
    }

    public override string ToString()
    {
        return $"{Name}; {Type}; {OrbitAltitude.ToString(CultureInfo.InvariantCulture)}; {LaunchDate:dd.MM.yyyy}";
    }

    public static Satellite Parse(string s)
    {
        if (s == null)
            throw new Exception("Рядок пустий");
        string[] parts = s.Split("; ");

        if (parts.Length != 4)
        {
            throw new Exception($"Очікується 4 частини через '; '. Отримано {parts.Length}");
        }
        
        string name = parts[0];
        
        if (!Enum.TryParse<SType>(parts[1], true, out SType type))
            throw new Exception($"Некоректний тип супутника: '{parts[1]}'");
        
        if (!double.TryParse(parts[2], System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double altitude))
            throw new Exception($"Некоректне значення висоти орбіти: '{parts[2]}'");
        
        if (!DateTime.TryParseExact(parts[3], "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None, out DateTime launchDate))
            throw new Exception($"Некоректна дата запуску: '{parts[3]}' (очікується формат dd.MM.yyyy)");

        return new Satellite(name, type, altitude, launchDate);
    }

    public static bool TryParse(string s, out Satellite obj)
    {
        try
        {
            obj = Parse(s);
            return true;
        }
        catch
        {
            obj = null;
            return false;
        }
    }
    
    public string GetData()
    {
        EnsureActive();
        return $"Супутник '{Name}' успішно передав дані з висоти {OrbitAltitude} км " +
               $"(потужність передавача {TransmissionPower} Вт)";
    }

    public string SwitchPower()
    {
        EnsureConnection();
        IsActive = !IsActive;
        return $"Живлення супутника '{Name}' переключено на {(IsActive ? "On" : "Off")}";
    }

    public string AdjustAltitude(double delta)
    {
        EnsureActive();

        double requiredFuel = CalculateRequiredFuel(delta);
        EnsureEnoughFuel(requiredFuel);

        OrbitAltitude += delta;
        FuelLevel -= requiredFuel;
        UsedFuel += requiredFuel;

        return $"Нова висота супутника '{Name}': {OrbitAltitude} км ({OrbitCategory})";
    }

    public string AdjustAltitude()
    {
        AdjustAltitude(100);
        return $"Нова висота супутника '{Name}': {OrbitAltitude} км ({OrbitCategory})";
    }

    public string AdjustAltitude(double delta, bool isEmergency)
    {
        EnsureActive();
        
        double multiplier = isEmergency ? 1.3 : 1.0;
        double requiredFuel = CalculateRequiredFuel(delta) * multiplier;
        
        EnsureEnoughFuel(requiredFuel);
        
        OrbitAltitude += delta;
        FuelLevel -= requiredFuel;
        UsedFuel += requiredFuel;

        string mode = isEmergency ? "Екстрений" : "Звичайний";
        return $"Нова висота супутника '{Name}': {OrbitAltitude} км ({OrbitCategory}). Режим: {mode}";
    }

    public string LeaveTimePassing(int days)
    {
        EnsureActive();

        if (OrbitAltitude >= 600)
            return $"Пройшло днів: {days}. Висота не змінилася (опір атмосфери відсутній)";

        double totalDrop = CalculateDailyDrop() * days;
        double newAltitude = OrbitAltitude - totalDrop;

        if (newAltitude < 160)
        {
            IsActive = false;
            _orbitAltitude = 0;
            return $"Супутник '{Name}' зійшов з орбіти та згорів у щільних шарах атмосфери";
        }

        _orbitAltitude = newAltitude;
        return $"Пройшло днів: {days}. Супутник '{Name}' втратив {totalDrop} км висоти. " +
               $"Поточна висота: {OrbitAltitude} км";
    }

    private void EnsureActive()
    {
        if (!IsActive)
            throw new Exception($"Супутник '{Name}' неактивний");
    }

    private void EnsureConnection()
    {
        if (OrbitAltitude < 160)
            throw new Exception($"Немає зв'язку із супутником '{Name}'");
    }

    private double CalculateRequiredFuel(double delta)
    {
        return Math.Abs(delta) * 0.1;
    }

    private void EnsureEnoughFuel(double required)
    {
        if (FuelLevel < required)
            throw new Exception(
                $"Недостатньо палива. Потрібно {required}, є {FuelLevel}");
    }

    private double CalculateDailyDrop()
    {
        if (OrbitAltitude < 300)
            return 0.8;
        return 0.2;
    }
}