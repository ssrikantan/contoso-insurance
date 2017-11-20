using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoInsAuthorityAdminPortal.Models;
using ContosoInsAuthorityAdminPortal.Services;
using Microsoft.AspNetCore.Authorization;

namespace ContosoInsAuthorityAdminPortal.Controllers
{
    [Authorize]
    public class VehiclePoliciesController : Controller
    {
        private readonly ContosoinsauthdbContext _context;
        private AKVServiceClient _akvclient;

        public VehiclePoliciesController(ContosoinsauthdbContext context, AKVServiceClient akvclient)
        {
            _context = context;
            _akvclient = akvclient;
        }

        // GET: VehiclePolicies
        public async Task<IActionResult> Index()
        {
            return View(await _context.VehPoliciesMaster.ToListAsync());
        }

        // GET: VehiclePolicies/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var _Policies = await _context.VehPoliciesMaster
                .SingleOrDefaultAsync(m => m.Id == id);
            if (_Policies == null)
            {
                return NotFound();
            }

            return View(_Policies);
        }

        // GET: VehiclePolicies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: VehiclePolicies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Uidname,Version,Inscompany,Policyno,Vehicleno,Userid,Status,Firstname,Lastname,Lastmod,Startdate,Enddate")] VehiclePolicies _Policies)
        {
            _Policies.Id = Guid.NewGuid().ToString();
            await _akvclient.CreateSecret(_Policies);
            if (ModelState.IsValid)
            {
                _context.Add(_Policies);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(_Policies);
        }

        // GET: VehiclePolicies/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var _Policies = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Id == id);
            if (_Policies == null)
            {
                return NotFound();
            }
            return View(_Policies);
        }

        // POST: VehiclePolicies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Uidname,Version,Inscompany,Policyno,Vehicleno,Userid,Status,Firstname,Lastname,Lastmod,Startdate,Enddate")] VehiclePolicies _Policies)
        {
            if (id != _Policies.Id)
            {
                return NotFound();
            }

            await _akvclient.UpdateSecret(_Policies);
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(_Policies);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VehPoliciesMasterExists(_Policies.Id))
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
            return View(_Policies);
        }

        // GET: VehiclePolicies/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var _Policies = await _context.VehPoliciesMaster
                .SingleOrDefaultAsync(m => m.Id == id);
            if (_Policies == null)
            {
                return NotFound();
            }

            return View(_Policies);
        }

        // POST: VehiclePolicies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var _Policies = await _context.VehPoliciesMaster.SingleOrDefaultAsync(m => m.Id == id);
            _context.VehPoliciesMaster.Remove(_Policies);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool VehPoliciesMasterExists(string id)
        {
            return _context.VehPoliciesMaster.Any(e => e.Id == id);
        }
    }
}
