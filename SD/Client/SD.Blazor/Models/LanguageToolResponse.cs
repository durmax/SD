using System.Collections.Generic;

namespace SD.Client.Models
{
    public class LanguageToolResponse
    {
        public Software Software { get; set; }
        public Warnings Warnings { get; set; }
        public LanguageInfo Language { get; set; }
        public List<Match> Matches { get; set; }
        public List<int[]> SentenceRanges { get; set; }
    }

    public class Software
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public string BuildDate { get; set; }
        public int ApiVersion { get; set; }
        public bool Premium { get; set; }
        public string PremiumHint { get; set; }
        public string Status { get; set; }
    }

    public class Warnings
    {
        public bool IncompleteResults { get; set; }
    }

    public class LanguageInfo
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public DetectedLanguage DetectedLanguage { get; set; }
    }

    public class DetectedLanguage
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Confidence { get; set; }
        public string Source { get; set; }
    }

    public class Match
    {
        public string Message { get; set; }
        public string ShortMessage { get; set; }
        public List<Replacement> Replacements { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
        public Context Context { get; set; }
        public string Sentence { get; set; }
        public MatchType Type { get; set; }
        public Rule Rule { get; set; }
        public bool IgnoreForIncompleteSentence { get; set; }
        public int ContextForSureMatch { get; set; }
    }

    public class Replacement
    {
        public string Value { get; set; }
    }

    public class Context
    {
        public string Text { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }
    }

    public class MatchType
    {
        public string TypeName { get; set; }
    }

    public class Rule
    {
        public string Id { get; set; }
        public string Description { get; set; }
        public string IssueType { get; set; }
        public Category Category { get; set; }
        public bool IsPremium { get; set; }
    }

    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}