// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage( "Usage", "CA2234:Pass system uri objects instead of strings", Justification = "Using a Uri doesn't work without protocol", Scope = "module" )]
[assembly: SuppressMessage( "Design", "CA1036:Override methods on comparable types", Justification = "For Region and GenesysKey this is too muchg work", Scope = "module" )]
[assembly: SuppressMessage( "Design", "CA1056:URI properties should not be strings", Justification = "Using a Uri doesn't work without protocol", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP04:A misspelled word has been found", Justification = "DNIS is correct", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP05:A misspelled word has been found", Justification = "DNIS is correct", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP06:A misspelled word has been found", Justification = "Dnis is okay", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "Dnis is okay", Scope = "namespace", Target = "~N:DnisMaintenance" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP08:A misspelled word has been found", Justification = "AzureBlobService", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP11:A misspelled word has been found", Justification = "Dnis is okay", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP12:A misspelled word has been found", Justification = "DNIS is correct", Scope = "module" )]
[assembly: SuppressMessage( "Spellchecker", "CRRSP13:A misspelled word has been found", Justification = "DNIS is correct", Scope = "module" )]
