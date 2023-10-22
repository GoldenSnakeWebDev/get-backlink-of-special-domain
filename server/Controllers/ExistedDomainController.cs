using MySql.Data.MySqlClient;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Data;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using Microsoft.AspNetCore.Mvc;

// LinkCrawler.cs
using HtmlAgilityPack;
using System.Net;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace server.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ExistedDomainController : ControllerBase 
{
    
    [HttpPost]
    public async Task<IActionResult> PostAsync()
    {
        string connectionString = "server=localhost;userid=root;password=;database=backlink";

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        using MySqlCommand command = new MySqlCommand($"select domain from domain", connection);
        var reader =  command.ExecuteReader();

        List<string[]> result = new List<string[]>();

        while (reader.Read()) {
            var domain = new string[] {reader.GetString(0)};
            result.Add(domain);
        }

        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");

        await connection.CloseAsync();

        return Ok(result);
    }
}

[ApiController]
[Route("api/existedBacklink")]
public class ExistedBacklinkController : ControllerBase 
{
    
    [HttpPost]
    public async Task<IActionResult> PostAsync([FromBody] dynamic param)
    {

        JsonDocument  jsonDoc = JsonDocument.Parse(param.ToString());

        JsonElement domainElement = jsonDoc.RootElement.GetProperty("domain");
        
        string? domain = domainElement.ValueKind != JsonValueKind.Undefined ? domainElement.GetString():null;

        using var connection = new MySqlConnection("server=localhost;userid=root;password=;database=backlink");

        connection.Open();

        using MySqlCommand command = new MySqlCommand($"SELECT backlink, created_time FROM backlinks WHERE domain=@domain", connection);

        command.Parameters.AddWithValue("@domain", domain);

        var reader =  command.ExecuteReader();

        List<string[]> result_Backlink = new List<string[]>();

        List<string[]> result_time = new List<string[]>();

        while (reader.Read()) {
            var backlink = new string[] {reader.GetString(0)};
            var created_time = new string[] {reader.GetString(1)};

            // string[] temp = {backlink, created_time};

            result_Backlink.Add(backlink);
            result_time.Add(created_time);
        }

        Response.Headers.Add("Access-Control-Allow-Origin", "http://localhost:3000");

        return Ok(new {result_Backlink, result_time});
    }
}