namespace CRM.Models.BotServices;

using System.Collections.Generic;
using Enum;

public class BotEndpointSettings
{
    public string ServiceUrl { get; set; }
    public string AuthenticatePath { get; set; }
    public string AppId { get; set; }
    public string Token { get; set; }
    public List<ApiConfig> ApiConfigs { get; set; }
}

public class ApiConfig
{
    public BotChannel Channel { get; set; }
    public string SendMultipleMessagePath { get; set; }
    public string SendSingleMessagePath { get; set; }
    public string SendMultipleAnnouncementPath { get; set; }
    public string SendOnlyImagesMultiplePath { get; set; }
}