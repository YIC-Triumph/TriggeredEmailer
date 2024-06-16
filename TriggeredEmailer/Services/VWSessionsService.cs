using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Constants;
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
        /// <param name="roleID">role id</param>
        /// <returns></returns>
        public async Task<ICollection<vwSession>> GetAll(DateTime fromDate, DateTime toDate, Roles roleID)
        {

            var pSundayDate = new SqlParameter("@StartDate", SqlDbType.VarChar);
            var pSaturdayDate = new SqlParameter("@EndDate", SqlDbType.VarChar);
            var roleId = new SqlParameter("@RoleID", SqlDbType.Int);

            pSundayDate.Value = fromDate.Date;
            pSaturdayDate.Value = toDate.Date;
            roleId.Value = roleID;

            var vwSessions = await _dbContext.vwSessions.FromSqlRaw(
                    @"SELECT sessID, 
	                    vs.TherapistID, 
	                    vs.SessDate, 
	                    vs.AffirmedDate, 
	                    vs.SessionStatus, 
	                    vs.studentID,
	                    vs.FullName,
	                    pp.PP_ID,
	                    vp.RoleID
                    FROM vwSessions vs
                    LEFT JOIN tblPayPeriod pp
                        ON vs.SessDate between pp.PP_StartDate and pp.PP_EndDate
                    left join vwProviders vp
	                    on vs.therapistid = vp.providerid    
                    WHERE   
                        vs.SessDate >= CAST(@StartDate AS Datetime) AND
                        vs.SessDate <= CAST(@EndDate AS datetime) AND
                        vp.RoleID = @roleId",
                    [pSundayDate, pSaturdayDate, roleId])
                .OrderByDescending(d => d.Sessdate)
                .AsNoTracking()
                .ToListAsync();

            return vwSessions;
        }
    }
}
