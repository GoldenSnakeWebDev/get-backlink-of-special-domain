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

            if (check_link(link, domain)) {
                if (!check_existing(links, link)) {

                    links.Add(link);
                }
            }
        }

        return links;
    }

    public bool check_link (string link, string domain) {
        if ( link.Contains("https://") || link.Contains("http://")) {
            if (!link.Contains(domain)) {
                return true;
            }
        }

        return false;
    }

    public bool check_existing (List<string> links, string link) {
        if (links.Contains(link)) {
            return true;
        }
        return false;
    }
}

public class BacklinkService
{
   
    public async Task<List<string>> GetBacklinks10(string domain, string cx, string apikey)
    {
        var backlinks = new List<string>();

        int start = 1;
        int numResults = 100;


        var apiUrl =$"https://customsearch.googleapis.com/customsearch/v1?cx={cx}&key={apikey}&q={domain}&start={start}";
        HttpClient httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(apiUrl);

        // var response = http.Request(apiUrl);
        var jResponse = JObject.Parse(response);

        Console.WriteLine($"response>>>> {jResponse}");

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

        // List<string> links = crawler.CrawlLinks(domain ?? "");

        /////////////////////////////////////////////////////////////////////////////////////////////////////

        // Console.WriteLine($"backlinks>>>>>  {links}");

        // var retriever = new BacklinkRetriever();
        // string links = await retriever.GetBacklinks(domain ?? "");

        //////////////////////////////////////////////////////////////////////////////////////////////
        
        // var backlinkService = new BacklinkService();

        // List<string> links = await backlinkService.GetBacklinks(domain ?? "");

        //////////////////////////////////////////////////////////////////////////////////////////////////

        // var googleSearchService = new GoogleSearchService();

        // List<string> links = await googleSearchService.Search(domain ?? "");

        //////////////////////////////////////////////////////////////////////////////////////////////////

        

        // string links = "hello";
        // save_Backlink(links, domain);
        //////////////////////////////////////////////////////////////////////////////////////////////////
        string apiKey = "AIzaSyCqY41oLU4KL9JBIPsZyFF4W9A00WmlKsI";
        string searchEngineId = "d61f36940c5cf411e";
        var backlinkService = new BacklinkService();
        List<string> links = await backlinkService.GetBacklinks10( domain, searchEngineId, apiKey);
        //////////////////////////////////////////////////////////////////////////////////////////////////    

        save_Backlink(links, domain);    
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



