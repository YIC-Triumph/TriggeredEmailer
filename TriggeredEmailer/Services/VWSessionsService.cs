using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Data;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Services
{
    public class VWSessionsService : IVWSessionsService
    {
        private readonly ApplicationDbContext _dbContext;
        public VWSessionsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all sessions from vwsession
        /// </summary>
        /// <param name="fromDate">start date</param>
        /// <param name="toDate">end date</param>
        /// <returns></returns>
        public async Task<ICollection<vwSession>> GetAll(DateTime fromDate, DateTime toDate)
        {

            var pSundayDate = new SqlParameter("@StartDate", SqlDbType.VarChar);
            var pSaturdayDate = new SqlParameter("@EndDate", SqlDbType.VarChar);

            pSundayDate.Value = fromDate.Date;
            pSaturdayDate.Value = toDate.Date;

            var vwSessions = await _dbContext.vwSessions.FromSqlRaw(
                    @"SELECT sessID, 
                        TherapistID, 
                        sessdate, 
                        AffirmedDate, 
                        SessionStatus, 
                        studentID, 
                        FullName
                    FROM vwSessions 
                    WHERE   
                        sessdate >= CAST(@StartDate AS Datetime) AND
                        sessdate <= CAST(@EndDate AS datetime)",
                    [pSundayDate, pSaturdayDate])
                .OrderByDescending(d => d.sessdate)
                .AsNoTracking()
                .ToListAsync();

            return vwSessions;
        }
    }
}
