using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Models
{
    /// <summary>
    /// not mapped
    /// </summary>
    [Keyless]
    public class SessionStudent
    {
        public int? studentID { get; set; }
        public string? strIDs { get; set; }
    }
}
