namespace Gestion_EDT.Models
{
    public class Mention
    {
        public int Id { get; set; }
        public string code_mention { get; set; }
        public string nom_mention { get; set; }

        public ICollection<Cycle> Cycles { get; set; }
    }
}