using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using PP.Signicat.CredentialManager.Models;

namespace PP.Signicat.CredentialManager.Controllers
{
    public class SubscriptionController : Controller
    {
        public ActionResult Index()
        {
            var response = new SubscriptionHandler().GetAllSubscriptions();
            return View(response);
        }

        public ActionResult Create()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(SubscriptionModel model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var upsert = new SubscriptionHandler().CreateSubscription(model);
                }
            }
            else return View(model);

            //return View("Edit", model);
            return RedirectToAction("Index");
        }

        public ActionResult Edit(string customer, string subscription)
        {
            //Get credentials
            var response = new SubscriptionHandler().GetSubscription(customer, subscription);

            if (response == null)
                throw new NotImplementedException("Cannot find credentials");

            var model = response;

            return View("Edit", model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(SubscriptionModel model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var response = new SubscriptionHandler().UpdateSubscription(model);

                    if (response == null)
                        throw new NotImplementedException("Cannot update the subscription.");

                    model = response;
                }
            }
            else return Edit(model.customer, model.subscription);

            //return View(model);
            return RedirectToAction("Index");
        }

       
        public ActionResult Delete(string customer, string subscription)
        {
            var response = new SubscriptionHandler().DeleteSubscription(customer, subscription);
            if (response)
            {
                var allCreds = new SubscriptionHandler().GetAllSubscriptions();
                return RedirectToAction("Index", "Subscription");
            }
            return Json(false);
        }
    }
}
