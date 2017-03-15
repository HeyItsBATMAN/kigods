using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.PhantomJS;
using OpenQA.Selenium.Chrome;
using System.Net;
using System.IO;

namespace ArachneDL
{
    class Program
    {
        static void Main(string[] args)
        {
			//var chromeOptions = new ChromeOptions();
			var jsOptions = new PhantomJSOptions ();
			jsOptions.SetLoggingPreference(LogType.Driver,LogLevel.Off);
            IWebDriver jsbrowser = new PhantomJSDriver(jsOptions);
            List<IWebElement> links = new List<IWebElement>();
            List<string> entities = new List<string>();
            List<string> gods = new List<string> { "Poseidon" , "Poseidon" };
            int totalimages = 0;

            for (int i = 0; i < gods.Count(); i++)
            {
                jsbrowser.Navigate().GoToUrl("https://arachne.dainst.org/search?fq=facet_kategorie:%22Bilder%22&fl=20&q=" + gods[i]);
                Console.WriteLine(jsbrowser.Title);
                while (totalimages == 0)
                {
                    totalimages = Convert.ToInt32(jsbrowser.FindElements(By.ClassName("text-muted"))[0].FindElements(By.TagName("strong"))[0].Text.Replace(",",""));
                }
				links = jsbrowser.FindElements(By.ClassName("ar-imagegrid-cell-link")).ToList();
                Console.WriteLine("Total images: " + totalimages);
                jsbrowser.Close();
				for (int k = 0; k < 100 / 50; k++) {
                    jsbrowser = new PhantomJSDriver(jsOptions);
                    jsbrowser.Navigate().GoToUrl("https://arachne.dainst.org/search?fq=facet_kategorie:%22Bilder%22&offset=" + (k*50) + "&fl=20&q=" + gods[i]);
					links = jsbrowser.FindElements(By.ClassName("ar-imagegrid-cell-link")).ToList();
                    Console.WriteLine("Getting entities: ");
					for (int j = 0; j < links.Count (); j++) {
                        Console.CursorTop = Console.CursorTop - 1;
                        Console.WriteLine("Getting entities: " + ((j+1)+k*50) + "/" + totalimages);
						//Console.WriteLine (links [j].GetAttribute ("href").Split ('/') [4].Split ('?') [0]);
						entities.Add(links [j].GetAttribute ("href").Split ('/') [4].Split ('?') [0]);
					}
                    Console.CursorTop = Console.CursorTop - 1;
                    jsbrowser.Close();
                }

                jsbrowser = new PhantomJSDriver(jsOptions);
                for (int k = 0; k < entities.Count(); k++)
                {
                    try
                    {
                        //http://arachne.uni-koeln.de/OAI-PMH/oai-pmh.xml?verb=GetRecord&identifier=oai:arachne.uni-koeln.de:marbilder/2769968&metadataPrefix=origin
                        jsbrowser.Navigate().GoToUrl("https://arachne.dainst.org/entity/" + entities[k]);
                    //jsbrowser.FindElement(By.XPath("/html/body/div/div[2]/div/div[3]/div[1]/div/div[2]/table/tbody/tr[2]/td[2]")).Text
                    String matnum = jsbrowser.FindElement(By.XPath("/html/body/div/div[2]/div/div[3]/div[1]/div/div[2]/table/tbody/tr[2]/td[2]")).Text;
                    jsbrowser.Navigate().GoToUrl("http://arachne.uni-koeln.de/OAI-PMH/oai-pmh.xml?verb=GetRecord&identifier=oai:arachne.uni-koeln.de:marbilder/" + matnum + "&metadataPrefix=origin");
              
                    using (var client = new WebClient())
                    {
                        //File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "pagesource.txt", jsbrowser.PageSource);
                        Directory.CreateDirectory(gods[i]);

                            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + gods[i] + "\\" + jsbrowser.FindElement(By.TagName("dateinamemarbilder")).Text))
                            {
                                client.DownloadFile(new Uri(jsbrowser.FindElement(By.TagName("pfad")).Text), AppDomain.CurrentDomain.BaseDirectory + "\\" + gods[i] + "\\" + jsbrowser.FindElement(By.TagName("dateinamemarbilder")).Text);
                            }
                        
                    }
                    }
                    catch { }
                }
                jsbrowser.Close();
                entities.RemoveRange(0, entities.Count());
            }
        }
    }
}
