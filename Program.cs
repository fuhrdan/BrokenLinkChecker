//*****************************************************************************
//** Broken Link Checker                                                     **
//** A simple program to check for broken links on a website.      -Dan      **
//*****************************************************************************


using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace BrokenLinkChecker
{
    class Program
    {
        private static readonly HttpClient httpClient = new HttpClient();

        static async Task Main(string[] args)
        {
            Console.WriteLine("Enter the URL to check for broken links:");
            string url = Console.ReadLine();

            if (string.IsNullOrEmpty(url))
            {
                Console.WriteLine("Invalid URL. Please try again.");
                return;
            }

            Console.WriteLine($"Checking links in {url}...");
            var brokenLinks = await CheckForBrokenLinks(url);

            if (brokenLinks.Count > 0)
            {
                Console.WriteLine("Broken links found:");
                foreach (var link in brokenLinks)
                {
                    Console.WriteLine(link);
                }
            }
            else
            {
                Console.WriteLine("No broken links found.");
            }
        }

        private static async Task<List<string>> CheckForBrokenLinks(string url)
        {
            List<string> brokenLinks = new List<string>();
            try
            {
                var response = await httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                string htmlContent = await response.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                // Extract all links (href attributes)
                foreach (var link in htmlDocument.DocumentNode.SelectNodes("//a[@href]"))
                {
                    string linkHref = link.GetAttributeValue("href", string.Empty);
                    if (IsValidUrl(linkHref))
                    {
                        var statusCode = await CheckLink(linkHref);
                        if (statusCode != 200)
                        {
                            brokenLinks.Add($"{linkHref} - Status Code: {statusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking links: {ex.Message}");
            }
            return brokenLinks;
        }

        private static bool IsValidUrl(string url)
        {
            return Uri.IsWellFormedUriString(url, UriKind.Absolute);
        }

        private static async Task<int> CheckLink(string url)
        {
            try
            {
                var response = await httpClient.GetAsync(url);
                return (int)response.StatusCode;
            }
            catch
            {
                return 0; // Return 0 for failed requests (broken links)
            }
        }
    }
}