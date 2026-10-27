using OOPLab7;

namespace OOPLabTests;

[TestClass]
public class ProgramTests
{
    private List<Satellite> _list = null;

    private static readonly DateTime ValidDate = new DateTime(2020, 3, 15);

    private string _csvPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".csv");
    private string _jsonPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

    [TestInitialize]
    public void Setup()
    {
        _list = new List<Satellite>
        {
            new Satellite("Alpha", SType.Weather, 500, ValidDate),
            new Satellite("Beta", SType.Military, 800, ValidDate),
            new Satellite("Gamma", SType.Weather, 1200, ValidDate)
        };
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_csvPath))
            File.Delete(_csvPath);
        if (File.Exists(_jsonPath))
            File.Delete(_jsonPath);

        _list.Clear();
        _list = null;
    }

    [TestMethod]
    public void AddSatelliteToList_ValidSatellite_AddsToEnd()
    {
        Satellite sat = new Satellite("Delta", SType.Scientific, 400, ValidDate);

        Program.AddSatelliteToList(_list, sat, 10);

        Assert.AreEqual(4, _list.Count);
        Assert.AreSame(sat, _list[3]);
    }

    [TestMethod]
    public void AddSatelliteToList_ListIsFull_Throws()
    {
        Satellite sat = new Satellite("Delta", SType.Scientific, 400, ValidDate);

        Exception ex = Assert.ThrowsExactly<Exception>(() => Program.AddSatelliteToList(_list, sat, 3));
        StringAssert.Contains(ex.Message, "максимальної");
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void AddSatelliteToList_NullSatellite_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.AddSatelliteToList(_list, null, 10));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void AddSatelliteToList_NullList_Throws()
    {
        Satellite sat = new Satellite("Delta", SType.Scientific, 400, ValidDate);

        Assert.ThrowsExactly<Exception>(() => Program.AddSatelliteToList(null, sat, 10));
    }

    [TestMethod]
    public void CreateDefaultSatellite_ReturnsExperimentalSatellite()
    {
        Satellite sat = Program.CreateDefaultSatellite();

        StringAssert.StartsWith(sat.Name, "Experimental");
        Assert.AreEqual(SType.Communication, sat.Type);
        Assert.AreEqual(400.0, sat.OrbitAltitude, 0.0001);
        Assert.IsTrue(sat.IsActive);
    }

    [TestMethod]
    public void FindByType_ExistingType_ReturnsOnlyThatType()
    {
        List<Satellite> result = Program.FindByType(_list, SType.Weather);

        Assert.AreEqual(2, result.Count);
        foreach (Satellite s in result)
            Assert.AreEqual(SType.Weather, s.Type);
    }

    [TestMethod]
    public void FindByType_NoMatches_ReturnsEmptyList()
    {
        List<Satellite> result = Program.FindByType(_list, SType.Scientific);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(99)]
    public void FindByType_UndefinedType_Throws(int value)
    {
        Assert.ThrowsExactly<Exception>(() => Program.FindByType(_list, (SType)value));
    }

    [TestMethod]
    [DataRow("alpha", 1)]
    [DataRow("ALPHA", 1)]
    [DataRow("lph", 1)]
    [DataRow("a", 3)]
    [DataRow("zzz", 0)]
    public void FindByName_Query_ReturnsExpectedCount(string query, int expectedCount)
    {
        List<Satellite> result = Program.FindByName(_list, query);

        Assert.AreEqual(expectedCount, result.Count);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void FindByName_EmptyQuery_Throws(string query)
    {
        Assert.ThrowsExactly<Exception>(() => Program.FindByName(_list, query));
    }

    [TestMethod]
    public void RemoveByIndex_ValidPosition_RemovesAndReturnsName()
    {
        string removed = Program.RemoveByIndex(_list, 2);

        Assert.AreEqual("Beta", removed);
        Assert.AreEqual(2, _list.Count);
        Assert.AreEqual("Gamma", _list[1].Name);
    }

    [TestMethod]
    [DataRow(1)]
    [DataRow(3)]
    public void RemoveByIndex_FirstAndLastPosition_Works(int position)
    {
        Program.RemoveByIndex(_list, position);

        Assert.AreEqual(2, _list.Count);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(4)]
    [DataRow(-1)]
    public void RemoveByIndex_InvalidPosition_ThrowsAndKeepsList(int position)
    {
        Assert.ThrowsExactly<Exception>(() => Program.RemoveByIndex(_list, position));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void RemoveByType_ExistingType_RemovesAllAndReturnsCount()
    {
        int removed = Program.RemoveByType(_list, SType.Weather);

        Assert.AreEqual(2, removed);
        Assert.AreEqual(1, _list.Count);
        Assert.AreEqual("Beta", _list[0].Name);
    }

    [TestMethod]
    public void RemoveByType_NoMatches_ReturnsZeroAndKeepsList()
    {
        int removed = Program.RemoveByType(_list, SType.Scientific);

        Assert.AreEqual(0, removed);
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(99)]
    public void RemoveByType_UndefinedType_ThrowsAndKeepsList(int value)
    {
        Assert.ThrowsExactly<Exception>(() => Program.RemoveByType(_list, (SType)value));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void FormatTable_EmptyList_ReturnsNotFoundMessage()
    {
        string result = Program.FormatTable(new List<Satellite>());

        StringAssert.Contains(result, "Жодного супутника");
    }

    [TestMethod]
    public void FormatTable_NullList_ReturnsNotFoundMessage()
    {
        string result = Program.FormatTable(null);

        StringAssert.Contains(result, "Жодного супутника");
    }

    [TestMethod]
    public void FormatTable_NonEmptyList_ContainsHeaderAndAllNames()
    {
        string result = Program.FormatTable(_list);

        StringAssert.Contains(result, "Назва");
        StringAssert.Contains(result, "Alpha");
        StringAssert.Contains(result, "Beta");
        StringAssert.Contains(result, "Gamma");
    }

    [TestMethod]
    public void FormatStatistics_ContainsAllThreeCounters()
    {
        string result = Program.FormatStatistics();

        StringAssert.Contains(result, "Всього супутників");
        StringAssert.Contains(result, "експерементальних");
        StringAssert.Contains(result, "витрачено палива");
    }

    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    private static string Json(string name, string type, string altitude, string date)
    {
        return "[{\"satellite_name\":\"" + name + "\",\"satellite_type\":\"" + type + "\"," +
               "\"altitude_km\":" + altitude + ",\"launch_date\":\"" + date + "\"}]";
    }

    // SaveToCsv

    [TestMethod]
    public void SaveToCsv_ValidList_WritesHeaderAndOneLinePerSatellite()
    {
        Program.SaveToCsv(_list, _csvPath);

        string[] lines = File.ReadAllLines(_csvPath);
        Assert.AreEqual(4, lines.Length);
        Assert.AreEqual(Program.CsvHeader, lines[0]);
        Assert.AreEqual(_list[0].ToString(), lines[1]);
    }

    [TestMethod]
    public void SaveToCsv_EmptyList_WritesOnlyHeader()
    {
        Program.SaveToCsv(new List<Satellite>(), _csvPath);

        string[] lines = File.ReadAllLines(_csvPath);
        Assert.AreEqual(1, lines.Length);
        Assert.AreEqual(Program.CsvHeader, lines[0]);
    }

    [TestMethod]
    public void SaveToCsv_NullList_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.SaveToCsv(null, _csvPath));
    }

    [TestMethod]
    [DataRow((string)null)]
    [DataRow("")]
    [DataRow("   ")]
    public void SaveToCsv_EmptyPath_Throws(string path)
    {
        Assert.ThrowsExactly<Exception>(() => Program.SaveToCsv(_list, path));
    }

    // LoadFromCsv

    [TestMethod]
    public void LoadFromCsv_AfterSave_RestoresAllSatellites()
    {
        Program.SaveToCsv(_list, _csvPath);
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromCsv(loadedList, _csvPath, 10);

        Assert.AreEqual(3, loaded);
        Assert.AreEqual(3, loadedList.Count);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(_list[i].Name, loadedList[i].Name);
            Assert.AreEqual(_list[i].Type, loadedList[i].Type);
            Assert.AreEqual(_list[i].OrbitAltitude, loadedList[i].OrbitAltitude, 0.0001);
            Assert.AreEqual(_list[i].LaunchDate.Date, loadedList[i].LaunchDate.Date);
        }
    }

    [TestMethod]
    public void LoadFromCsv_ListAlreadyHasItems_AddsToExisting()
    {
        File.WriteAllLines(_csvPath, new string[]
        {
            Program.CsvHeader,
            "Delta; Scientific; 700; 01.01.2021"
        });

        int loaded = Program.LoadFromCsv(_list, _csvPath, 10);

        Assert.AreEqual(1, loaded);
        Assert.AreEqual(4, _list.Count);
        Assert.AreEqual("Alpha", _list[0].Name);
        Assert.AreEqual("Delta", _list[3].Name);
    }

    [TestMethod]
    public void LoadFromCsv_MixedValidAndBrokenRows_LoadsOnlyValid()
    {
        File.WriteAllLines(_csvPath, new string[]
        {
            Program.CsvHeader,
            "Delta; Scientific; 700; 01.01.2021",
            "Bad; Weather; abc; 15.03.2020",
            "Xx; Weather; 500; 15.03.2020",
            "Beta; Unknown; 500; 15.03.2020",
            "Gamma; Military; 800;",
            "; Weather; 500; 15.03.2020",
            "Omega; Military; 800; 99.99.2020",
            "",
            "Epsilon; Military; 900; 02.02.2022"
        });
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromCsv(loadedList, _csvPath, 10);

        Assert.AreEqual(2, loaded);
        Assert.AreEqual(2, loadedList.Count);
        Assert.AreEqual("Delta", loadedList[0].Name);
        Assert.AreEqual("Epsilon", loadedList[1].Name);
    }

    [TestMethod]
    [DataRow("Bad; Weather; abc; 15.03.2020")]
    [DataRow("Alpha; Weather; 5; 15.03.2020")]
    [DataRow("Alpha; Weather; 99999; 15.03.2020")]
    [DataRow("Alpha; Unknown; 500; 15.03.2020")]
    [DataRow("Alpha; 99; 500; 15.03.2020")]
    [DataRow("Alpha; Weather; 500; 2999-01-01")]
    [DataRow("Alpha; Weather; 500; 01.01.1900")]
    [DataRow("Alpha; Weather; 500")]
    [DataRow("Alpha")]
    [DataRow("якийсь довільний текст")]
    public void LoadFromCsv_SingleBrokenRow_IsSkipped(string row)
    {
        File.WriteAllLines(_csvPath, new string[] { Program.CsvHeader, row });
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromCsv(loadedList, _csvPath, 10);

        Assert.AreEqual(0, loaded);
        Assert.AreEqual(0, loadedList.Count);
    }

    [TestMethod]
    public void LoadFromCsv_OnlyHeader_ReturnsZero()
    {
        File.WriteAllLines(_csvPath, new string[] { Program.CsvHeader });

        int loaded = Program.LoadFromCsv(_list, _csvPath, 10);

        Assert.AreEqual(0, loaded);
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void LoadFromCsv_NoHeader_Throws()
    {
        File.WriteAllLines(_csvPath, new string[] { "Delta; Scientific; 700; 01.01.2021" });

        Exception ex = Assert.ThrowsExactly<Exception>(() => Program.LoadFromCsv(_list, _csvPath, 10));
        StringAssert.Contains(ex.Message, "заголовка");
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void LoadFromCsv_EmptyFile_Throws()
    {
        File.WriteAllText(_csvPath, "");

        Assert.ThrowsExactly<Exception>(() => Program.LoadFromCsv(_list, _csvPath, 10));
    }

    [TestMethod]
    public void LoadFromCsv_FileDoesNotExist_Throws()
    {
        Exception ex = Assert.ThrowsExactly<Exception>(() => Program.LoadFromCsv(_list, _csvPath, 10));
        StringAssert.Contains(ex.Message, "не знайдено");
    }

    [TestMethod]
    public void LoadFromCsv_LimitReached_StopsAtMax()
    {
        Program.SaveToCsv(_list, _csvPath);
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromCsv(loadedList, _csvPath, 2);

        Assert.AreEqual(2, loaded);
        Assert.AreEqual(2, loadedList.Count);
    }

    [TestMethod]
    public void LoadFromCsv_NullListOrEmptyPath_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.LoadFromCsv(null, _csvPath, 10));
        Assert.ThrowsExactly<Exception>(() => Program.LoadFromCsv(_list, "", 10));
    }

    // SaveToJson

    [TestMethod]
    public void SaveToJson_ValidList_UsesRenamedPropertiesOnly()
    {
        Program.SaveToJson(_list, _jsonPath);

        string json = File.ReadAllText(_jsonPath);
        StringAssert.Contains(json, "satellite_name");
        StringAssert.Contains(json, "satellite_type");
        StringAssert.Contains(json, "altitude_km");
        StringAssert.Contains(json, "launch_date");
        Assert.IsFalse(json.Contains("\"Name\""));
        Assert.IsFalse(json.Contains("\"OrbitAltitude\""));
        StringAssert.Contains(json, "Weather");
        Assert.IsFalse(json.Contains("FuelLevel"));
        Assert.IsFalse(json.Contains("IsActive"));
        Assert.IsFalse(json.Contains("TransmissionPower"));
        Assert.IsFalse(json.Contains("OrbitCategory"));
    }

    [TestMethod]
    public void SaveToJson_EmptyList_WritesEmptyArray()
    {
        Program.SaveToJson(new List<Satellite>(), _jsonPath);

        string json = File.ReadAllText(_jsonPath).Trim();
        Assert.AreEqual("[]", json);
    }

    [TestMethod]
    public void SaveToJson_NullListOrEmptyPath_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.SaveToJson(null, _jsonPath));
        Assert.ThrowsExactly<Exception>(() => Program.SaveToJson(_list, "  "));
    }

    // LoadFromJson

    [TestMethod]
    public void LoadFromJson_AfterSave_RestoresAllSatellites()
    {
        Program.SaveToJson(_list, _jsonPath);
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromJson(loadedList, _jsonPath, 10);

        Assert.AreEqual(3, loaded);
        Assert.AreEqual(3, loadedList.Count);
        for (int i = 0; i < 3; i++)
        {
            Assert.AreEqual(_list[i].Name, loadedList[i].Name);
            Assert.AreEqual(_list[i].Type, loadedList[i].Type);
            Assert.AreEqual(_list[i].OrbitAltitude, loadedList[i].OrbitAltitude, 0.0001);
            Assert.AreEqual(_list[i].LaunchDate, loadedList[i].LaunchDate);
        }
    }

    [TestMethod]
    public void LoadFromJson_ListAlreadyHasItems_AddsToExisting()
    {
        File.WriteAllText(_jsonPath, Json("Delta", "Scientific", "700", "2021-01-01T00:00:00"));

        int loaded = Program.LoadFromJson(_list, _jsonPath, 10);

        Assert.AreEqual(1, loaded);
        Assert.AreEqual(4, _list.Count);
        Assert.AreEqual("Delta", _list[3].Name);
    }

    [TestMethod]
    public void LoadFromJson_RestoredSatellite_HasDefaultStateOfNonSavedProperties()
    {
        _list[0].AdjustAltitude(100);
        Program.SaveToJson(_list, _jsonPath);
        List<Satellite> loadedList = new List<Satellite>();

        Program.LoadFromJson(loadedList, _jsonPath, 10);

        Assert.AreEqual(100.0, loadedList[0].FuelLevel, 0.0001);
        Assert.IsTrue(loadedList[0].IsActive);
    }

    [TestMethod]
    [DataRow("Xx", "Weather", "500", "2020-03-15T00:00:00")]
    [DataRow("Alpha", "Weather", "5", "2020-03-15T00:00:00")]
    [DataRow("Alpha", "Weather", "99999", "2020-03-15T00:00:00")]
    [DataRow("Alpha", "Unknown", "500", "2020-03-15T00:00:00")]
    [DataRow("Alpha", "Weather", "500", "not a date")]
    [DataRow("Alpha", "Weather", "500", "2999-01-01T00:00:00")]
    [DataRow("Alpha", "Weather", "\"abc\"", "2020-03-15T00:00:00")]
    public void LoadFromJson_IncorrectValue_ThrowsAndKeepsList(string name, string type, string altitude, string date)
    {
        File.WriteAllText(_jsonPath, Json(name, type, altitude, date));

        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, _jsonPath, 10));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("{ це не json")]
    [DataRow("[{\"satellite_name\":")]
    [DataRow("{}")]
    [DataRow("12345")]
    public void LoadFromJson_BrokenFile_ThrowsAndKeepsList(string content)
    {
        File.WriteAllText(_jsonPath, content);

        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, _jsonPath, 10));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void LoadFromJson_NullContent_Throws()
    {
        File.WriteAllText(_jsonPath, "null");

        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, _jsonPath, 10));
    }

    [TestMethod]
    public void LoadFromJson_MissingProperty_ThrowsAndKeepsList()
    {
        File.WriteAllText(_jsonPath,
            "[{\"satellite_name\":\"Alpha\",\"satellite_type\":\"Weather\",\"altitude_km\":500}]");

        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, _jsonPath, 10));
        Assert.AreEqual(3, _list.Count);
    }

    [TestMethod]
    public void LoadFromJson_FileDoesNotExist_Throws()
    {
        Exception ex = Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, _jsonPath, 10));
        StringAssert.Contains(ex.Message, "не знайдено");
    }

    [TestMethod]
    public void LoadFromJson_LimitReached_StopsAtMax()
    {
        Program.SaveToJson(_list, _jsonPath);
        List<Satellite> loadedList = new List<Satellite>();

        int loaded = Program.LoadFromJson(loadedList, _jsonPath, 2);

        Assert.AreEqual(2, loaded);
        Assert.AreEqual(2, loadedList.Count);
    }

    [TestMethod]
    public void LoadFromJson_NullListOrEmptyPath_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(null, _jsonPath, 10));
        Assert.ThrowsExactly<Exception>(() => Program.LoadFromJson(_list, null, 10));
    }

    // ClearList

    [TestMethod]
    public void ClearList_NonEmptyList_EmptiesListAndReturnsCount()
    {
        int removed = Program.ClearList(_list);

        Assert.AreEqual(3, removed);
        Assert.AreEqual(0, _list.Count);
    }

    [TestMethod]
    public void ClearList_EmptyList_ReturnsZero()
    {
        int removed = Program.ClearList(new List<Satellite>());

        Assert.AreEqual(0, removed);
    }

    [TestMethod]
    public void ClearList_NullList_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Program.ClearList(null));
    }
}