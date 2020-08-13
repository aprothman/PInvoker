using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicPInvoke
{
    public class NameTransforms
    {
        /// <summary>
        /// Returns the input string unchanged
        /// </summary>
        public static Func<string, string> NoOp => new Func<string, string>(m => m);

        /// <summary>
        /// Returns a name with the form: funcCall
        /// </summary>
        public static Func<string, string> Camel => new Func<string, string>(CamelImpl);

        /// <summary>
        /// Returns a name with the form: func_call
        /// </summary>
        public static Func<string, string> Snake => new Func<string, string>(SnakeImpl);

        /// <summary>
        /// Returns a name with the form FuncCall
        /// </summary>
        public static Func<string, string> Pascal => new Func<string, string>(PascalImpl);

        /// <summary>
        /// Returns a name with the form FUNC_CALL
        /// </summary>
        public static Func<string, string> CapSnake => new Func<string, string>(CapSnakeImpl);

        private static string CamelImpl(string name)
        {
            var tokens = Tokenize(name);
            var result = new StringBuilder();

            result.Append(tokens.Dequeue().ToLower());
            while (tokens.Count > 0) {
                result.Append(CapCase(tokens.Dequeue()));
            }

            return result.ToString();
        }

        private static string SnakeImpl(string name)
        {
            var tokens = Tokenize(name);
            var result = new StringBuilder();

            result.Append(tokens.Dequeue().ToLower());
            while (tokens.Count > 0) {
                result.Append("_");
                result.Append(tokens.Dequeue().ToLower());
            }

            return result.ToString();
        }

        private static string PascalImpl(string name)
        {
            var tokens = Tokenize(name);
            var result = new StringBuilder();

            while (tokens.Count > 0) {
                result.Append(CapCase(tokens.Dequeue()));
            }

            return result.ToString();
        }

        private static string CapSnakeImpl(string name)
        {
            var tokens = Tokenize(name);
            var result = new StringBuilder();

            result.Append(tokens.Dequeue().ToUpper());
            while (tokens.Count > 0) {
                result.Append("_");
                result.Append(tokens.Dequeue().ToUpper());
            }

            return result.ToString();
        }

        private static Queue<string> Tokenize(string name)
        {
            var workingName = name;

            // attempt to automatically tokenize it from snake case
            char[] separator = { '_' };
            var result = new Queue<string>(workingName.Split(separator, StringSplitOptions.RemoveEmptyEntries));
            if (result.Count > 1) {
                return result;
            } else if (result.Count == 1) {
                // we've incidentally stripped the name of leading and trailing underscores,
                // so use the trimmed version while also clearing the queue
                workingName = result.Dequeue();
            }

            // manually tokenize based on capitalization
            var charArray = workingName.ToCharArray();
            for (int i = 0, tokenStart = 0; i < charArray.Length; ++i) {
                if (charArray.Length == i + 1 || char.IsUpper(charArray[i + 1])) {
                    result.Enqueue(new string(charArray, tokenStart, i - tokenStart + 1));
                    tokenStart = i + 1;
                }
            }

            return result;
        }

        public static string CapCase(string s)
        {
            if (string.IsNullOrEmpty(s)) {
                return string.Empty;
            }

            return char.ToUpper(s[0]) + s.Substring(1);
        }
    }
}
