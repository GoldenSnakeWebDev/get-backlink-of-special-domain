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

namespace server.Controllers;

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

public class BacklinkService
{
   
    public async Task<List<string>> GetBacklinks(string domain)
    {
        var backlinks = new List<string>();

        string searchUrl = $"https://www.google.com/search?q=link:{Uri.EscapeDataString(domain)}";

        Console.WriteLine($"searchUrl>>>> {searchUrl}");

        HtmlWeb htmlWeb = new HtmlWeb();
        HtmlDocument htmlDocument = htmlWeb.Load(searchUrl);

        // HtmlNode backlinkNodes = htmlDocument.DocumentNode.SelectNodes("//a[@href]");

        foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
        {
            string link = WebUtility.HtmlDecode(node.GetAttributeValue("href", ""));
            backlinks.Add(link);
        }

        return backlinks;
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

        // Console.WriteLine("okay");

        // List<string> links = crawler.CrawlLinks(domain ?? "");

        /////////////////////////////////////////////////////////////////////////////////////////////////////

        // Console.WriteLine($"backlinks>>>>>  {links}");

        // var retriever = new BacklinkRetriever();
        // string links = await retriever.GetBacklinks(domain ?? "");

        //////////////////////////////////////////////////////////////////////////////////////////////
        
        var backlinkService = new BacklinkService();

        List<string> links = await backlinkService.GetBacklinks(domain ?? "");

        //////////////////////////////////////////////////////////////////////////////////////////////////
        save_Backlink(links, domain);

        // string links = "hello";
        // save_Backlink(links, domain);

        return Ok(links);
        // return Ok();
    }

    public async void save_Backlink(List<string> links, string domain) {
    // public async void save_Backlink(string links, string domain1) {

        DateTime current_time = DateTime.Now;

        string connectionString = "server=localhost;userid=root;password=;database=backlink";

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        Console.WriteLine("save okay?");

        foreach (var link in links) {

            Console.WriteLine($"backlink>>>>, {link}");

            using MySqlCommand command = new MySqlCommand($"INSERT INTO backlinks (domain, backlink, created_time) VALUES(@domain, @backlink, @created_time)", connection);

            command.Parameters.AddWithValue("@domain", domain);
            command.Parameters.AddWithValue("@backlink", link);
            command.Parameters.AddWithValue("@created_time", current_time);

            command.ExecuteNonQuery();

            Console.WriteLine("save okay!!!");

        }

        using MySqlCommand command2 = new MySqlCommand($"INSERT INTO domain (domain) VALUES(@domain)", connection);

        command2.Parameters.AddWithValue("@domain", domain);
        
        command2.ExecuteNonQuery();

        await connection.CloseAsync();
        
    }
}



