using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TriggeredEmailer.Models
{
    public class SessionUserJoin
    {
        // Properties from vwSessions
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int? sessID { get; set; }
        public int? TherapistID { get; set; }
        public int? Affirmed { get; set; }
        public string? AbsenceReason { get; set; }

        // Properties from users
        public int? UserID { get; set; }
        public string? UserName { get; set; }
        public string? EmailAddress { get; set; }
    }
}
