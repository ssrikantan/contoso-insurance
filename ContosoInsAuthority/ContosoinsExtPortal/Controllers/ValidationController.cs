using ContosoinsExtPortal.Models;
using ContosoinsExtPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ContosoinsExtPortal.Controllers
{
      [Authorize]

    [Produces("application/json")]
    [Route("api/Validation")]
    public class ValidationController : Controller
    {
        private readonly ContosoinsauthdbContext _context;
        private AKVServiceClient _akvclient;
        public ValidationController(ContosoinsauthdbContext context,AKVServiceClient akvclient)
        {
            _context = context;
            _akvclient = akvclient;
        }

        // GET: api/Validation
        [HttpGet]
        public IEnumerable<VehiclePolicies> GetVehPoliciesMaster()
        {
            return _context.VehPoliciesMaster;
        }

        // GET: api/Validation/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehPoliciesMaster([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vehPoliciesMaster = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Id == id);

            if (vehPoliciesMaster == null)
            {
                return NotFound();
            }

            return Ok(vehPoliciesMaster);
        }

        // PUT: api/Validation/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVehPoliciesMaster([FromRoute] string id, [FromBody] VehiclePolicies vehPoliciesMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != vehPoliciesMaster.Id)
            {
                return BadRequest();
            }

            _context.Entry(vehPoliciesMaster).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehPoliciesMasterExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Validation
        [HttpPost]
        public async Task<bool> PostVehPoliciesMaster([FromBody] VehiclePolicies vehPoliciesMaster)
        {
            bool isValid = false;
            if (!ModelState.IsValid)
            {
                // return BadRequest(ModelState);
                return isValid;
            }

            var _vehPoliciesMaster = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Policyno == vehPoliciesMaster.Policyno);

            if (_vehPoliciesMaster == null)
            {
                //return NotFound();
                return isValid;
            }

            if (vehPoliciesMaster.Id.Equals(_vehPoliciesMaster.Id) &&
                "Active".Equals(_vehPoliciesMaster.Status) &&
               (_vehPoliciesMaster.Enddate>= DateTime.UtcNow) &&
               (DateTime.UtcNow >= _vehPoliciesMaster.Startdate) &&
                vehPoliciesMaster.Policyno.Equals(_vehPoliciesMaster.Policyno) &&
                vehPoliciesMaster.Vehicleno.Equals(_vehPoliciesMaster.Vehicleno) &&
                vehPoliciesMaster.Firstname.Equals(_vehPoliciesMaster.Firstname) &&
                vehPoliciesMaster.Lastname.Equals(_vehPoliciesMaster.Lastname) &&
                vehPoliciesMaster.Inscompany.Equals(_vehPoliciesMaster.Inscompany)
                )
            {
                try
                {
                    isValid = await _akvclient.ValidateAsync(_vehPoliciesMaster);
                }
                catch(Exception ex)
                {
                    string error = ex.StackTrace;
                }
            }
            return isValid;
        }


        private bool VehPoliciesMasterExists(string id)
        {
            return _context.VehPoliciesMaster.Any(e => e.Id == id);
        }
    }
}