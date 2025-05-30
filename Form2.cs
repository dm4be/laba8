using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Diagnostics;
using static System.Windows.Forms.Timer;
using System.Reflection;

namespace TextEditor
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip;
        private ToolStrip toolStrip;
        private SplitContainer splitContainer;
        private RichTextBox inputArea;
        private FileManager fileManager;
        private EditManager editManager;
        private HelpManager helpManager;
        private TabPage syntaxTab;
        private RichTextBox syntaxOutput;

        // 🔽 Новые поля
        private TabControl outputTabs;
        private DataGridView lexerGrid;
        private DataGridView parserGrid;
        private System.Windows.Forms.Timer debounceTimer;
        private const int DebounceInterval = 300; // миллисекунд

        public MainForm()
        {
            InitializeComponent();
            fileManager = new FileManager();
            editManager = new EditManager();
            helpManager = new HelpManager();
            this.FormClosing += MainForm_FormClosing;
            //inputArea.TextChanged += inputArea_TextChanged;
            //debounceTimer = new System.Windows.Forms.Timer();
            //debounceTimer.Interval = DebounceInterval;
            //debounceTimer.Tick += DebounceTimer_Tick;

            
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!fileManager.CheckUnsavedChanges(inputArea))
            {
                e.Cancel = true;
            }
        }

        private string GetLineNumbers(RichTextBox textBox)
        {
            int lineCount = textBox.Lines.Length;
            string lineNumbers = "";
            for (int i = 1; i <= lineCount; i++)
            {
                lineNumbers += i + "\n";
            }
            return lineNumbers;
        }

        private void InitializeComponent()
        {
            var iconStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("TextEditor.Resources.icon.ico");
            this.Icon = new Icon(iconStream);
            this.Text = "Compiler";
            this.Width = 800;
            this.Height = 600;

            menuStrip = new MenuStrip();
            menuStrip.Dock = DockStyle.Top;
            menuStrip.Items.AddRange(new ToolStripItem[] {
                new ToolStripMenuItem("Файл", null,
                    new ToolStripMenuItem("Создать", null, (s, e) => fileManager.NewFile(inputArea)),
                    new ToolStripMenuItem("Открыть", null, (s, e) => fileManager.OpenFile(inputArea)),
                    new ToolStripMenuItem("Сохранить", null, (s, e) => fileManager.SaveFile(inputArea)),
                    new ToolStripMenuItem("Сохранить как", null, (s, e) => fileManager.SaveFileAs(inputArea)),
                    new ToolStripMenuItem("Выход", null, (s, e) => fileManager.ExitApplication(inputArea))
                ),
                new ToolStripMenuItem("Правка", null,
                    new ToolStripMenuItem("Отменить", null, (s, e) => editManager.Undo(inputArea)),
                    new ToolStripMenuItem("Повторить", null, (s, e) => editManager.Redo(inputArea)),
                    new ToolStripMenuItem("Вырезать", null, (s, e) => editManager.Cut(inputArea)),
                    new ToolStripMenuItem("Копировать", null, (s, e) => editManager.Copy(inputArea)),
                    new ToolStripMenuItem("Вставить", null, (s, e) => editManager.Paste(inputArea)),
                    new ToolStripMenuItem("Удалить", null, (s, e) => editManager.Delete(inputArea)),
                    new ToolStripMenuItem("Выделить все", null, (s, e) => editManager.SelectAll(inputArea))
                ),
                new ToolStripMenuItem("Текст", null, new ToolStripItem[]
                {
                    new ToolStripMenuItem("Постановка задачи", null, (s, e) => ShowEmbeddedHtml("TextEditor.Resources.postanovka.html")),
                    new ToolStripMenuItem("Грамматика", null, (s, e) => helpManager.ShowGrammar()),
                    new ToolStripMenuItem("Классификация грамматики", null, (s, e) => helpManager.ShowClassification()),
                    new ToolStripMenuItem("Метод анализа", null, (s, e) =>
                    {
                        // Извлекаем HTML и картинки
                        string htmlPath = ExtractEmbeddedResource("TextEditor.Resources.method.html", "method.html");
                        ExtractEmbeddedResource("TextEditor.Resources.graph.png", "graph.png");
                        ExtractEmbeddedResource("TextEditor.Resources.scaa.png", "scaa.png");

                        // Открываем в браузере
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlPath)
                        {
                            UseShellExecute = true
                        });
                    }),
                    new ToolStripSeparator(),
                    new ToolStripMenuItem("Тестовый пример", null, (s, e) =>
                    {
                        // Извлекаем HTML и картинки
                        string htmlPath = ExtractEmbeddedResource("TextEditor.Resources.test_example.html", "test_example.html");
                        ExtractEmbeddedResource("TextEditor.Resources.test1.png", "test1.png");
                        ExtractEmbeddedResource("TextEditor.Resources.test2.png", "test2.png");
                        ExtractEmbeddedResource("TextEditor.Resources.test3.png", "test3.png");

                        // Открываем в браузере
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(htmlPath)
                        {
                            UseShellExecute = true
                        });
                    }),
                    new ToolStripMenuItem("Список литературы", null, (s, e) =>
                    {
                        string htmlPath = ExtractEmbeddedResource("TextEditor.Resources.literature.html", "literature.html");

                        if (!string.IsNullOrEmpty(htmlPath) && File.Exists(htmlPath))
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo(htmlPath) { UseShellExecute = true });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось загрузить список литературы");
                        }
                    }),
                    new ToolStripMenuItem("Исходный код программы", null, (s, e) =>
                    {
                        string htmlPath = ExtractEmbeddedResource(
                            "TextEditor.Resources.source_code.html", "source_code.html");

                        if (!string.IsNullOrEmpty(htmlPath) && File.Exists(htmlPath))
                        {
                            try
                            {
                                Process.Start(new ProcessStartInfo(htmlPath) { UseShellExecute = true });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Не удалось загрузить листинг программы");
                        }
                    }),
                }),
                new ToolStripMenuItem("Пуск", null, (s, e) =>  RunRegexTimeSearch()), 
                new ToolStripMenuItem("Справка", null,
                    new ToolStripMenuItem("Вызов справки", null, (s, e) => helpManager.ShowHelp()),
                    new ToolStripMenuItem("О программе", null, (s, e) => helpManager.ShowAbout())
                )
            });

            toolStrip = new ToolStrip();
            toolStrip.Dock = DockStyle.Top;

            AddToolStripButton("Создать", "new.png", (s, e) => fileManager.NewFile(inputArea));
            AddToolStripButton("Открыть", "open.png", (s, e) => fileManager.OpenFile(inputArea));
            AddToolStripButton("Сохранить", "save.png", (s, e) => fileManager.SaveFile(inputArea));
            toolStrip.Items.Add(new ToolStripSeparator());
            AddToolStripButton("Отменить", "undo.png", (s, e) => editManager.Undo(inputArea));
            AddToolStripButton("Повторить", "redo.png", (s, e) => editManager.Redo(inputArea));
            toolStrip.Items.Add(new ToolStripSeparator());
            AddToolStripButton("Копировать", "copy.png", (s, e) => editManager.Copy(inputArea));
            AddToolStripButton("Вырезать", "cut.png", (s, e) => editManager.Cut(inputArea));
            AddToolStripButton("Вставить", "paste.png", (s, e) => editManager.Paste(inputArea));
           // AddToolStripButton("Анализ", "analyze.png", (s, e) => RunLexer());
            toolStrip.Items.Add(new ToolStripSeparator());
            AddToolStripButton("Справка", "help.png", (s, e) => helpManager.ShowHelp());
            AddToolStripButton("О программе", "info.png", (s, e) => helpManager.ShowAbout());
            toolStrip.ImageScalingSize = new Size(50, 50); // размер изображений
            toolStrip.Items.Add(new ToolStripSeparator());
            AddToolStripButton("Время", "time.png", (s, e) => RunRegexTimeSearch());
            AddToolStripButton("Пользователи", "user.png", (s, e) => RunRegexUserSearch());
            AddToolStripButton("UUID", "uuid.png", (s, e) => RunUUIDSearch());
            toolStrip.Items.Add(new ToolStripSeparator());
            AddToolStripButton("Рекурсивный парсер", "", (s, e) => RunRecursiveDescentParser());
            toolStrip.AutoSize = false;
            toolStrip.Height = 85; // немного выше, чем сами кнопки

            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            inputArea = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = false };

            var lineNumbers = new RichTextBox
            {
                Padding = new Padding(0, 5, 0, 0),
                Dock = DockStyle.Left,
                Width = 40,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                BackColor = SystemColors.ControlLight,
                BorderStyle = BorderStyle.None,
                Enabled = false
            };

            var sharedFont = new Font("Consolas", 10);
            inputArea.Font = sharedFont;
            lineNumbers.Font = sharedFont;
            lineNumbers.SelectionAlignment = HorizontalAlignment.Right;

            inputArea.TextChanged += (s, e) =>
            {
                fileManager.MarkTextChanged();
                lineNumbers.Text = GetLineNumbers(inputArea);
            };
            inputArea.VScroll += (s, e) =>
            {
                int d = inputArea.GetPositionFromCharIndex(0).Y % (int)inputArea.Font.GetHeight();
                lineNumbers.Location = new Point(0, d);
                lineNumbers.Text = GetLineNumbers(inputArea);
            };

            var inputPanel = new Panel { Dock = DockStyle.Fill };
            inputPanel.Controls.Add(inputArea);
            inputPanel.Controls.Add(lineNumbers);
            splitContainer.Panel1.Controls.Add(inputPanel);

            // ========== TabControl с двумя вкладками ==========
            outputTabs = new TabControl { Dock = DockStyle.Fill };

            lexerGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            lexerGrid.Columns.Add("Code", "Код");
            lexerGrid.Columns.Add("Type", "Тип");
            lexerGrid.Columns.Add("Lexeme", "Лексема");
            lexerGrid.Columns.Add("Position", "Позиция");

            parserGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            parserGrid.Columns.Add("Expected", "Ожидалось");
            parserGrid.Columns.Add("Found", "Найдено");
            parserGrid.Columns.Add("Line", "Строка");
            parserGrid.Columns.Add("Position", "Позиция");
            parserGrid.Columns.Add("Skipped", "Отброшено");

            //var lexerTab = new TabPage("");
            //var parserTab = new TabPage("");
           // lexerTab.Controls.Add(lexerGrid);
           // parserTab.Controls.Add(parserGrid);
           //outputTabs.TabPages.Add(lexerTab);
           //outputTabs.TabPages.Add(parserTab);

            splitContainer.Panel2.Controls.Add(outputTabs);

            this.Controls.Add(splitContainer);
            this.Controls.Add(toolStrip);
            this.Controls.Add(menuStrip);
        }
        private void inputArea_TextChanged(object sender, EventArgs e)
        {
            debounceTimer.Stop();  // Сбросить старый таймер
            debounceTimer.Start(); // Запустить заново
        }
        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop(); // Останавливаем, чтобы не вызывался повторно

            Lexer lexer = new Lexer();
            var tokens = lexer.Analyze(inputArea.Text);
            lexer.HighlightInvalidTokens(inputArea, tokens);
        }
        private void AddToolStripButton(string text, string resourceName, EventHandler onClick)
        {
            var button = new ToolStripButton()
            {
                Text = text,
                Image = LoadEmbeddedImage(resourceName), // Загружаем иконку из ресурсов
                DisplayStyle = ToolStripItemDisplayStyle.Image,
                ToolTipText = text,
                ImageScaling = ToolStripItemImageScaling.None // чтобы не сжималось
            };

            button.Click += onClick;
            toolStrip.Items.Add(button);
        }
        private Image LoadEmbeddedImage(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();  // Получаем текущую сборку
            string fullResourceName = $"TextEditor.Resources.{resourceName}";  // Формируем путь к ресурсу

            using (var stream = assembly.GetManifestResourceStream(fullResourceName))
            {
                if (stream != null)
                {
                    return new Bitmap(stream);  // Возвращаем изображение, если ресурс найден
                }
            }

            // Если ресурс не найден, возвращаем null
            return null;
        }

        private void RunLexer()
        {
            Lexer lexer = new Lexer();
            List<Token> tokens = lexer.Analyze(inputArea.Text);

            lexerGrid.Rows.Clear();
            foreach (var token in tokens)
            {
                lexerGrid.Rows.Add(token.Code, token.Type, token.Lexeme, token.Position);
            }

            outputTabs.SelectedTab = outputTabs.TabPages[0];
            lexer.HighlightInvalidTokens(inputArea, tokens);
        }
        private void ShowEmbeddedHtml(string resourceName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "temp_page.html");

            using (var stream = typeof(MainForm).Assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(tempPath, false, System.Text.Encoding.UTF8))
            {
                writer.Write(reader.ReadToEnd());
            }

            // Открываем HTML файл в браузере
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = tempPath,
                UseShellExecute = true // чтобы открыть в браузере
            });
        }
        private string ExtractEmbeddedResource(string resourceName, string fileName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), fileName);

            if (!File.Exists(tempPath))  // Записываем ресурс только если его нет
            {
                using (var stream = typeof(MainForm).Assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream != null)
                    {
                        using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
                        {
                            stream.CopyTo(fileStream);
                        }
                    }
                }
            }

            return tempPath;  // Возвращаем путь к временному файлу
        }

        //private void RunParser()
        //{
        //    Parser parser = new Parser();
        //    var errors = parser.Analyze(inputArea.Text);
        //    parser.HighlightErrors(inputArea, errors);

        //    parserGrid.Rows.Clear();
        //    foreach (var error in errors)
        //    {
        //        parserGrid.Rows.Add(error.Expected, error.Found, error.Line, error.Position, error.SkippedFragment);
        //    }

        //    outputTabs.SelectedTab = outputTabs.TabPages[1];
        //}
        private void RunRegexTimeSearch()
        {
            // Убедимся, что вкладка "Regex поиск" есть
            TabPage regexTab = null;
            DataGridView regexGrid = null;

            foreach (TabPage tab in outputTabs.TabPages)
            {
                if (tab.Text == "Регулярные выражения")
                {
                    regexTab = tab;
                    regexGrid = tab.Controls[0] as DataGridView;
                    break;
                }
            }

            // Если ещё не создавали — создаём
            if (regexTab == null)
            {
                regexTab = new TabPage("Регулярные выражения");

                regexGrid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                regexGrid.Columns.Add("Start", "Начало");
                regexGrid.Columns.Add("Match", "Совпадение");

                regexTab.Controls.Add(regexGrid);
                outputTabs.TabPages.Add(regexTab);
            }

            // Очищаем старые результаты
            regexGrid.Rows.Clear();

            string text = inputArea.Text;
            var regex = new System.Text.RegularExpressions.Regex(@"(?:[01]?\d|2[0-3]):[0-5]\d:[0-5]\d");

            var matches = regex.Matches(text);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                int start = match.Index + 1; // Позиция с 1, как ты хочешь
                regexGrid.Rows.Add(start, match.Value);
            }

            outputTabs.SelectedTab = regexTab;
        }
        private void RunRegexUserSearch()
        {
            // Проверка и создание вкладки
            TabPage regexTab = null;
            DataGridView regexGrid = null;

            foreach (TabPage tab in outputTabs.TabPages)
            {
                if (tab.Text == "Пользователи")
                {
                    regexTab = tab;
                    regexGrid = tab.Controls[0] as DataGridView;
                    break;
                }
            }

            if (regexTab == null)
            {
                regexTab = new TabPage("Пользователи");

                regexGrid = new DataGridView
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    AllowUserToAddRows = false,
                    AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
                };

                regexGrid.Columns.Add("Start", "Начало");
                regexGrid.Columns.Add("Match", "Имя пользователя");

                regexTab.Controls.Add(regexGrid);
                outputTabs.TabPages.Add(regexTab);
            }

            regexGrid.Rows.Clear();

            string text = inputArea.Text;
            var regex = new System.Text.RegularExpressions.Regex(@"@[A-Za-z0-9]{1,14}");

            var matches = regex.Matches(text);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                int start = match.Index + 1;
                regexGrid.Rows.Add(start, match.Value);
            }

            outputTabs.SelectedTab = regexTab;
        }
        private void RunUUIDSearch()
        {
            string input = inputArea.Text;

            var uuidGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            uuidGrid.Columns.Add("Index", "№");
            uuidGrid.Columns.Add("Start", "Начальная позиция");
            uuidGrid.Columns.Add("Match", "UUID");

            int index = 1;

            for (int i = 0; i <= input.Length - 36; i++)
            {
                int state = 0;

                for (int j = 0; j < 36; j++)
                {
                    char c = input[i + j];

                    // Переходы автомата:
                    if ((state == 8 || state == 13 || state == 18 || state == 23))
                    {
                        if (c == '-')
                            state++;
                        else
                            break;
                    }
                    else
                    {
                        if (IsHexChar(c))
                            state++;
                        else
                            break;
                    }
                }

                if (state == 36)
                {
                    string match = input.Substring(i, 36);
                    uuidGrid.Rows.Add(index++, i + 1, match);
                    i += 35; // чтобы не искать пересекающиеся UUID
                }
            }

            var uuidTab = new TabPage("UUID");
            uuidTab.Controls.Add(uuidGrid);

            // Удалим вкладку, если уже есть
            for (int i = 0; i < outputTabs.TabPages.Count; i++)
            {
                if (outputTabs.TabPages[i].Text == "UUID")
                {
                    outputTabs.TabPages.RemoveAt(i);
                    break;
                }
            }

            outputTabs.TabPages.Add(uuidTab);
            outputTabs.SelectedTab = uuidTab;
        }

        // Проверка на шестнадцатеричный символ
        private bool IsHexChar(char c)
        {
            return (c >= '0' && c <= '9') ||
                   (c >= 'a' && c <= 'f') ||
                   (c >= 'A' && c <= 'F');
        }
        private void RunRecursiveDescentParser()
        {
            Parser parser = new Parser(); // ← твой уже рабочий класс
            bool success = parser.Analyze(inputArea.Text);
            string trace = parser.GetCallTrace();

            if (syntaxTab == null)
            {
                syntaxTab = new TabPage("Синтаксис");

                syntaxOutput = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10)
                };

                syntaxTab.Controls.Add(syntaxOutput);
                outputTabs.TabPages.Add(syntaxTab);
            }

            syntaxOutput.Clear();
            syntaxOutput.Text = success
                ? $"Анализ завершен успешно\n\nРазбор: {trace}"
                : $"Анализ завершен с ошибками\n\nРазбор: {trace}";

            outputTabs.SelectedTab = syntaxTab;
        }



    }
}
