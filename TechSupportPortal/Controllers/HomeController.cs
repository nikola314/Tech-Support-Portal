using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TechSupportPortal.Models;

namespace TechSupportPortal.Controllers
{
    
    public class HomeController : Controller
    {
        private MyDbContext db = new MyDbContext();

        public ActionResult Modifikacija()
        {
            var user = Session["user"] as Account;
            if(user==null || user.Role!= AccountRole.Admin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var clients = db.Accounts.Where(a => a.Role == AccountRole.Client).Count();
            var agents = db.Accounts.Where(a => a.Role == AccountRole.Agent).Count();
            ViewBag.clients = clients;
            ViewBag.agents = agents;

            var orders = db.Orders.ToList();
            var sum = 0;
            foreach(var order in orders)
            {
                sum += order.Price;
            }
            ViewBag.sum = sum;

            return View();
        }


        public ActionResult Index(string category = null, int? ofUser = null, string search = null, int? page = 1, int? channel = null)
        {
            List<Question> questions = new List<Question>();
            if (channel == null)
            {
                var categoryFilter = db.Categories.Where(c => c.Name == category).FirstOrDefault();
                if (category == "all") categoryFilter = null;
                var userFilter = db.Accounts.Where(a => a.AccountId == ofUser).FirstOrDefault();
                if (categoryFilter != null)
                {
                    questions = categoryFilter.Questions.ToList().Where(a=>a.Channels.Count == 0).ToList();
                }
                else
                {
                    questions = db.Actions.OfType<Question>().Include(a=>a.Channels).Where(a=>a.Channels.Count == 0).ToList();
                }
                if (search == "") search = null;
                if (search != null)
                {
                    var terms = search.Split(' ');
                    var all = questions.ToList();
                    var result = new List<Question>();
                    foreach(var q in all)
                    {
                        foreach(var term in terms)
                        {
                            if(q.Title.Contains(term))
                            {
                                result.Add(q);
                                break;
                            }
                        }
                    }
                    //questions = questions.Where(q => (q.Title.Contains(search ?? "") || q.Text.Contains(search ?? "") || q.Author.Username.Contains(search ?? ""))).ToList();
                    questions = result;
                }

                if (ofUser != null)
                {
                    questions = questions.Where(q => q.AccountId == ofUser).ToList();
                }
            }
            else
            {
                // TODO: Check if current user can access this page ( bulletproof)
                // QUESTIONS OF A CHANNEL
                questions = db.Channels.Where(c => c.ChannelId == channel).Include(c => c.Questions).FirstOrDefault().Questions.ToList();
                var ch = db.Channels.Where(c => c.ChannelId == channel).FirstOrDefault();
                ViewBag.owner = ch.AccountId;
                ViewBag.channelName = ch.Name;
            }
            ViewBag.count = questions.Count();
            questions = questions.Skip(((page ?? 1) - 1) * Util.ITEMS_PER_PAGE).Take(Util.ITEMS_PER_PAGE).ToList();

            ViewBag.channel = channel;
            ViewBag.page = page;
            ViewBag.categories = db.Categories.ToList();
            ViewBag.category = category;
            ViewBag.ofUser = ofUser;
            ViewBag.search = search;

            return View(questions);
        }

        public ActionResult Question(int? id, SortTypes? sort=null, int? channel= null)
        {
            if(sort!= null)
            {
                if (sort == SortTypes.Time)
                {
                    ViewBag.Responses = db.Actions.OfType<Response>().Where(r => r.RespondingTo.ActionId == id).OrderByDescending(o=>o.CreatedAt).ToList();
                }
                else if (sort == SortTypes.Votes)
                {
                    ViewBag.Responses = db.Actions.OfType<Response>().Where(r => r.RespondingTo.ActionId == id).OrderByDescending(o => o.Upvotes).ToList();
                }
            }
            else
            {
                ViewBag.Responses = db.Actions.OfType<Response>().Where(r => r.RespondingTo.ActionId == id).ToList();
            }
            ViewBag.canReply = true;
            if(channel != null)
            {
                var user = Session["user"] as Account;
                if(user.Role!= AccountRole.Client)
                {
                    var ch = db.Channels.Where(c => c.ChannelId == channel).Include(c => c.Agents).FirstOrDefault();
                    ViewBag.canReply = ch.Agents.ToList().Find(u => u.AccountId == user.AccountId) != null;
                }              
            }          
            Models.Action a = db.Actions.Find(id) as Models.Action;
            return View(a);
        }

        public ActionResult ReplyToQuestion(string ResponseText, string RespondingTo)
        {
            var user = Session["user"] as Account;
            if (user != null)
            {
                var questionId = int.Parse(RespondingTo);
                var q = db.Actions.Find(questionId);
                bool locked = false ;
                if(q!= null)
                {
                    locked = (q as Question).IsLocked;
                }
                if (locked)
                {
                    return RedirectToAction("Question", new { id = int.Parse(RespondingTo) });
                }
                Response response = new Response();
                response.AccountId = user.AccountId;
                response.ActionRespToId = int.Parse(RespondingTo);
                response.Text = ResponseText;
                response.CreatedAt = DateTime.Now;
                db.Actions.Add(response);
                db.SaveChanges();
            }
            return RedirectToAction("Question", new { id = int.Parse(RespondingTo) });
        }

        public ActionResult ToggleQuestionLock(int? id, bool value)
        {
            var question = db.Actions.OfType<Question>().Where(a => a.ActionId == id).FirstOrDefault();
            if (question != null)
            {
                if (value == true)
                {
                    question.LastLockedAt = DateTime.Now;
                }
                question.IsLocked = value;
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Question", new { id = id });
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: Questions/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Question question = db.Actions.OfType<Question>().Where(p => p.ActionId == id).SingleOrDefault();
            if (question == null)
            {
                return HttpNotFound();
            }
            return View(question);
        }

        // GET: Questions/Create
        public ActionResult Create(int? channel = null)
        {
            ViewBag.ActionId = new SelectList(db.Actions, "ActionId", "Text");
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name");
            ViewBag.channel = channel;
            return View();
        }

        // POST: Questions/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ActionId,Text,AccountId,CreatedAt,Title,CategoryId,IsLocked,LastLockedAt")] Question question, int? channel=null, HttpPostedFileBase imageFile = null)
        {
            question.AccountId = (Session["user"] as Account).AccountId;
            question.CreatedAt = DateTime.Now;

            if(imageFile != null && imageFile.ContentLength > 0)
            {
                question.Image = new byte[imageFile.ContentLength];
                imageFile.InputStream.Read(question.Image, 0, imageFile.ContentLength);
            }

            if (ModelState.IsValid)
            {            
                db.Actions.Add(question);
                if (channel != null)
                {
                    db.Channels.Where(c => c.ChannelId == channel).Include(c => c.Questions).FirstOrDefault().Questions.Add(question);
                }              
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", question.AccountId);
            ViewBag.ActionId = new SelectList(db.Actions, "ActionId", "Text", question.ActionId);
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", question.CategoryId);
            return View(question);
        }

        // GET: Questions/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var user = Session["user"] as Account;
            Models.Action action = db.Actions.Where(p => p.ActionId == id).SingleOrDefault();
            if (action == null || user == null || user.AccountId != action.AccountId)
            {
                return HttpNotFound();
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", action.AccountId);
            ViewBag.ActionId = new SelectList(db.Actions, "ActionId", "Text", action.ActionId);
            if (action is Question)
            {
                ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", (action as Question).CategoryId);
            }
            if (action is Question) return View(action as Question);
            return View("EditResponse", action as Response);
        }

        // POST: Questions/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ActionId,Text,Title,Image,CategoryId")] Question question)
        {
            var user = Session["user"] as Account;
            question.AccountId = user.AccountId;
            if (ModelState.IsValid)
            {
                db.Entry(question).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", question.AccountId);
            ViewBag.ActionId = new SelectList(db.Actions, "ActionId", "Text", question.ActionId);
            ViewBag.CategoryId = new SelectList(db.Categories, "CategoryId", "Name", question.CategoryId);
            return View(question);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditResponse([Bind(Include = "ActionId,ActionRespToId,Text")] Response response)
        {
            var user = Session["user"] as Account;
            response.AccountId = user.AccountId;
            if (ModelState.IsValid)
            {
                db.Entry(response).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.AccountId = new SelectList(db.Accounts, "AccountId", "FirstName", response.AccountId);
            ViewBag.ActionId = new SelectList(db.Actions, "ActionId", "Text", response.ActionId);
            return View(response);
        }


        // GET: Questions/Delete/5
        public ActionResult Delete(int? id)
        {
            var user = Session["user"] as Account;
            bool isAdmin = false;
            if (user != null && user.Role == AccountRole.Admin) isAdmin = true;
            Models.Action action = db.Actions.Where(p => p.ActionId == id).SingleOrDefault();
            if (user.AccountId == action.AccountId) isAdmin = true;
            if (id == null || !isAdmin)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            if (action == null)
            {
                return HttpNotFound();
            }
            return View(action);
        }

        public void removeAnswers(Response response)
        {
            while (true)
            {
                Response a = db.Actions.OfType<Response>().Where(r => r.ActionRespToId == response.ActionId).FirstOrDefault();
                if (a == null)
                {
                    db.Actions.Remove(response);
                    db.SaveChanges();
                    break;
                }
                removeAnswers(a);
            }
        }

        // POST: Questions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Models.Action action = db.Actions.Where(p => p.ActionId == id).SingleOrDefault();
            var target = db.Actions.Include(x => x.Responses).FirstOrDefault(x => x.ActionId == action.ActionId);
            while (true)
            {
                Response response = db.Actions.OfType<Response>().Where(a => a.ActionRespToId == id).FirstOrDefault();
                if (response == null)
                {
                    break;
                }
                removeAnswers(response);
            }

            db.Actions.Remove(action);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Vote(int? id, bool value)
        {
            var action = db.Actions.Find(id);
            var user = Session["user"] as Account;
            if (user == null)
            {
                return View(action);
            }
            var u = db.Accounts.Include(a => a.Votes).Where(a => a.AccountId == user.AccountId).FirstOrDefault();
            var response = db.Actions.OfType<Response>().Where(r => r.ActionId == id).FirstOrDefault();
            int from = response.ActionRespToId;
            // User did not vote yet
            if (u.Votes.Where(r => r.ActionId == id).FirstOrDefault() == null)
            {
                if (value == true)
                {
                    response.Upvotes = response.Upvotes + 1;
                }
                else
                {
                    response.Downvotes = response.Downvotes + 1;
                }
                response.Voters.Add(u);
                db.Entry(response).State = EntityState.Modified;
                db.SaveChanges();
            }

            return RedirectToAction("Question", new { id = from });
        }

        public ActionResult Sort(int? id, SortTypes type)
        {
            ViewBag.sort = type;
            return RedirectToAction("Question", new { id = id, sort = type });
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