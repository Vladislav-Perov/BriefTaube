using System;
using System.ComponentModel;
using System.Security.Cryptography;

namespace BriefTaube
{
    public partial class MainPage : ContentPage
    {
        //Поле для файла
        private FileResult? _selectedFile;

        //Обработчик xaml
        public MainPage()
        {
            InitializeComponent();
        }

       //Обработчик кнопки Выбор файла
       private async void OnPickFileClicked(object? sender, EventArgs e)
       {
            try
            {
                FileResult? file = await FilePicker.Default.PickAsync();

                if(file is null)
                {
                    return;
                }

                _selectedFile = file;
                StatusLabel.Text = $"Братиш, выбран файл: {file.FileName}";

            }

            catch (Exception ex)
            {
                await DisplayAlertAsync("Ошибка выбора файла братиш", ex.Message, "ОК");
            }
       }

       //Обработчик кнопки Сведений о файле
       private async void OnFileInfoCliсked(object? sender, EventArgs e)
       {
            var file = _selectedFile;

            if (file is null)
            {
                StatusLabel.Text = "Сначала выбери файл";
                return;
            }

            try
            {
                //Открываем файл для чтения using закроет поток при выходе из блока try
                using var stream = await file.OpenReadAsync();

                //У некоторых потоков размер заранее неизвестен Хеш можно вычислить и в этом случае
                string sizeText = stream.CanSeek
                    ? $"{stream.Length:N0} байт"
                    : "размер неизвестен";

                StatusLabel.Text = $"Вычисляю SHA-256 для {file.FileName}...";

                //SHA-256 читает файл по частям и вычисляет хеш его содержимого длиной 32 байта
                //После передачи получатель сможет вычислить хеш своей копии и сравнить оба значения
                byte[] hashBytes = await SHA256.HashDataAsync(stream);

                // Для вывода на экран записываем каждый байт двумя шестнадцатеричными символами
                // Например, байт со значением 255 записывается как FF. Весь хеш занимает 64 символа
                string hashText = Convert.ToHexString(hashBytes);

                //Пока шло чтение пользователь мог выбрать другой файл, показываем хеш только для текущего выбора
                if(file == _selectedFile)
                {
                    StatusLabel.Text = $"{file.FileName} - {sizeText}\nSHA-256: {hashText}";
                }
            }

            catch (Exception ex)
            {
                await DisplayAlertAsync("Не удалось открыть файл", ex.Message, "ОК");
            }
       }
    }
}