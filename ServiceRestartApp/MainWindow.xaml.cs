using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Windows;
using System.Windows.Input;

namespace ServiceRestartApp
{
    public partial class MainWindow : Window
    {
        private List<ServiceInfo> services = new List<ServiceInfo>();

        public MainWindow()
        {
            InitializeComponent();
            LoadServicesFromConfig();
            servicesListView.ItemsSource = services;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }


        private void LoadServicesFromConfig()
        {
            try
            {
                string jsonFilePath = "appsettings.json"; // Путь к вашему JSON-файлу
                string jsonContent = File.ReadAllText(jsonFilePath);

                dynamic json = JsonConvert.DeserializeObject<dynamic>(jsonContent);

                foreach (var serviceItem in json["Services"])
                {
                    string serviceName = serviceItem["ServiceName"];
                    string path = serviceItem["Path"];

                    services.Add(new ServiceInfo
                    {
                        ServiceName = serviceName,
                        Path = path,
                        Status = GetServiceStatus(serviceName)
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading services from config: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetServiceStatus(string serviceName)
        {
            // Получение статуса службы
            ServiceController service = new ServiceController(serviceName);
            return service.Status.ToString();
        }

        private void RestartService_Click(object sender, RoutedEventArgs e)
        {
            if (servicesListView.SelectedItem is ServiceInfo selectedService)
            {
                ServiceController service = new ServiceController(selectedService.ServiceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);
                    
                }
                service.Start();
                selectedService.Status = service.Status.ToString();
            }
        }



        private void StopService_Click(object sender, RoutedEventArgs e)

        {
            if (servicesListView.SelectedItem is ServiceInfo selectedService)
            {
                ServiceController service = new ServiceController(selectedService.ServiceName);
                if (service.Status == ServiceControllerStatus.Running)
                {
                    service.Stop();
                    service.WaitForStatus(ServiceControllerStatus.Stopped);

                    // Обновляем статус сервиса в интерфейсе после его остановки
                    UpdateServiceStatus(selectedService.ServiceName, ServiceControllerStatus.Stopped);

                }
                selectedService.Status = service.Status.ToString();

            }

        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (servicesListView.SelectedItem is ServiceInfo selectedService)
            {
                string folderPath = selectedService.Path;

                if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
                {
                    label1.Content = "";
                    System.Diagnostics.Process.Start(folderPath);
                }
                else
                {
                    label1.Content = "Такого пути не существует";
                }
            }
        }

        private void UpdateServiceStatus(string serviceName, ServiceControllerStatus status)
        {
            // Найдем сервис по имени
            ServiceInfo service = services.FirstOrDefault(s => s.ServiceName == serviceName);

            if (service != null)
            {
                // Обновляем статус сервиса
                service.Status = status.ToString();

                // Обновляем GUI
                UpdateServiceStatusInUI(serviceName, status);
            }
        }

        private void UpdateServiceStatusInUI(string serviceName, ServiceControllerStatus status)
        {
            // Здесь вы можете обновить GUI, например, найти нужный элемент в ListView и обновить его
            // Примерно так:
            foreach (ServiceInfo serviceInfo in servicesListView.Items)
            {
                if (serviceInfo.ServiceName == serviceName)
                {
                    serviceInfo.Status = status.ToString();
                    break;
                }
            }
        }

        private void ClearDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (servicesListView.SelectedItem is ServiceInfo selectedService)
            {
                string directoryPath = selectedService.Path;

                try
                {
                    if (Directory.Exists(directoryPath))
                    {
                        DirectoryInfo directory = new DirectoryInfo(directoryPath);

                        foreach (FileInfo file in directory.GetFiles())
                        {
                            file.Delete();
                        }

                        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
                        {
                            subDirectory.Delete(true);
                        }

                        MessageBox.Show("Содержимое директории успешно удалено.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Директория не существует.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении содержимого директории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        
    }
}
