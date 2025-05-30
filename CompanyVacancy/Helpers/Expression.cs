using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.Design;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Text.RegularExpressions;


namespace CompanyVacancy
{
    public static class SearchQueryParser
    {
        public static string ParseToDynamicLinq(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return "no results found"; 

            // Define known job roles and company names
            string[] knownJobRoles = {
                "Developer", "Analyst", "Tester", "Researcher", "Designer",
                "Coordinator", "Architect", "HR", "Engineer", "Manager", "Consultant","technician","support","mechanical engineer","devops engineer",
                "embedded engineer","automation","electrical engineer","data analyst",
            };

            string[] knownCompanyNames = {
                "Amazon", "Google", "wipro","ibm","oracle","samsung","infosys","intel","siemens","Tesla"
            };

            string[] separators = { "and", "which", "wherever", "those are","those","where","about"};
            string pattern = @"\b(" + string.Join("|", separators.Select(Regex.Escape)) + @")\b";

            var parts = Regex.Split(input, pattern, RegexOptions.IgnoreCase)
                             .Where(p => !separators.Contains(p.Trim(), StringComparer.OrdinalIgnoreCase))
                             .Select(p => p.Trim())
                             .Where(p => !string.IsNullOrWhiteSpace(p))
                             .ToList();

            List<string> conditions = new List<string>();

            foreach (var part in parts)
            {
                // Directly match company names in free-text parts
                var matchedCompany = knownCompanyNames
                    .FirstOrDefault(name => part.Contains(name, StringComparison.OrdinalIgnoreCase));
                if (matchedCompany != null)
                {
                    // Updated to use Name property instead of CompanyName
                    conditions.Add($"Name.Contains(\"{matchedCompany}\")");
                    continue;
                }

                var condition = ParseConditionPart(part);
                if (!string.IsNullOrEmpty(condition))
                {
                    conditions.Add(condition);
                }
            }

            // Match job roles and company names from individual words
            var remainingWords = Regex.Split(input, @"[\s,\.]+")
                                      .Where(w => !string.IsNullOrWhiteSpace(w))
                                      .Select(w => w.Trim())
                                      .Distinct(StringComparer.OrdinalIgnoreCase)
                                      .ToList();

            List<string> jobRoleMatches = new List<string>();
            List<string> companyNameMatches = new List<string>();

            foreach (var word in remainingWords)
            {
                if (knownJobRoles.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    jobRoleMatches.Add($"Job_roles.Contains(\"{word}\")");
                }

                if (knownCompanyNames.Contains(word, StringComparer.OrdinalIgnoreCase))
                {
                    // Updated to use Name property
                    companyNameMatches.Add($"Name.Contains(\"{word}\")");
                }
            }

            if (jobRoleMatches.Count > 0)
            {
                conditions.Add("(" + string.Join(" OR ", jobRoleMatches) + ")");
            }

            if (companyNameMatches.Count > 0)
            {
                conditions.Add("(" + string.Join(" OR ", companyNameMatches) + ")");
            }

            return string.Join(" AND ", conditions);
        }

        static string ParseConditionPart(string part)
        {
            string[] words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            HashSet<string> countryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
            {
                try
                {
                    RegionInfo region = new RegionInfo(culture.Name);
                    countryNames.Add(region.EnglishName);
                }
                catch { }
            }

            string[] startWords = {
                "Vacancies", "Job_roles", "Need_Factor", "Location", "Vacancy_Factor",
                "Name", "name", "job role", "located", "company", "companyname", "company name", "organization"
            };

            string[] endwords = {
                "Developer", "Analyst", "Tester", "HR", "Researcher", "Designer", "Coordinator", "Architect",
                "amazon", "Amazon"
            };

            int startIndex = -1;
            for (int i = 0; i < words.Length; i++)
            {
                if (startWords.Contains(words[i], StringComparer.OrdinalIgnoreCase))
                {
                    startIndex = i;
                    break;
                }
            }

            int endIndex = -1;
            for (int i = startIndex + 1; i < words.Length; i++)
            {
                if (int.TryParse(words[i], out _) ||
                    countryNames.Contains(words[i], StringComparer.OrdinalIgnoreCase) ||
                    new[] { "yearly", "half-yearly", "weekly", "monthly" }
                        .Contains(words[i], StringComparer.OrdinalIgnoreCase) ||
                    endwords.Contains(words[i], StringComparer.OrdinalIgnoreCase))
                {
                    endIndex = i;
                    break;
                }
            }

            if (startIndex != -1)
            {
                int effectiveEndIndex = (endIndex != -1 && endIndex >= startIndex) ? endIndex : words.Length - 1;
                string[] trimmedWords = words.Skip(startIndex).Take(effectiveEndIndex - startIndex + 1).ToArray();
                var dict = new Dictionary<string, List<string>>();

                if (trimmedWords.Length >= 4)
                {
                    string key = trimmedWords[0];
                    string combined1 = $"{trimmedWords[1]} {trimmedWords[2]}";
                    string combined2 = trimmedWords[3];
                    dict[key] = new List<string> { combined1, combined2 };
                }
                else
                {
                    string key = trimmedWords[0];
                    List<string> values = trimmedWords.Skip(1).ToList();
                    dict[key] = values;
                }

                // Alias mapping updated to map all company aliases to "Name"
                var fieldAliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["company"] = "Name",
                    ["companies"]="Name",
                    ["companyname"] = "Name",
                    ["company names"] = "Name",
                    ["company name"] = "Name",
                    ["organization"] = "Name",
                    ["name"] = "Name",
                    ["located"] = "Location",
                    ["Vacancy Factor"]="Vacancy_Factor",
                    ["vacancy factor"]= "Vacancy_Factor",
                    ["Vacancy factor"]= "Vacancy_Factor",
                    ["vacancy Factor"]= "Vacancy_Factor",
                    ["Need Factor"] = "Need_Factor",
                    ["need factor"] = "Need_Factor",
                    ["Need factor"] = "Need_Factor",
                    ["need Factor"] = "Need_Factor",
                    ["job role"] = "Job_roles",
                    ["roles"]="Job_roles",
                    ["Roles"]= "Job_roles",
                    ["Role"]= "Job_roles",
                    ["locate"]="Location",
                    ["role"] = "Job_roles",
                };

                var updatedDict = new Dictionary<string, List<string>>();
                foreach (var kvp in dict)
                {
                    var key = fieldAliasMap.ContainsKey(kvp.Key) ? fieldAliasMap[kvp.Key] : kvp.Key;
                    updatedDict[key] = kvp.Value;
                }

                return ConvertToCondition(updatedDict);
            }

            return string.Empty;
        }

        static string ConvertToCondition(Dictionary<string, List<string>> dict)
        {
            foreach (var kvp in dict)
            {
                string key = kvp.Key;
                List<string> values = kvp.Value;

                if (values.Count == 2)
                {
                    string operation = values[0].ToLower();
                    string operand = values[1];

                    bool IsNumeric(string val) => double.TryParse(val, out _);
                    bool IsBoolean(string val) => val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase);
                    string FormatValue(string val) => IsNumeric(val) || IsBoolean(val) ? val : $"\"{val}\"";

                    return operation switch
                    {
                        "present in" => $"{key} == {FormatValue(operand)}",
                        "equals" => $"{key} == {FormatValue(operand)}",
                        "equal to" => $"{key} == {FormatValue(operand)}",
                        "more than" => $"{key} > {FormatValue(operand)}",
                        "less than" => $"{key} < {FormatValue(operand)}",
                        "except" => $"{key} != {FormatValue(operand)}",
                        "not present in" => $"{key} != {FormatValue(operand)}",
                        "like" => $"{key}.Contains({FormatValue(operand)})",
                        "with" => $"{key} == ({FormatValue(operand)})",
                        "is" => $"{key}.Contains({FormatValue(operand)})",
                        "likely" => $"{key}.Contains({FormatValue(operand)})",
                        "exactly" => $"{key} == {FormatValue(operand)}",
                        "exact" => $"{key} == {FormatValue(operand)}",
                        "greater than" => $"{key} > {FormatValue(operand)}",
                        "above" => $"{key} > {FormatValue(operand)}",
                        "higher than" => $"{key} > {FormatValue(operand)}",
                        "beyond" => $"{key} > {FormatValue(operand)}",
                        "outside" => $"{key} > {FormatValue(operand)}",
                        "on top of" => $"{key} > {FormatValue(operand)}",
                        "lower than" => $"{key} < {FormatValue(operand)}",
                        "under" => $"{key} < {FormatValue(operand)}",
                        "smaller than" => $"{key} < {FormatValue(operand)}",
                        "in" => $"{key}.Contains({FormatValue(operand)})",
                        _ => $"{key} == {FormatValue(operand)}"
                    };
                }
                else if (values.Count == 1)
                {
                    return $"{key}.Contains(\"{values[0]}\")";
                }
            }
            return string.Empty;
        }
    }
}
// Split by "and"


// Split query into parts based on keywords

// Define known job roles

//            if (string.IsNullOrWhiteSpace(input))
//                return string.Empty;

//            // Define known job roles and company names
//            string[] knownJobRoles = {
//                "Developer", "Analyst", "Tester", "Researcher", "Designer",
//                "Coordinator", "Architect", "HR", "Engineer", "Manager", "Consultant"
//            };

//            string[] knownCompanyNames = {
//                "Amazon", "Google", "Microsoft", "Apple", "Facebook",
//                "Meta", "Netflix", "Tesla"
//            };

//            string[] separators = { "and", "which", "wherever", "those are", "with" };
//            string pattern = @"\b(" + string.Join("|", separators.Select(Regex.Escape)) + @")\b";

//            var parts = Regex.Split(input, pattern, RegexOptions.IgnoreCase)
//                             .Where(p => !separators.Contains(p.Trim(), StringComparer.OrdinalIgnoreCase))
//                             .Select(p => p.Trim())
//                             .Where(p => !string.IsNullOrWhiteSpace(p))
//                             .ToList();

//            List<string> conditions = new List<string>();

//            foreach (var part in parts)
//            {
//                // Directly match company names in free-text parts
//                var matchedCompany = knownCompanyNames
//                    .FirstOrDefault(name => part.Contains(name, StringComparison.OrdinalIgnoreCase));
//                if (matchedCompany != null)
//                {
//                    conditions.Add($"Name.Contains(\"{matchedCompany}\")");
//                    continue;
//                }

//                var condition = ParseConditionPart(part);
//                if (!string.IsNullOrEmpty(condition))
//                {
//                    conditions.Add(condition);
//                }
//            }

//            // Match job roles and company names from individual words
//            var remainingWords = Regex.Split(input, @"[\s,\.]+")
//                                      .Where(w => !string.IsNullOrWhiteSpace(w))
//                                      .Select(w => w.Trim())
//                                      .Distinct(StringComparer.OrdinalIgnoreCase)
//                                      .ToList();

//            List<string> jobRoleMatches = new List<string>();
//            List<string> companyNameMatches = new List<string>();

//            foreach (var word in remainingWords)
//            {
//                if (knownJobRoles.Contains(word, StringComparer.OrdinalIgnoreCase))
//                {
//                    jobRoleMatches.Add($"Job_roles.Contains(\"{word}\")");
//                }

//                if (knownCompanyNames.Contains(word, StringComparer.OrdinalIgnoreCase))
//                {
//                    companyNameMatches.Add($"Name.Contains(\"{word}\")");
//                }
//            }

//            if (jobRoleMatches.Count > 0)
//            {
//                conditions.Add("(" + string.Join(" OR ", jobRoleMatches) + ")");
//            }

//            if (companyNameMatches.Count > 0)
//            {
//                conditions.Add("(" + string.Join(" OR ", companyNameMatches) + ")");
//            }

//            return string.Join(" AND ", conditions);
//        }

//        static string ParseConditionPart(string part)
//        {
//            string[] words = part.Split(' ', StringSplitOptions.RemoveEmptyEntries);

//            HashSet<string> countryNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
//            foreach (var culture in CultureInfo.GetCultures(CultureTypes.SpecificCultures))
//            {
//                try
//                {
//                    RegionInfo region = new RegionInfo(culture.Name);
//                    countryNames.Add(region.EnglishName);
//                }
//                catch { }
//            }

//            string[] startWords = {
//                "Vacancies", "Job_roles", "Need_Factor", "Location", "Vacancy_Factor",
//                "Name", "name", "job role", "located", "company", "companyname", "company name", "organization"
//            };

//            string[] endwords = {
//                "Developer", "Analyst", "Tester", "HR", "Researcher", "Designer", "Coordinator", "Architect",
//                "amazon", "Amazon"
//            };

//            int startIndex = -1;
//            for (int i = 0; i < words.Length; i++)
//            {
//                if (startWords.Contains(words[i], StringComparer.OrdinalIgnoreCase))
//                {
//                    startIndex = i;
//                    break;
//                }
//            }

//            int endIndex = -1;
//            for (int i = startIndex + 1; i < words.Length; i++)
//            {
//                if (int.TryParse(words[i], out _) ||
//                    countryNames.Contains(words[i], StringComparer.OrdinalIgnoreCase) ||
//                    new[] { "yearly", "half-yearly", "weekly", "monthly" }
//                        .Contains(words[i], StringComparer.OrdinalIgnoreCase) ||
//                    endwords.Contains(words[i], StringComparer.OrdinalIgnoreCase))
//                {
//                    endIndex = i;
//                    break;
//                }
//            }

//            if (startIndex != -1)
//            {
//                int effectiveEndIndex = (endIndex != -1 && endIndex >= startIndex) ? endIndex : words.Length - 1;
//                string[] trimmedWords = words.Skip(startIndex).Take(effectiveEndIndex - startIndex + 1).ToArray();
//                var dict = new Dictionary<string, List<string>>();

//                if (trimmedWords.Length >= 4)
//                {
//                    string key = trimmedWords[0];
//                    string combined1 = $"{trimmedWords[1]} {trimmedWords[2]}";
//                    string combined2 = trimmedWords[3];
//                    dict[key] = new List<string> { combined1, combined2 };
//                }
//                else
//                {
//                    string key = trimmedWords[0];
//                    List<string> values = trimmedWords.Skip(1).ToList();
//                    dict[key] = values;
//                }

//                // Alias mapping
//                var fieldAliasMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
//                {
//                    ["company"] = "Name",
//                    ["companyname"] = "Name",
//                    ["company name"] = "Name",
//                    ["organization"] = "Name",
//                    ["name"] = "Name",
//                    ["located"] = "Location",
//                    ["job role"] = "Job_roles"
//                };

//                var updatedDict = new Dictionary<string, List<string>>();
//                foreach (var kvp in dict)
//                {
//                    var key = fieldAliasMap.ContainsKey(kvp.Key) ? fieldAliasMap[kvp.Key] : kvp.Key;
//                    updatedDict[key] = kvp.Value;
//                }

//                return ConvertToCondition(updatedDict);
//            }

//            return string.Empty;
//        }

//        static string ConvertToCondition(Dictionary<string, List<string>> dict)
//        {
//            foreach (var kvp in dict)
//            {
//                string key = kvp.Key;
//                List<string> values = kvp.Value;

//                if (values.Count == 2)
//                {
//                    string operation = values[0].ToLower();
//                    string operand = values[1];

//                    bool IsNumeric(string val) => double.TryParse(val, out _);
//                    bool IsBoolean(string val) => val.Equals("true", StringComparison.OrdinalIgnoreCase) || val.Equals("false", StringComparison.OrdinalIgnoreCase);
//                    string FormatValue(string val) => IsNumeric(val) || IsBoolean(val) ? val : $"\"{val}\"";

//                    return operation switch
//                    {
//                        "present in" => $"{key} == {FormatValue(operand)}",
//                        "equals" => $"{key} == {FormatValue(operand)}",
//                        "equal to" => $"{key} == {FormatValue(operand)}",
//                        "is equal to" => $"{key} == {FormatValue(operand)}",
//                        "more than" => $"{key} > {FormatValue(operand)}",
//                        "less than" => $"{key} < {FormatValue(operand)}",
//                        "except" => $"{key} != {FormatValue(operand)}",
//                        "not present in" => $"{key} != {FormatValue(operand)}",
//                        "like" => $"{key}.Contains({FormatValue(operand)})",
//                        "with" => $"{key}.Contains({FormatValue(operand)})",
//                        "is" => $"{key}.Contains({FormatValue(operand)})",
//                        "likely" => $"{key}.Contains({FormatValue(operand)})",
//                        "exactly" => $"{key} == {FormatValue(operand)}",
//                        "exact" => $"{key} == {FormatValue(operand)}",
//                        "graeter than" => $"{key} > {FormatValue(operand)}",
//                        "greater than" => $"{key} > {FormatValue(operand)}",
//                        "above" => $"{key} > {FormatValue(operand)}",
//                        "higher than" => $"{key} > {FormatValue(operand)}",
//                        "beyond" => $"{key} > {FormatValue(operand)}",
//                        "outside" => $"{key} > {FormatValue(operand)}",
//                        "on top of" => $"{key} > {FormatValue(operand)}",
//                        "lower than" => $"{key} < {FormatValue(operand)}",
//                        "under" => $"{key} < {FormatValue(operand)}",
//                        "smaller than" => $"{key} < {FormatValue(operand)}",
//                        "in" => $"{key}.Contains({FormatValue(operand)})",
//                        _ => $"{key} == {FormatValue(operand)}"
//                    };
//                }
//                else if (values.Count == 1)
//                {
//                    return $"{key}.Contains(\"{values[0]}\")";
//                }
//            }
//            return string.Empty;
//        }
//    }
//}


