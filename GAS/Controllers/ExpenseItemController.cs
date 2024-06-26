﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Web.Http.Cors;
using GAS.Models;

using GAS.Infrastructure;


namespace GAS.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [RoutePrefix("api/ExpenseItem")]
    public class ExpenseItemController : ApiController
    {
       

        // Get expense list of an Organization in given month of a year
        [Route("Organization/{id}/{year}/{month}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetByOrgMonth(int id, int year, int month)
        {
            try {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.OrganizationId == id && ex.ExpenseDate.Year == year && ex.ExpenseDate.Month == month
                                && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex);
                return expData;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        // Get expense list in a project

        [Route("Project/{id}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetByProject(int id)
        {
            try
            {
                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.ProjectID == id
                                && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex);
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Get expense list of an employee in an Organization

        [Route("{OrgId}/Employee/{EmployeeID}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetExpensesEmployee(int OrgId, int EmployeeID)
        {
            try
            {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.OrganizationId == OrgId && ex.EmployeeID == EmployeeID
                               && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex).Take(5);
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Get expense list of an employee in a project

        [Route("Project/{id}/Employee/{EmployeeID}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetProjectByEmployee(int id, int EmployeeID)
        {
            try
            {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.ProjectID == id && ex.EmployeeID == EmployeeID 
                               && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex);
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Get expense list of an Activity
        [Route("Activity/{id}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetByActivity(int id)
        {
            try
            {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.ActivityID == id && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex);
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        
        // Get expense list of an employee by month of year

        [Route("Organization/{orgId}/Employee/{employeeID}/Year/{year}/{month}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetByEmployee(int orgId, int employeeID, int year, int month)
        {
            try
            {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.EmployeeID == employeeID && ((DateTime)ex.ExpenseDate).Year == year && ((DateTime)ex.ExpenseDate).Month == month
                                 && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex );
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Get expense list of an employee by month of year

        [Route("Organization/{orgId}/Employee/{employeeID}/{year}/{month}")]
        [HttpGet]
        public HttpResponseMessage GetByDate(int orgId, int employeeID, int year, int month)
        {
            try
            {

                /*  using (var ctx = new XPenEntities())
                  {
                      var query =
                      from act in ctx.Activities
                      join eItms in ctx.ExpenseItems on act.ActivityID equals eItms.ActivityID
                      join prj in ctx.Projects on act.ProjectID equals prj.ProjectID
                      where act.OrgID == orgId && act.EmployeeID == employeeID
                      && eItms.ExpenseDate.Year == year && eItms.ExpenseDate.Month == month
                      orderby eItms.ExpenseDate descending
                      select new 
                      { 
                          ItemName = eItms.ItemName,
                          ExpenseAmount = eItms.ExpenseAmount,
                          ExpenseDate = eItms.ExpenseDate,
                          ActivityName = act.ActivityName,
                          ProjectName = prj.ProjectName

                      };

                      return query.ToList();
                  }*/

                String resp = "{\"Response\":\"Undefine\"}";
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
                return response;

            }
            catch (Exception ex)
            {
                return null;
            }
            
        }
        // Get Daily Expense with Status for a Manager


        [Route("Manager/{id}")]
        [HttpGet]
        public IEnumerable<ExpenseItem> GetByManager(int id)
        {
            try
            {

                var ctx = new XPenEntities();
                var expData = (from ex in ctx.ExpenseItems
                               where ex.ApproverID == id 
                               && (ex.Action == "Added" || ex.Action == "Quick" || ex.Action == "Paid")
                               orderby ex.ExpenseDate descending
                               select ex);
                return expData;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        // Add set/Array of Expense Item
        [Route("Add")]
        [HttpPost]
        public HttpResponseMessage PostAdd([FromBody]ExpenseItem[] eItem)
        {
            String resp = "{\"Response\":\"Undefine\"}";
            try
            {
                var ctx = new XPenEntities();

                if (eItem != null)
                {
                    var id = ctx.ExpenseItems.AddRange(eItem);
                
                    ctx.SaveChanges();
                    resp = "{\"Response\":\"OK\"}";
                }
            }
            catch (Exception ex)
            {
                int a = 1;
                resp = "{\"Response\":\"Fail\"}";

            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        // Add Single Expense Item
        [Route("AddItem")]
        [HttpPost]
        public HttpResponseMessage PostAddItem([FromBody]ExpenseItem eItem)
        {
            String resp = "{\"Response\":\"Undefine\"}";
            var ctx = new XPenEntities();
            using (var dbContextTransaction = ctx.Database.BeginTransaction())
            {
                try
                {
                    if (eItem != null)
                    {

                        var id = ctx.ExpenseItems.Add(eItem);
                        ctx.SaveChanges();
                        var transaction = Common.CreateTransactionForExpenseSettlement(eItem, ctx);
                        ctx.Transactions.Add(transaction);
                        ctx.SaveChanges();
                        dbContextTransaction.Commit();
                        resp = "{\"Response\":\"OK\"}";
                    }
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

        // Select a single Expense Item
        [Route("Delete")]
        [HttpPost]
        public HttpResponseMessage PostDelete([FromBody]ExpenseItem eItem)
        {
            String resp = "{\"Response\":\"Undefine\"}";
            try
            {
                var ctx = new XPenEntities();

                if (eItem != null)
                {
                    var item = (from ex in ctx.ExpenseItems
                                where ex.ItemID == eItem.ItemID 
                              select ex).First();
                    item.Action = "Deleted";
                    ctx.SaveChanges();
                    resp = "{\"Response\":\"OK\"}";
                }
            }
            catch (Exception ex)
            {
                int a = 1;
                resp = "{\"Response\":\"Fail\"}";

            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(resp, System.Text.Encoding.UTF8, "application/json");
            return response;
        }

        // PUT: api/ExpenseItem/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/ExpenseItem/5
        public void Delete(int id)
        {
        }
    }
}
