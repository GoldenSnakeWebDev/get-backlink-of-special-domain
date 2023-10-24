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

namespace GoogleSearch;

public class GoogleSearch {
    public async Task<List<string>> GetBacklinks(string domain, string cx, string apikey)
    {
        var backlinks = new List<string>();
        var googleSearchResult = new List<string>();

        int start = 1;
        int numResults = 10;

        while (backlinks.Count < numResults)
        {
            var apiUrl =$"https://customsearch.googleapis.com/customsearch/v1?cx={cx}&key={apikey}&q={domain}&start={start}";
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(apiUrl);

            // var response = http.Request(apiUrl);
            var jResponse = JObject.Parse(response);

            foreach (var item in jResponse["items"])
            {
                string link = item["link"].ToString();
                googleSearchResult.Add(link);
            }

            if (jResponse["queries"]["nextPage"] == null)
            {
                break; // No more results available
            }

            int nextStart = int.Parse(jResponse["queries"]["nextPage"][0]["startIndex"].ToString());

            if (nextStart <= start)
            {
                break; // Ensure we don't get stuck in an infinite loop
            }

            start = nextStart;

        }

        foreach (var link in googleSearchResult)
        {
            Console.WriteLine(link);
        }

        // backlinks = await filterSearchResult(googleSearchResult, domain);

        return googleSearchResult;
    }
    
    public async Task<List<string>> filterSearchResult(List<string> searchResult, string domain) {
        
        var links = new List<string>();

        Console.WriteLine($"filter okay???? {searchResult}");

        foreach (var link in searchResult)
        {

            Console.WriteLine($"filter link>> {link}");

            if (!check_link(link, domain))
            {
                Console.WriteLine("checked link");

                // if (!check_existing(searchResult, link))
                // {
                    // Console.WriteLine("don't exist");

                    if (CrawlLinksAndCheckDomain(link, domain))
                    {
                        links.Add(link);
                    }    
                // }
                
            }
        }

        return links;

    }

    public bool check_link (string link, string domain) {

        return link.Contains(domain);
    }

    public bool check_existing (List<string> links, string link) {
        if (links.Contains(link)) {
            return true;
        }
        return false;
    }

    public bool CrawlLinksAndCheckDomain(string URL, string domain)
    {
        var links = new List<string>();

        HtmlWeb web = new HtmlWeb();

        HtmlDocument doc = web.Load(URL);

        foreach (HtmlNode linkNode in doc.DocumentNode.SelectNodes("//a[@href]"))
        {
            string link = WebUtility.HtmlDecode(linkNode.GetAttributeValue("href", ""));

            Console.WriteLine($"checked link>>>> {link}");


            if (check_link(link, domain)) {
                return true;
            }
        }

        return false;
    }
}