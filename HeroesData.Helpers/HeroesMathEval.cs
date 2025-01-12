﻿using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace HeroesData.Helpers
{
    public static class HeroesMathEval
    {
        private static readonly DataTable DataTable = new DataTable();

        public static double CalculatePathEquation(string input)
        {
            input = ParenthesisValidator(input);
            input = SpecificEquationModifier(input);
            input = GetInnerParenthesisValues(input);

            // finalize calculations
            input = CalculateInput(input);

            return double.Parse(input);
        }

        private static string GetInnerParenthesisValues(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\(([^()]+|(?<Level>\()|(?<-Level>\)))+(?(Level)(?!))\)", RegexOptions.IgnorePatternWhitespace);

            foreach (Match match in matches)
            {
                string matchInput = match.Value;

                if (matchInput.StartsWith("("))
                    matchInput = matchInput.Substring(1);
                if (matchInput.EndsWith(")"))
                    matchInput = matchInput.Remove(matchInput.Length - 1);

                matchInput = GetInnerParenthesisValues(matchInput);

                int position = input.IndexOf(match.Value);
                if (position >= 0)
                {
                    input = input.Substring(0, position) + CalculateInput(matchInput) + input.Substring(position + match.Length);
                }
            }

            return input;
        }

        private static string CalculateInput(string input)
        {
            input = RemoveAllSpaces(input);
            input = RemoveDoubleNegative(input);
            string[] parts = SplitExpression(input);

            while (parts.Length > 2)
            {
                string toBeComputed;
                if (parts[0] == "-") // first is negative number
                {
                    toBeComputed = parts[0] + parts[1] + parts[2];

                    if (parts[3] == "-") // second is negative number
                        toBeComputed += parts[3] + parts[4];
                    else
                        toBeComputed += parts[3];
                }
                else
                {
                    if (parts[2] == "-")
                        toBeComputed = parts[0] + parts[1] + parts[2] + parts[3];
                    else
                        toBeComputed = parts[0] + parts[1] + parts[2];
                }

                double value = double.Parse(DataTable.Compute(toBeComputed, string.Empty).ToString());

                int pos = input.IndexOf(toBeComputed);
                if (pos >= 0)
                {
                    input = input.Substring(0, pos) + value + input.Substring(pos + toBeComputed.Length);
                }

                parts = SplitExpression(input);
            }

            return input;
        }

        private static string[] SplitExpression(string input)
        {
            return Regex.Split(input, @"([+\-*/])").Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
        }

        private static string RemoveDoubleNegative(string input)
        {
            if (input.Length > 2 && input.StartsWith("--"))
                input = input.Remove(0, 2);

            return input.Replace("--", "+");
        }

        private static string RemoveAllSpaces(string input)
        {
            return Regex.Replace(input, @"\s+", string.Empty);
        }

        private static string ParenthesisValidator(string input)
        {
            int leftCount = 0;
            int rightCount = 0;

            foreach (char c in input)
            {
                if (c == '(')
                    leftCount++;
                else if (c == ')')
                    rightCount++;
            }

            if (leftCount == rightCount)
            {
                return input; // no change
            }

            // add parenthesis to left
            while (leftCount < rightCount)
            {
                input = $"({input}";
                leftCount++;
            }

            // add parenthesis to right
            while (leftCount > rightCount)
            {
                input = $"{input})";
                rightCount++;
            }

            return input;
        }

        private static string SpecificEquationModifier(string input)
        {
            // MedivhTemporalFlux - build 72481 - change negative to positive
            Match match = Regex.Match(input, @"\-\-\d+\s*\*\-\d+\s*", RegexOptions.IgnorePatternWhitespace);

            if (!string.IsNullOrEmpty(match.Value))
            {
                if (input.Length > 2 && input.StartsWith("--"))
                    return input.Remove(0, 1);
            }

            return input;
        }
    }
}
