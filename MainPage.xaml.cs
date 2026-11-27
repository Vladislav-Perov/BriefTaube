using System;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Net.Sockets;
using BriefTaube.Core;

namespace BriefTaube
{
    public partial class MainPage : ContentPage
    {
        //Поле для файла
        private FileResult? _selectedFile;

        //Обработчик xaml
        public MainPage()
        {
            //Загрузка xaml-разметки
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

                //! Основные действия по SHA-256(На данный момент пока вычисляет хеш для ознакомнления(инфо))
                //SHA-256 читает файл по частям и вычисляет хеш его содержимого длиной 32 байта
                //После передачи получатель сможет вычислить хеш своей копии и сравнить оба значения
                byte[] hashBytes = await SHA256.HashDataAsync(stream);

                //Для вывода на экран записываем каждый байт двумя шестнадцатеричными символами
                //Весь хеш занимает 64 символа
                string hashText = Convert.ToHexString(hashBytes);

                //Пока шло чтение пользователь мог выбрать другой файл, показываем хеш только для текущего выбора
                if(file == _selectedFile)
                {
                    StatusLabel.Text = $"{file.FileName} - {sizeText}\nSHA-256: {hashText}";
                }
            }

            catch (Exception ex)
            {
                await DisplayAlertAsync("Не удалось открыть файл!", ex.Message, "ОК");
            }
       }

       //Обрабочик кнопки отправки(Пока имени файла)
       private async void OnSendNameClicked(object? sender, EventArgs e)
       {
            var file = _selectedFile;

            if(file is null)
            {
                StatusLabel.Text = "Сначала выберите файл!";
                return;
            }

            try
            {   
                //! Основные махинации с TCP:
                //Создаем объект на стороне отправителя
                using var client = new TcpClient();

                //Запрос на установку TCP-соединения(ConnectAsync)
                await client.ConnectAsync("127.0.0.1", 5055);

                using var stream = client.GetStream();
                await FileNameProtocol.WriteAsync(stream, file.FileName);

                StatusLabel.Text = $"Отправлено имя файла: {file.FileName}";
            }

            catch(SocketException ex)
            {
                await DisplayAlertAsync("Ошибка подключения!", ex.Message, "OK");
            }

            catch(System.IO.IOException ex)
            {
                await DisplayAlertAsync("Ошибка отправки!", ex.Message, "ОК");
            }
       }
    }
}