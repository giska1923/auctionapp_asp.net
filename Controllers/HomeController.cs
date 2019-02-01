using Aukcija.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.Text.RegularExpressions;

namespace Aukcija.Controllers
{
    public class HomeController : Controller
    {
        Entities db = new Entities();
        public ActionResult Index(string search, string minPrice, string maxPrice, bool? statusRD, bool? statusOP, bool? statusCO, int? page)
        {
            int pageSize = (int)db.Token.FirstOrDefault().NumN;
            int pageNumber = (page ?? 1);
            ViewBag.minPrice = "";
            ViewBag.maxPrice = "";
            ViewBag.RD = false;
            ViewBag.OP = false;
            ViewBag.CO = false;
            checkDatesOfAuctions();
            if (search == null)
            {
                ViewBag.Search = search = "";
            }
            else
            {
                ViewBag.Search = search;
            }
            List<Auction> list = new List<Auction>();
            IQueryable<Auction> query = null;
            decimal minPriceDec = 0;
            decimal maxPriceDec = Decimal.MaxValue;
            string[] searchStringArray = Regex.Split(search, @"\W+");

            if (minPrice != null && minPrice != "")
            {
                ViewBag.minPrice = minPrice;
                minPriceDec = Decimal.Parse(minPrice);
            }
            if (maxPrice != null && maxPrice != "")
            {
                ViewBag.maxPrice = maxPrice;
                maxPriceDec = Decimal.Parse(maxPrice);
            }
            bool RD, OP, CO;
            ViewBag.RD = RD = (statusRD == null ? false : (bool)statusRD);
            ViewBag.OP = OP = (statusOP == null ? false : (bool)statusOP);
            ViewBag.CO = CO = (statusCO == null ? false : (bool)statusCO);
            query = checkStatus(RD, OP, CO);

            if(query != null)
            {
                if (searchStringArray.Count() > 1)
                {
                    foreach (string it in searchStringArray)
                    {
                        list = list.Union(query.Where(x => x.Name.Contains(it))
                                                    .Where(x => x.StartingP > minPriceDec)
                                                    .Where(x => x.StartingP < maxPriceDec)
                                                    .ToList()).ToList();
                    }
                }
                else if (search != "")
                {
                    list = query.Where(x => x.Name.Contains(search))
                                        .Where(x => x.StartingP > minPriceDec)
                                        .Where(x => x.StartingP < maxPriceDec)
                                        .ToList();
                }
                else
                {
                    list = query.Where(x => x.StartingP > minPriceDec)
                                        .Where(x => x.StartingP < maxPriceDec)
                                        .ToList();
                }
            }
            else
            {
                if (searchStringArray.Count() > 1)
                {
                    foreach (string it in searchStringArray)
                    {
                        list = list.Union(db.Auction.Where(x => x.Name.Contains(it))
                                                    .Where(x => x.StartingP > minPriceDec)
                                                    .Where(x => x.StartingP < maxPriceDec)
                                                    .ToList()).ToList();
                    }
                }
                else if (search != "")
                {
                    list = db.Auction.Where(x => x.Name.Contains(search))
                                        .Where(x => x.StartingP > minPriceDec)
                                        .Where(x => x.StartingP < maxPriceDec)
                                        .ToList();
                }
                else
                {
                    list = db.Auction.Where(x => x.StartingP > minPriceDec)
                                        .Where(x => x.StartingP < maxPriceDec)
                                        .ToList();
                }
            }
            return View(list.ToPagedList(pageNumber, pageSize));
        }

        private IQueryable<Auction> checkStatus(bool statusRD, bool statusOP, bool statusCO)
        {
            IQueryable<Auction> query = null;

            if(statusRD && statusOP && statusCO)
            {
                return null;
            }

            if (statusRD)
            {
                query = db.Auction.Where(x => x.Status == "RD");
            }

            if (statusOP)
            {
                if (query == null)
                {
                    query = db.Auction.Where(x => x.Status == "OP");
                }
                else
                {
                    query = db.Auction.Where(x => x.Status == "RD" || x.Status == "OP");
                }
            }
            if (statusCO)
            {
                if(query == null)
                {
                    query = db.Auction.Where(x => x.Status == "CO");
                }
                else
                {
                    if(statusRD && !statusOP)
                    {
                        query = db.Auction.Where(x => x.Status == "CO" || x.Status == "RD");
                    }
                    else if(!statusRD && statusOP)
                    {
                        query = db.Auction.Where(x => x.Status == "CO" || x.Status == "OP");
                    }
                }
            }
            return query;
        }

        private void checkDatesOfAuctions()
        {
            List<Auction> list = db.Auction.ToList();
            if (list != null && list.Count > 0)
            {
            foreach (var auction in list)
            {
                if (auction.Status == "OP")
                {
                    if (DateTime.Compare(DateTime.Now, (DateTime)auction.Closed) >= 0)
                    {
                        auction.Status = "CO";
                        var bid = db.Bid.Where(x => x.GUIDAuction == auction.GUID)
                                        .OrderByDescending(x => x.Timestamp)
                                        .FirstOrDefault();
                        if (bid != null)
                        {
                            auction.AspNetUsers.TokenSum += bid.TokenNum;
                        }
                    }
                }
            }
            db.SaveChanges();
            }
        }

        public ActionResult About()
        {
            var token = db.Token.FirstOrDefault();
            ViewBag.ActiveCurr = token.ActiveCurrency;
            ViewBag.EUR = ((decimal)token.EUR).ToString("0.##");
            ViewBag.USD = ((decimal)token.USD).ToString("0.##");
            ViewBag.RSD = ((decimal)token.RSD).ToString("0.##");
            return View();
        }

        [Authorize(Roles = "User")]
        public ActionResult AddAuction()
        {
            Token token = db.Token.FirstOrDefault();
            ViewBag.numD = (int)token.NumD;
            ViewBag.Currency = token.ActiveCurrency;
            return View();
        }

        [HttpPost]
        public ActionResult AddAuction(AuctionImageVM model)
        {
            string fileName = Path.GetFileNameWithoutExtension(model.ImageFile.FileName);
            string extension = Path.GetExtension(model.ImageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            model.Path = "~/Images/" + fileName;
            fileName = Path.Combine(Server.MapPath("~/Images/"), fileName);
            model.ImageFile.SaveAs(fileName);
            Image image = new Image
            {
                Name = model.Name,
                Path = model.Path
            };
            int idImg = addImage(image);

            Auction auction = new Auction
            {
                Name = model.Name,
                Duration = model.Duration,
                StartingP = model.StartingP,
                IdImage = idImg
            };
            addAuction(auction);
            ModelState.Clear();
            return RedirectToAction("Index");
        }

        private void addAuction(Auction model)
        {
            model.GUID = Guid.NewGuid();
            model.Created = DateTime.Now;
            model.Status = "RD";
            model.CurrentP = 0;
            model.IdUser = User.Identity.GetUserId();
            Token token = db.Token.FirstOrDefault();
            model.Currency = token.ActiveCurrency;
            db.Auction.Add(model);
            db.SaveChanges();
        }

        private int addImage(Image imageModel)
        {
            db.Image.Add(imageModel);
            db.SaveChanges();

            return imageModel.Id;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult OpenAuction(string openedAuctionGuid, int? page)
        {
            int pageSize = (int)db.Token.FirstOrDefault().NumN;
            int pageNumber = (page ?? 1);
            if (openedAuctionGuid != null)
            {
                Guid guid = Guid.Parse(openedAuctionGuid);
                Auction model = db.Auction.Where(x => x.GUID == guid).FirstOrDefault();
                model.CurrentP = model.StartingP;
                model.Opened = DateTime.Now;
                model.Closed = ((DateTime)(model.Opened)).AddSeconds(model.Duration);
                model.Status = "OP";
                db.SaveChanges();
            }
            return View(db.Auction.Where(x => x.Status == "RD").ToList().ToPagedList(pageNumber, pageSize));
        }
        
        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult BidNow(Guid id, BidNowStatusMsg? message)
        {
            ViewBag.StatusMessage =
               message == BidNowStatusMsg.NotEnoughTokens ? "You don't have enough tokens for that action."
               : message == BidNowStatusMsg.LowOffer ? "Your offer must be higher than the last one."
               : "";
            var product = db.Auction.Where(x => x.GUID == id).FirstOrDefault();
            ViewBag.Email = product.AspNetUsers.Email;
            ViewBag.Guid = id.ToString();

            var token = db.Token.FirstOrDefault();
            int tokens = 0;
            switch (product.Currency)
            {
                case "RSD":
                    ViewBag.CurrencyVal = ((decimal)(token.RSD)).ToString("0.##");
                    tokens = (int)(product.CurrentP / token.RSD);
                    break;
                case "USD":
                    ViewBag.CurrencyVal = ((decimal)(token.USD)).ToString("0.##");
                    tokens = (int)(product.CurrentP / token.USD);
                    break;
                case "EUR":
                    ViewBag.CurrencyVal = ((decimal)(token.EUR)).ToString("0.##");
                    tokens = (int)(product.CurrentP / token.EUR);
                    break;
            }
            var bnvm = new BidNowVM()
            {
                Name = product.Name,
                Duration = product.Duration,
                Closed = (DateTime)product.Closed,
                CurrentP = product.CurrentP,
                Currency = product.Currency,
                Email = product.AspNetUsers.Email,
                ImagePath = product.Image.Path,
                ImageName = product.Image.Name
            };

            ViewBag.listBids = db.Bid.Where(x => x.GUIDAuction == id).OrderByDescending(x => x.Timestamp).ToList();
            ViewBag.tokensNow = tokens;
            return View(bnvm);
        }

        [HttpGet]
        [Authorize(Roles = "User")]
        public ActionResult BidNowOffer(string offer, Guid id)
        {
            int tokenToOffer = int.Parse(offer);
            var product = db.Auction.Where(x => x.GUID == id).FirstOrDefault();
            string userid = User.Identity.GetUserId();
            var user = db.AspNetUsers.Where(x => x.Id == userid).FirstOrDefault();

            if (tokenToOffer > user.TokenSum)
            {
                return RedirectToAction("BidNow", new { id = id, message = BidNowStatusMsg.NotEnoughTokens });
            }
            var token = db.Token.FirstOrDefault();
            decimal offerInCurrency = 0;

            switch (product.Currency)
            {
                case "RSD":
                    offerInCurrency = tokenToOffer * (decimal)token.RSD;
                    break;
                case "USD":
                    offerInCurrency = tokenToOffer * (decimal)token.USD;
                    break;
                case "EUR":
                    offerInCurrency = tokenToOffer * (decimal)token.EUR;
                    break;
            }
            if(offerInCurrency <= product.CurrentP)
            {
                return RedirectToAction("BidNow", new { id = id, message = BidNowStatusMsg.LowOffer });
            }
            
            product.CurrentP = offerInCurrency;
            user.TokenSum -= tokenToOffer;

            var bid = db.Bid.Where(x => x.GUIDAuction == id)
                            .OrderByDescending(x => x.Timestamp)
                            .FirstOrDefault();
            if(bid != null)
            {
                bid.AspNetUsers.TokenSum += bid.TokenNum;
            }

            bid = new Bid()
            {
                GUIDAuction = id,
                IdUser = userid,
                TokenNum = tokenToOffer,
                Timestamp = DateTime.Now
            };
            db.Bid.Add(bid);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public JsonResult DisplayObject(Guid id)
        {
            var product = db.Auction.Where(x => x.GUID == id).FirstOrDefault();
            return Json(new { CurrentP = product.CurrentP.ToString("0.##"), Currency = product.Currency }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetLastBid(Guid id)
        {
            var bid = db.Bid.Where(x => x.GUIDAuction == id).OrderByDescending(x => x.Timestamp).FirstOrDefault();
            if (bid == null) return null;
            return Json(new { Email = bid.AspNetUsers.UserName, TokensBidded = bid.TokenNum.ToString(), Timestamp = bid.Timestamp.ToString() }, JsonRequestBehavior.AllowGet);
        }

        public enum BidNowStatusMsg
        {
            NotEnoughTokens, LowOffer
        }

    }
}