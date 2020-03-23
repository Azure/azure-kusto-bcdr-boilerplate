using BcdrTestAppADX.ADX;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BcdrTestAppADX.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BcdrTestAsyncController : ControllerBase
    {
        private readonly ILogger<BcdrTestController> _logger;
        private readonly TelemetryClient _telemetry;
        private readonly AdxBcdrClient _client;

        public BcdrTestAsyncController(ILogger<BcdrTestController> logger, TelemetryClient telemetry, AdxBcdrClient client)
        {
            _logger = logger;
            _telemetry = telemetry;
            _client = client;
        }

        [HttpGet]
        public async Task<int> Get()
        {
            string query = $@"Analytics_Anomaly()";

            Stopwatch sw = Stopwatch.StartNew();

            var result = await _client.ExecuteQueryAsync("sensordata", query, _telemetry);

            sw.Stop();

            _telemetry.TrackMetric("adxResponseTime", sw.ElapsedMilliseconds);

            if (result != null)
            {
                int resultCount = 0;

                while (result.Read())
                {
                    var value = result.GetValue(1);
                    resultCount++;
                }

                return resultCount;
            }

            return -1;
        }
    }
}
