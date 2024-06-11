using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Services
{
    public interface IVWSessionsService
    {
        Task<ICollection<vwSession>> GetAll(DateTime from, DateTime to);
    }
}
