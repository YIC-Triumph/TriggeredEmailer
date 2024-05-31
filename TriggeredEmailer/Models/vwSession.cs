using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Models
{
    /// <summary>
    /// Properties from vwSessions
    /// </summary>
    public class vwSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int sessID { get; set; }
        public int? TherapistID { get; set; }
        public DateTime? sessdate { get; set; }
        public DateTime? AffirmedDate { get; set; }
        public int? SessionStatus { get; set; }
        public int? studentID { get; set; }
        public string? FullName { get; set; }
    }
}
