using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TriggeredEmailer.Data;
using TriggeredEmailer.Models;

namespace TriggeredEmailer.Services
{
    public class VWStaffService: IVWStaffService
    {
        private readonly ApplicationDbContext _dbContext;
        public VWStaffService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Get all staff from VWStaff
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ICollection<Staff>> GetAll()
        {
            //TODO: 1. Map db objects to entity in vwStaff
            //TODO: 2. Change the query to this
            //var providers = _dbContext.Staffs.ToList();

            var providers = _dbContext.Staffs.FromSqlRaw(
                @"SELECT LoginID,
                         R_ID,
                         FirstName,
                         LastName
                    FROM vwStaff").ToList();

            return providers;
        }
    }
}
