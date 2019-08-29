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
    public class ChannelsController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // GET: Channels
        public ActionResult Index()
        {
            Account user = Session["user"] as Account;
            if (user == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Channel> ch = null;
            var userWithChannels = db.Accounts.Include(p => p.AgentChannels).Include(p => p.UserChannels).Where(a => a.AccountId == user.AccountId).FirstOrDefault();
            if (user.Role == AccountRole.Client)
            {
                ch = userWithChannels.UserChannels.Where(c=> c.IsOpen==true).ToList();
            }
            else if(user.Role == AccountRole.Agent)
            {
                //channels = userWithChannels.AgentChannels.ToList();
                ch = db.Channels.Include(c => c.Questions).Include(c=>c.Agents).Where(c=>c.IsOpen==true).ToList();
            }
            // For agent joining and leaving channels
            ViewBag.channels = user.AgentChannels.ToList();
            return View(ch);
        }

        public ActionResult ToggleJoinLeaveChannel(int? channelId)
        {
            var user = Session["user"] as Account;
            if (user.Role != AccountRole.Agent) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Channel channel = db.Channels.Include(ca => ca.Agents).Where(ca => ca.ChannelId == channelId).FirstOrDefault();
            
            var usr = db.Accounts.Where(a => a.AccountId == user.AccountId).Include(a => a.AgentChannels).FirstOrDefault();
            if (channel.Agents.Contains(usr))
            {
                // Leave
                usr.AgentChannels.Remove(channel);
                channel.Agents.Remove(usr);
            }
            else
            {
                // Join
                usr.AgentChannels.ToList().Add(channel);
                channel.Agents.Add(usr);
            }                  
            db.SaveChanges();
           db.Entry(channel).Reload();
            db.Entry(usr).Reload();

            Session["user"] = usr;
            return RedirectToAction("Index");
        }

        public ActionResult CloseChannel(int? channelId)
        {
            var user = Session["user"] as Account;
            var channel = db.Channels.Find(channelId);
            if(user.AccountId == channel.AccountId)
            {
                channel.IsOpen = false;
                db.Entry(channel).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Channel(int? id, int? page = 1)
        {
            var questions = db.Channels.Where(c => c.ChannelId == id).Include(c => c.Questions).FirstOrDefault().Questions;

            ViewBag.count = questions.Count();

            
            ViewBag.id = id;
            return View(questions); // TODO: pass questionlist as argument
        }



        // GET: Channels/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            return View(channel);
        }

        // GET: Channels/Create
        public ActionResult Create()
        {
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName");
            return View();
        }

        // POST: Channels/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ChannelId,Name,CreatedAt")] Channel channel)
        {
            var user = Session["user"] as Account;
            if(user == null || user.Role!= AccountRole.Client)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            channel.AccountId = user.AccountId;
            channel.IsOpen = true;
            var price = db.Packs.Where(p => p.Amount == -1).FirstOrDefault().Price;
            if (ModelState.IsValid && user.Tokens>price)
            {
                db.Channels.Add(channel);
                user.Tokens = user.Tokens - price;
                user.ConfirmPassword = user.Password;
                db.Entry(user).State = EntityState.Modified;

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", channel.AccountId);
            return View(channel);
        }

        // GET: Channels/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", channel.AccountId);
            return View(channel);
        }

        // POST: Channels/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ChannelId,Name,CreatedAt,AccountId,IsOpen")] Channel channel)
        {
            if (ModelState.IsValid)
            {
                db.Entry(channel).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", channel.AccountId);
            return View(channel);
        }

        // GET: Channels/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Channel channel = db.Channels.Find(id);
            if (channel == null)
            {
                return HttpNotFound();
            }
            return View(channel);
        }

        // POST: Channels/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Channel channel = db.Channels.Find(id);
            db.Channels.Remove(channel);
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
