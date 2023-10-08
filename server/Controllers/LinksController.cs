using Microsoft.AspNetCore.Mvc;
namespace server.Controllers;

// LinkCrawler.cs
using HtmlAgilityPack;
using System.Net;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    public class ParamsModel {
        public string? Param {get; set;}
    }

    [HttpPost]
    public IActionResult Post([FromBody] dynamic input)
    {
        // LinkCrawler crawler = new LinkCrawler();

        Console.WriteLine("start>>>>");

        JsonDocument  jsonDoc = JsonDocument.Parse(input.ToString());

        JsonElement domainElement = jsonDoc.RootElement.GetProperty("domain");
        
        string? domain = domainElement.ValueKind != JsonValueKind.Undefined ? domainElement.GetString():null;
        Console.WriteLine($"domain>>>, {domain}");

        // List<string> links = crawler.CrawlLinks(data.domain.ToString());

        // return Ok(links);
        return Ok();
    }
}

