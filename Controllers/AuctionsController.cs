using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Aukcija.Models;
using Microsoft.AspNet.Identity;
using PagedList;

namespace Aukcija.Controllers
{
    public class AuctionsController : Controller
    {
        private Entities db = new Entities();

        // GET: Auctions/Details/5
        public ActionResult Details(Guid id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            if (auction.Status == "OP")
            {
                if (DateTime.Compare(DateTime.Now, (DateTime)auction.Closed) >= 0)
                {
                    auction.Status = "CO";
                    var bid = db.Bid.Where(x => x.GUIDAuction == id)
                                    .OrderByDescending(x => x.Timestamp)
                                    .FirstOrDefault();
                    if (bid != null)
                    {
                        auction.AspNetUsers.TokenSum += bid.TokenNum;
                    }
                    db.SaveChanges();
                }
            }

            ViewBag.listBids = db.Bid.Where(x => x.GUIDAuction == id).OrderByDescending(x => x.Timestamp).ToList();
            return View(auction);
        }

        public JsonResult SwitchQueues(Guid id)
        {
            var auction = db.Auction.Where(x => x.GUID == id).FirstOrDefault();
            if (auction.Status == "OP")
            {
                if (DateTime.Compare(DateTime.Now, (DateTime)auction.Closed) >= 0)
                {
                    auction.Status = "CO";
                    var bid = db.Bid.Where(x => x.GUIDAuction == id)
                                    .OrderByDescending(x => x.Timestamp)
                                    .FirstOrDefault();
                    if (bid != null)
                    {
                        auction.AspNetUsers.TokenSum += bid.TokenNum;
                    }
                    db.SaveChanges();
                }
            }
            return Json(new { Status = auction.Status }, JsonRequestBehavior.AllowGet);
        }

        // GET: Auctions/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            ViewBag.IdUser = new SelectList(db.AspNetUsers, "Id", "FirstName", auction.IdUser);
            ViewBag.IdImage = new SelectList(db.Image, "Id", "Name", auction.IdImage);
            return View(auction);
        }

        // POST: Auctions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "GUID,Name,Duration,StartingP,CurrentP,Created,Opened,Closed,Status,IdUser,IdImage,Currency")] Auction auction)
        {
            if (ModelState.IsValid)
            {
                db.Entry(auction).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.IdUser = new SelectList(db.AspNetUsers, "Id", "FirstName", auction.IdUser);
            ViewBag.IdImage = new SelectList(db.Image, "Id", "Name", auction.IdImage);
            return View(auction);
        }

        // GET: Auctions/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Auction auction = db.Auction.Find(id);
            if (auction == null)
            {
                return HttpNotFound();
            }
            return View(auction);
        }

        // POST: Auctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            Auction auction = db.Auction.Find(id);
            db.Auction.Remove(auction);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //Treba da se napravi View
        [Authorize(Roles = "User")]
        public ActionResult ShowWonAuctions(int? page)
        {
            int pageSize = (int)db.Token.FirstOrDefault().NumN;
            int pageNumber = (page ?? 1);
            string userid = User.Identity.GetUserId();
            var list1 = db.Bid.Where(x => x.Auction.Status == "CO")
                             .OrderByDescending(x => x.Timestamp)
                             .ToList();

            List<Bid> list = new List<Bid>();
            string last = null;
            foreach(var item in list1)
            {
                if(last == null || !item.GUIDAuction.Equals(Guid.Parse(last)))
                {
                    if(item.IdUser == userid)
                    {
                        list.Add(item);
                    }
                    last = item.GUIDAuction.ToString();
                }
            }
            return View(list.ToPagedList(pageNumber, pageSize));
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
