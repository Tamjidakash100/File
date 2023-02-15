using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using File.Data;
using File.Models;
using Microsoft.Net.Http.Headers;
using System.IO;
using static System.Net.WebRequestMethods;

namespace File.Controllers
{
    public class FilesController : Controller
    {
        private readonly FileDbContext _context;
        private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment _hostingEnvironment;

        public FilesController(FileDbContext context, Microsoft.AspNetCore.Hosting.IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment= hostingEnvironment;
        }
        public IActionResult FileList(int id)
        {

            var fileList = _context.files.Include(f=>f.Person).Where(c=>c.PersonId==id).ToList();
            return View(fileList);
        }
        

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.files == null)
            {
                return NotFound();
            }

            var files = await _context.files
                .Include(f => f.Person.Name)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (files == null)
            {
                return NotFound();
            }

            return View(files);
        }

        public IActionResult Create()
        {
            ViewData["PersonId"] = new SelectList(_context.person, "Id", "Name");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Create(List<IFormFile> files, int personId)
        {
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
                            PersonId = personId,
                            ContentType = file.ContentType
                        };

                        _context.files.Add(fileEntity);
                    }
                }
            }

            await _context.SaveChangesAsync();

            
            return RedirectToAction("Index", "Person");
        }
        public async Task<IActionResult> Edit(int id)
        {

            var file = await _context.files.FindAsync(id);
            
            if (file == null)
            {
                return NotFound();
            }
            ViewData["PersonId"] = new SelectList(_context.person, "Id", "Name", file.PersonId);
            
            return View(file);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormFile file,Files files)
        {
            string folderName = "Files";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string newPath = Path.Combine(webRootPath, folderName);
            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }
            
                if (file.Length > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string fullPath = Path.Combine(newPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);

                    var fileEntity = _context.files.FirstOrDefault( c => c.Id==id);
                    
                    if(fileEntity == null || files.Id!=fileEntity.Id)
                    {
                        return NotFound();
                    }
                    int personId = fileEntity.PersonId;
                    System.IO.File.Delete(Path.Combine(newPath,fileEntity.FileName));
                    fileEntity.FileName=fileName;
                    fileEntity.ContentType= file.ContentType;
                    try
                    {
                        _context.files.Update(fileEntity);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!FilesExists(id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    System.IO.File.Delete(fileName);
                    return RedirectToAction("FileList", "Files", new { id = personId });
                }
                
            }
                
            ViewData["PersonId"] = new SelectList(_context.person, "Id", "Id", files.PersonId);
            return View(files);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.files == null)
            {
                return NotFound();
            }

            var files = await _context.files
                .Include(f => f.Person)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (files == null)
            {
                return NotFound();
            }

            return View(files);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            if (_context.files == null)
            {
                return Problem("Entity set 'FileDbContext.files'  is null.");
            }
            var files = await _context.files.FindAsync(id);
            if(files==null)
            {
                return NotFound();
            }
            string folderName = "Files";
            string webRootPath = _hostingEnvironment.WebRootPath;
            string filePath = Path.Combine(webRootPath, folderName,files.FileName);
            int personId = files.PersonId;

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
            if (files != null)
            {
                _context.files.Remove(files);
            }
            

            await _context.SaveChangesAsync();
            return RedirectToAction("FileList", "Files", new { id = personId});
        }

        private bool FilesExists(int id)
        {
          return _context.files.Any(e => e.Id == id);
        }
    }
}
