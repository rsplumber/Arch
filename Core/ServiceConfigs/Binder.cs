﻿namespace Core.ServiceConfigs;

public class Binder
{
    public string Id { get; set; }

    public string ApiUrl { get; set; }

    public Dictionary<string, string> Infos { get; set; }
}