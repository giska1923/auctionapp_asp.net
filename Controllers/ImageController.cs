using Aukcija.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Aukcija.Controllers
{
    public class ImageController : Controller
    {
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }
        
       //[HttpPost]
       //public ActionResult Add(Image imageModel)
       //{
       //    string fileName = imageModel.ImageFile.FileName;
       //    imageModel.Path = "~/Images/" + fileName;
       //    fileName = Path.Combine(Server.MapPath("~/Images/"), fileName);
       //    imageModel.ImageFile.SaveAs(fileName);
       //    using (Entities db = new Entities())
       //    {
       //        db.Image.Add(imageModel);
       //        db.SaveChanges();
       //    }
       //
       //    ModelState.Clear();
       //    return View();
       //}
        [HttpGet]
        public ActionResult View(int id)
        {
            Image imageModel = new Models.Image();
            using (Entities db = new Entities())
            {
                imageModel = db.Image.Where(x => x.Id == id).FirstOrDefault();
            }
            return View(imageModel);
        }
        
    }
}