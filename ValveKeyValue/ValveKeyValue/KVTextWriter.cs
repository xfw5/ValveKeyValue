﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValveKeyValue
{
    class KVTextWriter : IDisposable
    {
        public KVTextWriter(Stream stream, KVSerializerOptions options)
        {
            Require.NotNull(stream, nameof(stream));
            Require.NotNull(options, nameof(options));

            this.options = options;
            writer = new StreamWriter(stream, Encoding.UTF8, bufferSize: 1024, leaveOpen: true);
            writer.NewLine = "\n";
        }

        readonly KVSerializerOptions options;
        readonly TextWriter writer;
        int indentation = 0;

        public void WriteObject(KVObject data)
        {
            if (data.Value.ValueType == KVValueType.Collection)
            {
                WriteStartObject(data.Name);

                var children = data.Children;
                foreach (var item in children)
                {
                    WriteObject(item);
                }

                WriteEndObject();
            }
            else
            {
                WriteKeyValuePair(data.Name, data.Value);
            }
        }

        public void Dispose()
        {
            writer.Dispose();
        }

        void WriteStartObject(string name)
        {
            WriteIndentation();
            WriteText(name);
            WriteLine();
            WriteIndentation();
            writer.Write('{');
            indentation++;
            WriteLine();
        }

        void WriteEndObject()
        {
            indentation--;
            WriteIndentation();
            writer.Write('}');
            writer.WriteLine();
        }

        void WriteIndentation()
        {
            if (indentation == 0)
            {
                return;
            }

            var text = new string('\t', indentation);
            writer.Write(text);
        }

        void WriteKeyValuePair(string name, KVValue value)
        {
            WriteIndentation();
            WriteText(name);
            writer.Write('\t');
            WriteText((string)value);
            WriteLine();
        }

        void WriteText(string text)
        {
            writer.Write('"');

            foreach (var @char in text)
            {
                switch (@char)
                {
                    case '"':
                        writer.Write("\\\"");
                        break;

                    case '\\':
                        writer.Write(options.HasEscapeSequences ? "\\\\" : "\\");
                        break;

                    default:
                        writer.Write(@char);
                        break;
                }
            }

            writer.Write('"');
        }

        void WriteLine()
        {
            writer.WriteLine();
        }
    }
}
