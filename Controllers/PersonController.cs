using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using File.Data;
using File.Models;

namespace File.Controllers
{
    public class PersonController : Controller
    {
        private readonly FileDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        public PersonController(IWebHostEnvironment hostingEnvironment, FileDbContext context)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<IActionResult> Index()
        {
              return View(await _context.person.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.person == null)
            {
                return NotFound();
            }

            var person = await _context.person
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(List<IFormFile> files, Person person )
        {
            string name = person.Name;
            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
            }
            var getperson = _context.person.FirstOrDefault(f=> f.Name==name);
            if(getperson== null)
            {
                return NotFound();
            }
            string folderName = "Files";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string fullPath = Path.Combine(newPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);

                        var fileEntity = new Files
                        {
                            FileName = fileName,
                            PersonId = getperson.Id,
                            ContentType = file.ContentType
                        };

                        _context.files.Add(fileEntity);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return View(person);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.person == null)
            {
                return NotFound();
            }

            var person = await _context.person.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PersonExists(person.Id))
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
            return View(person);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.person == null)
            {
                return NotFound();
            }

            var person = await _context.person
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.person == null)
            {
                return Problem("Entity set 'FileDbContext.person'  is null.");
            }
            var person = await _context.person.FindAsync(id);
            if (person != null)
            {
                _context.person.Remove(person);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
          return _context.person.Any(e => e.Id == id);
        }
    }
}
