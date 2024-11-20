namespace shared.Model;

public class DagligSkaev : Ordination {
    public List<Dosis> doser { get; set; } = new List<Dosis>();

    public DagligSkaev(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel) : base(laegemiddel, startDen, slutDen) {
	}

    public DagligSkaev(DateTime startDen, DateTime slutDen, Laegemiddel laegemiddel, Dosis[] doser) : base(laegemiddel, startDen, slutDen) {
        this.doser = doser.ToList();
    }    

    public DagligSkaev() : base(null!, new DateTime(), new DateTime()) {
    }

	public void opretDosis(DateTime tid, double antal) {
        doser.Add(new Dosis(tid, antal));
    }

	public override double samletDosis() {
		return base.antalDage() * doegnDosis();
	}

	public override double doegnDosis() {
		// TODO: Implement!
		double total = 0;
		foreach (Dosis d in doser)
		{
			total += d.antal;
		}
			return total ;
	}

	public override String getType() {
		return "DagligSkæv";
	}
}
