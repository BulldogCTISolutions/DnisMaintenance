//-----------------------------------------------------------------------
// <copyright file="LoggingService.cs" company="Bulldog CTI Solutions">
// Author: Jay McCormick
// Copyright (c) Bulldog CTI Solutions. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace DnisMaintenance.Services;

/// <summary>
///  Based on CA1848:
///  https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1848
///  Using <see cref="LoggerMessage"/> provides performance advantages over <see cref="Logger"/> extension methods.
///  This is a partial wrapper class for using LoggerMessage.
/// </summary>
public static partial class LoggingService
{
    [LoggerMessage( EventId = 100, Level = LogLevel.Debug, EventName = "DEBUG", Message = "{message}" )]
    public static partial void LogDebug( ILogger logger, string message );

    [LoggerMessage( EventId = 200, Level = LogLevel.Trace, EventName = "TRACE", Message = "{message}" )]
    public static partial void LogTrace( ILogger logger, string message );

    [LoggerMessage( EventId = 300, Level = LogLevel.Information, EventName = "INFORMATIONAL", Message = "{message}" )]
    public static partial void LogInfo( ILogger logger, string message );

    [LoggerMessage( EventId = 400, Level = LogLevel.Warning, EventName = "WARNING", Message = "{message}" )]
    public static partial void LogWarning( ILogger logger, string message );

    [LoggerMessage( EventId = 500, Level = LogLevel.Error, EventName = "ERROR", Message = "{message}" )]
    public static partial void LogError( ILogger logger, string message, Exception ex );

    [LoggerMessage( EventId = 600, Level = LogLevel.Critical, EventName = "CRITICAL", Message = "{message}" )]
    public static partial void LogCritical( ILogger logger, string message, Exception ex );
}
