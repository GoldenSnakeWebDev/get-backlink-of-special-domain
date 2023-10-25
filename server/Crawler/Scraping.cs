using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Net;
using MySql.Data.MySqlClient;
using HtmlAgilityPack;
using crawler;

namespace scraping;

public class LinkScraping {
    public void Scraper(string domain) {
        Console.WriteLine("start?");
        List<string> backlinks = GetBacklinks(domain);
        
        Console.WriteLine("Incoming backlinks for " + domain + ":");
        foreach (string backlink in backlinks)
        {
            Console.WriteLine(backlink);
        }
    } 

    public List<string> GetBacklinks(string domain)
    {

        Console.WriteLine("start scraping");

        List<string> backlinks = new List<string>();

        // string searchUrl = "https://www.google.com/search?q=link:" + domain;
        string searchUrl = $"https://www.google.com/search?q=link%3A{domain}";
        int numPages = 1;
        for(int page = 0; page < numPages; page ++) {

            HtmlWeb web = new HtmlWeb();
            
            // Extract the backlinks from the search results page
            
            HtmlDocument doc = web.Load(searchUrl );

            foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

                backlinks.Add(link);
            }
        }

        return backlinks;
    }

    public List<string> get_outgoing_link(string domain)
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

                if (!Check_relative_url(link)) {

                    links.Add(PickDomainFromURL(link));
                }

            }

            Console.WriteLine("get initial link");

            var temp_links = new List<string>();

            try {

                foreach (string link in links) {
                    if (Check_relative_url(link)) {

                        if(link.Contains(".html")) {

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


    public async void Save_link(List<string> links, string domain) {

        DateTime current_time = DateTime.Now;

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        Console.WriteLine("save okay?");

        var index = 0;

        foreach (var link in links) {

            using MySqlCommand command3 = new MySqlCommand($"INSERT INTO links (domain, link, created_time) VALUES(@domain, @backlink, @created_time)", connection);

            command3.Parameters.AddWithValue("@domain", domain);
            command3.Parameters.AddWithValue("@backlink", link);
            command3.Parameters.AddWithValue("@created_time", current_time);

            command3.ExecuteNonQuery();

            index ++;
        }

        await connection.CloseAsync();
        
    }

    public async Task<bool> Check_and_Save_on_DB(string domain, string link) {

        DateTime current_time = DateTime.Now;

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        MySqlCommand command3 = new MySqlCommand($"SELECT COUNT(*) FROM links WHERE link = @link", connection);

        command3.Parameters.AddWithValue("@link", link);

        // var reader =  command.ExecuteReader();
        int count = Convert.ToInt32(command3.ExecuteScalar());

        if (count > 0) {
            await connection.CloseAsync();

            return true;
        }
        else {
            command3 = new MySqlCommand($"INSERT INTO links (domain, link, created_time) VALUES(@domain, @backlink, @created_time)", connection);
            command3.Parameters.AddWithValue("@domain", domain);
            command3.Parameters.AddWithValue("@backlink", link);
            command3.Parameters.AddWithValue("@created_time", current_time);
            command3.ExecuteNonQuery();
            await connection.CloseAsync();

            return false;
        }

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

    public bool Check_relative_url(string url) {

        Console.WriteLine($"url>>>> {url}");
        Uri uri;
        bool isRelative = Uri.TryCreate(url, UriKind.Relative, out uri);

        Console.WriteLine($"is Relative url????>>>> {isRelative}");
        return isRelative;
    }
    public string Make_absolute_url (string domain, string relativeUrl) {

        Console.WriteLine("make absolute url");
        Uri baseUri = new Uri(domain);
        Uri absoluteUri = new Uri(baseUri, relativeUrl);
        string absoluteUrl = absoluteUri.ToString();

        return absoluteUrl;
    }
    public string ConvertToURI(string domain)
    {
        string URI = $"http://{domain}";
        
        return URI;
    }

    public Dictionary<string, List<string>> Get_backlink_from_DB(string domain) {

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        using MySqlCommand command = new MySqlCommand($"SELECT link, created_time FROM links WHERE link=@domain", connection);

        command.Parameters.AddWithValue("@domain", domain);

        var reader =  command.ExecuteReader();

        List<string> result_Backlink = new List<string>();

        List<string> result_time = new List<string>();

        while (reader.Read()) {
            var backlink =reader.GetString(0);
            var created_time = reader.GetString(1);

            // string[] temp = {backlink, created_time};

            result_Backlink.Add(backlink);
            result_time.Add(created_time);
        }

        Dictionary<string, List<string>> result = new Dictionary<string, List<string>>();
        result["backlink"] = result_Backlink;
        result["time"] = result_time;

        return result;
    }

    public string[] Get_Lists() {

        Console.WriteLine("okay?");
        var lists = System.IO.File.ReadAllLines("link lists.txt");

        Console.WriteLine($"lists>>>: {lists}");

        return lists;
    }

}