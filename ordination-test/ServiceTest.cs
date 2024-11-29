namespace ordination_test;

using Microsoft.EntityFrameworkCore;

using Service;
using Data;
using shared.Model;
using static shared.Util;

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

    //sum
    //valid, null or invalid ordination 
    [TestMethod]
    public void InvalidOrdinationType()
    {
        var ordinationType = OrdinationType.DagligSkaev; 
        var invalidType = (OrdinationType)999; 

        var result = service.GetPNs().FirstOrDefault(pn => pn.getType() == invalidType.ToString()); 

        // Assert
        Assert.IsNull(result, "Fejlbesked: ordination findes ikke"); 
    }

    [TestMethod]
    public void ValidOrdinationType()
    {
        var ordinationType = OrdinationType.PN; 

        var result = service.GetPNs().FirstOrDefault(pn => pn.getType() == ordinationType.ToString()); 

        // Assert
        Assert.IsNotNull(result, "Ordination bør være fundet"); 
        Assert.AreEqual(ordinationType.ToString(), result.getType(), "Ordination type should match the input.");
    }

    [TestMethod]
    public void NullOrdinationType()
    {

        var invalidPN = new PN();

        // Act
        var result = service.GetPNs().FirstOrDefault(pn => pn.getType() == null);

        // Assert
        Assert.IsNull(result, "Ordination med null type bør ikke kunne findes.");
    }


    //sum
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

    //sum
    //valid, null or invalid medication type
    //missing invalid and null
    [TestMethod]
    public void OpretPN_WithValidMedication()
    {
        var patientId = 1; //valid patient id
        var medicationId = 1; //valid id
        var amount = 1;
        var startDate = DateTime.Now;
        var endDate = DateTime.Now.AddDays(1);

        var result = service.OpretPN(patientId, medicationId, amount, startDate, endDate);

        Assert.IsNotNull(result, "Result should not be null for valid medication.");
        Assert.AreEqual(medicationId, result.laegemiddel.LaegemiddelId, "The medication ID should match the input.");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidMedicationNull()
    {
        var patient = service.GetPatienter().First();

        Laegemiddel laegemiddel = null;

        service.OpretDagligFast(
            patient.PatientId,
            laegemiddel?.LaegemiddelId ?? 0, 
            2, 2, 1, 0,  
            new DateTime(2024, 11, 29),  
            new DateTime(2024, 11, 30)  
        );
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
    [TestMethod]
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

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretDagligSkæv_ThrowsException_ForNegativeAntal()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Create the Dosis objects, including one with a negative value for antal
        Dosis[] doser = new Dosis[]
    {
        new Dosis(CreateTimeOnly(12, 0, 0), 0.5),   // Valid dose
        new Dosis(CreateTimeOnly(12, 40, 0), 1),    // Valid dose
        new Dosis(CreateTimeOnly(16, 0, 0), -2.5)   // Invalid negative dose
    };

        // Act - try to create DagligSkæv, should throw an exception
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato);
    }
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretDagligSkæv_ThrowsException_ForZero()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Create the Dosis objects, including one with a negative value for antal
        Dosis[] doser = new Dosis[]
    {
        new Dosis(CreateTimeOnly(12, 0, 0), 0),   
        new Dosis(CreateTimeOnly(12, 40, 0), 0),    
        new Dosis(CreateTimeOnly(16, 0, 0), 0)   
    };

        // Act - try to create DagligSkæv, should throw an exception
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato);
    }
    [TestMethod]

    public void OpretDagligSkæv()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Create the Dosis objects, including one with a negative value for antal
        Dosis[] doser = new Dosis[]
    {
        new Dosis(CreateTimeOnly(12, 0, 0), 1),   // Valid dose
        new Dosis(CreateTimeOnly(12, 40, 0), 0),    // Valid dose
        new Dosis(CreateTimeOnly(16, 0, 0), 0)  // Valid dose
    };

        // Act
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException), "Enhed har overskrevet anbefalet dosis")]
    public void OpretDagligSkæv_ThrowsException_ForExceedingAnbefaletDosis()
    {
        // Arrange
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        DateTime startDato = new DateTime(2024, 11, 1);
        DateTime slutDato = new DateTime(2024, 11, 5);

        // Get the recommended daily dose
        double anbefaletDosis = service.GetAnbefaletDosisPerDøgn(patient.PatientId, lm.LaegemiddelId);

        // Create doses that collectively exceed the recommended daily dose
        Dosis[] doser = new Dosis[]
        {
        new Dosis(CreateTimeOnly(12, 0, 0), anbefaletDosis / 2),
        new Dosis(CreateTimeOnly(12, 40, 0), anbefaletDosis / 2),
        new Dosis(CreateTimeOnly(16, 0, 0), 1) // Exceeds the limit
        };

        // Act
        service.OpretDagligSkaev(patient.PatientId, lm.LaegemiddelId, doser, startDato, slutDato);
    }


    //sum
    //invalid or valid start date and end date
    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OpretDagligFast_ThrowsException_WhenEndDateBeforeStartDate()
    {
        var patient = service.GetPatienter().First();
        var laegemiddel = service.GetLaegemidler().First();

        DateTime invalidStartDate = new DateTime(2024, 12, 10);
        DateTime invalidEndDate = new DateTime(2024, 12, 5); //ivalid end date before start date

        double morgenAntal = 1, middagAntal = 1, aftenAntal = 1, natAntal = 1;

        service.OpretDagligFast(patient.PatientId, laegemiddel.LaegemiddelId, morgenAntal, middagAntal, aftenAntal, natAntal, invalidStartDate, invalidEndDate);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void InvalidDateRange_EndDateBeforeStartDate()
    {
        var patient = service.GetPatienter().First();
        var laegemiddel = service.GetLaegemidler().First();

        service.OpretDagligFast(
            patient.PatientId,
            laegemiddel.LaegemiddelId,
            2, 2, 1, 0,  // Example doses
            new DateTime(2024, 11, 30),  // Start date
            new DateTime(2024, 11, 29)   // End date (before start date)
        );

      
    }


    [TestMethod]
    public void ValidSingleDayRange()
    {
        var patient = service.GetPatienter().First();
        var laegemiddel = service.GetLaegemidler().First();

        var result = service.OpretDagligFast(
            patient.PatientId,
            laegemiddel.LaegemiddelId,
            2, 2, 1, 0,  
            new DateTime(2024, 11, 29),  
            new DateTime(2024, 11, 29)
        );

        Assert.AreEqual(new DateTime(2024, 11, 29), result.startDen);
        Assert.AreEqual(new DateTime(2024, 11, 29), result.slutDen);
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

   
}
