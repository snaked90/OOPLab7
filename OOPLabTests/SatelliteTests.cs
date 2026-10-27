using OOPLab7;

namespace OOPLabTests;

[TestClass]
public class SatelliteTests
{
    private Satellite _sat = null;

    private static readonly DateTime ValidDate = new DateTime(2020, 3, 15);

    [TestInitialize]
    public void Setup()
    {
        _sat = new Satellite("Alpha", SType.Weather, 500, ValidDate);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _sat = null;
    }

    [TestMethod]
    public void ConstructorShort_ValidParameters_CreatesSatellite()
    {
        Satellite sat = new Satellite("Delta", SType.Scientific);

        Assert.AreEqual("Delta", sat.Name);
        Assert.AreEqual(SType.Scientific, sat.Type);
        Assert.AreEqual(500.0, sat.OrbitAltitude, 0.0001);
        Assert.AreEqual(new DateTime(2001, 2, 3), sat.LaunchDate);
    }

    [TestMethod]
    public void LauchDateSetter_FirstSatteliteDay_IsAccepted()
    {
        DateTime firstDay = new DateTime(1957, 10, 4);

        _sat.LaunchDate = firstDay;
        Assert.AreEqual(firstDay, _sat.LaunchDate);
    }

    [TestMethod]
    public void LaunchDateSetter_BeforeFirstSatellite_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => _sat.LaunchDate = new DateTime(1957, 10, 3));
    }

    [TestMethod]
    public void LaunchDateSetter_FutureDate_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => _sat.LaunchDate = DateTime.Now.AddDays(1));
    }

    [TestMethod]
    [DataRow(160.0, "LEO")]
    [DataRow(2000.0, "MEO")]
    [DataRow(35786.0, "GEO")]
    public void OrbitCategory_ByAltitude_ReturnsExpectedCategory(double altitude, string expectedPrefix)
    {
        _sat.OrbitAltitude = altitude;

        string category = _sat.OrbitCategory;

        StringAssert.StartsWith(category, expectedPrefix);
    }

    // set Name

    [TestMethod]
    [DataRow("abc")]
    [DataRow("123456789012345")]
    [DataRow("Alpha1")]
    public void NameSetter_ValidLength_SetsValue(string name)
    {
        _sat.Name = name;

        Assert.AreEqual(name, _sat.Name);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("ab")]
    [DataRow("1234567890123456")]
    public void NameSetter_InvalidLength_Throws(string badName)
    {
        Assert.ThrowsExactly<Exception>(() => _sat.Name = badName);
    }

    // set Type

    [TestMethod]
    [DataRow(1)]
    [DataRow(4)]
    public void TypeSetter_DefinedValue_SetsValue(int value)
    {
        _sat.Type = (SType)value;

        Assert.AreEqual((SType)value, _sat.Type);
    }

    [TestMethod]
    [DataRow(0)]
    [DataRow(5)]
    [DataRow(99)]
    public void TypeSetter_UndefinedValue_Throws(int value)
    {
        Assert.ThrowsExactly<Exception>(() => _sat.Type = (SType)value);
    }

    // set OrbitAltitude

    [TestMethod]
    [DataRow(160.0)]
    [DataRow(500.0)]
    [DataRow(40000.0)]
    public void OrbitAltitudeSetter_InRange_SetsValue(double value)
    {
        _sat.OrbitAltitude = value;

        Assert.AreEqual(value, _sat.OrbitAltitude, 0.0001);
    }

    [TestMethod]
    [DataRow(159.99)]
    [DataRow(40000.01)]
    [DataRow(-50.0)]
    public void OrbitAltitudeSetter_OutOfRange_Throws(double value)
    {
        Assert.ThrowsExactly<Exception>(() => _sat.OrbitAltitude = value);
        Assert.AreEqual(500.0, _sat.OrbitAltitude, 0.0001);
    }

    // GetData, SwitchPower

    [TestMethod]
    public void GetData_ActiveSatellite_ReturnsInfoWithNameAndPower()
    {
        string result = _sat.GetData();

        StringAssert.Contains(result, "Alpha");
        StringAssert.Contains(result, "500");
        StringAssert.Contains(result, "20");
    }

    [TestMethod]
    public void GetData_InactiveSatellite_Throws()
    {
        _sat.SwitchPower();

        Assert.ThrowsExactly<Exception>(() => _sat.GetData());
    }

    [TestMethod]
    public void SwitchPower_ActiveSatellite_BecomesInactive()
    {
        _sat.SwitchPower();

        Assert.IsFalse(_sat.IsActive);
    }

    [TestMethod]
    public void SwitchPower_CalledTwice_ReturnsToActive()
    {
        _sat.SwitchPower();
        _sat.SwitchPower();

        Assert.IsTrue(_sat.IsActive);
    }

    [TestMethod]
    public void SwitchPower_NoConnection_Throws()
    {
        Satellite lost = new Satellite();

        Assert.ThrowsExactly<Exception>(() => lost.SwitchPower());
    }

    // AdjustAltitude

    [TestMethod]
    public void AdjustAltitude_PositiveDelta_RaisesAltitudeAndSpendsFuel()
    {
        _sat.AdjustAltitude(100);

        Assert.AreEqual(600.0, _sat.OrbitAltitude, 0.0001);
        Assert.AreEqual(90.0, _sat.FuelLevel, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_NoArguments_RaisesBy100()
    {
        _sat.AdjustAltitude();

        Assert.AreEqual(600.0, _sat.OrbitAltitude, 0.0001);
        Assert.AreEqual(90.0, _sat.FuelLevel, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_EmergencyMode_SpendsThirtyPercentMoreFuel()
    {
        _sat.AdjustAltitude(100, true);

        Assert.AreEqual(87.0, _sat.FuelLevel, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_NotEnoughFuel_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => _sat.AdjustAltitude(1001));

        Assert.AreEqual(500.0, _sat.OrbitAltitude, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_InactiveSatellite_Throws()
    {
        _sat.SwitchPower();

        Assert.ThrowsExactly<Exception>(() => _sat.AdjustAltitude(10));
    }

    [TestMethod]
    public void AdjustAltitude_AboveMaxAltitude_ThrowsAndKeepsState()
    {
        Satellite sat = new Satellite("Highflyer", SType.Scientific, 39900, ValidDate);

        Assert.ThrowsExactly<Exception>(() => sat.AdjustAltitude(500));
        Assert.AreEqual(39900.0, sat.OrbitAltitude, 0.0001);
        Assert.AreEqual(100.0, sat.FuelLevel, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_BelowMinAltitude_ThrowsAndKeepsState()
    {
        Satellite sat = new Satellite("Lowflyer", SType.Scientific, 200, ValidDate);

        Assert.ThrowsExactly<Exception>(() => sat.AdjustAltitude(-100));
        Assert.AreEqual(200.0, sat.OrbitAltitude, 0.0001);
    }

    [TestMethod]
    public void AdjustAltitude_NormalModeFlag_SpendsRegularFuel()
    {
        _sat.AdjustAltitude(100, false);

        Assert.AreEqual(90.0, _sat.FuelLevel, 0.0001);
    }

    // LeaveTimePassing

    [TestMethod]
    [DataRow(600.0)]
    [DataRow(1000.0)]
    public void LeaveTimePassing_AtOrAbove600_AltitudeUnchanged(double altitude)
    {
        _sat.OrbitAltitude = altitude;

        string result = _sat.LeaveTimePassing(100);

        Assert.AreEqual(altitude, _sat.OrbitAltitude, 0.0001);
        StringAssert.Contains(result, "не змінилася");
    }

    [TestMethod]
    public void LeaveTimePassing_MediumOrbit_DropsPoint2PerDay()
    {
        _sat.LeaveTimePassing(100);

        Assert.AreEqual(480.0, _sat.OrbitAltitude, 0.0001);
    }

    [TestMethod]
    public void LeaveTimePassing_LowOrbit_DropsPoint8PerDay()
    {
        _sat.OrbitAltitude = 250;

        _sat.LeaveTimePassing(10);

        Assert.AreEqual(242.0, _sat.OrbitAltitude, 0.0001);
    }

    [TestMethod]
    public void LeaveTimePassing_FallsBelow160_SatelliteBurnsUp()
    {
        _sat.OrbitAltitude = 170;

        string result = _sat.LeaveTimePassing(20);

        Assert.IsFalse(_sat.IsActive);
        Assert.AreEqual(0.0, _sat.OrbitAltitude, 0.0001);
        StringAssert.Contains(result, "згорів");
    }

    [TestMethod]
    public void LeaveTimePassing_InactiveSatellite_Throws()
    {
        _sat.SwitchPower();

        Assert.ThrowsExactly<Exception>(() => _sat.LeaveTimePassing(10));
    }

    // CalculateFuel

    [TestMethod]
    public void CalculateFuel_ValidPoints_ReturnsDistanceAndFuel()
    {
        string result = Satellite.CalculateFuel(100, 200);

        StringAssert.Contains(result, "100");
        StringAssert.Contains(result, "10 відсотків");
    }

    [TestMethod]
    public void CalculateFuel_FirstPointHigherThanSecond_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Satellite.CalculateFuel(200, 100));
    }

    // ToString, Parse, TryParse

    [TestMethod]
    public void ToString_ValidSatellite_ReturnsExpectedFormat()
    {
        string result = _sat.ToString();

        Assert.AreEqual("Alpha; Weather; 500; 15.03.2020", result);
    }

    [TestMethod]
    public void Parse_ValidString_CreatesSatellite()
    {
        Satellite sat = Satellite.Parse("Alpha; Weather; 500; 15.03.2020");

        Assert.AreEqual("Alpha", sat.Name);
        Assert.AreEqual(SType.Weather, sat.Type);
        Assert.AreEqual(500.0, sat.OrbitAltitude, 0.0001);
        Assert.AreEqual(new DateTime(2020, 3, 15), sat.LaunchDate);
    }

    [TestMethod]
    public void Parse_Null_Throws()
    {
        Assert.ThrowsExactly<Exception>(() => Satellite.Parse(null));
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("Alpha; Weather; 500")]
    [DataRow("Alpha; Nope; 500; 15.03.2020")]
    [DataRow("Alpha; 99; 500; 15.03.2020")]
    [DataRow("Alpha; Weather; abc; 15.03.2020")]
    [DataRow("Alpha; Weather; 500; 2020-03-15")]
    [DataRow("ab; Weather; 500; 15.03.2020")]
    [DataRow("Alpha; Weather; 50000; 15.03.2020")]
    public void Parse_InvalidString_Throws(string input)
    {
        Assert.ThrowsExactly<Exception>(() => Satellite.Parse(input));
    }

    [TestMethod]
    public void TryParse_ValidString_ReturnsTrueAndObject()
    {
        bool ok = Satellite.TryParse("Alpha; Weather; 500; 15.03.2020", out Satellite result);

        Assert.IsTrue(ok);
        Assert.IsNotNull(result);
        Assert.AreEqual("Alpha", result.Name);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("garbage")]
    [DataRow("Alpha; Weather; 50000; 15.03.2020")]
    public void TryParse_InvalidString_ReturnsFalseAndNull(string input)
    {
        bool ok = Satellite.TryParse(input, out Satellite result);

        Assert.IsFalse(ok);
        Assert.IsNull(result);
    }

    [TestMethod]
    public void ToStringThenParse_RoundTrip_RestoresSameValues()
    {
        Satellite original = new Satellite("Round", SType.Scientific, 500.5, ValidDate);

        Satellite copy = Satellite.Parse(original.ToString());

        Assert.AreEqual(original.Name, copy.Name);
        Assert.AreEqual(original.Type, copy.Type);
        Assert.AreEqual(original.OrbitAltitude, copy.OrbitAltitude, 0.0001);
        Assert.AreEqual(original.LaunchDate, copy.LaunchDate);
    }
}