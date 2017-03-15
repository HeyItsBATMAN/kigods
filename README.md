# Künstliche Intelligenz und Cultural Heritage
## Ein Kurs bei Dr. Jan Gerrit Wieners und Prof. Dr. Øyvind Eide

<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/17357225_1419599171392757_2132309679_o.png" />
</p>

**Anmerkung**: 
Für den größten Teils des Projekt wurde das Deep Learning Framework *CAFFE* verwendet. Da das kompilierte Projekt mit allen Dependencies *mehrere* Gigabyte an Daten sind, ist dieses Github-Repository nur eine *Ergebnissammlung*
[CAFFE](http://caffe.berkeleyvision.org/) [(Repository)](https://github.com/BVLC/caffe/tree/master)

[**Arachne**](https://arachne.dainst.org/): 
Als Quelle der Bilder haben wir **Arachne** verwendet. Über Arachne 
*"iDAI.objects arachne (Kurzform: Arachne) ist die zentrale Objektdatenbank des Deutschen Archäologischen Instituts (DAI) und der Arbeitsstelle für Digitale Archäologie (CoDArchLab) des Archäologischen Instituts der Universität zu Köln."*

[**Image Crawler**](https://github.com/HeyItsBATMAN/kigods/blob/master/Program.cs): 
Um die Bilder von Arachne schnell runterladen zu können, haben wir einen *Image Crawler* in **C#** geschrieben. Über eine [PhantomJS](http://phantomjs.org/)-Browserinstanz werden alle Bilder, die über die Arachne-Suche in der Kategorie *"Bilder"* zu finden sind, in einem Array gemerkt
```csharp
var jsOptions = new PhantomJSOptions ();
IWebDriver jsbrowser = new PhantomJSDriver(jsOptions);
```
```csharp
List<string> entities = new List<string>();
List<IWebElement> links = new List<IWebElement>();

links = jsbrowser.FindElements(By.ClassName("ar-imagegrid-cell-link")).ToList();
Console.WriteLine("Getting entities: ");
for (int j = 0; j < links.Count (); j++) {
  Console.CursorTop = Console.CursorTop - 1;
  Console.WriteLine("Getting entities: " + ((j+1)+k*50) + "/" + totalimages);
  entities.Add(links [j].GetAttribute ("href").Split ('/') [4].Split ('?') [0]);
}
```
Eine weitere PhantomJS-Instanz loopt nun durch die *Entities*-Liste und sucht über einen Request an die OAI-PMH-Api von Arachne die URL der Bilder und speichert das Bild über einen *WebClient-Download*
```csharp
jsbrowser.Navigate().GoToUrl("http://arachne.uni-koeln.de/OAI-PMH/oai-pmh.xml?verb=GetRecord&identifier=oai:arachne.uni-koeln.de:marbilder/" + matnum + "&metadataPrefix=origin");

using (var client = new WebClient())
{
  Directory.CreateDirectory(gods[i]);
  if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\" + gods[i] + "\\" + jsbrowser.FindElement(By.TagName("dateinamemarbilder")).Text))
  {
    client.DownloadFile(new Uri(jsbrowser.FindElement(By.TagName("pfad")).Text), AppDomain.CurrentDomain.BaseDirectory + "\\" + gods[i] + "\\" + jsbrowser.FindElement(By.TagName("dateinamemarbilder")).Text);
  }
}
```


[**Image Resizer**](https://github.com/HeyItsBATMAN/kigods/blob/master/hello.py): Um die Bilder auf eine einheitliche Größe zu bringen, was erstens Grundvoraussetzung ist damit *CAFFE* mit den Bildern arbeiten kann und zweitens das Training, durch die geringe Auflösung, verschnellert, haben wir ein kurzes **Python** Script verwendet, welches mittels der *Python Image Library* die Bilder verkleinert
```python
from PIL import Image
import os, sys

path = "C:/Users/Bene/Desktop/bilder/"
dirs = os.listdir( path )

def resize():
    for item in dirs:
        if os.path.isfile(path+item):
            im = Image.open(path+item)
            f, e = os.path.splitext(path+item)
            imResize = im.resize((64,64), Image.ANTIALIAS)
            imResize.save(f + ' resized.jpg', 'JPEG', quality=90)

resize()
```


**Weiteres Vorgehen und Arbeiten mit CAFFE**:
Die runtergeladenen Bilder sind alle in einem Ordner gelagert. Da unter anderem auch Bilder von z.B. einem Zeus-*Tempel* unter den Bildern waren, wurden diese manuell umbenannt. Dadurch soll CAFFE dann unterscheiden können, welche Bilder ein Zeus Kopf sind und welche einfach nur ein Tempel.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20imageset.PNG" />
</p>


Die Bilder in dem Ordner müssen nun in mehreren Schritten zu einem CAFFE-kompatiblen Imageset konvertiert werden.
Schritt 1:
Die Bilddateien bekommen in einer Textdatei ihr Label, also ihre *Kategorie* zugewiesen. Hier in dem Bild sind die Labels einfach durchnummeriert. Poseidonbilder bekommen eine 0, Tempelbilder eine 1 und Zeusbilder eine 2.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20labeltext.PNG" />
</p>
