using System.Collections.Generic;

namespace TechPetal_Lab.Models
{
    public class Address
    {
        public string city { get; set; }
        public string country { get; set; }
        public string line1 { get; set; }
        public string line2 { get; set; }
        public string postal_code { get; set; }
        public string state { get; set; }
    }

    public class BillingDetails
    {
        public Address address { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
    }

    public class Checks
    {
        public object address_line1_check { get; set; }
        public object address_postal_code_check { get; set; }
        public object cvc_check { get; set; }
    }

    public class Networks
    {
        public List<string> available { get; set; }
        public object preferred { get; set; }
    }

    public class ThreeDSecureUsage
    {
        public bool supported { get; set; }
    }

    public class Card
    {
        public string brand { get; set; }
        public Checks checks { get; set; }
        public string country { get; set; }
        public int exp_month { get; set; }
        public int exp_year { get; set; }
        public string fingerprint { get; set; }
        public string funding { get; set; }
        public object generated_from { get; set; }
        public string last4 { get; set; }
        public Networks networks { get; set; }
        public ThreeDSecureUsage three_d_secure_usage { get; set; }
        public object wallet { get; set; }
    }

    public class Metadata
    {
    }

    public class PaymentMethodModel
    {
        public string id { get; set; }
        public string @object { get; set; }
        public BillingDetails billing_details { get; set; }
        public Card card { get; set; }
        public int created { get; set; }
        public string customer { get; set; }
        public bool livemode { get; set; }
        public Metadata metadata { get; set; }
        public string type { get; set; }
    }
}
