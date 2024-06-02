using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HeatingSystem
{
    public partial class MainForm : Form
    {
        private List<HeatingSystemRecord> records;
        private int longSeasonCount;
        private Label labelCount; // Объявление переменной labelCount
        private TextBox textBoxLocation; // Объявление переменной textBoxLocation

        public MainForm()
        {
            InitializeComponent();
            records = new List<HeatingSystemRecord>();
            LoadData();
            tabControl1.Selected += new TabControlEventHandler(tabControl1_Selected);

            // Добавление кнопок навигации на все вкладки
            AddNavigationButtons();

            // Инициализация labelCount
            labelCount = new Label
            {
                Location = new System.Drawing.Point(10, 220),
                Size = new System.Drawing.Size(300, 20)
            };
            this.Controls.Add(labelCount);

            // Инициализация textBoxLocation
            textBoxLocation = new TextBox
            {
                Location = new System.Drawing.Point(120, 10),
                Size = new System.Drawing.Size(200, 20)
            };
            this.Controls.Add(textBoxLocation);

            // Установка высоты DataGridView
            dataGridView1.Height = 180;

            // Создаем кнопки для переключения на другие вкладки
            Button btnAllBoilers = new Button
            {
                Text = "Всі котельні",
                Location = new System.Drawing.Point(20, 700),
                Size = new System.Drawing.Size(100, 30),
            };
            btnAllBoilers.Click += (sender, e) => tabControl1.SelectedIndex = 1; // Переключение на вкладку "Всі котельні"
            tabPage1.Controls.Add(btnAllBoilers);
            btnAllBoilers.Click += (sender, e) => tabControl1.SelectedIndex = 1; // Переключение на вкладку "Всі котельні"
            tabPage1.Controls.Add(btnAllBoilers); // Добавляем кнопку на первую вкладку

            Button btnLateSeason = new Button
            {
                Text = "Сезон пізніше 15 жовтня",
                Location = new System.Drawing.Point(140, 700),
                Size = new System.Drawing.Size(180, 30),
            };
            btnLateSeason.Click += (sender, e) => tabControl1.SelectedIndex = 2; // Переключение на вкладку "Сезон пізніше 15 жовтня"
            tabPage1.Controls.Add(btnLateSeason); // Добавляем кнопку на первую вкладку

            Button btnLongSeason = new Button
            {
                Text = "Більше 6 місяців",
                Location = new System.Drawing.Point(340, 700),
                Size = new System.Drawing.Size(120, 30),
            };
            btnLongSeason.Click += (sender, e) => tabControl1.SelectedIndex = 3; // Переключение на вкладку "Більше 6 місяців"
            tabPage1.Controls.Add(btnLongSeason); // Добавляем кнопку на первую вкладку

            Button btnShortestSeason = new Button
            {
                Text = "Найкоротший сезон",
                Location = new System.Drawing.Point(480, 700),
                Size = new System.Drawing.Size(140, 30),
            };
            btnShortestSeason.Click += (sender, e) => tabControl1.SelectedIndex = 4; // Переключение на вкладку "Найкоротший сезон"
            tabPage1.Controls.Add(btnShortestSeason); // Добавляем кнопку на первую вкладку
        }

        private void AddNavigationButtons()
        {
            // Создание кнопок для каждой вкладки, кроме первой
            for (int i = 1; i < tabControl1.TabPages.Count; i++)
            {
                var tabPage = tabControl1.TabPages[i];
                var button = new Button
                {
                    Text = tabPage.Text,
                    Location = new System.Drawing.Point(20 + (i - 1) * 110, 10),
                    Size = new System.Drawing.Size(100, 30),
                    Tag = i // Используем Tag для хранения индекса вкладки
                };
                button.Click += NavigateToTabPage;
                tabPage.Controls.Add(button);
            }
        }

        private void NavigateToTabPage(object sender, EventArgs e)
        {
            var button = (Button)sender;
            int tabPageIndex = Convert.ToInt32(button.Tag);
            tabControl1.SelectTab(tabPageIndex);
        }

        private void LoadData()
        {
            if (File.Exists("data.txt"))
            {
                var lines = File.ReadAllLines("data.txt");
                foreach (var line in lines)
                {
                    var parts = line.Split(';');
                    if (parts.Length == 6)
                    {
                        records.Add(new HeatingSystemRecord
                        {
                            Location = parts[0],
                            BoilerNumber = int.Parse(parts[1]),
                            HeatingObjects = int.Parse(parts[2]),
                            StartDate = DateTime.Parse(parts[3]),
                            StartTemperature = double.Parse(parts[4]),
                            EndDate = DateTime.Parse(parts[5])
                        });
                    }
                }
            }
            RefreshDataGridView();
        }

        private void SaveData()
        {
            var lines = records.Select(r => $"{r.Location};{r.BoilerNumber};{r.HeatingObjects};{r.StartDate.ToShortDateString()};{r.StartTemperature};{r.EndDate.ToShortDateString()}").ToArray();
            File.WriteAllLines("data.txt", lines);
        }

        private void RefreshDataGridView()
        {
            // Сортування за датою початку сезону за допомогою сортування Шелла
            records = ShellSort(records);

            dataGridView1.DataSource = null;
            dataGridView1.DataSource = records.Select(r => new
            {
                Місцезнаходження = r.Location,
                Номер_котла = r.BoilerNumber,
                Обєкти_опалення = r.HeatingObjects,
                Дата_початку = r.StartDate.ToShortDateString(),
                Температура_початку = r.StartTemperature,
                Дата_кінця = r.EndDate.ToShortDateString()
            }).ToList();

            FormatDataGridView(); // Применение форматирования после обновления данных
        }

        // Метод сортування Шелла
        private List<HeatingSystemRecord> ShellSort(List<HeatingSystemRecord> array)
        {
            int n = array.Count;
            int gap = n / 2;
            while (gap > 0)
            {
                for (int i = gap; i < n; i++)
                {
                    HeatingSystemRecord temp = array[i];
                    int j = i;
                    while (j >= gap && array[j - gap].StartDate > temp.StartDate)
                    {
                        array[j] = array[j - gap];
                        j -= gap;
                    }
                    array[j] = temp;
                }
                gap /= 2;
            }
            return array;
        }


        private void FormatDataGridView()
        {
            // Установка ширины первого столбца
            if (dataGridView1.Columns.Count > 0)
            {
                dataGridView1.Columns[0].Width = 260;
            }

            // Установка заголовков столбцов на украинском языке
            if (dataGridView1.Columns.Count > 5)
            {
                dataGridView1.Columns[0].HeaderText = "Місцезнаходження";
                dataGridView1.Columns[1].HeaderText = "Номер котла";
                dataGridView1.Columns[2].HeaderText = "Об'єкти опалення";
                dataGridView1.Columns[3].HeaderText = "Дата початку";
                dataGridView1.Columns[4].HeaderText = "Температура початку";
                dataGridView1.Columns[5].HeaderText = "Дата кінця";
            }

            // Установка стиля DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Arial", 10, System.Drawing.FontStyle.Bold);
            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Arial", 10);
            dataGridView1.DefaultCellStyle.ForeColor = System.Drawing.Color.Black;
            dataGridView1.DefaultCellStyle.BackColor = System.Drawing.Color.Beige;
            dataGridView1.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            dataGridView1.DefaultCellStyle.SelectionBackColor = System.Drawing.Color.DarkSlateGray;
            dataGridView1.RowHeadersDefaultCellStyle.BackColor = System.Drawing.Color.LightSlateGray;
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            dataGridView1.EnableHeadersVisualStyles = false;
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var record = new HeatingSystemRecord
            {
                Location = comboBoxLocation.SelectedItem.ToString(),
                BoilerNumber = (int)numericUpDownBoilerNumber.Value,
                HeatingObjects = (int)numericUpDownHeatingObjects.Value,
                StartDate = dateTimePickerStartDate.Value.Date,
                StartTemperature = (double)numericUpDownStartTemperature.Value,
                EndDate = dateTimePickerEndDate.Value.Date
            };

            if (!records.Any(r => r.Equals(record)))
            {
                records.Add(record);
                SaveData();
                RefreshDataGridView();
            }
            else
            {
                MessageBox.Show("Такий запис вже існує!", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedIndex = dataGridView1.SelectedRows[0].Index;
                var selectedRecord = records[selectedIndex];

                selectedRecord.Location = textBoxLocation.Text;
                selectedRecord.BoilerNumber = (int)numericUpDownBoilerNumber.Value;
                selectedRecord.HeatingObjects = (int)numericUpDownHeatingObjects.Value;
                selectedRecord.StartDate = dateTimePickerStartDate.Value.Date;
                selectedRecord.StartTemperature = (double)numericUpDownStartTemperature.Value;
                selectedRecord.EndDate = dateTimePickerEndDate.Value.Date;

                SaveData();
                RefreshDataGridView();
            }
            else
            {
                MessageBox.Show("Виберіть запис для редагування", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedIndex = dataGridView1.SelectedRows[0].Index;
                records.RemoveAt(selectedIndex);
                SaveData();
                RefreshDataGridView();
            }
            else
            {
                MessageBox.Show("Виберіть запис для видалення", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonSearch_Click(object sender, EventArgs e)
        {
            var searchText = textBoxSearch.Text.ToLower();


            // Бінарний пошук у відсортованому за датою списку
            int index = BinarySearch(records, ((DateTime)dateTimePickerSearchDate.Value).Date);


            if (index >= 0)
            {
                dataGridView1.DataSource = null;
                dataGridView1.DataSource = new List<HeatingSystemRecord> { records[index] };
                FormatDataGridView();
            }
            else
            {
                MessageBox.Show("Запис не знайдено", "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private int BinarySearch(List<HeatingSystemRecord> records, object searchDate)
        {
            throw new NotImplementedException();
        }

        // Бінарний пошук за датою початку сезону
        private int BinarySearch(List<HeatingSystemRecord> array, DateTime searchDate)
        {
            int left = 0;
            int right = array.Count - 1;

            while (left <= right)
            {
                int mid = left + (right - left) / 2;
                if (array[mid].StartDate == searchDate)
                {
                    return mid;
                }
                else if (array[mid].StartDate < searchDate)
                {
                    left = mid + 1;
                }
                else
                {
                    right = mid - 1;
                }
            }

            return -1; // Запис не знайдено
        }



        private void buttonSort_Click(object sender, EventArgs e)
        {
            var sortedRecords = records.OrderBy(r => r.StartDate).ToList();
            foreach (var record in sortedRecords)
            {
                var fileName = $"{record.StartDate.ToShortDateString().Replace('/', '_')}.txt";
                var line = $"{record.Location};{record.BoilerNumber};{record.HeatingObjects};{record.StartDate.ToShortDateString()};{record.StartTemperature};{record.EndDate.ToShortDateString()}";
                File.AppendAllText(fileName, line + Environment.NewLine);
            }
            MessageBox.Show("Дані відсортовано та збережено у файлах!", "Інформація", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void tabControl1_Selected(object sender, TabControlEventArgs e)
        {
            if (e.TabPageIndex == 0) // Перша вкладка
            {
                RefreshDataGridView(); // Оновлення DataGridView з усіма записами
                dataGridView1.Height = 180; // Зміна висоти DataGridView для першої вкладки
            }
            else if (e.TabPageIndex == 1) // "Всі котельні"
            {
                dataGridView1.DataSource = null;
                var allRecords = records.Select(r => new
                {
                    Місцезнаходження = r.Location,
                    Номер_котла = r.BoilerNumber,
                    Обєкти_опалення = r.HeatingObjects,
                    Дата_початку = r.StartDate,
                    Температура_початку = r.StartTemperature,
                    Дата_кінця = r.EndDate,
                    Тривалість = (r.EndDate - r.StartDate).Days
                }).ToList();
                dataGridView1.DataSource = allRecords;
                FormatDataGridView(); // Применение форматирования после обновления данных
            }
            else if (e.TabPageIndex == 2) // "Сезон пізніше 15 жовтня"
            {
                dataGridView1.DataSource = null;
                var filteredRecords = records
                    .Where(r => r.StartDate > new DateTime(r.StartDate.Year, 10, 15))
                    .Select(r => new
                    {
                        Місцезнаходження = r.Location,
                        Номер_котла = r.BoilerNumber,
                        Обєкти_опалення = r.HeatingObjects,
                        Дата_початку = r.StartDate,
                        Температура_початку = r.StartTemperature,
                        Дата_кінця = r.EndDate,
                        Тривалість = (r.EndDate - r.StartDate).Days
                    }).ToList();
                dataGridView1.DataSource = filteredRecords;
                FormatDataGridView(); // Применение форматирования после обновления данных
            }
            else if (e.TabPageIndex == 3) // "Більше 6 місяців"
            {
                dataGridView1.DataSource = null;
                longSeasonCount = records.Count(r => (r.EndDate - r.StartDate).TotalDays > 180);
                var filteredRecords = records.Where(r => (r.EndDate - r.StartDate).TotalDays > 180)
                    .Select(r => new
                    {
                        Місцезнаходження = r.Location,
                        Номер_котла = r.BoilerNumber,
                        Обєкти_опалення = r.HeatingObjects,
                        Дата_початку = r.StartDate,
                        Температура_початку = r.StartTemperature,
                        Дата_кінця = r.EndDate,
                        Тривалість = (r.EndDate - r.StartDate).Days
                    }).ToList();
                dataGridView1.DataSource = filteredRecords;
                labelCount.Text = $"Кількість котельній з сезоном більше 6 місяців: {longSeasonCount}";
                FormatDataGridView(); // Применение форматирования после обновления данных
            }
            else if (e.TabPageIndex == 4) // "Найкоротший сезон"
            {
                dataGridView1.DataSource = null;
                var minDuration = records.Min(r => (r.EndDate - r.StartDate).Days);
                var filteredRecords = records.Where(r => (r.EndDate - r.StartDate).Days == minDuration)
                                             .Select(r => new
                                             {
                                                 Місцезнаходження = r.Location,
                                                 Номер_котла = r.BoilerNumber,
                                                 Обєкти_опалення = r.HeatingObjects,
                                                 Дата_початку = r.StartDate,
                                                 Температура_початку = r.StartTemperature,
                                                 Дата_кінця = r.EndDate,
                                                 Тривалість = (r.EndDate - r.StartDate).Days
                                             }).ToList();
                dataGridView1.DataSource = filteredRecords;
                FormatDataGridView(); // Применение форматирования после обновления данных
            }
            dataGridView1.Height = (e.TabPageIndex == 0) ? 180 : 340; // Зміна висоти DataGridView для інших вкладок
        }


        private void numericUpDownStartTemperature_ValueChanged(object sender, EventArgs e)
        {
            // Обработчик изменения значения температуры, если потребуется
        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }
    }

    internal class dateTimePickerSearchDate
    {
        internal static object Value;
    }

    public class HeatingSystemRecord
    {
        public string Location { get; set; }
        public int BoilerNumber { get; set; }
        public int HeatingObjects { get; set; }
        public DateTime StartDate { get; set; }
        public double StartTemperature { get; set; }
        public DateTime EndDate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is HeatingSystemRecord other)
            {
                return Location == other.Location &&
                       BoilerNumber == other.BoilerNumber &&
                       HeatingObjects == other.HeatingObjects &&
                       StartDate == other.StartDate &&
                       StartTemperature == other.StartTemperature &&
                       EndDate == other.EndDate;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + Location.GetHashCode();
            hash = hash * 23 + BoilerNumber.GetHashCode();
            hash = hash * 23 + HeatingObjects.GetHashCode();
            hash = hash * 23 + StartDate.GetHashCode();
            hash = hash * 23 + StartTemperature.GetHashCode();
            hash = hash * 23 + EndDate.GetHashCode();
            return hash;
        }
    }
}


