# BLink
<b>Asp.Net Web Api Facebook-Like Link Preview.</b>
<p>Publish project on your IIS and use:</p>

&bull; C#

```cs
string method = "http://localhost/BLink/Api/Preview";
string previewLink = "http://james-nicoll.livejournal.com/5605983.html";
using (WebClient wc = new WebClient())
{
  wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
  wc.Encoding = Encoding.UTF8;
  string preview = wc.UploadString(method, "POST", String.Format("={0}", previewLink));
  
  // Working with link preview...
  
}
```

&bull; JavaScript (JQuery)

```javascript
var method = 'http://localhost/BLink/Api/Preview';
var previewLink = 'http://james-nicoll.livejournal.com/5605983.html';
$.post(method, { '': previewLink }, function (data) {

  // Working with link preview...
      
});
```

Method /Api/Preview/ returns json like this:

```javascript
{
    "Url": 'http://james-nicoll.livejournal.com/5605983.html',
    "Host": "james-nicoll.livejournal.com",
    "Title": "my 55th birthday present! - More Words, Deeper Hole",
    "Description": "Cut in case Also posted at Dreamwidth , where there are comment(s); comment here or there .",
    "Text": "How very cool, thanks for sharing that! I wish it was bigger; even at this size it's fascinating. I was thinking ...",
    "Images": [
        "https://cdn.shopify.com/s/files/1/0211/4926/products/P-Space_ImgA_75cd7df5-3020-4839-8953-ad1358db2e9a_1024x1024.jpg?v=1457740955",
        "http://s07.flagcounter.com/count/BDyO/bg=FFFFFF/txt=FFFFF1/border=FFFFFF/columns=1/maxflags=1/viewers=3/labels=0/pageviews=0/flags=0/",
        "http://l-stat.livejournal.net/img/schemius/print-logo.png?v=49361"
    ] 
}
```




