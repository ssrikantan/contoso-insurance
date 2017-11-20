using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoinsExtPortal.Models;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authorization;

namespace ContosoinsExtPortal.Controllers
{
    [Authorize]
    public class VehiclePoliciesController : Controller
    {
        private readonly ContosoinsauthdbContext _context;
        public VehiclePoliciesController(ContosoinsauthdbContext context, IOptions<AzureAdB2COptions> b2cOptions)
        {
            _context = context;
        }

        // GET: VehPoliciesMasters
        public async Task<IActionResult> Index()
        {
            return View(await _context.VehPoliciesMaster.Where(m => m.Userid == User.Identity.Name).ToListAsync());
            //return View(await _context.VehPoliciesMaster.ToListAsync());
        }

        // GET: VehPoliciesMasters/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehPoliciesMaster = await _context.VehPoliciesMaster
                .SingleOrDefaultAsync(m => m.Id == id);
            if (vehPoliciesMaster == null)
            {
                return NotFound();
            }

            return View(vehPoliciesMaster);
        }

        // GET: VehPoliciesMasters/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VehPoliciesMasters/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Uidname,Version,Inscompany,Policyno,Vehicleno,Userid,Status,Firstname,Lastname,Lastmod,Startdate,Enddate")] VehiclePolicies vehPoliciesMaster)
        {
            if (ModelState.IsValid)
            {
                _context.Add(vehPoliciesMaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(vehPoliciesMaster);
        }

        // GET: VehPoliciesMasters/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var vehPoliciesMaster = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Id == id);
            if (vehPoliciesMaster == null)
            {
                return NotFound();
            }
            return View(vehPoliciesMaster);
        }

        // GET: VehPoliciesMasters/ActivatePolicy/user4@consto.com
        public async Task<IActionResult> PolicyDownload(string id)
        {
            //if (UserId == null)
            //{
            //    return NotFound();
            //}
            //string useriden = string.Format("{0}." + _options.Domain);
            //var vehPoliciesMaster = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => (m.Userid == User.Identity.Name));
            var vehPoliciesMaster = await _context.VehPoliciesMaster.Where(m => m.Userid == User.Identity.Name && (m.Id == id)).FirstOrDefaultAsync();
            if (vehPoliciesMaster == null)
            {
                return NotFound();
            }

            InsuranceData policyData = new InsuranceData();
            policyData.Inscompany = vehPoliciesMaster.Inscompany;
            policyData.Policyno = vehPoliciesMaster.Policyno;
            policyData.Vehicleno = vehPoliciesMaster.Vehicleno;
            policyData.Lastname = vehPoliciesMaster.Lastname;
            policyData.Firstname = vehPoliciesMaster.Firstname;
            policyData.Id = vehPoliciesMaster.Id;
            policyData.Startdate = vehPoliciesMaster.Startdate;
            policyData.Enddate = vehPoliciesMaster.Enddate;
            policyData.qrcodeData = string.Empty;
            string _qrdata = JsonConvert.SerializeObject(policyData);
            policyData.qrcodeData = _qrdata;
            return View(policyData);
        }


        // This metod is called when the Citizen activates the insurance Policy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Uidname,Version,Inscompany,Policyno,Vehicleno,Userid,Status,Firstname,Lastname,Lastmod,Startdate,Enddate")] VehiclePolicies vehPoliciesMaster)
        {
            if (id != vehPoliciesMaster.Id)
            {
                return NotFound();
            }

            if("Active".Equals(vehPoliciesMaster.Status))
            {
                // Status is already active, no action
                return View(vehPoliciesMaster);
            }
            vehPoliciesMaster.Status = "Active";
            vehPoliciesMaster.Lastmod = DateTime.UtcNow;


            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(vehPoliciesMaster);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehPoliciesMasterExists(vehPoliciesMaster.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
         
            return View(vehPoliciesMaster);
        }

         // GET: VehPoliciesMasters/Delete/5
        //public async Task<IActionResult> Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var vehPoliciesMaster = await _context.VehPoliciesMaster
        //        .SingleOrDefaultAsync(m => m.Id == id);
        //    if (vehPoliciesMaster == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(vehPoliciesMaster);
        //}

        // POST: VehPoliciesMasters/Delete/5
       // [HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(string id)
        //{
        //    var vehPoliciesMaster = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Id == id);
        //    _context.VehPoliciesMaster.Remove(vehPoliciesMaster);
        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool VehPoliciesMasterExists(string id)
        {
            return _context.VehPoliciesMaster.Any(e => e.Id == id);
        }
    }
}
