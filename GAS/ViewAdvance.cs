//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GAS
{
    using System;
    using System.Collections.Generic;
    
    public partial class ViewAdvance
    {
        public int ActivityID { get; set; }
        public string ActivityName { get; set; }
        public int ProjectID { get; set; }
        public string ProjectName { get; set; }
        public int EmployeeID { get; set; }
        public string Employee { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public Nullable<int> RequestAmount { get; set; }
        public Nullable<int> ReceivedAmount { get; set; }
        public string AdvanceName { get; set; }
        public string AdvanceStatus { get; set; }
        public Nullable<System.DateTime> AdvanceModifiedDate { get; set; }
        public int Approver { get; set; }
        public string ApproverName { get; set; }
        public int OrgID { get; set; }
    }
}
