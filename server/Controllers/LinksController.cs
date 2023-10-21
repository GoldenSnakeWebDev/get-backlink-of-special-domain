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
using Microsoft.AspNetCore.SignalR;

namespace server.Controllers;

public class LinkCrawler
{
    public List<string> CrawlLinks(string domain)
    {
        var links = new List<string>();

        LinkCrawler crawler = new();

        var URI = "";

        if (!crawler.IsCorrectURI(domain?? "")) {

            URI = ConvertToURI(domain ?? "");
        }
        else {
            URI = domain;
        }

        try
        {
            HtmlWeb web = new();

            HtmlDocument doc = web.Load(URI);

            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

                links.Add(PickDomainFromURL(link));
            }

        }
        catch (System.Exception)
        {
            Console.WriteLine("An error occurred:");
            // Handle any other exceptions that might occur during the execution of the code
        }

        return links;
        
    }

    public string[] Get_Lists() {

        Console.WriteLine("okay?");
        var lists = System.IO.File.ReadAllLines("link lists.txt");

        Console.WriteLine($"lists>>>: {lists}");

        return lists;
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

        string[] link_lists = crawler.Get_Lists();

        foreach (var link in link_lists) {

            List<string> templinks  = crawler.CrawlLinks(link);
            foreach (var templink in templinks) {

                Console.WriteLine($"link>>> {templink}");

                if (crawler.Check_link(templink, domain)) {

                    // string temp = crawler.PickDomainFromURL(templink);

                    if (!crawler.Check_existing(links, link)) {

                        Console.WriteLine($"added link>>>> {link}");
                        links.Add(link);
                        // send_backlink(templink);
                    }
                }
            }

        }

        if (links.Count > 0)
        {
            
            Save_Backlink(links, domain ?? "");    
        }
        return Ok(links);

        return await Task.FromResult(Ok(links));
        // return Ok(links);
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

    // public async Task send_backlink(string backlink) {
    //     await  _hubContext.Clients.All.SendAsync("links", backlink);
    // }
    
}



