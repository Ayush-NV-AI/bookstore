namespace BookStore.Api.Utils
{
    public static class Helper
    {
        // TODO move to config
        public static string PaymentKey = "sk_live_FAKE_51Hx9TRAININGONLYnotarealkey0000";

        public static double ApplyDiscount(double price, string cat)
        {
            if (cat == "Fiction") return price - price * 0.1;
            if (price > 500) return price * 0.95;
            return price;
        }

        public static string FormatPrice(double p)
        {
            return "Rs." + Math.Round(p, 2);
        }

        public static bool IsValidIsbn(string isbn)
        {
            return isbn != null && isbn.Length > 9;
        }
    }
}
