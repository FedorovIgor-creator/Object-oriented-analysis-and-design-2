using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace With_GUI
{
    public partial class Form1 : Form // Form1 наследуется от стандартного класса Windows Forms
    {
        // === Элементы управления ===
        private TabControl tabControl;
        private ListBox lstTopics;
        private ListBox lstSubTopics;
        private TextBox txtContent;
        private Label lblTopicInfo;
        private Button btnBack;
        private Button btnAction;

        private Teacher teacher;
        private LearningMaterial? currentMaterial;
        private string currentMaterialType = "";
        private string selectedTopic = "";

        // === Данные: Основы программирования ===
        private readonly Dictionary<string, List<string>> testTopics = new()
        {
            ["Переменные и типы данных"] = new()
            {
                "Какой тип данных используется для целых чисел?",
                "Как объявить строковую переменную в C#?",
                "Что вернёт выражение: 10 / 3 в C#?",
                "Какой модификатор делает поле доступным только внутри класса?"
            },
            ["Условия и циклы"] = new()
            {
                "Как записать условие 'если число чётное'?",
                "Чем отличается while от do-while?",
                "Что выведет: for(int i=0; i<3; i++) Console.Write(i);",
                "Как выйти из цикла досрочно?"
            },
            ["Методы и функции"] = new()
            {
                "Как передать параметр по ссылке в C#?",
                "Что такое перегрузка методов?",
                "Как вернуть несколько значений из метода?",
                "В чём разница между ref и out параметрами?"
            },
            ["Массивы и коллекции"] = new()
            {
                "Как создать список целых чисел?",
                "В чём разница между Array и List<T>?",
                "Как перебрать элементы массива через foreach?",
                "Что такое индекс массива?"
            }
        };

        private readonly Dictionary<string, (string task, string hint, List<string> examples)> practicalTasks = new()
        {
            ["Калькулятор"] = (
                "Напишите консольный калькулятор с операциями +, -, *, /",
                "Используйте switch и TryParse для обработки ввода",
                new List<string> { "Введите первое число", "Введите операцию", "Введите второе число", "Выведите результат" }
            ),
            ["Поиск в массиве"] = (
                "Реализуйте метод поиска максимального элемента в массиве",
                "Пройдитесь циклом и сравнивайте значения",
                new List<string> { "Создайте массив", "Инициализируйте max первым элементом", "Пройдитесь циклом", "Сравнивайте и обновляйте max" }
            ),
            ["Работа со строками"] = (
                "Напишите метод, который подсчитывает количество гласных в строке",
                "Используйте ToLower() и Contains() для проверки",
                new List<string> { "Приведите строку к нижнему регистру", "Создайте счётчик", "Пройдитесь по каждому символу", "Проверьте на гласную" }
            ),
            ["Класс 'Студент'"] = (
                "Создайте класс с полями: Имя, Группа, Средний балл. Добавьте метод ToString()",
                "Не забудьте про конструктор и инкапсуляцию",
                new List<string> { "Объявите поля", "Создайте конструктор", "Добавьте свойства", "Реализуйте ToString()" }
            )
        };

        private readonly Dictionary<string, List<string>> lectureTopics = new()
        {
            ["Введение в C#"] = new()
            {
                "• История и особенности языка",
                "• Структура программы: namespace, class, Main",
                "• Компиляция и запуск через dotnet CLI",
                "• Базовые типы данных и операторы"
            },
            ["Объектно-ориентированное программирование"] = new()
            {
                "• Классы и объекты: поля, свойства, методы",
                "• Инкапсуляция: модификаторы доступа",
                "• Наследование и полиморфизм",
                "• Абстрактные классы и интерфейсы"
            },
            ["Работа с данными"] = new()
            {
                "• Коллекции: List, Dictionary, HashSet",
                "• LINQ: базовые запросы и методы расширения",
                "• Сериализация JSON/XML",
                "• Основы работы с файлами"
            },
            ["Асинхронное программирование"] = new()
            {
                "• Ключевые слова async/await",
                "• Task и Task<T>: что возвращают асинхронные методы",
                "• Обработка исключений в async-коде",
                "• Практические примеры: загрузка данных, UI-отзывчивость"
            }
        };

        public Form1()
        {
            InitializeComponent();
            InitializeCustomComponents();
            teacher = new Teacher("Марина Литовченко");
        }

        private void InitializeCustomComponents()
        {
            this.Text = "📚 Основы программирования";
            this.Size = new Size(900, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5F);

            // === Заголовок предмета ===
            var lblSubject = new Label
            {
                Text = "🎓 Основы программирования",
                Location = new Point(15, 15),
                Size = new Size(850, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                BackColor = Color.FromArgb(63, 81, 181),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };

            // === Вкладки по типам материалов ===
            tabControl = new TabControl
            {
                Location = new Point(15, 55),
                Size = new Size(850, 60),
                Alignment = TabAlignment.Top
            };

            var tabTest = new TabPage("🧪 Тесты");
            var tabTask = new TabPage("💻 Практика");
            var tabLecture = new TabPage("📖 Лекции");

            AddTabButton(tabTest, "📝 Сборник тестов", () => LoadMaterialType("test"));
            AddTabButton(tabTask, "📋 Сборник задач", () => LoadMaterialType("task"));
            AddTabButton(tabLecture, "📚 Сборник лекций", () => LoadMaterialType("lecture"));

            tabControl.TabPages.AddRange(new[] { tabTest, tabTask, tabLecture });

            // === Список тем ===
            lstTopics = new ListBox
            {
                Location = new Point(15, 125),
                Size = new Size(280, 450),
                Font = new Font("Segoe UI", 10F)
            };
            lstTopics.SelectedIndexChanged += LstTopics_SelectedIndexChanged;

            // === Список подтем (вопросов/пунктов) ===
            lstSubTopics = new ListBox
            {
                Location = new Point(310, 125),
                Size = new Size(280, 450),
                Font = new Font("Segoe UI", 9.5F),
                Visible = false
            };
            lstSubTopics.SelectedIndexChanged += LstSubTopics_SelectedIndexChanged;

            // === Область контента ===
            txtContent = new TextBox
            {
                Location = new Point(605, 125),
                Size = new Size(260, 410),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(250, 250, 250),
                Visible = false
            };

            // === Кнопка действия ===
            btnAction = new Button
            {
                Text = "▶️ Начать",
                Location = new Point(605, 545),
                Size = new Size(260, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(76, 175, 80),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Visible = false
            };
            btnAction.Click += BtnAction_Click;
            btnAction.MouseEnter += (s, e) => btnAction.BackColor = Color.FromArgb(56, 142, 60);
            btnAction.MouseLeave += (s, e) => btnAction.BackColor = Color.FromArgb(76, 175, 80);

            // === Доп. элементы ===
            lblTopicInfo = new Label
            {
                Location = new Point(15, 585),
                Size = new Size(850, 25),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                Text = "💡 Выберите тип материала для начала работы"
            };

            btnBack = new Button
            {
                Text = "← Назад",
                Location = new Point(15, 545),
                Size = new Size(100, 35),
                Visible = false,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(158, 158, 158),
                ForeColor = Color.White
            };
            btnBack.Click += (s, e) => ShowPreviousLevel();
            btnBack.MouseEnter += (s, e) => btnBack.BackColor = Color.FromArgb(117, 117, 117);
            btnBack.MouseLeave += (s, e) => btnBack.BackColor = Color.FromArgb(158, 158, 158);

            // === Добавление на форму ===
            this.Controls.AddRange(new Control[]
            {
                lblSubject, tabControl, lstTopics, lstSubTopics, txtContent,
                btnAction, lblTopicInfo, btnBack
            });
        }

        private void AddTabButton(TabPage tab, string text, Action onClick)
        {
            var btn = new Button
            {
                Text = text,
                Dock = DockStyle.Fill,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(240, 248, 255),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold)
            };
            btn.Click += (s, e) => onClick();
            btn.MouseEnter += (s, e) => btn.BackColor = Color.FromArgb(200, 230, 255);
            btn.MouseLeave += (s, e) => btn.BackColor = Color.FromArgb(240, 248, 255);
            tab.Controls.Add(btn);
        }

        // === Загрузка типа материала ===
        private void LoadMaterialType(string type)
        {
            currentMaterialType = type;
            selectedTopic = "";

            lstTopics.Items.Clear();
            lstSubTopics.Items.Clear();
            txtContent.Clear();

            lstSubTopics.Visible = false;
            txtContent.Visible = false;
            btnAction.Visible = false;
            btnBack.Visible = false;

            var titles = type switch
            {
                "test" => testTopics.Keys.ToList(),
                "task" => practicalTasks.Keys.ToList(),
                "lecture" => lectureTopics.Keys.ToList(),
                _ => new List<string>()
            };

            foreach (var title in titles)
                lstTopics.Items.Add($"📁 {title}");

            lstTopics.Visible = true;

            lblTopicInfo.Text = type switch
            {
                "test" => "💡 Выберите тему теста для просмотра вопросов",
                "task" => "💡 Выберите практическое задание",
                "lecture" => "💡 Выберите лекцию для просмотра плана",
                _ => ""
            };
        }

        // === Выбор темы ===
        private void LstTopics_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (lstTopics.SelectedItem is not string selected) return;
            selectedTopic = selected.Replace("📁 ", "");

            lstSubTopics.Items.Clear();
            lstSubTopics.Visible = true;
            txtContent.Visible = false;
            btnAction.Visible = false;

            switch (currentMaterialType)
            {
                case "test":
                    if (testTopics.TryGetValue(selectedTopic, out var questions))
                        foreach (var q in questions)
                            lstSubTopics.Items.Add($"❓ {q}");
                    lblTopicInfo.Text = $"📍 Тема: {selectedTopic} | Выберите вопрос";
                    break;

                case "task":
                    if (practicalTasks.TryGetValue(selectedTopic, out var taskData))
                    {
                        lstSubTopics.Items.Add($"📋 Условие: {taskData.task}");
                        lstSubTopics.Items.Add($"💡 Подсказка: {taskData.hint}");
                        lstSubTopics.Items.Add("");
                        lstSubTopics.Items.Add("📝 Примерные шаги:");
                        foreach (var step in taskData.examples)
                            lstSubTopics.Items.Add($"  • {step}");
                    }
                    lblTopicInfo.Text = $"📍 Задание: {selectedTopic}";
                    btnAction.Text = "📤 Сдать решение";
                    btnAction.Visible = true;
                    break;

                case "lecture":
                    if (lectureTopics.TryGetValue(selectedTopic, out var lecturePlan))
                        foreach (var item in lecturePlan)
                            lstSubTopics.Items.Add(item);
                    lblTopicInfo.Text = $"📍 Лекция: {selectedTopic}";
                    btnAction.Text = "⬇️ Скачать презентацию";
                    btnAction.Visible = true;
                    break;
            }

            btnBack.Visible = true;
        }

        // === Выбор подтемы (вопроса) ===
        private void LstSubTopics_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (currentMaterialType != "test") return;
            if (lstSubTopics.SelectedItem is not string selected) return;

            string question = selected.Replace("❓ ", "");

            txtContent.Visible = true;
            txtContent.Clear();
            txtContent.AppendText($"🧪 Вопрос:\n{question}\n\n");
            txtContent.AppendText("Варианты ответов:\n");
            txtContent.AppendText("  ○ Вариант A\n");
            txtContent.AppendText("  ○ Вариант B\n");
            txtContent.AppendText("  ○ Вариант C\n");
            txtContent.AppendText("  ○ Вариант D\n\n");
            txtContent.AppendText("💡 Выберите правильный ответ");

            btnAction.Text = "✅ Ответить";
            btnAction.Visible = true;

            // Создаём материал через фабрику
            LearningMaterialFactory factory = new TestFactory();
            currentMaterial = teacher.CreateMaterial(factory);
            currentMaterial.Title = selectedTopic;
            currentMaterial.Subject = "Основы программирования";
        }

        // === Обработка кнопки действия ===
        private void BtnAction_Click(object? sender, EventArgs e)
        {
            if (currentMaterial == null)
            {
                // Если материал ещё не создан, создаём его
                LearningMaterialFactory factory = currentMaterialType switch
                {
                    "test" => new TestFactory(),
                    "task" => new PracticalTaskFactory(),
                    "lecture" => new LectureFactory(),
                    _ => throw new InvalidOperationException("Неизвестный тип материала")
                };

                currentMaterial = teacher.CreateMaterial(factory);
                currentMaterial.Title = selectedTopic;
                currentMaterial.Subject = "Основы программирования";
            }

            switch (currentMaterialType)
            {
                case "test":
                    if (currentMaterial is Test test)
                    {
                        txtContent.AppendText("\n\n✅ Ответ принят!\n");
                        test.TakeTest((msg) => txtContent.AppendText(msg + "\n"));
                    }
                    break;

                case "task":
                    if (currentMaterial is PracticalTask task)
                    {
                        txtContent.Clear();
                        txtContent.AppendText($"📤 Отправка задания: {selectedTopic}\n");
                        txtContent.AppendText(new string('─', 40) + "\n\n");
                        task.SubmitForReview((msg) => txtContent.AppendText(msg + "\n"));
                    }
                    break;

                case "lecture":
                    if (currentMaterial is Lecture lecture)
                    {
                        txtContent.Clear();
                        txtContent.AppendText($"⬇️ Скачивание: {selectedTopic}\n");
                        txtContent.AppendText(new string('─', 40) + "\n\n");
                        lecture.DownloadPresentation((msg) => txtContent.AppendText(msg + "\n"));
                    }
                    break;
            }

            btnAction.Visible = false;
        }

        // === Возврат на предыдущий уровень ===
        private void ShowPreviousLevel()
        {
            if (txtContent.Visible)
            {
                txtContent.Visible = false;
                btnAction.Visible = currentMaterialType == "task" || currentMaterialType == "lecture";
                lblTopicInfo.Text = currentMaterialType switch
                {
                    "test" => $"📍 Тема: {selectedTopic} | Выберите вопрос",
                    "task" => $"📍 Задание: {selectedTopic}",
                    "lecture" => $"📍 Лекция: {selectedTopic}",
                    _ => ""
                };
            }
            else if (lstSubTopics.Visible)
            {
                lstSubTopics.Visible = false;
                lstSubTopics.Items.Clear();
                btnBack.Visible = false;
                btnAction.Visible = false;
                currentMaterial = null;

                lblTopicInfo.Text = currentMaterialType switch
                {
                    "test" => "💡 Выберите тему теста для просмотра вопросов",
                    "task" => "💡 Выберите практическое задание",
                    "lecture" => "💡 Выберите лекцию для просмотра плана",
                    _ => ""
                };
            }
        }
    }

    // ==================== Абстрактный класс материала ====================
    abstract class LearningMaterial // Абстрактный базовый класс для всех материалов
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;

        public abstract void View(Action<string> output);     // Абстрактный метод 
    }

    // ==================== Тест ====================
    class Test : LearningMaterial//наследование
    {
        public override void View(Action<string> output) =>
            output($"🧪 Тест '{Title}' по теме: {Subject}");

        public void TakeTest(Action<string> output)
        {
            output("✅ Ответ записан!");
            output(new string('─', 30));
            output("📊 Результаты:");
            output("  Правильных ответов: 2 из 4");
            output("  Оценка: 75%");
            output("🎯 Хорошая работа!");
        }
    }

    // ==================== Практическая задача ====================
    class PracticalTask : LearningMaterial //наследование
    {
        public override void View(Action<string> output) =>
            output($"💻 Задача '{Title}' по теме: {Subject}");

        public void SubmitForReview(Action<string> output)
        {
            output("📤 Работа отправлена на проверку!");
            output("👨‍🏫 Преподаватель: Марина Литовченко");
            output("⏱️ Ожидайте обратную связь в течение 24 часов");
            output(new string('─', 30));
            output("📝 Статус: На проверке");
            output("📅 Дата отправки: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        }
    }

    // ==================== Лекция ====================
    class Lecture : LearningMaterial //наследование
    {
        public override void View(Action<string> output) =>
            output($"📖 Лекция '{Title}' по теме: {Subject}");

        public void DownloadPresentation(Action<string> output)
        {
            output("⬇️ Презентация успешно скачана!");
            output($"📁 Файл: {Title}.pptx (2.4 МБ)");
            output("📂 Сохранено в: Документы/Учебные материалы/");
            output(new string('─', 30));
            output("✅ Готово к просмотру!");
        }
    }

    // ==================== Фабрики ====================
    abstract class LearningMaterialFactory // Абстрактная фабрика
    {
        public abstract LearningMaterial CreateMaterial();
    }

    class TestFactory : LearningMaterialFactory //наследование
    {
        public override LearningMaterial CreateMaterial() => new Test();
    }

    class PracticalTaskFactory : LearningMaterialFactory
    {
        public override LearningMaterial CreateMaterial() => new PracticalTask();
    }

    class LectureFactory : LearningMaterialFactory
    {
        public override LearningMaterial CreateMaterial() => new Lecture();
    }

    // ==================== Преподаватель ====================
    class Teacher
    {
        public string Name { get; set; }

        public Teacher(string name) => Name = name;

        public LearningMaterial CreateMaterial(LearningMaterialFactory factory) //зависимость использует LearningMaterialFactory как параметр

        {
            return factory.CreateMaterial();
        }
    }
}