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


Die Bilder in dem Ordner müssen nun in zwei Schritten zu einem CAFFE-kompatiblen Imageset konvertiert werden.
Schritt 1:
Die Bilddateien bekommen in einer Textdatei ihr Label, also ihre *Kategorie* zugewiesen. Hier in dem Bild sind die Labels einfach durchnummeriert. Poseidonbilder bekommen eine 0, Tempelbilder eine 1 und Zeusbilder eine 2.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20labeltext.PNG" />
</p>

Schritt 2:
Nach dem kompilieren von CAFFE kann das beigefügte Tool zum konvertieren des Imagesets mit wenigen Anweisungen benutzt werden
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20create_imageset.PNG" />
</p>

Nun muss ein CAFFE-Modell gebaut und trainiert werden.
Schritt 1: Der Output des eben erstellten Imagesets 
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20train_lmdb.PNG" />
</p>
wird verwendet um den *Image Mean* (Mittelwert) der Elemente des Imagesets auszurechnen.
Mit folgendem Befehl wird aus dem Imageset (train_lmdb) eine Mittelwertdatei (mean.binaryproto)
```
compute_image_mean -backend=lmdb C:\caffe\bin\images\train_lmdb\ C:\caffe\bin\images\mean.binaryproto
```

Schritt 2: 
Unser CaffeNet muss nun konfiguriert werden mit einer Trainingskonfiguration und einer Solverkonfiguration.
Ein Teil der [Trainingskonfiguration](https://github.com/HeyItsBATMAN/kigods/blob/master/train_val.prototxt) sieht z.B. so aus
```
name: "CaffeNet"
layer {
  name: "data"
  type: "Data"
  top: "data"
  top: "label"
  include {
    phase: TRAIN
  }
  transform_param {
    mirror: true
    crop_size: 64
    mean_file: "images/mean.binaryproto"
  }
  data_param {
    source: "C:\\caffe\\bin\\images\\train_lmdb"
    batch_size: 256
    backend: LMDB
  }
}
```
Die Konfiguration für unser Modell wurde aus dem Beispielmodell vom [CAFFE-Standard](https://github.com/BVLC/caffe/blob/master/models/bvlc_reference_caffenet/train_val.prototxt) abgeleitet. Es mussten lediglich einige Eingabewerte, wie die Auflösung unserer Bilder und die Anzahl an Labels/Kategorien, und die Pfade zu unserem Imageset und der Mittelwertdatei geändert werden

Die [Solverkonfiguration](https://github.com/HeyItsBATMAN/kigods/blob/master/solver.prototxt) für unser Modell sieht so aus:
```
net: "images/train_val.prototxt"
test_iter: 1000
test_interval: 1000
base_lr: 0.001
lr_policy: "step"
gamma: 0.1
stepsize: 2500
display: 50
max_iter: 40000
momentum: 0.9
weight_decay: 0.0005
snapshot: 5000
snapshot_prefix: "images/models/model_1"
solver_mode: GPU
```
Sie beinhaltet die Informationen, wie viele Durchgänge das Netz maximal an einem Modell am Stück trainiert, und wie die Ergebnisse des Trainings weiter Einfluss auf das Training haben, wodurch der Deep Learning Faktor des Frameworks entsteht. Außerdem kann eingestellt werden, dass Rechenleistung der GPU genutzt wird, was dass Training erheblich verschnellert

Schritt 3:
Das Modell kann nun trainiert werden. Während dem Training gibt CAFFE nicht viel Output was es tut, sondern nur, dass es überhaupt etwas tut.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20train.PNG" />
</p>

Wie in der Solverkonfiguration eingestellt, gibt es alle 50 Schritte einen kurzen Überblick über den *Loss* des neuralen Netzwerks
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20iteration%2050.PNG" />
</p>

In einem [Log](https://github.com/HeyItsBATMAN/kigods/blob/master/model_1.log) kann sich dann später durchlesen wie es lief, oder, wie wir es hier gemacht haben, einen Graphen erstellen, der darstellt wie sich der errechnete *Loss*-Wert alle 50 Durchgänge verändert hat.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/loss%20chart.PNG" />
</p>


# Und jetzt?
Mit unseren Modellen kann man nun mittels CAFFE eine Klassifizierung durchführen, die ein Eingabebild mit dem Modell abgleicht und eine Vorhersage trifft, welches Label dem neuen Bild am nächsten kommt.
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20finished%20model.PNG" />
</p>
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20finished%20prediction.PNG" />
</p>
<p align="center">
  <img src="https://raw.githubusercontent.com/HeyItsBATMAN/kigods/master/caffe%20korrigierte%20labels.PNG" />
</p>
Unser eigenes Modell, welches mit knapp 250 Bildern trainiert hat, kann also nun Vorhersagen treffen, ob ein Eingabebild zu Poseidon, Zeus oder einer alten Gebäudestruktur passt
