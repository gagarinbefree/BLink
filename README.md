# BLink
Asp.Net Web Api Facebook-Like Link Preview.
<p>
  Publish project on your IIS and use:
</p>

1 &bull; C#

```html
string method = "http://localhost/BLink/Preview";
string previewLink = "http://james-nicoll.livejournal.com/5605983.html";
using (WebClient wc = new WebClient())
{
  wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
  wc.Encoding = Encoding.UTF8;
  string preview = wc.UploadString(method, "POST", String.Format("={0}", previewLink));
}
```
