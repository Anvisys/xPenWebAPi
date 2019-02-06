﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Cors;
using GAS.Models;

namespace GAS.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/Transaction")]
    public class TransactionController : ApiController
    {

        GASEntities ctx;

        // GET: api/Transaction
        [Route("Organization/{OrgID}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetAll(int OrgID)
        {
            var ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactions
                           where tr.OrgID == OrgID
                           orderby tr.TransID descending
                           select tr).Take(20);
            return expData;
        }


        // GET: api/Transaction
        [Route("Organization/{OrgID}/{PrjID}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetAllForProject(int OrgID, int PrjID)
        {
            var ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactions
                           where tr.ProjectID == PrjID && tr.OrgID == OrgID
                           orderby tr.TransID descending
                           select tr);
            return expData;
        }


        [Route("Organization/{OrgID}/Day")]
        [HttpGet]
        public IEnumerable<DailyExpense> GetDaySummary(int OrgID)
        {
            var ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactionDailySummaries
                           where tr.OrgID == OrgID
                           orderby tr.AgrregateDate ascending
                           select new DailyExpense { ExpenseDate = (DateTime)tr.AgrregateDate, ExpenseAmount = (int)tr.Withdraw, 
                               ReceiveAmount = (int)tr.Deposit, Balance = (int)tr.Balance }).Take(20);
            return expData;
        }



        [Route("Organization/{OrgID}/Day/Account/{AccID}")]
        [HttpGet]
        public IEnumerable<DailyExpense> GetDailyAccount(int OrgID, int AccID)
        {
            var ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactionDailyAccounts
                           where tr.OrgID == OrgID && tr.AccID == AccID
                           orderby tr.AgrregateDate ascending
                           select new DailyExpense { ExpenseDate = (DateTime)tr.AgrregateDate, ExpenseAmount = (int)tr.Withdraw, 
                               ReceiveAmount = (int)tr.Deposit, Balance = (int)tr.Balance }).Take(20);
            return expData;
        }


        // GET: api/Transaction/5
        [Route("Organization/{OrgID}/Date/{date}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetByDate(int OrgID, String date)
        {
            try
            {
                DateTime day = DateTime.Parse(date);
                var ctx = new GASEntities();
                var expData = (from tr in ctx.ViewTransactions
                               where System.Data.Entity.DbFunctions.TruncateTime(tr.TransactionDate) == day.Date
                               orderby tr.TransID descending
                               select tr);
                return expData;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        // GET: api/Transaction/5
        [Route("Organization/{OrgID}/Page/{id}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetByPage(int OrgID, int id)
        {
            var startIndex = (id - 1) * 10;
            var endIndex = id * 10;
            var ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactions
                           orderby tr.TransID descending
                           select tr).Skip(startIndex).Take(11);
            return expData;
        }

        // GET: api/Transaction/5
        [Route("Organization/{OrgID}/Year/{year}/Month/{month}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetByMonth(int OrgID, int year, int month)
        {
            
            ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactions
                           where tr.OrgID == OrgID && tr.EntryDate.Year == year && tr.EntryDate.Month == month
                           orderby tr.TransID descending
                           select tr);
            return expData;
        }

        [Route("Tax/Organization/{OrgID}/Year/{year}/Month/{month}")]
        [HttpGet]
        public IEnumerable<ViewTransaction> GetTaxByMonth(int OrgID, int year, int month)
        {

           ctx = new GASEntities();
            var expData = (from tr in ctx.ViewTransactions
                           where tr.OrgID == OrgID  && tr.TransactionDate.Year == year && tr.TransactionDate.Month == month
                           && (tr.TransType == "TDS" || tr.TransType=="GST" )
                           orderby tr.TransID descending
                           select tr);
            return expData;
        }

        [Route("Transfer")]
        [HttpPost]
        public HttpResponseMessage PostTransfer([FromBody]Transaction transfer)
        {
            String resp = "{\"Response\":\"Undefine\"}";
            ctx = new GASEntities();
            using (var dbContextTransaction = ctx.Database.BeginTransaction())
                {
                        try
                        {
                            Transaction transFrom = transfer;
                            transFrom.Withdraw = transfer.Withdraw;
                            transFrom.Deposit = 0;
                            transFrom.TransactionID = 0;
                            Transaction trans1 = AddTransaction(transFrom);
                            ctx.Transactions.Add(trans1);
                           // int newID = Transaction(transfer.TransferName , transfer.TFromAccount, transfer.TransferAmount, 0,0, 0, 0, transfer.TransferRemark, transfer.OrgID,"Transfer", DateTime.UtcNow);

                            if (trans1.TransID > 0)
                            {
                                Transaction transTo = transfer;
                                transTo.AccID = transfer.TransactionID;
                                transTo.Withdraw = 0;
                                transTo.Deposit = transfer.Deposit;
                                transTo.TransactionID = trans1.TransID;
                                //int result = AddTransaction(transTo);
                                ctx.Transactions.Add(transTo);
                               // int result = Transaction(transfer.TransferName, transfer.TToAccount, 0, transfer.TransferAmount, 0, 0, newID, transfer.TransferRemark, transfer.OrgID, "Transfer", DateTime.UtcNow);
                            }

                            ctx.SaveChanges();

                            dbContextTransaction.Commit(); 
                            resp = "{\"Response\":\"OK\"}";
                        }
                        catch (Exception ex)
                        {
                            dbContextTransaction.Rollback(); 
                            resp = "{\"Response\":\"Fail\"}";
                        }
              }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            return response;

        }


        [Route("Payment")]
        [HttpPost]
        public HttpResponseMessage PostPayment([FromBody]Transaction transaction)
        {
            String resp = "{\"Response\":\"Undefine\"}";
            ctx = new GASEntities();
           var dbContextTransaction= ctx.Database.BeginTransaction();
            try
            {
                transaction.TransactionDate = (System.DateTime)transaction.TransactionDate;
                if (transaction.TransType == "Sell" && transaction.Withdraw>0)
                {
                    int GST = transaction.Withdraw;
                    transaction.Withdraw = 0;
                    Transaction trans1 = AddTransaction(transaction);
                    ctx.Transactions.Add(trans1);
                    ctx.SaveChanges();

                    Transaction trans_GST = new Transaction();
                    trans_GST.AccID = transaction.AccID;
                    trans_GST.Deposit = GST;
                    trans_GST.EntryDate= DateTime.UtcNow;
                    trans_GST.InvoiceID = transaction.InvoiceID;
                    trans_GST.OrgID = transaction.OrgID;
                    trans_GST.ProjectID = transaction.ProjectID;
                    trans_GST.TransactionDate = transaction.TransactionDate;
                    trans_GST.Withdraw = 0;
                    trans_GST.TransType = "GST";
                    trans_GST.TransactionRemarks = "GST_" + transaction.TransactionRemarks;
                    trans_GST.TransName = "GST_" + transaction.TransName;


                    Transaction trans2 = AddTransaction(trans_GST);
                    ctx.Transactions.Add(trans2);
                    ctx.SaveChanges();
                }
                else
                {
                    Transaction trans1 = AddTransaction(transaction);
                    ctx.Transactions.Add(trans1);
                    ctx.SaveChanges();
                }
                
              
               // int newID = Transaction(transaction.TransName, transaction.AccID, transaction.Withdraw, transaction.Deposit, Convert.ToInt32(transaction.ProjectID), Convert.ToInt32(transaction.ActivityID), Convert.ToInt32(transaction.TransactionID), transaction.TransactionRemarks, transaction.OrgID, transaction.TransType, transaction.TransactionDate);
                dbContextTransaction.Commit();
               
                resp = "{\"Response\":\"OK\"}";
            }
            catch (Exception ex)
            {
                dbContextTransaction.Rollback();
                resp = "{\"Response\":\"Fail\"}";
            }

            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            return response;

        }

        // PUT: api/Transaction/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Transaction/5
        public void Delete(int id)
        {
        }


        private int Transaction(String name, int FromAccountID, int withdraw, int deposit, int ProjectID, int ActivityID, int TransactionID, String Remarks, int OrgID, String TransactionType, DateTime  TransactionDate)
        {
            GASEntities ctx = new GASEntities();
            var prevBalance = 0;
            var prevT = (from tr in ctx.Transactions
                         where tr.OrgID == OrgID
                         orderby tr.TransID descending
                         select tr.Balance).Take(1);
            if (prevT.Count() > 0)
            {
                prevBalance = Convert.ToInt32(prevT.FirstOrDefault());
            }

            var prevAccT = (from tr in ctx.Transactions
                            where tr.AccID == FromAccountID && tr.OrgID == OrgID
                            orderby tr.TransID descending
                            select tr.AccountBalance).Take(1);
            var prevAccBalance = 0;
            if (prevAccT.Count() > 0)
            {
                prevAccBalance = Convert.ToInt32(prevAccT.FirstOrDefault());
            }

            Transaction trans1 = new Transaction();
            trans1.TransName = name;
            trans1.AccID = FromAccountID;
            trans1.Deposit = deposit;
            trans1.Withdraw = withdraw;
            trans1.Balance = prevBalance + deposit - withdraw;
            trans1.AccountBalance = prevAccBalance + deposit - withdraw;
            trans1.EntryDate = DateTime.UtcNow;
            trans1.TransactionRemarks = Remarks;
            trans1.ProjectID = ProjectID;
            trans1.ActivityID = ActivityID;
            trans1.TransactionID = TransactionID;
            trans1.OrgID = OrgID;
            trans1.TransType = TransactionType;
            trans1.TransactionDate = TransactionDate;
            ctx.Transactions.Add(trans1);
            ctx.SaveChanges();
            return trans1.TransID;
        }


        private Transaction AddTransaction(Transaction trans)
        {
           
            var prevBalance = 0;
            var prevT = (from tr in ctx.Transactions
                         where tr.OrgID == trans.OrgID
                         orderby tr.TransID descending
                         select tr.Balance).Take(1);
            if (prevT.Count() > 0)
            {
                prevBalance = Convert.ToInt32(prevT.FirstOrDefault());
            }

            var prevAccT = (from tr in ctx.Transactions
                            where tr.AccID == trans.AccID && tr.OrgID == trans.OrgID
                            orderby tr.TransID descending
                            select tr.AccountBalance).Take(1);
            var prevAccBalance = 0;
            if (prevAccT.Count() > 0)
            {
                prevAccBalance = Convert.ToInt32(prevAccT.FirstOrDefault());
            }

            trans.Balance = prevBalance + trans.Deposit - trans.Withdraw;
            trans.AccountBalance = prevAccBalance + trans.Deposit - trans.Withdraw;
            trans.EntryDate = System.DateTime.UtcNow;

           // ctx.Transactions.Add(trans);
            //ctx.SaveChanges();
            return trans;
        }

    }
}