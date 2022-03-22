## song/random

| arguments    | description                                | optional |
|:-------------|:-------------------------------------------|----------|
| start        | range of start (9+ => 9p , 10+ => 10p)     | true     |
| end          | range of end                               | true     |
| withsonginfo | boolean. if true, will reply with songinfo | true     |

#### ConvertFunction

```c#
private static (int, int) ConvertToArcaeaRange(string rawdata) =>
    rawdata switch
    {
        "11"  => (110, 116),
        "10p" => (107, 109),
        "10"  => (100, 106),
        "9p"  => (97, 99),
        "9"   => (90, 96),
        "8"   => (80, 89),
        "7"   => (70, 79),
        "6"   => (60, 69),
        "5"   => (50, 59),
        "4"   => (40, 49),
        "3"   => (30, 39),
        "2"   => (20, 29),
        "1"   => (10, 19),
        _ => double.TryParse(rawdata, out var value)
            ? ((int)Math.Round(value * 10), (int)Math.Round(value * 10))
            : (-1, -1)
    };
```

#### Example

+ `{apiurl}/botarcapi/song/random?start=9p&end=10.2&withsonginfo=true`

###### Return data

```json
{
  "status": 0,
  "content": {
    "id": "scarletlance",
    "ratingClass": 2,
    "songinfo": {
      "id": "scarletlance",
      "title_localized": {
        "en": "Scarlet Lance"
      },
      "artist": "MASAKI (ZUNTATA)",
      "bpm": "185",
      "bpm_base": 185.0,
      "set": "groovecoaster",
      "set_friendly": "Groove Coaster Collaboration",
      "world_unlock": false,
      "remote_dl": true,
      "side": 0,
      "time": 129,
      "date": 1546992003,
      "version": "1.9",
      "difficulties": [
        {
          "ratingClass": 0,
          "chartDesigner": "闇運",
          "jacketDesigner": "",
          "jacketOverride": false,
          "realrating": 40,
          "totalNotes": 517
        },
        {
          "ratingClass": 1,
          "chartDesigner": "闇運",
          "jacketDesigner": "",
          "jacketOverride": false,
          "realrating": 70,
          "totalNotes": 644
        },
        {
          "ratingClass": 2,
          "chartDesigner": "闇運",
          "jacketDesigner": "",
          "jacketOverride": false,
          "realrating": 101,
          "totalNotes": 1130
        }
      ]
    }
  }
}
```

