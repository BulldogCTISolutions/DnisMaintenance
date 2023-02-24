//-----------------------------------------------------------------------
// <copyright file="Settings.cs" company="Bulldog CTI Solutions">
// Author: Jay McCormick
// Copyright (c) Bulldog CTI Solutions. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
namespace DnisMaintenance.Models;

/// <summary>
///  The DTO for the entries in 'appSettings.json'
/// </summary>
public sealed class Settings
{
    /// <summary>
    ///  The URL to the Azure Blob Storage 'container'.
    /// </summary>
    public string BlobContainerUrl { get; set; }

    /// <summary>
    ///  The query string part of the complete SAS URL.
    ///  Contains the permissions and timestamps.
    /// </summary>
    public string SASTokenUrl { get; set; }
}
