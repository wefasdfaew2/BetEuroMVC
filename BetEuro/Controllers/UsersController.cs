﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BetEuro;
using System.Net.Mail;

namespace BetEuro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private BEEntities db = new BEEntities();

        // GET: Users
        public async Task<ActionResult> Index()
        {
            var users = db.Users.Include(u => u.Leaderboard);
            return View(await users.ToListAsync());
        }

        // GET: Users/Details/5
        public async Task<ActionResult> Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.Id = new SelectList(db.Leaderboards, "UserId", "UserId");
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Id,Piwo,isActive,Comment,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.Id = new SelectList(db.Leaderboards, "UserId", "UserId", user.Id);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            ViewBag.Id = new SelectList(db.Leaderboards, "UserId", "UserId", user.Id);
            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "Id,Piwo,Paid,isActive,Comment,Email,EmailConfirmed,PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Single(p => p.Id == user.Id).isActive = user.isActive;
                db.Users.Single(p => p.Id == user.Id).Piwo = user.Piwo;
                db.Users.Single(p => p.Id == user.Id).Paid = user.Paid;
                await db.SaveChangesAsync();

                UpdateLeaderboard();

                return RedirectToAction("Index");
            }
            ViewBag.Id = new SelectList(db.Leaderboards, "UserId", "UserId", user.Id);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            User user = await db.Users.FindAsync(id);
            foreach (Bet bet in db.Bets.Where(p => p.UserId == id))
                db.Bets.Remove(bet);
            if (db.Leaderboards.Any(p => p.UserId == id))
            {
                Leaderboard ld = await db.Leaderboards.SingleAsync(p => p.UserId == id);
                db.Leaderboards.Remove(ld);
            }
            db.Users.Remove(user);
            await db.SaveChangesAsync();

            UpdateLeaderboard();

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

        private void UpdateLeaderboard()
        {
            db.Database.ExecuteSqlCommand("TRUNCATE TABLE [Leaderboard]");
            db.SaveChanges();

            foreach (User u in db.Users.Where(p => p.isActive))
            {
                Leaderboard lb = new Leaderboard();
                lb.User = u;
                lb.UserId = u.Id;
                lb.PlacedBets = 0;
                lb.ResultHit = 0;
                lb.ScoreHit = 0;
                lb.Points = 0;

                foreach (Bet b in u.Bets)
                {
                    Match m = db.Matches.Single(p => p.Id == b.MatchId);

                    if (m.Score != null)
                    {
                        if (m.Score.HomeScore == b.HomeScore && m.Score.AwayScore == b.AwayScore)
                        {
                            //SCORE
                            lb.ScoreHit++;
                            lb.Points += m.Factor.Value * db.Points.Single(p => p.Id == "Score").Points;
                        }
                        else if (m.Score.Result == b.Result)
                        {
                            //RESULT
                            lb.ResultHit++;
                            lb.Points += m.Factor.Value * db.Points.Single(p => p.Id == "Result").Points;
                        }
                        else
                        {
                            // BET POINTS
                            lb.PlacedBets++;
                            lb.Points += m.Factor.Value * db.Points.Single(p => p.Id == "Bet").Points;
                        }
                    }

                }

                db.Leaderboards.Add(lb);
            }

            db.SaveChanges();
        }

        private async Task SendMail(string email, string subject, string body)
        {
            var credentialUserName = "ivaan.development@gmail.com";
            var sentFrom = "beteuro.com.pl";
            var pwd = "para$OLKA17";

            MailAddress from = new MailAddress(credentialUserName, sentFrom);
            MailAddress to = new MailAddress(email);

            // Configure the client:
            System.Net.Mail.SmtpClient client =
                new System.Net.Mail.SmtpClient("smtp.gmail.com");

            client.Port = 587;
            client.EnableSsl = true;
            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;

            // Creatte the credentials:
            System.Net.NetworkCredential credentials =
                new System.Net.NetworkCredential(credentialUserName, pwd);

            client.EnableSsl = true;
            client.Credentials = credentials;

            // Create the message:
            var mail =
                new System.Net.Mail.MailMessage(from, to);

            mail.Subject = subject;
            mail.Body = body;

            // Send:
            await client.SendMailAsync(mail);
        }
    }
}
