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
            string[] link_lists = crawler.Get_Lists();
            foreach (var link in link_lists) {
                await Clients.All.SendAsync("link", link);

                List<string> templinks  = crawler.CrawlLinks(link);
                foreach (var templink in templinks) {

                    Console.WriteLine($"link>>> {templink}");

                    if (crawler.Check_link(templink, domain ?? "")) {

                        // string temp = crawler.PickDomainFromURL(templink);

                        if (!crawler.Check_existing(links, link)) {

                            Console.WriteLine($"added link>>>> {link}");
                            await Clients.All.SendAsync("link", link);
                            links.Add(link);
                        }
                    }
                }

            }

             if (links.Count > 0)
            {
                
                crawler.Save_Backlink(links, domain ?? "");    
            }
           
        }
    }
}
