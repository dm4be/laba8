using System;
using System.Collections.Generic;

namespace TextEditor
{
    public class Token
    {
        public int Code { get; set; }
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public string Position { get; set; }

        public Token(int code, string type, string lexeme, int line, int start, int end)
        {
            Code = code;
            Type = type;
            Lexeme = lexeme;
            Position = $"Строка {line}, символы {start}-{end}";
        }
    }

    public class Lexer
    {
        private readonly Dictionary<string, int> keywords = new Dictionary<string, int>()
        {
            { "create", 1 }, { "type", 2 }, { "as", 3 }, { "enum", 4 }
        };

        public List<Token> Analyze(string text)
        {
            var tokens = new List<Token>();
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                int i = 0;
                bool insideQuotes = false;
                bool lastWasKeywordOrId = false;

                while (i < line.Length)
                {
                    char c = line[i];

                    if (char.IsWhiteSpace(c))
                    {
                        if (!insideQuotes && !IsInsideEnumParentheses(line, i))
                        {
                            int spaceStart = i;
                            while (i < line.Length && char.IsWhiteSpace(line[i])) i++;

                            if (lastWasKeywordOrId && i < line.Length && (char.IsLetter(line[i]) || line[i] == '_'))
                            {
                                tokens.Add(new Token(8, "Разделитель", "пробел", lineIndex + 1, spaceStart + 1, spaceStart + 1));
                            }
                        }
                        else
                        {
                            i++;
                        }
                        continue;
                    }

                    if (!insideQuotes && IsCyrillic(c))
                    {
                        tokens.Add(new Token(-1, "Недопустимый символ", c.ToString(), lineIndex + 1, i + 1, i + 1));
                        i++;
                        continue;
                    }

                    if (char.IsLetter(c) || c == '_')
                    {
                        int start = i;
                        while (i < line.Length && (char.IsLetterOrDigit(line[i]) || line[i] == '_')) i++;
                        string lexeme = line.Substring(start, i - start);

                        if (!insideQuotes && ContainsCyrillic(lexeme))
                        {
                            tokens.Add(new Token(-1, "Недопустимый символ", lexeme, lineIndex + 1, start + 1, i));
                        }
                        else if (!char.IsLetter(lexeme[0]))
                        {
                            tokens.Add(new Token(-1, "Недопустимый символ", lexeme, lineIndex + 1, start + 1, i));
                        }
                        else if (keywords.ContainsKey(lexeme))
                        {
                            tokens.Add(new Token(keywords[lexeme], "Ключевое слово", lexeme, lineIndex + 1, start + 1, i));
                        }
                        else
                        {
                            tokens.Add(new Token(5, "Идентификатор", lexeme, lineIndex + 1, start + 1, i));
                        }

                        lastWasKeywordOrId = true;
                        continue;
                    }

                    lastWasKeywordOrId = false;

                    if (char.IsDigit(c))
                    {
                        int start = i;
                        while (i < line.Length && char.IsDigit(line[i])) i++;
                        string lexeme = line.Substring(start, i - start);

                        tokens.Add(new Token(-1, "Недопустимый символ", lexeme, lineIndex + 1, start + 1, i));
                        continue;
                    }

                    if (c == '\'')
                    {
                        int start = i;
                        i++;
                        insideQuotes = true;
                        while (i < line.Length && line[i] != '\'') i++;

                        if (i < line.Length && line[i] == '\'')
                        {
                            i++;
                            insideQuotes = false;
                            string lexeme = line.Substring(start, i - start);
                            tokens.Add(new Token(7, "Строка (константа)", lexeme, lineIndex + 1, start + 1, i));
                        }
                        else
                        {
                            string lexeme = line.Substring(start);
                            tokens.Add(new Token(-1, "Недопустимый символ", lexeme, lineIndex + 1, start + 1, line.Length));
                            break; // прекращаем обработку строки после ошибки
                        }
                        continue;
                    }

                    if (c == '(')
                    {
                        tokens.Add(new Token(8, "Открывающая скобка", "(", lineIndex + 1, i + 1, i + 1));
                        i++;
                        continue;
                    }

                    if (c == ')')
                    {
                        tokens.Add(new Token(9, "Закрывающая скобка", ")", lineIndex + 1, i + 1, i + 1));
                        i++;
                        continue;
                    }

                    if (c == ';')
                    {
                        tokens.Add(new Token(10, "Точка с запятой", ";", lineIndex + 1, i + 1, i + 1));
                        i++;
                        continue;
                    }

                    if (c == ',')
                    {
                        tokens.Add(new Token(11, "Запятая", ",", lineIndex + 1, i + 1, i + 1));
                        i++;
                        continue;
                    }

                    tokens.Add(new Token(-1, "Недопустимый символ", c.ToString(), lineIndex + 1, i + 1, i + 1));
                    i++;
                }
            }

            // Проверка на незакрывающую скобку
            int openParens = text.Split('(').Length - 1;
            int closeParens = text.Split(')').Length - 1;
            if (openParens > closeParens)
            {
                tokens.Add(new Token(-1, "Недопустимый символ", "Отсутствует закрывающая скобка ')'", lines.Length, 1, 1));
            }

            return tokens;
        }

        private bool IsCyrillic(char c)
        {
            return c >= '\u0400' && c <= '\u04FF';
        }

        private bool ContainsCyrillic(string text)
        {
            foreach (char c in text)
            {
                if (IsCyrillic(c)) return true;
            }
            return false;
        }

        private bool IsInsideEnumParentheses(string line, int index)
        {
            int open = line.IndexOf("(");
            int close = line.LastIndexOf(")");
            return open != -1 && close != -1 && index > open && index < close;
        }
        public void HighlightInvalidTokens(RichTextBox richTextBox, List<Token> tokens)
        {
            int originalStart = richTextBox.SelectionStart;
            int originalLength = richTextBox.SelectionLength;

            richTextBox.SelectAll();
            richTextBox.SelectionBackColor = Color.White;

            foreach (var token in tokens)
            {
                if (token.Code == -1 && !string.IsNullOrWhiteSpace(token.Lexeme))
                {
                    try
                    {
                        // Парсим позицию токена
                        var parts = token.Position.Split(',');
                        int lineNumber = int.Parse(parts[0].Split(' ')[1]) - 1;
                        string[] range = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        int start = int.Parse(range[1].Split('-')[0]) - 1;
                        int length = int.Parse(range[1].Split('-')[1]) - int.Parse(range[1].Split('-')[0]) + 1;

                        int absoluteIndex = richTextBox.GetFirstCharIndexFromLine(lineNumber) + start;
                        if (absoluteIndex >= 0 && absoluteIndex + length <= richTextBox.Text.Length)
                        {
                            richTextBox.Select(absoluteIndex, length);
                            richTextBox.SelectionBackColor = Color.LightPink;
                        }
                    }
                    catch
                    {
                        // Игнорируем ошибки выхода за границы
                    }
                }
            }

            // Восстановление позиции курсора
            richTextBox.Select(originalStart, originalLength);
        }
    }
}
