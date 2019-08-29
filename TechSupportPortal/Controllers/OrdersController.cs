using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TechSupportPortal;
using TechSupportPortal.Models;

namespace TechSupportPortal.Controllers
{
    public class OrdersController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // GET: Orders
        public ActionResult Index()
        {
            var user = Session["user"] as Account;
            if(user == null || user.Role!= AccountRole.Client)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var orders = db.Accounts.Include(a => a.Orders).Where(a => a.AccountId == user.AccountId).FirstOrDefault().Orders; 
            return View(orders);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            var user = Session["user"] as Account;
            if(user==null || user.Role!= AccountRole.Client)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.TokenPacks = db.Packs.ToList();
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "OrderId,TokenPack,Quantity")] Order order)
        {
            var user = Session["user"] as Account;
            if (user == null || user.Role != AccountRole.Client)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var pack = db.Packs.Where(p => p.PackId == (int)order.TokenPack).FirstOrDefault();
            order.Price = pack.Price * order.Quantity;
            order.AccountId = user.AccountId;
            Session["orderToCreate"] = order;
            // TODO: PAYPAL
            return RedirectToAction("PaymentWithPaypal", "Paypal");

        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", order.AccountId);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "OrderId,AccountId,TokenPack,Quantity,Price")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", order.AccountId);
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
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
