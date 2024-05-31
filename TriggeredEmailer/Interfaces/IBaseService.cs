using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Interfaces
{
    public interface IBaseService<T> where T : class
    {
        Task<ICollection<T>> GetAll();
    }
}
