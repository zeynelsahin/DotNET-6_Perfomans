using CarvedRock.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarvedRock.Domain
{
    public interface IApiCaller
    {
        Task<List<LocalClaim>?> CallExternalApiAsync();
    }
}
