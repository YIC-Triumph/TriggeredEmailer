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
        public int SessID { get; set; }
        public int? TherapistID { get; set; }
        public DateTime? Sessdate { get; set; }
        public DateTime? AffirmedDate { get; set; }
        public int? SessionStatus { get; set; }
        public int? StudentID { get; set; }
        public string? FullName { get; set; }
        public int PP_ID { get; set; }
    }
}
