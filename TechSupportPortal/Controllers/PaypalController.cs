using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TechSupportPortal.Models;
using Order = TechSupportPortal.Models.Order;

namespace TechSupportPortal.Controllers
{
    public class PaypalController : Controller
    {
        private MyDbContext db = new MyDbContext();

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            Order orderToCreate = Session["orderToCreate"] as Order;
            //getting the apiContext  
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal  
                //Payer Id will be returned when payment proceeds or click to pay  
                string payerId = Request.Params["PayerID"];
                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist  
                    //it is returned by the create function call of the payment class  
                    // Creating a payment  
                    // baseURL is the url on which paypal sendsback the data.  
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPayPal?";
                    //here we are generating guid for storing the paymentID received in session  
                    //which will be used in the payment execution  
                    var guid = Convert.ToString((new Random()).Next(100000));
                    //CreatePayment function gives us the payment approval url  
                    //on which payer is redirected for paypal account payment  
                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid, orderToCreate);
                    //get links returned from paypal in response to Create function call  
                    var links = createdPayment.links.GetEnumerator();
                    string paypalRedirectUrl = null;
                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;
                        if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment  
                            paypalRedirectUrl = lnk.href;
                        }
                    }
                    // saving the paymentID in the key guid  
                    Session.Add(guid, createdPayment.id);
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment  
                    var guid = Request.Params["guid"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                    //If executed payment failed then we will show payment failure message to user  
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index", "Home");
            }
            //on successful payment, show success page to user.  

            // TODO:  Create order for user
            if (orderToCreate != null)
            {
                db.Orders.Add(orderToCreate);
                db.SaveChanges();

                var user = Session["user"] as Account;
                var usr = db.Accounts.Find(user.AccountId);
                var pack = db.Packs.Where(p => p.PackId == (int)orderToCreate.TokenPack).FirstOrDefault();
                usr.Tokens = usr.Tokens + orderToCreate.Quantity * pack.Amount;
                usr.ConfirmPassword = usr.Password;
                db.Entry(usr).State = EntityState.Modified;
                db.SaveChanges();

                Session["user"] = usr;
                Session["orderToCreate"] = null;
            }


            return RedirectToAction("Details", "Accounts");
        }

        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }


        public Payment CreatePayment(APIContext apiContext, string redirectUrl, Order orderToCreate)
        {
            //create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            var pack = db.Packs.Where(p => p.PackId == (int)orderToCreate.TokenPack).FirstOrDefault();
            string name = "";
            
            if (orderToCreate.TokenPack == TokenPack.S)
            {
                name = "Silver Package";
            }
            if (orderToCreate.TokenPack == TokenPack.G)
            {
                name = "Gold Package";
            }
            if (orderToCreate.TokenPack == TokenPack.P)
            {
                name = "Platinum Package";
            }
            //Adding Item Details like name, currency, price etc
            itemList.items.Add(new Item()
            {
                name = name,
                currency = "USD",
                price = orderToCreate.Price.ToString(),
                quantity = "1",
                sku = "sku"
            });

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax = "1",
                shipping = "1",
                subtotal = orderToCreate.Price.ToString(),
                shipping_discount = "-1"
            };

            //Final amount with details
            string totalPrice = (orderToCreate.Price + 1 + 1 - 1).ToString();
            var amount = new Amount()
            {
                currency = "USD",
                total = totalPrice, // Total must be equal to sum of tax, shipping and subtotal.
                details = details
            };

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "Transaction description",
                invoice_number = Convert.ToString((new Random()).Next(100000)),
                amount = amount,
                item_list = itemList
            });


            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }
    }
}