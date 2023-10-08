using Microsoft.AspNetCore.Mvc;
namespace server.Controllers;

// LinkCrawler.cs
using HtmlAgilityPack;
using System.Net;
using System;
using System.Text.Json;

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
[Route("api/[controller]")]
public class LinksController : ControllerBase
{
    [HttpPost]
    public IActionResult Post([FromBody] dynamic data)
    {
        // LinkCrawler crawler = new LinkCrawler();

        Console.WriteLine($"domain>>>, {data}");

        // List<string> links = crawler.CrawlLinks(data.domain.ToString());

        // return Ok(links);
        return Ok();
    }
}

