using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Collections.Generic;

namespace MagicStay.Controllers
{
    [Route("api/Stay_API")]
    [ApiController]
    public class Stay_API : ControllerBase
    {
        [HttpGet]
        public IEnumerable<Villa> getVillas()
        {
            return new List<Villa>
            {
                new Villa{Id=1, Name="Pool View" },
                new Villa{Id=2, Name="Beach View" }
            };
        }
    }
}
