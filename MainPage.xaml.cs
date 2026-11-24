using System;
using System.ComponentModel;

namespace BriefTaube
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCheckClicked(object? sender, EventArgs e)
        {
            StatusLabel.Text = "Интерфейс работает братишки!";
        }

    }
}