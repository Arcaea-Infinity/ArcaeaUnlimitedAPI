﻿using ArcaeaUnlimitedAPI.Beans;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static ArcaeaUnlimitedAPI.PublicApi.Response;

namespace ArcaeaUnlimitedAPI.PublicApi;

public partial class PublicApi
{
    [EnableCors]
    [SongInfoConverter]
    [HttpGet("/botarcapi/song/info")]
    public object GetSongInfo([BindNever] ArcaeaSong song) => Success(song.ToJson());
}
