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
using System.Runtime.Serialization;

namespace crawler;

public class LinkCrawler
{
    public List<string> CrawlLinks(string domain)
    {
        var links = new List<string>();

        var result_links = new List<string>();

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

                links.Add(link);
            }

            Console.WriteLine("get initial link");

            var temp_links = new List<string>();

            try {

                foreach (string link in links) {
                    if (Check_relative_url(link)) {
                        string absoluteUrl = Make_absolute_url(URI, link);
                        
                        HtmlDocument tmep_doc = web.Load(absoluteUrl);

                        foreach (HtmlNode linkNode in tmep_doc.DocumentNode.SelectNodes("//a[@href]"))
                        {
                            string templink = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

                            Console.WriteLine($"templink>>>> {templink}");

                            temp_links.Add(templink);
                        }
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Error is occured: {ex.Message}");
            }


            result_links = links.Concat(temp_links).ToList();

        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            // Handle any other exceptions that might occur during the execution of the code
        }

        return result_links;
        
    }

    public string Make_absolute_url (string domain, string relativeUrl) {

        Console.WriteLine("make absolute url");
        Uri baseUri = new Uri(domain);
        Uri absoluteUri = new Uri(baseUri, relativeUrl);
        string absoluteUrl = absoluteUri.ToString();

        return absoluteUrl;
    }

    public bool Check_relative_url(string url) {

        Console.WriteLine($"url>>>> {url}");
        Uri uri;
        bool isRelative = Uri.TryCreate(url, UriKind.Relative, out uri);

        Console.WriteLine($"is Relative url????>>>> {isRelative}");
        return isRelative;
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
        if (link.Contains(domain)) {
            return true;
        }

        return false;
    }

    public bool Check_existing (List<string> links, string link) {
        if (links.Contains(link)) {
            return true;
        }
        return false;
    }

    public async Task<List<string>> get_Sublink_From_DB(string domain) {
        // string connectionString = "server=localhost;userid=root;password=;database=backlink";

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        using MySqlCommand command = new MySqlCommand($"select sublink from sublink where domain = @domain", connection);
        command.Parameters.AddWithValue("@domain", domain);
        var reader =  command.ExecuteReader();

        List<string> result = new List<string>();

        while (reader.Read()) {
            var sublink = reader.GetString(0);
            result.Add(sublink);
        }

        await connection.CloseAsync();

        return result;
    }

    public async void Save_Sublink (List<string> sublink, string domain) {
    
        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        Console.WriteLine("save okay?");
        
        var index = 0;

        foreach (var link in sublink) {

            using MySqlCommand command = new MySqlCommand($"INSERT INTO sublink (domain, sublink) VALUES(@domain, @sublink)", connection);

            command.Parameters.AddWithValue("@domain", domain);
            command.Parameters.AddWithValue("@sublink", link);
            
            command.ExecuteNonQuery();

            index ++;
        }

        await connection.CloseAsync();
    }

    public async void Save_Backlink(List<string> links, List<DateTime> date_times, string domain) {

        DateTime current_time = DateTime.Now;

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        Console.WriteLine("save okay?");
        using MySqlCommand command = new MySqlCommand($"DELETE FROM backlinks WHERE domain = @domain", connection);
        command.Parameters.AddWithValue("@domain", domain);
        command.ExecuteNonQuery();

        using MySqlCommand command1 = new MySqlCommand($"DELETE FROM domain WHERE domain = @domain", connection);
        command1.Parameters.AddWithValue("@domain", domain);
        command1.ExecuteNonQuery();

        var index = 0;

        foreach (var link in links) {

            using MySqlCommand command3 = new MySqlCommand($"INSERT INTO backlinks (domain, backlink, created_time) VALUES(@domain, @backlink, @created_time)", connection);

            command3.Parameters.AddWithValue("@domain", domain);
            command3.Parameters.AddWithValue("@backlink", link);
            command3.Parameters.AddWithValue("@created_time", date_times[index]);

            command3.ExecuteNonQuery();

            index ++;
        }

        using MySqlCommand command2 = new MySqlCommand($"INSERT INTO domain (domain) VALUES(@domain)", connection);

        command2.Parameters.AddWithValue("@domain", domain);
        
        command2.ExecuteNonQuery();

        await connection.CloseAsync();
        
    }
}
