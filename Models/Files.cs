using System.ComponentModel.DataAnnotations.Schema;

namespace File.Models
{
    public class Files
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int PersonId { get; set; }
        [ForeignKey("PersonId")]
        public Person Person { get; set; }
        public string ContentType { get; set; }
    }
}
