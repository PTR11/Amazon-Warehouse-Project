using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brute_Force.Model;

namespace Brute_Force.Persistence
{
    /// <summary>
    /// The interface of the data access.
    /// </summary>
    public interface IDataAccess
    {
        Task<Depot> LoadAsync(String path);

        Task<Depot> LoadAsyncMatrix(String path);
        Task SaveDepotAsync(String path, Depot depot);
        Task SaveStatsAsync(String path, Int32 steps, List<Robot> robots);
    }
}
