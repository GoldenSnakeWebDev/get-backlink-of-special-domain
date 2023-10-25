using Microsoft.AspNetCore.SignalR;
using crawler;
using scraping;
using SearchEngine;

namespace SignalR
{
    public class ChatHub : Hub
    {
        public async Task Get_backlink(string domain)
        {
            LinkCrawler crawler = new();
            List<string> links =  new();
            List<DateTime> date_time = new();
            LinkScraping scraper = new();

            Dictionary<string, List<string>> result_Backlink = new Dictionary<string, List<string>>();

            result_Backlink = scraper.Get_backlink_from_DB(domain);

            var index = 0;
            var number_of_list = result_Backlink.Count;
            foreach (var item in result_Backlink) {
                await Clients.Caller.SendAsync("progress_bar", (double)index/number_of_list * 100);
                await Clients.Caller.SendAsync("link", item['backlink'], item['time']);
            }

            await Clients.Caller.SendAsync("getting_end");


            ////////////////////////////////////////////

            // foreach (string backlink in link_lists)
            // {
            //     Console.WriteLine(backlink);
            // }
            //////////////////////////////////////////
            // string apiKey = "AIzaSyCqY41oLU4KL9JBIPsZyFF4W9A00WmlKsI";
            // string apiKey = "AIzaSyDK_BNn-W6zDYg4D1Jy-0mMQvR-hHDJTPA";
            // // string searchEngineId = "d61f36940c5cf411e";
            // string searchEngineId = "7559c0c631de64c8a";
            // GetBacklink_GoogleSearch searchEngine = new();
            // List<string> link_lists = await searchEngine.Search(domain, searchEngineId, apiKey);

            // foreach (string backlink in link_lists)
            // {
            //     Console.WriteLine(backlink);
            // }

            ///////////////////////////////////////

            // string[] link_lists = crawler.Get_Lists();

            // var number_of_list = link_lists.Count;
            // var index = 0;

            // foreach (var link in link_lists) {
            //     DateTime curTime = DateTime.Now;
                
            //     index ++;
            //     await Clients.Caller.SendAsync("progress_bar", (double)index/number_of_list * 100);

            //     List<string> templinks = new ();
            //     // templinks = await crawler.get_Sublink_From_DB(link);
            //     // if (templinks.Count < 1) {


            //     //     crawler.Save_Sublink(templinks, link);
            //     // }
            //     templinks  = crawler.CrawlLinks(link);

            //     foreach (var templink in templinks) {

            //         if (crawler.Check_link(templink, domain ?? "")) {
            //             Console.WriteLine($"link>>> {templink} domain>>>: {domain}");

            //             // string temp = crawler.PickDomainFromURL(templink);

            //             // string str_picked_domain = crawler.PickDomainFromURL(link);

            //             if (!crawler.Check_existing(links, link)) {
            //                 DateTime cur_Time = DateTime.Now;
            //                 Console.WriteLine($"added link>>>> {link}");
            //                 await Clients.Caller.SendAsync("link", link, cur_Time);
                            
            //                 links.Add(link);
            //                 date_time.Add(cur_Time);
            //                 break;
            //             }
            //         }
            //     }
            // }

            //  if (links.Count > 0)
            // {
                
            //     crawler.Save_Backlink(links, date_time, domain ?? "");    
            // }

            // await Clients.Caller.SendAsync("getting_end");
           
        }

        public async Task Get_and_save_to_DB(string domain) {
            LinkCrawler crawler = new();
            List<string> links =  new();
            List<DateTime> date_time = new();
            ////////////////////////////////////////////
            LinkScraping scraper = new();

            links[0] = domain;

            long index = 0;

            foreach (var link in links) {

                List<string> temp_links = scraper.get_outgoing_link(link);
                foreach (var temp_link in temp_links) {
                    if (!scraper.Check_and_Save_on_DB(link, temp_link)) {
                        links.Add(temp_link);

                    }
                }
            }

            await Clients.Caller.SendAsync("getting_end");
        }
    }
}
