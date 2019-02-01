using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Aukcija.Models;

namespace Aukcija.Controllers
{
    public class TokensController : Controller
    {
        private Entities db = new Entities();

        // GET: Tokens/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details()
        {
            Token token = db.Token.FirstOrDefault();

            if (token == null)
            {
                return HttpNotFound();
            }
            return View(token);
        }

        // GET: Tokens/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit()
        {
            Token token = db.Token.FirstOrDefault();
            if (token == null)
            {
                return HttpNotFound();
            }
            return View(token);
        }

        // POST: Tokens/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Token token)
        {
            if (ModelState.IsValid)
            {
                if(!token.ActiveCurrency.Equals("RSD") && !token.ActiveCurrency.Equals("USD") && !token.ActiveCurrency.Equals("EUR"))
                {
                    token.ActiveCurrency = "RSD";
                }
                db.Entry(token).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details");
            }
            return View(token);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
