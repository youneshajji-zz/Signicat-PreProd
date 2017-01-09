using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PP.Signicat.CredentialManager.Models;

namespace PP.Signicat.CredentialManager.Controllers
{
    public class TransactionController : Controller
    {
        public PartialViewResult TransactionIndex(string subscription)
        {
            var response = new TransactionHandler().GetAllTransactions(subscription);
            return PartialView(response);
        }

        public ActionResult Detail(string subscription, string period)
        {
            var response = new TransactionHandler().GetTransaction(subscription, period);

            if (response == null)
                throw new NotImplementedException("Cannot find credentials");

            var model = response;
            return View("Detail", model);
        }

        public ActionResult Create()
        {
            return View();
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Create(TransactionModel model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var upsert = new TransactionHandler().UpsertTransactions(model);
                }
            }
            else return View(model);

            return View("Edit", model);
        }

        public ActionResult Edit(string subscription, string period)
        {
            //Get credentials
            var response = new TransactionHandler().GetTransaction(subscription, period);

            if (response == null)
                throw new NotImplementedException("Cannot find credentials");

            var model = response;

            return View("Edit", model);
        }

        [System.Web.Mvc.HttpPost]
        public ActionResult Edit(TransactionModel model)
        {
            if (ModelState.IsValid)
            {
                if (model != null)
                {
                    var response = new TransactionHandler().UpsertTransactions(model);

                    if (response == null)
                        throw new NotImplementedException("Cannot update the transaction.");

                    model = response;
                }
            }
            else return Edit(model.subscription, model.period);

            return View(model);
        }
    }
}