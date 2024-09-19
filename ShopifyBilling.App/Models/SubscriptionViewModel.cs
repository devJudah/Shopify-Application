using ShopifySharp;

namespace ShopifyBilling.App.Models
{
    public class SubscriptionViewModel
    {
        // This model is used as the subscription details page which users can view details about their subscription

        public SubscriptionViewModel(RecurringCharge charge)
        {
            ChargeName = charge.Name;
            Price = charge.Price.Value;
            TestMode = charge.Test == true;
            DateCreated = charge.CreatedAt.Value;
            TrialEndsOn = charge.TrialEndsOn;
        }

        public string ChargeName { get; }
        public decimal Price { get;  }
        public bool TestMode { get;  }
        public DateTimeOffset DateCreated { get;  }
        public DateTimeOffset? TrialEndsOn { get;  }
        public bool IsTrialing => TrialEndsOn.HasValue;
    }
}
