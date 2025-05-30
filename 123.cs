//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Runtime.InteropServices;
//using System.Windows.Forms;

//namespace TextEditor
//{
//    public class ParseError
//    {
//        public string Expected { get; set; }
//        public string Found { get; set; }
//        public int Line { get; set; }
//        public int Position { get; set; }
//        public string SkippedFragment { get; set; }

//        public ParseError(string expected, string found, int line, int pos, string skipped)
//        {
//            Expected = expected;
//            Found = found;
//            Line = line;
//            Position = pos;
//            SkippedFragment = skipped;
//        }
//    }

//    public class Parser
//    {
//        private enum State
//        {
//            q0, q1, q2, q3, q4, q5, q6, q7, q8, q9, q10, q11, q13, q14, q15, q16, End
//        }

//        private State currentState;
//        private int position;
//        private int line;
//        private string input;
//        private List<ParseError> errors;

//        public List<ParseError> Analyze(string text)
//        {
//            errors = new List<ParseError>();
//            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

//            if (lines.Length > 0 && string.IsNullOrWhiteSpace(lines[^1]))
//            {
//                lines = lines[..^1];
//            }
//            int flagforend = 0;

//            for (line = 0; line < lines.Length; line++)
//            {
//                input = lines[line];
//                position = 0;
//                currentState = State.q0;

//                while (position < input.Length)
//                {
//                    switch (currentState)
//                    {
//                        case State.q0:
//                            MatchSpace();
//                            if (MatchWordWithGarbage("create", "ключевое слово 'create'")) currentState = State.q1;
//                            else Neutralize("ключевое слово 'create'");
//                            break;
//                        case State.q1:
//                            if (MatchSpace()) currentState = State.q2;
//                            else Neutralize("пробел");
//                            break;
//                        case State.q2:
//                            if (MatchWordWithGarbage("type", "ключевое слово 'type'")) currentState = State.q3;
//                            else Neutralize("ключевое слово 'type'");
//                            break;
//                        case State.q3:
//                            if (MatchSpace()) currentState = State.q4;
//                            else Neutralize("пробел");
//                            break;
//                        case State.q4:
//                            if (MatchIdentifier()) currentState = State.q5;
//                            else Neutralize("идентификатор");
//                            break;
//                        case State.q5:
//                            if (MatchSpace()) currentState = State.q6;
//                            else Neutralize("пробел");
//                            break;
//                        case State.q6:
//                            if (MatchWordWithGarbage("as", "ключевое слово 'as'")) currentState = State.q7;
//                            else Neutralize("ключевое слово 'as'");
//                            break;
//                        case State.q7:
//                            if (MatchSpace()) currentState = State.q8;
//                            else Neutralize("пробел");
//                            break;
//                        case State.q8:
//                            if (MatchWordWithGarbage("enum", "ключевое слово 'enum'")) currentState = State.q9;
//                            else Neutralize("ключевое слово 'enum'");
//                            break;
//                        case State.q9:
//                            MatchSpace(); // дополнительные пробелы ДО скобки допустимы
//                            if (position < input.Length && input[position] == '(')
//                            {
//                                position++;
//                                currentState = State.q10;
//                            }
//                            else Neutralize("'('");
//                            break;
//                        case State.q10:
//                            if (position < input.Length && input[position] == ')')
//                            {
//                                position++;
//                                currentState = State.q15;
//                            }
//                            else if (position < input.Length && input[position] == '\'')
//                            {
//                                if (MatchStringLiteral())
//                                    currentState = State.q14;
//                                else
//                                    currentState = State.q14; // ошибка строки уже зафиксирована в MatchStringLiteral
//                            }
//                            else
//                            {
//                                int errorStart = position;
//                                string skipped = "";

//                                if (position < input.Length &&
//                                    (input[position] == ',' || input[position] == ';' || input[position] == ')'))
//                                {
//                                    // Добавим символ в ошибку
//                                    skipped += input[position];
//                                    position++;

//                                    errors.Add(new ParseError("строка либо ')'", skipped, line + 1, errorStart + 1, skipped));

//                                    // Продолжим разбор
//                                    if (input[position - 1] == ')')
//                                        currentState = State.q15;
//                                    else
//                                        currentState = State.q14; // После запятой снова ищем строку
//                                }
//                                else
//                                {
//                                    while (position < input.Length &&
//                                           input[position] != '\'' &&
//                                           input[position] != ')' &&
//                                           input[position] != ';' &&
//                                           input[position] != ',')
//                                    {
//                                        skipped += input[position];
//                                        position++;
//                                    }

//                                    if (!string.IsNullOrEmpty(skipped))
//                                    {
//                                        errors.Add(new ParseError("строка либо ')'", skipped, line + 1, errorStart + 1, skipped));
//                                    }

//                                    // И обязательно продвигаем позицию, чтобы не застрять
//                                    if (position < input.Length) position++;
//                                    currentState = State.q14;
//                                }
//                            }
//                            break;

//                        case State.q14:
//                            if (position < input.Length && input[position] == ',')
//                            {
//                                position++;
//                                if (position < input.Length && input[position] == '\'')
//                                {
//                                    if (MatchStringLiteral())
//                                        currentState = State.q14;
//                                    else
//                                        currentState = State.q14; // ошибка уже обработана
//                                }
//                                else
//                                {
//                                    int start = position;
//                                    string skipped = "";

//                                    while (position < input.Length && input[position] != '\'' && input[position] != ')' && input[position] != ';')
//                                    {
//                                        skipped += input[position];
//                                        position++;
//                                    }

//                                    if (!string.IsNullOrEmpty(skipped))
//                                    {
//                                        errors.Add(new ParseError("строка", "после запятой не строка", line + 1, start + 1, skipped));
//                                    }

//                                    // Пробуем снова найти строку или завершение
//                                    currentState = State.q10;
//                                }
//                            }
//                            else if (position < input.Length && input[position] == ')')
//                            {
//                                position++;
//                                currentState = State.q15;
//                            }
//                            else
//                            {
//                                Neutralize("',' или ')'");
//                            }
//                            break;
//                        case State.q15:
//                            MatchSpace();
//                            if (position < input.Length && input[position] == ';')
//                            {
//                                position++;
//                                currentState = State.End;
//                            }
//                            else if (position >= input.Length)
//                            {
//                                errors.Add(new ParseError("';'", "конец строки", line + 1, position + 1, ""));
//                                currentState = State.End; // Завершаем, чтобы не висло
//                            }
//                            else
//                            {
//                                Neutralize("';'");
//                            }
//                            break;

//                        case State.End:
//                            position = input.Length;
//                            break;
//                    }
//                }
//            }
//            if ((currentState == State.q10 || currentState == State.q14) && (input[input.Length - 1] != ';'))
//            {
//                errors.Add(new ParseError("')'", "конец строки", line , position + 1, ""));
//            }

//            if ((currentState != State.End && errors.Count == 0) || (input[input.Length-1] != ';'))
//            {
//                errors.Add(new ParseError("';'", "конец строки", line, position + 1, ""));
//            }
//            if (errors.Count == 0)
//            {
//                errors.Add(new ParseError("-", "Успешно", 0, 0, "Всё корректно"));
//            }

//            return errors;
//        }

//        private bool MatchWord(string word, string expectedDescription)
//        {
//            int start = position;
//            int len = word.Length;

//            if (position + len <= input.Length)
//            {
//                string fragment = input.Substring(position, len);
//                if (string.Equals(fragment, word, StringComparison.OrdinalIgnoreCase))
//                {
//                    position += len;
//                    return true;
//                }
//            }

//            return false;
//        }

//        private bool MatchSpace()
//        {
//            bool matched = false;
//            while (position < input.Length && input[position] == ' ')
//            {
//                matched = true;
//                position++;
//            }
//            return matched;
//        }

//        private bool MatchIdentifier()
//        {
//            int start = position;
//            if (position < input.Length && char.IsLetter(input[position]) && input[position] <= 127)
//            {
//                position++;
//                while (position < input.Length &&
//                       (char.IsLetterOrDigit(input[position]) || input[position] == '_') &&
//                       input[position] <= 127)
//                {
//                    position++;
//                }
//                return true;
//            }

//            if (position < input.Length)
//            {
//                int errorStart = position;
//                string skipped = "";

//                while (position < input.Length && !char.IsWhiteSpace(input[position]) &&
//                       input[position] != '(' && input[position] != ';')
//                {
//                    skipped += input[position];
//                    position++;
//                }

//                if (!string.IsNullOrEmpty(skipped))
//                {
//                    errors.Add(new ParseError("идентификатор (только латиница)", skipped[0].ToString(), line + 1, errorStart + 1, skipped));
//                }
//            }

//            return false;
//        }

//        private bool MatchStringLiteral()
//        {
//            int start = position;

//            if (position >= input.Length || input[position] != '\'')
//            {
//                int skipStart = position;
//                string skipped = "";

//                while (position < input.Length &&
//                       input[position] != '\'' &&
//                       input[position] != ')' &&
//                       input[position] != ';' &&
//                       input[position] != ',')
//                {
//                    skipped += input[position];
//                    position++;
//                }

//                if (!string.IsNullOrEmpty(skipped))
//                {
//                    errors.Add(new ParseError("строка", "нет открывающей кавычки", line + 1, skipStart + 1, skipped));
//                }

//                return false;
//            }

//            position++;
//            int contentStart = position;

//            while (position < input.Length && input[position] != '\'')
//            {
//                position++;
//            }

//            if (position < input.Length && input[position] == '\'')
//            {
//                position++;
//                return true;
//            }
//            else
//            {
//                string skipped = input.Substring(start);
//                errors.Add(new ParseError("строка", "не найдена закрывающая кавычка", line + 1, start + 1, skipped));
//                position = input.Length;
//                return false;
//            }
//        }

//        private void Neutralize(string expected)
//        {
//            int errorStart = position;
//            string skipped = "";

//            while (position < input.Length &&
//                   !char.IsWhiteSpace(input[position]) &&
//                   !char.IsLetterOrDigit(input[position]) &&
//                   input[position] != '\'' &&
//                   input[position] != '(' &&
//                   input[position] != ')' &&
//                   input[position] != ',' &&
//                   input[position] != ';')
//            {
//                skipped += input[position];
//                position++;
//            }

//            if (position < input.Length && skipped.Length == 0)
//            {
//                skipped = input[position].ToString();
//                position++;
//            }

//            if (!string.IsNullOrEmpty(skipped))
//            {
//                errors.Add(new ParseError(expected, skipped[0].ToString(), line + 1, errorStart + 1, skipped));
//            }
//        }
//        private bool MatchWordWithGarbage(string expectedWord, string expectedDescription)
//        {
//            int startPos = position;
//            int matchIndex = 0;
//            string skipforall = "";
//            string result = "";

//            List<ParseError> delayedErrors = new();

//            while (position < input.Length && matchIndex < expectedWord.Length)
//            {
//                char current = input[position];
//                skipforall += current;
//                if (current == '(' && expectedWord == "enum")
//                {
//                    if (!string.IsNullOrEmpty(skipforall))
//                    {
//                        result = skipforall[..^1];
//                    }
//                    errors.Add(new ParseError($"ключевое слово '{expectedWord}'", result, line + 1, startPos + 1, result));
//                    return true; 
//                }

//                // Если пробел — ключевое слово оборвано
//                if (current == ' ')
//                {
//                    if (!string.IsNullOrEmpty(skipforall))
//                    {
//                        result = skipforall[..^1];
//                    }

                    

//                    errors.Add(new ParseError($"ключевое слово '{expectedWord}'", result, line + 1, startPos + 1, result));
//                    return true;
//                }

//                if (char.ToLower(current) == expectedWord[matchIndex])
//                {
//                    matchIndex++;
//                }
//                else
//                {
//                    delayedErrors.Add(new ParseError(
//                        $"ключевое слово '{expectedWord}'",
//                        current.ToString(),
//                        line + 1,
//                        position + 1,
//                        current.ToString()
//                    ));
//                }

//                position++;
//            }

//            // Если совпало полностью — добавляем ошибки
//            if (matchIndex == expectedWord.Length)
//            {
//                if (delayedErrors.Count > 0)
//                    AddCombinedErrors(delayedErrors);
//                return true;
//            }

//            // Не совпало — объединяем ошибки
//            if (delayedErrors.Count > 0)
//                AddCombinedErrors(delayedErrors);

//            return false;
//        }
//        private void AddCombinedErrors(List<ParseError> delayed)
//        {
//            if (delayed.Count == 0) return;

//            delayed.Sort((a, b) => a.Position.CompareTo(b.Position));

//            var combined = new List<ParseError>();
//            var current = delayed[0];

//            for (int i = 1; i < delayed.Count; i++)
//            {
//                var next = delayed[i];
//                if (next.Position == current.Position + current.SkippedFragment.Length)
//                {
//                    current.Found += next.Found;
//                    current.SkippedFragment += next.SkippedFragment;
//                }
//                else
//                {
//                    combined.Add(current);
//                    current = next;
//                }
//            }
//            combined.Add(current);

//            errors.AddRange(combined);
//        }




//        public void HighlightErrors(RichTextBox richTextBox, List<ParseError> errors)
//        {
//            int originalSelectionStart = richTextBox.SelectionStart;
//            int originalSelectionLength = richTextBox.SelectionLength;

//            richTextBox.SelectAll();
//            richTextBox.SelectionBackColor = Color.White;

//            foreach (var error in errors)
//            {
//                if (error.Position > 0 && !string.IsNullOrEmpty(error.SkippedFragment))
//                {
//                    try
//                    {
//                        int lineIndex = error.Line - 1;
//                        if (lineIndex >= 0 && lineIndex < richTextBox.Lines.Length)
//                        {
//                            int charIndex = richTextBox.GetFirstCharIndexFromLine(lineIndex) + error.Position - 1;

//                            if (charIndex >= 0 && charIndex + error.SkippedFragment.Length <= richTextBox.Text.Length)
//                            {
//                                richTextBox.Select(charIndex, error.SkippedFragment.Length);
//                                richTextBox.SelectionBackColor = Color.LightPink;
//                            }
//                        }
//                    }
//                    catch { }
//                }
//            }

//            richTextBox.Select(originalSelectionStart, originalSelectionLength);
//        }
//    }
//}
