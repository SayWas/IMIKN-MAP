using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace IMIKN_MAP.SERVER.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FilesController : Controller
    {
        private readonly ILogger<FilesController> _logger;

        public FilesController(ILogger<FilesController> logger)
        {
            _logger = logger;
        }

        [HttpGet("events.json")]
        public IEnumerable<Event> Get()
        {
            Event[] events = new Event[5];
            for (int i = 0; i < 5; i++)
            {
                events[i] = new Event(new string[2] {"qwfqw", "fqfwq"}, "2022-05-14T08:30:00", "2022-05-14T08:30:00", "УЛК-05", i);
            }
            return events;
        }
    }
}
