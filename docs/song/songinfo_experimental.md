## test/song/info

| arguments  | description                                                                | optional                                        |
|:-----------|:---------------------------------------------------------------------------|-------------------------------------------------|
| songname   | any song name for fuzzy querying                                           | true when songid is not null, otherwise false   |
| songid     | sid in Arcaea songlist                                                     | true when songname is not null, otherwise false |

#### Note
+ This api will replace the original song/info api in 2022.5.

+ Please prepare in advance.

#### Example

+ `{apiurl}/botarcapi/test/song/info?songname=infinity`

###### Return data

```json
{
  "status": 0,
  "content": [
    {
      "name_en": "Red and Blue",
      "name_jp": "",
      "artist": "Silentroom",
      "bpm": "150",
      "bpm_base": 150.0,
      "set": "base",
      "set_friendly": "Arcaea",
      "time": 121,
      "side": 1,
      "world_unlock": true,
      "remote_download": false,
      "bg": "",
      "date": 1509667202,
      "version": "1.5",
      "difficulty": 8,
      "rating": 40,
      "note": 464,
      "chart_designer": "-chartaesthesia- RED side",
      "jacket_designer": "Khronetic",
      "jacket_override": false,
      "audio_override": false
    },
    {
      "name_en": "Red and Blue",
      "name_jp": "",
      "artist": "Silentroom",
      "bpm": "150",
      "bpm_base": 150.0,
      "set": "base",
      "set_friendly": "Arcaea",
      "time": 121,
      "side": 1,
      "world_unlock": true,
      "remote_download": false,
      "bg": "",
      "date": 1509667202,
      "version": "1.5",
      "difficulty": 14,
      "rating": 75,
      "note": 597,
      "chart_designer": "side BLUE -chartaesthesia-",
      "jacket_designer": "Khronetic",
      "jacket_override": false,
      "audio_override": false
    },
    {
      "name_en": "Red and Blue",
      "name_jp": "",
      "artist": "Silentroom",
      "bpm": "150",
      "bpm_base": 150.0,
      "set": "base",
      "set_friendly": "Arcaea",
      "time": 121,
      "side": 1,
      "world_unlock": true,
      "remote_download": false,
      "bg": "",
      "date": 1509667202,
      "version": "1.5",
      "difficulty": 18,
      "rating": 94,
      "note": 845,
      "chart_designer": "-chartaesthesia-\nLEFT=BLUE RED=RIGHT ",
      "jacket_designer": "Khronetic",
      "jacket_override": false,
      "audio_override": false
    },
    {
      "name_en": "Red and Blue and Green",
      "name_jp": "",
      "artist": "fn(ArcaeaSoundTeam)",
      "bpm": "160",
      "bpm_base": 160.0,
      "set": "base",
      "set_friendly": "Arcaea",
      "time": 133,
      "side": 1,
      "world_unlock": true,
      "remote_download": false,
      "bg": "byd_conflict",
      "date": 1646784001,
      "version": "3.12",
      "difficulty": 20,
      "rating": 100,
      "note": 1194,
      "chart_designer": "moonquay \"retrograde\"\nBLUE=LEFT RIGHT=RED",
      "jacket_designer": "yusi.",
      "jacket_override": true,
      "audio_override": true
    }
  ]
}
```

