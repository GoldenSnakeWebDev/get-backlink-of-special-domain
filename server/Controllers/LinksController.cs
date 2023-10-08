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

public class BacklinkRetriever
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<string> GetBacklinks(string domain)
    {
        string url = $"https://api.ahrefs.com/v1/backlinks?target={domain}&output=json&limit=100";
        
        HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        HttpResponseMessage response = await client.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        else
        {
            throw new Exception($"Failed to retrieve backlinks. StatusCode: {response.StatusCode}");
        }
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
    public async Task<IActionResult> PostAsync([FromBody] dynamic input)
    {

        Console.WriteLine("start>>>>");

        JsonDocument  jsonDoc = JsonDocument.Parse(input.ToString());

        JsonElement domainElement = jsonDoc.RootElement.GetProperty("domain");
        
        string? domain = domainElement.ValueKind != JsonValueKind.Undefined ? domainElement.GetString():null;
        Console.WriteLine($"domain>>>, {domain}");

        // LinkCrawler crawler = new LinkCrawler();

        // List<string> links = crawler.CrawlLinks(domain ?? "");

        // Console.WriteLine($"backlinks>>>>>  {links}");

        var retriever = new BacklinkRetriever();
        string links = await retriever.GetBacklinks(domain ?? "");
        
        return Ok(links);
        // return Ok();
    }
}

