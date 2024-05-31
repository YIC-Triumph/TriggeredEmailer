using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Models
{
    /// <summary>
    /// Properties from Staff
    /// </summary>
    public class Staff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LoginID { get; set; }
        public int R_ID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
