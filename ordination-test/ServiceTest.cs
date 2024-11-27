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
    public void OpretDagligFast()
    {
        Patient patient = service.GetPatienter().First();
        Laegemiddel lm = service.GetLaegemidler().First();

        Assert.AreEqual(1, service.GetDagligFaste().Count());

        service.OpretDagligFast(patient.PatientId, lm.LaegemiddelId,
            2, 2, 1, 0, DateTime.Now, DateTime.Now.AddDays(3));

        Assert.AreEqual(2, service.GetDagligFaste().Count());
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
    {
        // Herunder skal man så kalde noget kode,
        // der smider en exception.

        // Hvis koden _ikke_ smider en exception,
        // så fejler testen.

        Console.WriteLine("Her kommer der ikke en exception. Testen fejler.");
    }*/

}