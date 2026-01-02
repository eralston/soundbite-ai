using System;

namespace Masticore.DirectorySync.Okta
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class ChangePassword
    {
        public string href { get; set; }
    }

    public class ChangeRecoveryQuestion
    {
        public string href { get; set; }
    }

    public class Credentials
    {
        public Password password { get; set; }
        public RecoveryQuestion recovery_question { get; set; }
        public Provider provider { get; set; }
    }

    public class Deactivate
    {
        public string href { get; set; }
    }

    public class ExpirePassword
    {
        public string href { get; set; }
    }

    public class ForgotPassword
    {
        public string href { get; set; }
    }

    public class UserLinks
    {
        public ResetPassword resetPassword { get; set; }
        public ResetFactors resetFactors { get; set; }
        public ExpirePassword expirePassword { get; set; }
        public ForgotPassword forgotPassword { get; set; }
        public ChangeRecoveryQuestion changeRecoveryQuestion { get; set; }
        public Deactivate deactivate { get; set; }
        public ChangePassword changePassword { get; set; }
    }

    public class Password
    {
    }

    public class OktaUserProfile
    {
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string email { get; set; }
        public string login { get; set; }
        public string mobilePhone { get; set; }
    }

    public class Provider
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class RecoveryQuestion
    {
        public string question { get; set; }
    }

    public class ResetFactors
    {
        public string href { get; set; }
    }

    public class ResetPassword
    {
        public string href { get; set; }
    }

    public class OktaUserResponse
    {
        public string id { get; set; }
        public string status { get; set; }
        public DateTime? created { get; set; }
        public DateTime? activated { get; set; }
        public DateTime? statusChanged { get; set; }
        public DateTime? lastLogin { get; set; }
        public DateTime? lastUpdated { get; set; }
        public DateTime? passwordChanged { get; set; }
        public OktaUserProfile profile { get; set; }
        public Credentials credentials { get; set; }
        public UserLinks _links { get; set; }
    }


}
