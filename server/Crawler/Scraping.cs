using System;
using System.Collections.Generic;
using System.Net;
using HtmlAgilityPack;

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
}