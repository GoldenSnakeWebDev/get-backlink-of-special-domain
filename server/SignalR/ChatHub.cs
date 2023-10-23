using Microsoft.AspNetCore.SignalR;
using crawler;
namespace SignalR
{
    public class ChatHub : Hub
    {
        public async Task Get_backlink(string domain)
        {
            LinkCrawler crawler = new();
            List<string> links =  new();
            List<DateTime> date_time = new();

            string[] link_lists = crawler.Get_Lists();

            var number_of_list = link_lists.Length;
            var index = 0;

            foreach (var link in link_lists) {
                DateTime curTime = DateTime.Now;
                
                // await Clients.All.SendAsync("link", link, curTime);
                index ++;
                await Clients.Caller.SendAsync("progress_bar", (double)index/number_of_list * 100);

                // links.Add(link);
                // date_time.Add(curTime);

                // Console.WriteLine($"progress: {(int)Math.Ceiling((double)index/number_of_list * 100)} number of list: {number_of_list} index: {index}");
                List<string> templinks = new ();
                templinks = await crawler.get_Sublink_From_DB(link);
                if (templinks.Count < 1) {

                    templinks  = crawler.CrawlLinks(link);

                    crawler.Save_Sublink(templinks, link);
                }

                foreach (var templink in templinks) {

                    // await Clients.All.SendAsync("link", templink, curTime);

                    if (crawler.Check_link(templink, domain ?? "")) {
                        
                        Console.WriteLine($"link>>> {templink} domain>>>: {domain}");

                        // string temp = crawler.PickDomainFromURL(templink);

                        if (!crawler.Check_existing(links, link)) {
                            DateTime cur_Time = DateTime.Now;
                            Console.WriteLine($"added link>>>> {link}");
                            await Clients.Caller.SendAsync("link", link, cur_Time);
                            // await Clients.All.SendAsync("progress_bar", (int)Math.Ceiling((double)index/number_of_list * 100));
                            links.Add(link);
                            date_time.Add(cur_Time);
                            break;
                        }
                    }
                }
            }

             if (links.Count > 0)
            {
                
                crawler.Save_Backlink(links, date_time, domain ?? "");    
            }

            await Clients.All.SendAsync("getting_end");
           
        }
    }
}
