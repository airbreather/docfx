// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace Microsoft.Docs.Build
{
    internal class Error
    {
        public static readonly IEqualityComparer<Error> Comparer = new EqualityComparer();

        public ErrorLevel Level { get; }

        public string Code { get; }

        public string Message { get; }

        public string File { get; }

        public Range Range { get; }

        public int Line => Range.StartLine;

        public int Column => Range.StartCharacter;

        public Error(
            ErrorLevel level,
            string code,
            string message,
            string file = null,
            in Range range = default)
        {
            Debug.Assert(!string.IsNullOrEmpty(code));
            Debug.Assert(Regex.IsMatch(code, "^[a-z0-9-]{5,32}$"), "Error code should only contain dash and letters in lowercase");
            Debug.Assert(!string.IsNullOrEmpty(message));

            Level = level;
            Code = code;
            Message = message;
            File = file;
            Range = range;
        }

        public override string ToString() => ToString(Level);

        public string ToString(ErrorLevel level)
        {
            object[] payload = { level, Code, Message, File, Line, Column };

            var i = payload.Length - 1;
            while (i >= 0 && (Equals(payload[i], null) || Equals(payload[i], "") || Equals(payload[i], 0)))
            {
                i--;
            }
            return JsonUtility.Serialize(payload.Take(i + 1));
        }

        public DocfxException ToException(Exception innerException = null)
        {
            return new DocfxException(this, innerException);
        }

        private class EqualityComparer : IEqualityComparer<Error>
        {
            public bool Equals(Error x, Error y)
            {
                return x.Level == y.Level &&
                       x.Code == y.Code &&
                       x.Message == y.Message &&
                       x.File == y.File &&
                       x.Range.StartLine == y.Range.StartLine &&
                       x.Range.StartCharacter == y.Range.StartCharacter &&
                       x.Range.EndLine == y.Range.EndLine &&
                       x.Range.EndCharacter == y.Range.EndCharacter;
            }

            public int GetHashCode(Error obj)
            {
                return HashCode.Combine(
                    obj.Level,
                    obj.Code,
                    obj.Message,
                    obj.File,
                    obj.Range.StartLine,
                    obj.Range.StartCharacter,
                    obj.Range.EndLine,
                    obj.Range.EndCharacter);
            }
        }
    }
}