# ArcaeaUnlimitedAPI

(or BotArcAPI CSharp implementation)

A fast and easy Arcaea API for your bot.

For the deploy tutorial of AUA, please refer to the sub-branch branch.

### Note

> Accessing some api requires a specific User-Agent

### Nuget Packages Dependencies

| PackageName                             | Version |
|:----------------------------------------|:--------|
| Microsoft.AspNetCore.Mvc.NewtonsoftJson | 6.0.1   |
| sqlite-net-pcl                          | 1.7.335 |
| Newtonsoft.Json                         | 13.0.1  |

### Other Sortware Dependence

+ ariac2

### API Entry

+ [user/info](/docs/user/userinfo.md)
+ [user/best](/docs/user/userbest.md)
+ [user/best30](/docs/user/userbest30.md)
+ [song/info](/docs/song/songinfo.md)
+ [song/alias](/docs/song/songalias.md)
+ [song/random](/docs/song/songrandom.md)
+ [assets/icon](/docs/assets/iconassets.md)
+ [assets/char](/docs/assets/charassets.md)
+ [assets/song](/docs/assets/songassets.md)
+ [update](/docs/others/update.md)

### Error status

| status | description                                               |
|:-------|:----------------------------------------------------------|
| -1     | invalid username or usercode                              |  
| -2     | invalid usercode                                          |  
| -3     | user not found                                            |  
| -4     | too many users                                            |  
| -5     | invalid songname or songid                                |  
| -6     | invalid songid                                            |  
| -7     | song not recorded                                         |  
| -8     | too many records                                          |  
| -9     | invalid difficulty                                        |  
| -10    | invalid recent/overflow number                            |  
| -11    | allocate an arc account failed                            |  
| -12    | clear friend failed                                       |  
| -13    | add friend failed                                         |  
| -14    | this song has no beyond level                             |  
| -15    | not played yet                                            |  
| -16    | user got shadowbanned                                     |
| -17    | querying best30 failed                                    |  
| -18    | update service unavailable                                |  
| -19    | invalid partner                                           |  
| -20    | file unavailable                                          |  
| -21    | invalid range                                             | 
| -22    | range of rating end smaller than its start                |
| -23    | potential is below the threshold of querying best30 (7.0) |  
| -24    | need to update arcaea, please contact maintainer          |  
| -233   | internal error occurred                                   |  

### Abnormal Example

+ `{apiurl}/botarcapi/song/alias?songid=gl`

+ ##### Return data

```json
{
    "status": -5,
    "message": "invalid songname or songid"
}
```

specially,

+ `{apiurl}/botarcapi/song/info?songname=lc`

+ ##### Return data

```json
{
  "status": -8,
  "message": "too many records",
  "content": {
    "songs": [
      "lostcivilization",
      "lastcelebration"
    ]
  }
}
```

### License

Licensed under `616 SB License`.

