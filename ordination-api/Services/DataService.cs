using Microsoft.EntityFrameworkCore;
using System.Text.Json;

using shared.Model;
using static shared.Util;
using Data;

namespace Service;

public class DataService
{
    private OrdinationContext db { get; }

    public DataService(OrdinationContext db) {
        this.db = db;
    }

    /// <summary>
    /// Seeder noget nyt data i databasen, hvis det er nødvendigt.
    /// </summary>
    public void SeedData() {

        // Patients
        Patient[] patients = new Patient[5];
        patients[0] = db.Patienter.FirstOrDefault()!;

        if (patients[0] == null)
        {
            patients[0] = new Patient("121256-0512", "Jane Jensen", 63.4);
            patients[1] = new Patient("070985-1153", "Finn Madsen", 83.2);
            patients[2] = new Patient("050972-1233", "Hans Jørgensen", 89.4);
            patients[3] = new Patient("011064-1522", "Ulla Nielsen", 59.9);
            patients[4] = new Patient("123456-1234", "Ib Hansen", 87.7);

            db.Patienter.Add(patients[0]);
            db.Patienter.Add(patients[1]);
            db.Patienter.Add(patients[2]);
            db.Patienter.Add(patients[3]);
            db.Patienter.Add(patients[4]);
            db.SaveChanges();
        }

        Laegemiddel[] laegemiddler = new Laegemiddel[5];
        laegemiddler[0] = db.Laegemiddler.FirstOrDefault()!;
        if (laegemiddler[0] == null)
        {
            laegemiddler[0] = new Laegemiddel("Acetylsalicylsyre", 0.1, 0.15, 0.16, "Styk");
            laegemiddler[1] = new Laegemiddel("Paracetamol", 1, 1.5, 2, "Ml");
            laegemiddler[2] = new Laegemiddel("Fucidin", 0.025, 0.025, 0.025, "Styk");
            laegemiddler[3] = new Laegemiddel("Methotrexat", 0.01, 0.015, 0.02, "Styk");
            laegemiddler[4] = new Laegemiddel("Prednisolon", 0.1, 0.15, 0.2, "Styk");

            db.Laegemiddler.Add(laegemiddler[0]);
            db.Laegemiddler.Add(laegemiddler[1]);
            db.Laegemiddler.Add(laegemiddler[2]);
            db.Laegemiddler.Add(laegemiddler[3]);
            db.Laegemiddler.Add(laegemiddler[4]);

            db.SaveChanges();
        }

        Ordination[] ordinationer = new Ordination[6];
        ordinationer[0] = db.Ordinationer.FirstOrDefault()!;
        if (ordinationer[0] == null) {
            Laegemiddel[] lm = db.Laegemiddler.ToArray();
            Patient[] p = db.Patienter.ToArray();

            ordinationer[0] = new PN(new DateTime(2024, 11, 1), new DateTime(2024, 11, 12), 123, lm[1]);    
            ordinationer[1] = new PN(new DateTime(2024, 12, 12), new DateTime(2024, 12, 14), 3, lm[0]);    
            ordinationer[2] = new PN(new DateTime(2024, 11, 20), new DateTime(2024, 11, 25), 5, lm[2]);    
            ordinationer[3] = new PN(new DateTime(2024, 11, 1), new DateTime(2024, 11, 12), 123, lm[1]);
            ordinationer[4] = new DagligFast(new DateTime(2024, 11, 10), new DateTime(2024, 11, 12), lm[1], 2, 0, 1, 0);
            ordinationer[5] = new DagligSkæv(new DateTime(2024, 11, 23), new DateTime(2024, 11, 24), lm[2]);
            
            ((DagligSkæv) ordinationer[5]).doser = new Dosis[] { 
                new Dosis(CreateTimeOnly(12, 0, 0), 0.5),
                new Dosis(CreateTimeOnly(12, 40, 0), 1),
                new Dosis(CreateTimeOnly(16, 0, 0), 2.5),
                new Dosis(CreateTimeOnly(18, 45, 0), 3)        
            }.ToList();
            

            db.Ordinationer.Add(ordinationer[0]);
            db.Ordinationer.Add(ordinationer[1]);
            db.Ordinationer.Add(ordinationer[2]);
            db.Ordinationer.Add(ordinationer[3]);
            db.Ordinationer.Add(ordinationer[4]);
            db.Ordinationer.Add(ordinationer[5]);

            db.SaveChanges();

            p[0].ordinationer.Add(ordinationer[0]);
            p[0].ordinationer.Add(ordinationer[1]);
            p[2].ordinationer.Add(ordinationer[2]);
            p[3].ordinationer.Add(ordinationer[3]);
            p[1].ordinationer.Add(ordinationer[4]);
            p[1].ordinationer.Add(ordinationer[5]);

            db.SaveChanges();
        }
    }

    
    public List<PN> GetPNs() {
        return db.PNs.Include(o => o.laegemiddel).Include(o => o.dates).ToList();
    }

    public List<DagligFast> GetDagligFaste() {
        return db.DagligFaste
            .Include(o => o.laegemiddel)
            .Include(o => o.MorgenDosis)
            .Include(o => o.MiddagDosis)
            .Include(o => o.AftenDosis)            
            .Include(o => o.NatDosis)            
            .ToList();
    }

    public List<DagligSkæv> GetDagligSkæve() {
        return db.DagligSkaeve
            .Include(o => o.laegemiddel)
            .Include(o => o.doser)
            .ToList();
    }

    public List<Patient> GetPatienter() {
        return db.Patienter.Include(p => p.ordinationer).ToList();
    }

    public List<Laegemiddel> GetLaegemidler() {
        return db.Laegemiddler.ToList();
    }

    public PN OpretPN(int patientId, int laegemiddelId, double antal, DateTime startDato, DateTime slutDato) {
        // TODO: Implement!

        //exception to check whether the antal is within the accepted range
        if (antal < 0)
        {
            throw new ArgumentException("Enhed kan ikke være negativ.");
        }
        if (antal == 0 )
        {
            throw new ArgumentException("Enhed skal være angivet");
        }

        var patient = db.Patienter.Include(p => p.ordinationer).FirstOrDefault(p => p.PatientId == patientId);
        var laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);


        double anbefaletDosis = GetAnbefaletDosisPerDøgn(patientId, laegemiddelId);

        //exception to check whether the antal is within the anbefaletdosis
        if (antal > anbefaletDosis)
        {
            throw new ArgumentException("Enhed har overskrevet anbefalet dosis");
        }


        var pn = new PN
        {
            laegemiddel = laegemiddel,          
            antalEnheder = antal,              
            startDen = startDato,              
            slutDen = slutDato                 
        };

       
        patient.ordinationer.Add(pn);

        db.PNs.Add(pn);

        db.SaveChanges();
        return pn;
    }

    public DagligFast OpretDagligFast(int patientId, int laegemiddelId,
    double antalMorgen, double antalMiddag, double antalAften, double antalNat,
    DateTime startDato, DateTime slutDato)
    {
     

        // Check if the start date ONLY DATE is after the end date
        if (startDato.Date > slutDato.Date)
        {
            throw new ArgumentException("Slutdato kan ikke være før startdato.");
        }

        var døgnDosis = antalMorgen + antalMiddag + antalAften + antalNat;
        if (døgnDosis < 0)
        {
            throw new ArgumentException("Enhed kan ikke være negativ.");
        }
        if (døgnDosis == 0)
        {
            throw new ArgumentException("Enhed skal være angivet");
        }

        var patient = db.Patienter.Include(p => p.ordinationer).FirstOrDefault(p => p.PatientId == patientId);
        var laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);

        if (laegemiddel == null)
        {
            throw new ArgumentException("Lægemiddel kan ikke være null.");
        }

        double anbefaletDosis = GetAnbefaletDosisPerDøgn(patientId, laegemiddelId);

        if (døgnDosis > anbefaletDosis)
        {
            throw new ArgumentException("Enhed har overskrevet anbefalet dosis");
        }

        var dagligFast = new DagligFast(startDato, slutDato, laegemiddel, antalMorgen, antalMiddag, antalAften, antalNat);

        patient.ordinationer.Add(dagligFast);
        db.DagligFaste.Add(dagligFast);
        db.SaveChanges();

        return dagligFast;
    }


    public DagligSkæv OpretDagligSkaev(int patientId, int laegemiddelId, Dosis[] doser, DateTime startDato, DateTime slutDato) {
        // TODO: Implement!
        double total = 0;
        foreach (Dosis d in doser)
        {
            total += d.antal;
        }
        if (total < 0)
        {
            throw new ArgumentException("Enhed kan ikke være negativ.");
        }
        if (total == 0)
        {
            throw new ArgumentException("Enhed skal være angivet");
        }

        var patient = db.Patienter.Include(p => p.ordinationer).FirstOrDefault(p => p.PatientId == patientId);
        var laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);

        double anbefaletDosis = GetAnbefaletDosisPerDøgn(patientId, laegemiddelId);

        //exception to check whether the antal is within the anbefaletdosis
        if (total > anbefaletDosis)
        {
            throw new ArgumentException("Enhed har overskrevet anbefalet dosis");
        }

        var dagligSkæv = new DagligSkæv(startDato, slutDato, laegemiddel, doser);
               

        patient.ordinationer.Add(dagligSkæv);

        db.DagligSkaeve.Add(dagligSkæv);
        db.SaveChanges();


        return dagligSkæv;
    }

    public string AnvendOrdination(int id, Dato dato) {
        // TODO: Implement!
        var ordination = db.Ordinationer
         .Include(o => (o as PN)!.dates)
        .FirstOrDefault(o => o.OrdinationId == id);

        //exception for checking the id for the specified ordination exists
        if (ordination == null)
        {
            return $"Ordination with ID {id} does not exist.";
        }

        //exception for checking the date is within the valid range
        if (dato.dato < ordination.startDen || dato.dato > ordination.slutDen)
        {
            return $"Date {dato} is outside the valid period for this ordination.";
        }

        if (ordination is PN pn)
        {
            if (pn.dates == null)
            {
                pn.dates = new List<Dato>();
            }
            pn.dates.Add(dato);
        }

            db.SaveChanges();

        return $"Ordination with ID {id} was successfully marked as used on {dato}.";   
}

    /// <summary>
    /// Den anbefalede dosis for den pågældende patient, per døgn, hvor der skal tages hensyn til
	/// patientens vægt. Enheden afhænger af lægemidlet. Patient og lægemiddel må ikke være null.
    /// </summary>
    /// <param name="patient"></param>
    /// <param name="laegemiddel"></param>
    /// <returns></returns>
	public double GetAnbefaletDosisPerDøgn(int patientId, int laegemiddelId) {
        // TODO: Implement!
            var patient = db.Patienter.FirstOrDefault(p => p.PatientId == patientId);
            var laegemiddel = db.Laegemiddler.FirstOrDefault(l => l.LaegemiddelId == laegemiddelId);

            //calculating the recc dosis based on the formula that we were given
            if (patient.vaegt < 25)
            {
                return patient.vaegt * laegemiddel.enhedPrKgPrDoegnLet;
            }
            else if (patient.vaegt <= 120)
            {
                return patient.vaegt * laegemiddel.enhedPrKgPrDoegnNormal;
            }
            else
            {
                return patient.vaegt * laegemiddel.enhedPrKgPrDoegnTung;
            }
        }

}