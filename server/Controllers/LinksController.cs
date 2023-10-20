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
using System.IO.FileSystem

namespace server.Controllers;

public class LinkCrawler
{
    public List<string> CrawlLinks(string domain)
    {
        var links = new List<string>();

        LinkCrawler crawler = new();

        var URI = "";

        if (IgnoreDomain(domain)) {
            Console.WriteLine("ignore>>>.");
            return links;
        }

        if (!crawler.IsCorrectURI(domain?? "")) {

            URI = ConvertToURI(domain ?? "");
        }
        else {
            URI = domain;
        }

        // string url = $"http://{domain}"; // Create the URL to crawl

        Console.WriteLine($"url>>>> {URI}");

        try
        {
            HtmlWeb web = new();

            HtmlDocument doc = web.Load(URI);

            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

                if (Check_link(link, domain ?? ""))
                {
                    if (!Check_existing(links, link))
                    {
                        links.Add(PickDomainFromURL(link));
                    }
                }
            }

        }
        catch (System.Exception)
        {
            Console.WriteLine("An error occurred:");
            // Handle any other exceptions that might occur during the execution of the code
        }


        return links;
        
    }

    public bool IsCorrectURI(string domain)
    {
        if (domain.Contains("https://") || domain.Contains("http://")) {
            return true;
        }

        return false;
    }
    public string ConvertToURI(string domain)
    {
        string URI = $"http://{domain}";
        
        return URI;
    }

    private bool IgnoreDomain(string domain) {
        List<string> IgnoreDomain = new (
            new []
            {
            "facebook.com",
            "linkedin.com",
            "twitter.com",
            "youtube.com",
            "skype.com",
            "instagram.com",
            "discord.com",
            "slack.com",
            "whatsapp.com",}
        );

        if (IgnoreDomain.Contains(domain)) {
            return true;
        }

        return false;
    }

    public string PickDomainFromURL(string URL) {
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

        Console.WriteLine($"start>>>>, {input}");

        JsonDocument  jsonDoc = JsonDocument.Parse(input.ToString());

        JsonElement domainElement = jsonDoc.RootElement.GetProperty("domain");
        
        string? domain = domainElement.ValueKind != JsonValueKind.Undefined ? domainElement.GetString():null;
        Console.WriteLine($"domain>>>, {domain}");

        List<string> links = new()
        {
            domain ?? ""
        };

        var crawler = new LinkCrawler();
        var index = 0;
        do {
            List<string> templinks  = crawler.CrawlLinks(links[index]);
            foreach (var link in templinks) {
                if (!crawler.Check_existing(links, link)) {
                    links.Add(link);
                }
            }

            index ++;
        } while(links.Count > index && links.Count < 1000);
        // List<string> links =  crawler.CrawlLinks(domain ?? "");
        //////////////////////////////////////////////////////////////////////////////////////////////////
        // string apiKey = "AIzaSyCqY41oLU4KL9JBIPsZyFF4W9A00WmlKsI";
        // string apiKey = "AIzaSyDK_BNn-W6zDYg4D1Jy-0mMQvR-hHDJTPA";
        // string apiKey = "AIzaSyAWp2AFSPxOxcPeJGB6iddHqHwPayphJDg";
        // string searchEngineId = "d61f36940c5cf411e";
        // string searchEngineId = "7559c0c631de64c8a";
        // string searchEngineId = "e6b0fb4939f2c4b50";
        // var backlinkService = new BacklinkService();
        // List<string> links = await backlinkService.GetBacklinks10( domain, searchEngineId, apiKey);
        // List<string> links = await backlinkService.GetBacklinks( domain ?? "", searchEngineId, apiKey);

        //////////////////////////////////////////////////////////////////////////////////////////////////    
        
        // var testAbotUse = new TestAbotUse();

        // await TestAbotUse.DemoSimpleCrawler(domain ?? "");

        // await TestAbotUse.DemoSinglePageRequest();


        if (links.Count > 0)
        {
            
            Save_Backlink(links, domain ?? "");    
        }
        return Ok(links);

        return await Task.FromResult(Ok(links));
        // return Ok(links);
    }

    public List<string> CrawlLinks(string domain) {

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



