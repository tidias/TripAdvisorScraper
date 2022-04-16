namespace TripAdvisorScraper.Models
{
    public class Reviews
    {
        public string Title { get; set; }
        public string Date { get; set; }
        public string Rating { get; set; }
        public string Review { get; set; }

        public Reviews(string title, string date, string rating, string review)
        {
            Title = title;
            Date = date;
            Rating = rating;
            Review = review;
        }
    }
}
