﻿using LIM.ApplicationCore.Enums;

namespace LIM.WebApp.Models;

public class ModifyInstrumentRequest
{
    public int ManufacturerId { get; set; }
    public ProtocolType Protocol { get; set; }
    public string Model { get; set; } = string.Empty;
}