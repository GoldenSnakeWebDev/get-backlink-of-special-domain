using Microsoft.AspNetCore.Mvc;

// LinkCrawler.cs
using HtmlAgilityPack;
using System.Net;

public class LinkCrawler
{
    public List<string> CrawlLinks(string domain)
    {
        var links = new List<string>();

        string url = $"http://{domain}"; // Create the URL to crawl

        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(url);

        foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));
            links.Add(link);
        }

        return links;
    }
}

// LinksController.cs

[ApiController]
[Route("api/get_Backlinks")]
public class LinksController : ControllerBase
{
    [HttpGet("{domain}")]
    public IActionResult GetLinks(string domain)
    {
        LinkCrawler crawler = new LinkCrawler();

        List<string> links = crawler.CrawlLinks(domain);

        return Ok(links);
    }
}

