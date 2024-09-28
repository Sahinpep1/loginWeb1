using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using loginWeb1.Data;
using loginWeb1.Models;
using WebGrease.Css.ImageAssemblyAnalysis.LogModel;

namespace loginWeb1.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }
        public ActionResult Log()
        {
            return View(db.LoginAttempts.OrderByDescending(l => l.AttemptedAt).ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }
        public ActionResult VerifyEmail()
        {
            return View();
        }
        // GET: Users/Create
        public ActionResult login()
        {
            return View();
        }
        public ActionResult Main(int id)
        {
            var user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            // Kullanıcı adı ve diğer bilgileri view'a gönderiyoruz
            ViewBag.Username = user.Username;
            ViewBag.UserId = user.Id;

            return View();
        }

        // POST: Users/Create
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Username,Email,PasswordHash,confirmPassword,IsVerified,VerificationToken")] User user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                    user.confirmPassword = user.PasswordHash;
                  user.PasswordLastChanged = DateTime.Now;
                  
                    var userpass = new UserPassword
                    {
                        PasswordHash = user.PasswordHash,
                        CreatedAt = DateTime.Now,
                    };
                    db.Users.Add(user);
                    db.UserPasswords.Add(userpass);
                    db.SaveChanges();
                    SendVerificationEmail(user.Email);
                    return RedirectToAction("Index");
                }
                catch (DbUpdateException ex)
                {
                    if (ex.InnerException?.InnerException is SqlException sqlEx && (sqlEx.Number == 2601 || sqlEx.Number == 2627))
                    {
                        ModelState.AddModelError("", "Bu email kullanılıyor.Başka bir email seçin.");
                    }
                    else
                    {
                        // Other database errors
                        ModelState.AddModelError("", "Bir hata oluştu.Lütfen tekrar deneyin.");
                    }
                }
            }

            return View(user);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult login([Bind(Include = "Id,Username,Email,PasswordHash,confirmPassword,IsVerified,VerificationToken")] User user)
        {
            ModelState.Remove("ConfirmPassword");
            ModelState.Remove("Username");
            // Giriş denemesini kaydetmek için LoginAttempt nesnesi oluşturuyoruz
            var loginAttempt = new LoginAttempt
            {
                Email = user.Email,
                AttemptedAt = DateTime.Now,
                IsSuccessful = false, // Başlangıçta başarısız olarak ayarlıyoruz, sonra güncelleyebiliriz
                FailureReason = ""
            };
            
            if (ModelState.IsValid)
            {
                var dbUser = db.Users.FirstOrDefault(u => u.Email == user.Email);

                if (dbUser != null && BCrypt.Net.BCrypt.Verify(user.PasswordHash, dbUser.PasswordHash))
                {
                    if (dbUser.IsVerified == true)
                    {
                        if (dbUser.PasswordLastChanged.AddDays(30) <= DateTime.Now)
                        {
                            loginAttempt.IsSuccessful = true;
                            db.LoginAttempts.Add(loginAttempt);
                            db.SaveChanges();

                            return RedirectToAction("Edit", new { id = dbUser.Id });
                        }
                        else
                        {
                        // Giriş başarılıysa, başarılı olarak işaretleyip kaydediyoruz
                            loginAttempt.IsSuccessful = true;
                             db.LoginAttempts.Add(loginAttempt);
                             db.SaveChanges();

                         return RedirectToAction("main", new { id = dbUser.Id });
                        }
                            
                    }
                    else
                    {
                        loginAttempt.FailureReason = " Lütfen Email adresinizi doğrulayınız .";
                        ModelState.AddModelError("", loginAttempt.FailureReason);
                        ViewBag.VerificationMessage = "Email adresinizi doğrulamadınız. Doğrulama bağlantısını tekrar gönderin.";
                        ViewBag.Email = user.Email;
                        return View(user);
                    }
                    
                }
                else
                {
                    // Hata durumunda hata nedenini kaydediyoruz
                    loginAttempt.FailureReason = "Geçersiz Email veya şifre.";
                    ModelState.AddModelError("", loginAttempt.FailureReason);
                    return View(user);
                }
            }
            else
            {
                // ModelState geçersizse, neden geçersiz olduğunu FailureReason alanında kaydediyoruz
                loginAttempt.FailureReason = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }

            // Başarısız giriş denemesini kaydediyoruz
            db.LoginAttempts.Add(loginAttempt);
            db.SaveChanges();

            // Hatalar varsa kullanıcıya tekrar login sayfasını gösteriyoruz
            return View(user);
        }
        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        // Aşırı gönderim saldırılarından korunmak için, bağlamak istediğiniz belirli özellikleri etkinleştirin, 
        // daha fazla bilgi için bkz. https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Username,Email,PasswordHash,confirmPassword,PasswordLastChanged")] User user)
        {
            if (ModelState.IsValid)
            {
                var lastThreePasswords = db.UserPasswords
                    .Where(up => up.UserId == user.Id)
                    .OrderByDescending(up => up.CreatedAt)
                    .Take(3)
                    .Select(up => up.PasswordHash)
                    .ToList();

                if (lastThreePasswords.Any(p => BCrypt.Net.BCrypt.Verify(user.PasswordHash, p)))
                {
                    ModelState.AddModelError("", "Yeni şifreniz , son üç şifreniz ile aynı olamaz.");
                    return View(user);  // Return view with the error
                }
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                user.confirmPassword = user.PasswordHash;
                user.PasswordLastChanged = DateTime.Now;
                db.Entry(user).State = EntityState.Modified;
                db.Entry(user).Property(u => u.IsVerified).IsModified = false;
                var userPassword = new UserPassword
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash,
                    CreatedAt = DateTime.Now
                };
                db.UserPasswords.Add(userPassword);
                db.SaveChanges();
                return RedirectToAction("main", new { id = user.Id });
            }
            return View(user);
        }
        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        public void SendVerificationEmail(string email)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == email);
            if (user != null)
            {
                var token = Guid.NewGuid().ToString(); // Yeni bir token oluşturun
                user.VerificationToken = token;
                user.VerificationTokenExpiry = DateTime.Now.AddMinutes(5); // Token geçerlilik süresi
                db.SaveChanges();

                var verificationLink = Url.Action("VerifyEmail", "Users", new { token = token }, protocol: Request.Url.Scheme);
                var message = new MailMessage();
                message.To.Add(new MailAddress(email));
                message.Subject = "E-posta Doğrulama";
                message.Body = $"Lütfen e-posta adresinizi doğrulamak için şu linke tıklayın: <a href='{verificationLink}'>Doğrulama Linki</a>";
                message.IsBodyHtml = true;

                using (var smtp = new SmtpClient())
                {
                    System.Net.ServicePointManager.ServerCertificateValidationCallback =
        delegate { return true; };
                    smtp.Send(message);
                }
            }

            
        }

        [HttpGet]
        public ActionResult VerifyEmail(string token)
        {
            var user = db.Users.FirstOrDefault(u => u.VerificationToken == token);

            if (user != null && user.VerificationTokenExpiry >= DateTime.Now)
            {
                user.IsVerified = true;
                user.VerificationToken = null; // Token'ı sıfırla
                db.SaveChanges();
                ViewBag.IsVerified = true; // Başarı mesajı gönder
            }
            else
            {
                ViewBag.IsVerified = false; // Hata mesajı gönder
            }

            return View();
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
