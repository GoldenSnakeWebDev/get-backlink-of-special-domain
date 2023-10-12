using System.Security.AccessControl;
using System.Reflection.Emit;
using System.Net.Mail;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Data;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;
// LinkCrawler.cs
using HtmlAgilityPack;
using System.Net;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using MySql.Data.MySqlClient;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System;
using System.Threading.Tasks;
using Abot2.Core;
using Abot2.Crawler;
using Abot2.Poco;
using Serilog;

namespace server.Controllers;

public class LinkCrawler
{
    public List<string> CrawlLinks(string domain)
    {
        var links = new List<string>();

        string url = $"http://{domain}"; // Create the URL to crawl

        Console.WriteLine($"url>>>> {url}");

        HtmlWeb web = new HtmlWeb();
            Console.WriteLine("oka!");

        HtmlDocument doc = web.Load(url);

            Console.WriteLine("oka!");
        foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

            if (Check_link(link, domain)) {
                if (!Check_existing(links, link)) {

                    links.Add(link);
                }
            }
        }

        return links;
    }

    public bool Check_link (string link, string domain) {
        if ( link.Contains("https://") || link.Contains("http://")) {
            if (!link.Contains(domain)) {
                return true;
            }
        }

        return false;
    }

    public bool Check_existing (List<string> links, string link) {
        if (links.Contains(link)) {
            return true;
        }
        return false;
    }
}

public class BacklinkService
{
    public async Task<List<string>> GetBacklinks11(string domain, string cx, string apikey) {

        var backlinks = new List<string>();

        // Create HttpClient instance
        HttpClient httpClient = new HttpClient();

        // Send GET request to Google search page
        string googleUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(domain)}";
        var html = await httpClient.GetStringAsync(googleUrl);

        Console.WriteLine($"okay>>>> {html}");

        // Read the content of the response
        // string htmlContent = await response.Content.ReadAsStringAsync();

        // // Parse the HTML using HtmlAgilityPack
        // HtmlDocument htmlDocument = new HtmlDocument();
        // htmlDocument.LoadHtml(htmlContent);

        // // Extract the search results
        // var resultNodes = htmlDocument.DocumentNode.SelectNodes("//div[@class='g']");
        // foreach (var resultNode in resultNodes)
        // {
        //     string title = resultNode.SelectSingleNode(".//h3")?.InnerText.Trim();
        //     string url = resultNode.SelectSingleNode(".//a")?.GetAttributeValue("href", "");
        //     Console.WriteLine($"Title: {title}");
        //     Console.WriteLine($"URL: {url}");
        // }

        return backlinks;
    }
   
    public async Task<List<string>> GetBacklinks(string domain, string cx, string apikey)
    {
        var backlinks = new List<string>();
        var googleSearchResult = new List<string>();

        int start = 1;
        int numResults = 100;

        while (backlinks.Count < numResults)
        {
            var apiUrl =$"https://customsearch.googleapis.com/customsearch/v1?cx={cx}&key={apikey}&q={domain}&start={start}";
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(apiUrl);

            // var response = http.Request(apiUrl);
            var jResponse = JObject.Parse(response);

            foreach (var item in jResponse["items"])
            {
                string link = item["link"].ToString();

                googleSearchResult.Add(pickDomainFromURL(link));
            }

            if (jResponse["queries"]["nextPage"] == null)
            {
                break; // No more results available
            }

            int nextStart = int.Parse(jResponse["queries"]["nextPage"][0]["startIndex"].ToString());

            if (nextStart <= start)
            {
                break; // Ensure we don't get stuck in an infinite loop
            }

            start = nextStart;

        }

        foreach (var link in googleSearchResult)
        {
            Console.WriteLine(link);
        }

        // backlinks = await FilterSearchResult(googleSearchResult, domain);

        // return backlinks;
        return googleSearchResult;
    }

    public List<string> FilterSearchResult(List<string> searchResult, string domain)
    {

        var links = new List<string>();

        Console.WriteLine($"filter okay???? {searchResult}");

        foreach (var link in searchResult)
        {

            Console.WriteLine($"filter link>> {link}");

            if (!Check_link(link, domain))
            {
                Console.WriteLine("checked link");

                // if (!Check_existing(searchResult, link))
                // {
                // Console.WriteLine("don't exist");

                if (CrawlLinksAndCheckDomain(link, domain))
                {
                    links.Add(link);
                }
                // }

            }
        }

        return links;

    }

    public bool Check_link (string link, string domain) {
        if ( link.Contains("https://") || link.Contains("http://")) {
            if (link.Contains(domain)) {
                return true;
            }
        }

        return false;
    }

    public bool Check_existing (List<string> links, string link) {
        if (links.Contains(link)) {
            return true;
        }
        return false;
    }

    public string pickDomainFromURL(string URL) {
        Regex regex = new Regex("(https?://)(www\\.)?([a-zA-Z0-9.-]+)");
        Match match = regex.Match(URL);
        
        if (match.Success)
        {
            string domain = match.Groups[3].Value;

            return domain;
            // Console.WriteLine(domain);
        }
        return URL;
    }

    public bool CrawlLinksAndCheckDomain(string URL, string domain)
    {
        var links = new List<string>();

        string gotenDoamin = pickDomainFromURL(URL);

        string fulldomain = $"http://{gotenDoamin}";

        Console.WriteLine($"full domain>>>> {fulldomain}");

        HtmlWeb web = new HtmlWeb();

        Console.WriteLine($"okay? >>> {fulldomain}");
        Console.WriteLine($"okay? >>> {domain}");

        try
        {
            HtmlDocument doc = web.Load(fulldomain);
        
            Console.WriteLine($"okay? >>> {fulldomain}");
            
            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

                Console.WriteLine($"checked link>>>> {link}");


                if (Check_link(link, domain)) {
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            
            Console.WriteLine($"An error occurred: {ex.Message}");        
        }
        

        return false;
    }
}

public class TestAbotUse {
    // static async Task Main(string[] args)
    // {
    //     Log.Logger = new LoggerConfiguration()
    //         .MinimumLevel.Information()
    //         .WriteTo.Console()
    //         .CreateLogger();

    //     Log.Logger.Information("Demo starting up!");

    //     await DemoSimpleCrawler("http://google.com");
    //     await DemoSinglePageRequest();
    // }

    public string ConvertToURI(string domain)
    {
        UriBuilder builder = new UriBuilder
        {
            Scheme = "https",
            Host = domain
        };
        
        return builder.Uri.ToString();
    }

    public bool IsCorrectURI(string domain)
    {
        return Uri.IsWellFormedUriString(domain, UriKind.Absolute);
    }

    public static async Task DemoSimpleCrawler(string domain)
    {
        var config = new CrawlConfiguration
        {
            MaxPagesToCrawl = 10, //Only crawl 10 pages
            MinCrawlDelayPerDomainMilliSeconds = 3000 //Wait this many millisecs between requests
        };
        var crawler = new PoliteWebCrawler(config);

        crawler.PageCrawlCompleted += PageCrawlCompleted!;//Several events available...

        TestAbotUse testAbotUse = new TestAbotUse();
        string uri;
        if (!testAbotUse.IsCorrectURI(domain)) {
            uri = testAbotUse.ConvertToURI(domain);
            Console.WriteLine($"uri>>>>>> {uri}");
        }
        else {
            uri = domain;
        }


        var crawlResult = await crawler.CrawlAsync(new Uri(uri));

        Console.WriteLine(Convert.ToInt32(crawlResult.CrawlContext.CrawlCountByDomain));
    }

    public static async Task DemoSinglePageRequest()
    {
        var pageRequester = new PageRequester(new CrawlConfiguration(), new WebContentExtractor());

        var crawledPage = await pageRequester.MakeRequestAsync(new Uri("http://google.com"));
        Log.Logger.Information("{result}", new
        {
            url = crawledPage.Uri,

            status = Convert.ToInt32(crawledPage.HttpResponseMessage.StatusCode)
        });
    }

    private static void PageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
    {
        var httpStatus = e.CrawledPage.HttpResponseMessage.StatusCode;
        var rawPageText = e.CrawledPage.Content.Text;

        Console.WriteLine($"page text >>>> {rawPageText}");
    }
}
[ApiController]
[Route("api/[controller]")]
public class LinksController : ControllerBase
{
    public class ParamsModel {
        public string? Param {get; set;}
    }

    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] dynamic input)
    {

        Console.WriteLine("start>>>>");

        JsonDocument  jsonDoc = JsonDocument.Parse(input.ToString());

        JsonElement domainElement = jsonDoc.RootElement.GetProperty("domain");
        
        string? domain = domainElement.ValueKind != JsonValueKind.Undefined ? domainElement.GetString():null;
        Console.WriteLine($"domain>>>, {domain}");

        var crawler = new LinkCrawler();
        List<string> links =  crawler.CrawlLinks(domain ?? "");
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // string apiKey = "AIzaSyCqY41oLU4KL9JBIPsZyFF4W9A00WmlKsI";
        // string apiKey = "AIzaSyDK_BNn-W6zDYg4D1Jy-0mMQvR-hHDJTPA";
        string apiKey = "AIzaSyAWp2AFSPxOxcPeJGB6iddHqHwPayphJDg";
        // string searchEngineId = "d61f36940c5cf411e";
        // string searchEngineId = "7559c0c631de64c8a";
        string searchEngineId = "e6b0fb4939f2c4b50";
        var backlinkService = new BacklinkService();
        // List<string> links = await backlinkService.GetBacklinks10( domain, searchEngineId, apiKey);
        // List<string> links = await backlinkService.GetBacklinks( domain ?? "", searchEngineId, apiKey);

        //////////////////////////////////////////////////////////////////////////////////////////////////    
        
        // var testAbotUse = new TestAbotUse();

        // await TestAbotUse.DemoSimpleCrawler(domain ?? "");

        // await TestAbotUse.DemoSinglePageRequest();


        // if (links.Count > 0)
        // {
            
        //     Save_Backlink(links, domain ?? "");    
        // }
        // return Ok(links);

        return Ok(links);
    }

    public async void Save_Backlink(List<string> links, string domain) {

        DateTime current_time = DateTime.Now;

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        Console.WriteLine("save okay?");

        foreach (var link in links) {

            using MySqlCommand command = new MySqlCommand($"INSERT INTO backlinks (domain, backlink, created_time) VALUES(@domain, @backlink, @created_time)", connection);

            command.Parameters.AddWithValue("@domain", domain);
            command.Parameters.AddWithValue("@backlink", link);
            command.Parameters.AddWithValue("@created_time", current_time);

            command.ExecuteNonQuery();

        }

        using MySqlCommand command2 = new MySqlCommand($"INSERT INTO domain (domain) VALUES(@domain)", connection);

        command2.Parameters.AddWithValue("@domain", domain);
        
        command2.ExecuteNonQuery();

        await connection.CloseAsync();
        
    }

    
}



