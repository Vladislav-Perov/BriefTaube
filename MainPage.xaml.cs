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
                using var stream = await file.OpenReadAsync();

                //SHA-256
                string sizeText = stream.CanSeek
                    ? $"{stream.Length:N0} байт"
                    : "размер неизвестен";

                StatusLabel.Text = $"Вычисляю SHA-256 для {file.FileName}...";

                byte[] hashBytes = await SHA256.HashDataAsync(stream);
                string hashText = Convert.ToHexString(hashBytes);

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