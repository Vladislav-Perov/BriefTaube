using System;
using System.ComponentModel;

namespace BriefTaube
{
    public partial class MainPage : ContentPage
    {
        private FileResult? _selectedFile;
        public MainPage()
        {
            InitializeComponent();
        }

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

                if (stream.CanSeek)
                {
                    StatusLabel.Text = $"{file.FileName} - {stream.Length:N0} байт";
                }
                else
                {
                    StatusLabel.Text = $"{file.FileName} - размер неизвестен";
                }
            }

            catch (Exception ex)
            {
                await DisplayAlertAsync("Не удалось открыть файл", ex.Message, "ОК");
            }
       }
    }
}