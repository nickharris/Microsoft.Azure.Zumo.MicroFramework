using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace GraphSensorData.Controllers
{
    public class SensorController : ApiController
    {
        // GET api/sensor
        public async Task<string> Get()
        {
            using (var client = new HttpClient())
            using (var response = await client.GetAsync("https://microframework.azure-mobile.net/tables/todoitem"))
            {
                return await response.Content.ReadAsStringAsync();
            }
        }

        // GET api/sensor/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/sensor
        public void Post([FromBody]string value)
        {
        }

        // PUT api/sensor/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/sensor/5
        public void Delete(int id)
        {
        }
    }
}
