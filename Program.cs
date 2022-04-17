using System.Globalization;
using CsvHelper;
using OpenQA.Selenium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using TripAdvisorScraper.Models;

var driverOptions = new EdgeOptions();
driverOptions.AddArgument("headless");
using var driver = new EdgeDriver(driverOptions);
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

var restaurantInputs = new CsvReader(File.OpenText("urls.csv"), CultureInfo.InvariantCulture).GetRecords<RestaurantInputs>();
var currentDatetimeString = DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss");

var results = new List<Reviews>();

foreach (var restaurant in restaurantInputs)
{
    driver.Navigate().GoToUrl(restaurant.url);

    TryClickIgnoreIfException(By.Id("onetrust-accept-btn-handler"));

    ScrapeReviews();

    results = new List<Reviews>();
}

void ScrapeReviews()
{
    Directory.CreateDirectory($"Reviews{Path.DirectorySeparatorChar}{currentDatetimeString}");

    var fileName = $"Reviews{Path.DirectorySeparatorChar}{currentDatetimeString}{Path.DirectorySeparatorChar}{wait.Until(ExpectedConditions.ElementExists(By.XPath(".//h1[@class='fHibz']"))).Text}";
    using var csvWriter = new CsvWriter(new StreamWriter(fileName + ".csv"), CultureInfo.InvariantCulture);

    TryClickIgnoreIfException(By.XPath("//span[@class='taLnk ulBlueLinks']"));


    var elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(".//div[@class='review-container']")));

    var i = 0;
    while (elements.Count != 15 && i <= 10)
    {
        elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.XPath(".//div[@class='review-container']")));
        Thread.Sleep(1000);
        i++;
    }

    foreach (var element in elements)
    {
        var title = element.FindElement(By.XPath(".//span[@class='noQuotes']")).Text;
        var date = element.FindElement(By.XPath(".//span[contains(@class, 'ratingDate')]")).GetAttribute("title");
        var rating = element.FindElement(By.XPath(".//span[contains(@class, 'ui_bubble_rating bubble_')]")).GetAttribute("class").Split("_")[3].Replace("0", "");
        var review = element.FindElement(By.XPath(".//p[@class='partial_entry']")).Text;

        results.Add(new Reviews(title, date, rating, review));
    }

    csvWriter.WriteRecords(results);
    csvWriter.Dispose();

    if (HasNextPage())
    {
        ScrapeReviews();
    }
}

bool HasNextPage()
{
    try
    {
        var element = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector(".ui_pagination .nav.next")));

        element.Click();

        wait.Until(ExpectedConditions.StalenessOf(element));

        return true;
    }
    catch (ElementClickInterceptedException)
    {
        return false;
    }
}

void TryClickIgnoreIfException(By by)
{
    try
    {
        wait.Until(ExpectedConditions.ElementToBeClickable(by)).Click();
    }
    catch
    {
        // ignored
    }
}