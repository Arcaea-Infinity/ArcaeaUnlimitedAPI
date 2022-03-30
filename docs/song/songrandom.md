## song/random

| arguments    | description                                | optional |
|:-------------|:-------------------------------------------|----------|
| start        | range of start (9+ => 9p , 10+ => 10p)     | true     |
| end          | range of end                               | true     |
| withsonginfo | boolean. if true, will reply with songinfo | true     |

#### ConvertFunction

```c#
var val = rating * 2 + (ratingPlus ? 1 : 0);
```

#### Example

+ `{apiurl}/botarcapi/song/random?start=19&end=22&withsonginfo=true`

###### Return data

```json
{
  "status": 0,
  "content": {
    "id": "gothiveofra",
    "ratingClass": 2,
    "songinfo": {
      "name_en": "Got hive of Ra",
      "name_jp": "",
      "artist": "E.G.G.",
      "bpm": "268",
      "bpm_base": 268.0,
      "set": "groovecoaster",
      "set_friendly": "Groove Coaster Collaboration",
      "time": 126,
      "side": 0,
      "world_unlock": false,
      "remote_download": true,
      "bg": "gc_light",
      "date": 1600214400,
      "version": "3.2",
      "difficulty": 19,
      "rating": 98,
      "note": 794,
      "chart_designer": "Groove Nitro",
      "jacket_designer": "",
      "jacket_override": false,
      "audio_override": false
    }
  }
}
```

