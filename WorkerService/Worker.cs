using System.Diagnostics;
using WorkerService.DTO;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;
        private Settings? _settings;    

        public event EventHandler? Shutdown;

        /// <summary>
        /// Construtor do Worker
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Método quando o serviço é iniciado
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker started at: {time}", DateTimeOffset.Now);

            SetSettings();

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await Task.Delay(1000, stoppingToken);

                try
                {
                    IsProcessRunnig();

                    if (!_settings.IsRunning)
                        ProcessStart();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred.");
                }
            }
        }

        /// <summary>
        /// Método quando o serviço é parado
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Worker stopped at: {time}", DateTimeOffset.Now);

            await DeleteLicense();

            OnShutdown(EventArgs.Empty);
            await base.StopAsync(cancellationToken);
        }

        /// <summary>
        /// Carrega as configurações do appsettings.json
        /// </summary>
        private void SetSettings()
        {
            _settings = new Settings();
            _configuration.GetSection("WorkerService").Bind(_settings);
        }


        #region Processo
        /// <summary>
        /// Verifica se o processo está rodando
        /// </summary>
        private void IsProcessRunnig()
        {
            var process = Process.GetProcessesByName(_settings.Process).FirstOrDefault();

            _settings.IsRunning = process != null ? true : false;

            if (_settings.IsRunning)
            {
                _logger.LogInformation($"{_settings.Process} está rodando.");
            }
            else
            {
                _logger.LogInformation($"{_settings.Process} não está rodando.");
            }
        }

        /// <summary>
        /// Inicia o processo
        /// </summary>
        private void ProcessStart()
        {
            Process.Start(_settings.Executable);
            _logger.LogInformation($"{_settings.Process} foi iniciado.");
        }

        /// <summary>
        /// Lista todos os processos rodando
        /// </summary>
        private static void ListProcesses()
        {
            Process[] allProcesses = Process.GetProcesses();
            Console.WriteLine("List of all running processes:");
            foreach (var pc in allProcesses)
            {
                Console.WriteLine($"Process Name: {pc.ProcessName}, Process ID: {pc.Id}");
            }
        }

        /// <summary>
        /// Mata o processo
        /// </summary>
        /// <returns></returns>
        private async Task KillProcess()
        {
            IsProcessRunnig();

            if (_settings.IsRunning)
            {
                var process = Process.GetProcessesByName(_settings.Process).FirstOrDefault();
                process?.Kill();
                _logger.LogInformation("Kill Process: {time}", DateTimeOffset.Now);
            }

            await Task.CompletedTask;
        }

        #endregion

        #region Licença
        /// <summary>
        /// Deleta a licença
        /// </summary>
        /// <returns></returns>
        private async Task DeleteLicense()
        {
            _logger.LogInformation("Kill license: {time}", DateTimeOffset.Now);
            await Task.Delay(1000);
        }

        /// <summary>
        /// Verifica se a licença existe
        /// </summary>
        /// <returns></returns>
        private async Task LicenseExists()
        {
            _logger.LogInformation("todo ... {time}", DateTimeOffset.Now);
            await Task.Delay(1000);
        }

        #endregion

        protected virtual void OnShutdown(EventArgs e)
        {
            Shutdown?.Invoke(this, e);
        }

        public async override void Dispose()
        {
            await KillProcess();
            _logger.LogInformation("Worker disposed at: {time}", DateTimeOffset.Now);
            base.Dispose();
        }
    }
}
