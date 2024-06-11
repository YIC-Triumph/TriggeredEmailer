using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TriggeredEmailer.Constants
{
    public enum Roles
    {
        Guest = 1,
        Developer = 2,
        BillingManager = 3,
        BCBA = 4,
        OfficeManager = 5,
        BT = 6,
        Parent = 7,
        Principal = 8,
        RegionalDirector = 9,
        ClinicalDirector = 10,
        Corporate = 11,
        AssessmentDirector = 15,
        AuthStaff = 16,
        HRStaff = 17,
        IntakeStaff = 18,
        CEO = 19,
        Recruiting = 20,
        OfficeStaff = 21,
        Payroll = 22,
        Billing = 23,
        PayrollDirector = 24,
        BillingDirector = 25,
        InsuranceBroker = 26,
        HRDirector = 27,
        EHR = 28,
        BCBASupervisor = 29,
        EHRDirector = 30,
        AssessmentTeam = 35,
        Credentialing = 31,
        Case_Manager = 32,
    }

    public enum SessionStatus
    {
        Appointment = 1,
        Incomplete = 2,
        Absent = 3,
        Completed = 4,
        ReadyForReview = 8,
        UnderReview = 10,
        ConfirmedAbsent = 11
    }

    public enum LogType
    {
        Console,
        File
    }
}