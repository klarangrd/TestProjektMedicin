namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;

[TestClass]
public class ServiceTest
{
    private DataService service;

    [TestInitialize]
    public void SetupBeforeEachTest()
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrdinationContext>();
        optionsBuilder.UseInMemoryDatabase(databaseName: "test-database");
        var context = new OrdinationContext(optionsBuilder.Options);
        service = new DataService(context);
        service.SeedData();
    }

    //invalid or valid patient tests
    [TestMethod]
    public void PatientsExist()
    {
        Assert.IsNotNull(service.GetPatienter());
    }

    [TestMethod]
    public void InvalidPatient()
    {
        var result = service.GetPatienter()
                            .FirstOrDefault(p => p.navn == "Magnus Møller");

        Assert.IsNull(result, "Patient 'Magnus Møller' should not exist in the database.");
    }

    [TestMethod]
    public void NullPatient()
    {
        //passinf as null
        Patient result = null;

        Assert.IsNull(result, "Patient input should be null.");
    }


    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretPN_ThrowsException_ForNegativeAntal()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        double TC1antal = -1; // Invalid input
        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, TC1antal, startDato, slutDato);

    
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretPN_ThrowsException_ForZeroAntal()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
        double TC2antal = 0; // Invalid input
        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, TC2antal, startDato, slutDato);

    }
    [TestMethod]
    public void OpretPN()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // TC1 
        double TC3antal = 1; // valid input
        // act
        var pn = service.OpretPN(patient.PatientId, lm.LaegemiddelId, TC3antal, startDato, slutDato);

        // assert 
        Assert.IsNotNull(pn, "PN should not be null.");
        Assert.AreEqual(TC3antal, pn.antalEnheder, "PN amount should match the input.");
        Assert.AreEqual(startDato, pn.startDen, "PN start date should match the input.");
        Assert.AreEqual(slutDato, pn.slutDen, "PN end date should match the input.");
        Assert.AreEqual(lm.LaegemiddelId, pn.laegemiddel.LaegemiddelId, "PN medication should match the input.");

    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException), "Enhed har overskrevet anbefalet dosis")]
    public void OpretPN_ThrowsException_WhenDoseExceedsRecommended()
    {
        var patient = service.GetPatienter().First(); // Get a valid patient
        var laegemiddel = service.GetLaegemidler().First(); // Get a valid medication
        double anbefaletDosis = service.GetAnbefaletDosisPerDøgn(patient.PatientId, laegemiddel.LaegemiddelId);
        double exceedingDose = anbefaletDosis + 1; // Exceed the recommended dose
        DateTime startDato = DateTime.Today;
        DateTime slutDato = DateTime.Today.AddDays(5);

        // Act
        service.OpretPN(patient.PatientId, laegemiddel.LaegemiddelId, exceedingDose, startDato, slutDato);

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretDagligFast_ThrowsException_ForNegativeAntal()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();
       
        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Act
        double morgenAntal = 0;
        double middagAntal = 0;
        double aftenAntal = -1;
        double natAntal = 0;

        double DagligDoseTC1 = morgenAntal + middagAntal + aftenAntal + natAntal;

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
        morgenAntal, middagAntal, aftenAntal, natAntal, DateTime.Now, DateTime.Now.AddDays(3));

    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretDagligDose_ThrowsException_ForZeroAntal()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Act
        double morgenAntal = 0;
        double middagAntal = 0;
        double aftenAntal = 0;
        double natAntal = 0;

        double DagligDoseTC2 = morgenAntal + middagAntal + aftenAntal + natAntal;

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
        morgenAntal, middagAntal, aftenAntal, natAntal, DateTime.Now, DateTime.Now.AddDays(3));
    }

    [TestMethod]
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        double morgenAntal = 2; 
        double middagAntal = 2; 
        double aftenAntal = 1;  
        double natAntal = 0;

        double DagligDose = morgenAntal + middagAntal + aftenAntal + natAntal;

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            morgenAntal, middagAntal, aftenAntal, natAntal, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());

        Assert.AreEqual(DagligDose, service.GetDagligFaste().Last().doegnDosis()); // Daglige dosis = forvetnted dosis

    }

    //invalid or valid start date and end date
    [TestMethod]
    public void ValidSingleDayRange()
    {
        var patient = service.GetPatienter().First();
        var laegemiddel = service.GetLaegemidler().First();

        //dummy data
        var result = service.OpretDagligFast(
            patient.PatientId,
            laegemiddel.LaegemiddelId,
            2, 2, 1, 0,
            new DateTime(2024, 11, 22),  // Start and End date are the same
            new DateTime(2024, 11, 22)
        );

        //is date in rage?
        Assert.AreEqual(new DateTime(2024, 11, 22), result.startDen);
        Assert.AreEqual(new DateTime(2024, 11, 22), result.slutDen);
    }

    [TestMethod]
    public void ValidDateRange()
    {
        var patient = service.GetPatienter().First();
        var laegemiddel = service.GetLaegemidler().First();

        //end date is after start date
        var result = service.OpretDagligFast(
            patient.PatientId,
            laegemiddel.LaegemiddelId,
            2, 2, 1, 0,
            new DateTime(2024, 11, 23),  //start 
            new DateTime(2024, 11, 26)   //end
        );

        Assert.AreEqual(new DateTime(2024, 11, 23), result.startDen);
        Assert.AreEqual(new DateTime(2024, 11, 26), result.slutDen);
    }

       




    /*[TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void TestAtKodenSmiderEnException()
    [ExpectedException(typeof(ArgumentException), "Enhed har overskrevet anbefalet dosis")]
    public void OpretDagligDose_ThrowsException_WhenDoseExceedsRecommended()
    {

        var patient = service.GetPatienter().First(); // Get a valid patient
        var laegemiddel = service.GetLaegemidler().First(); // Get a valid medication

        // Calculate the recommended daily dose
        double anbefaletDosis = service.GetAnbefaletDosisPerDøgn(patient.PatientId, laegemiddel.LaegemiddelId);

        // Create dose inputs that exceed the recommended daily dose
        double morgenAntal = anbefaletDosis / 2;
        double middagAntal = anbefaletDosis / 2;
        double aftenAntal = 1; // Exceed the limit by adding extra dose
        double natAntal = 0;

        double DagligDose = morgenAntal + middagAntal + aftenAntal + natAntal;

        Assert.IsTrue(DagligDose > anbefaletDosis, "DagligDose should exceed anbefaletDosis for this test.");

        DateTime startDato = DateTime.Today;
        DateTime slutDato = DateTime.Today.AddDays(3);

        // Act
        service.OpretDagligFast(patient.PatientId, laegemiddel.LaegemiddelId,
            morgenAntal, middagAntal, aftenAntal, natAntal, startDato, slutDato);
    }

}