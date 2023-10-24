using Microsoft.AspNetCore.SignalR;
using crawler;
using scraping;
using GoogleSearch;

namespace SignalR
{
    public class ChatHub : Hub
    {
        public async Task Get_backlink(string domain)
        {
            LinkCrawler crawler = new();
            List<string> links =  new();
            List<DateTime> date_time = new();
            ////////////////////////////////////////////
            // LinkScraping scraper = new();

            // List<string> link_lists = scraper.GetBacklinks(domain);

            // foreach (string backlink in link_lists)
            // {
            //     Console.WriteLine(backlink);
            // }
            //////////////////////////////////////////
            // string apiKey = "AIzaSyCqY41oLU4KL9JBIPsZyFF4W9A00WmlKsI";
            string apiKey = "AIzaSyDK_BNn-W6zDYg4D1Jy-0mMQvR-hHDJTPA";
            // string searchEngineId = "d61f36940c5cf411e";
            string searchEngineId = "7559c0c631de64c8a";
            GoogleSearch searchEngine = new GoogleSearch();
            List<string> link_lists = searchEngine.GetBacklinks();

            ///////////////////////////////////////

            // string[] link_lists = crawler.Get_Lists();

            var number_of_list = link_lists.Count;
            var index = 0;

            foreach (var link in link_lists) {
                DateTime curTime = DateTime.Now;
                
                index ++;
                await Clients.Caller.SendAsync("progress_bar", (double)index/number_of_list * 100);

                // List<string> templinks = new ();
                // templinks = await crawler.get_Sublink_From_DB(link);
                // if (templinks.Count < 1) {

                //     templinks  = crawler.CrawlLinks(link);

                //     crawler.Save_Sublink(templinks, link);
                // }

                // foreach (var templink in templinks) {

                    if (crawler.Check_link(link, domain ?? "")) {
                        Console.WriteLine($"link>>> {link} domain>>>: {domain}");

                        // string temp = crawler.PickDomainFromURL(templink);

                        string str_picked_domain = crawler.PickDomainFromURL(link);

                        if (!crawler.Check_existing(links, str_picked_domain)) {
                            DateTime cur_Time = DateTime.Now;
                            Console.WriteLine($"added link>>>> {link}");
                            await Clients.Caller.SendAsync("link", link, cur_Time);
                            
                            links.Add(str_picked_domain);
                            date_time.Add(cur_Time);
                            break;
                        }
                    }
                // }
            }

             if (links.Count > 0)
            {
                
                crawler.Save_Backlink(links, date_time, domain ?? "");    
            }

            await Clients.All.SendAsync("getting_end");
           
        }
    }
}
