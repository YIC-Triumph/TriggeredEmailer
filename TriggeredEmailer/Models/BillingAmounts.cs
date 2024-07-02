using Microsoft.EntityFrameworkCore;
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
    /// not mapped
    /// </summary>
    [Keyless]
    public class BillingAmount
    {
        public decimal TotalHours { get; set; }
        public decimal TotalPAy { get; set; }
    }
}
