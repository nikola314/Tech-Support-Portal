using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using TechSupportPortal;
using TechSupportPortal.Models;

namespace TechSupportPortal.Controllers
{
    public class AccountsController : Controller
    {
        private MyDbContext db = new MyDbContext();

        // GET: Accounts
        public ActionResult Index()
        {
            return View(db.Accounts.ToList());
        }

        //public ActionResult HashPasswordsHack()
        //{
        //    var users = db.Accounts.ToList();
        //    foreach(var user in users)
        //    {
        //        byte[] salt;
        //        new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
        //        var pbkdf2 = new Rfc2898DeriveBytes(user.Password, salt, 10000);
        //        byte[] hash = pbkdf2.GetBytes(20);
        //        byte[] hashBytes = new byte[36];
        //        Array.Copy(salt, 0, hashBytes, 0, 16);
        //        Array.Copy(hash, 0, hashBytes, 16, 20);
        //        string savedPasswordHash = Convert.ToBase64String(hashBytes);
        //        user.Password = user.ConfirmPassword = savedPasswordHash;
        //        db.Entry(user).State = EntityState.Modified;            
        //    }
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        // GET: Accounts/Details/
        public ActionResult Details()
        {
            Account account = Session["user"] as Account;
            if (account == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View(account);
        }

        // GET: Accounts/Register
        public ActionResult Register()
        {
            return View();
        }

        public ActionResult CreateAgent()
        {
            var user = Session["user"] as Account;
            if(user == null || user.Role != AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } 
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAgent([Bind(Include = "AccountId,FirstName,LastName,Mail,Username,Password,ConfirmPassword")] Account account)
        {
            var user = Session["user"] as Account;
            if (user == null || user.Role != AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            account.Role = AccountRole.Agent;
            account.Status = AccountStatus.Active;
            account.Tokens = 0;

            var usernameExists = db.Accounts.Where(p => p.Username == account.Username).FirstOrDefault();
            if (usernameExists != null)
            {
                ModelState.AddModelError("Username", "Same username already exists.");
            }

            if (ModelState.IsValid)
            {
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                var pbkdf2 = new Rfc2898DeriveBytes(account.Password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                string savedPasswordHash = Convert.ToBase64String(hashBytes);
                account.Password = account.ConfirmPassword = savedPasswordHash;
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(account);
        }


        // POST: Accounts/Register
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register([Bind(Include = "AccountId,FirstName,LastName,Mail,Username,Password,ConfirmPassword,Tokens,Role,Status")] Account account)
        {
            account.Role = AccountRole.Client;
            account.Status = AccountStatus.Active;
            account.Tokens = 0;

            var usernameExists = db.Accounts.Where(p => p.Username == account.Username).FirstOrDefault();
            if (usernameExists!=null)
            {
                ModelState.AddModelError("Username", "Same username already exists.");
            }

            if (ModelState.IsValid)
            {
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);
                var pbkdf2 = new Rfc2898DeriveBytes(account.Password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[36];
                Array.Copy(salt, 0, hashBytes, 0, 16);
                Array.Copy(hash, 0, hashBytes, 16, 20);
                string savedPasswordHash = Convert.ToBase64String(hashBytes);
                account.Password = account.ConfirmPassword = savedPasswordHash;
                db.Accounts.Add(account);
                db.SaveChanges();
                return RedirectToAction("Login");
            }

            return View(account);
        }

        // GET: Accounts/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Accounts/Login
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login([Bind(Include = "Username,Password")] Account account)
        {
            Account user = db.Accounts.Where(u => u.Username == account.Username).Include(u=>u.AgentChannels).FirstOrDefault();
            bool wrongPassword = false;
            string savedPasswordHash = user.Password;
            /* Extract the bytes */
            byte[] hashBytes = Convert.FromBase64String(savedPasswordHash);
            /* Get the salt */
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            /* Compute the hash on the password the user entered */
            var pbkdf2 = new Rfc2898DeriveBytes(account.Password, salt, 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            /* Compare the results */
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash[i])
                {
                    ModelState.AddModelError("Password", "Wrong password.");
                    wrongPassword = true;
                    break;
                }
                    

            if (user == null)
            {
                ModelState.AddModelError("Username", "Username does not exist.");
            }
            else if (user.Status == AccountStatus.Inactive)
            {
                ModelState.AddModelError("Username", "Your account is not active");
            }
            else
            {
                if (wrongPassword) return View(account);
                Session["user"] = user;
                return RedirectToAction("Index", "Home");
            }          
            return View(account);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }


        // GET: Accounts/Edit/5
        public ActionResult Edit(int? id)
        {
            var user = Session["user"] as Account;
            if(user == null || id==null || user.Role!= AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "AccountId,Password, ConfirmPassword,FirstName,LastName,Mail,Username,Tokens,Role,Status")] Account account)
        {
            var user = Session["user"] as Account;
            if (user == null  || user.Role != AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //account.ConfirmPassword = account.Password;
            if (ModelState.IsValid)
            {
                db.Entry(account).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(account);
        }

        // GET: Accounts/Delete/5
        public ActionResult Delete(int? id)
        {
            var user = Session["user"] as Account;
            if (user == null || id == null || user.Role != AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            if (account == null)
            {
                return HttpNotFound();
            }
            return View(account);
        }

        // POST: Accounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var user = Session["user"] as Account;
            if (user == null || id == null || user.Role != AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Account account = db.Accounts.Find(id);
            db.Accounts.Remove(account);
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
