using System.Diagnostics;
using System.ServiceProcess;
using System.Security.Principal;

namespace InterviewAssistant.Service.Utils
{
    /// <summary>
    /// Utility class for installing and uninstalling the Windows Service
    /// </summary>
    public static class ServiceInstaller
    {
        private const string ServiceName = "InterviewAssistant";
        private const string ServiceDisplayName = "Interview Assistant Service";
        private const string ServiceDescription = "Interview Assistant - Captures screenshots and processes them with AI";

        /// <summary>
        /// Install the Windows Service
        /// </summary>
        public static bool InstallService(string servicePath, string? arguments = null)
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("This operation requires administrator privileges.");
                    return false;
                }

                if (ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' already exists.");
                    return true;
                }

                var installUtilPath = GetInstallUtilPath();
                if (string.IsNullOrEmpty(installUtilPath))
                {
                    Console.WriteLine("InstallUtil not found. Please ensure .NET Framework is installed.");
                    return false;
                }

                var command = $"\"{installUtilPath}\" \"{servicePath}\"";
                if (!string.IsNullOrEmpty(arguments))
                {
                    command += $" {arguments}";
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Service installed successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Service installation failed. Exit code: {process.ExitCode}");
                    Console.WriteLine($"Output: {output}");
                    Console.WriteLine($"Error: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error installing service: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Uninstall the Windows Service
        /// </summary>
        public static bool UninstallService()
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("This operation requires administrator privileges.");
                    return false;
                }

                if (!ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' does not exist.");
                    return true;
                }

                var serviceController = new ServiceController(ServiceName);
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    serviceController.Stop();
                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                }

                var installUtilPath = GetInstallUtilPath();
                if (string.IsNullOrEmpty(installUtilPath))
                {
                    Console.WriteLine("InstallUtil not found. Please ensure .NET Framework is installed.");
                    return false;
                }

                var command = $"\"{installUtilPath}\" /u \"{GetServiceExecutablePath()}\"";
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/c {command}",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0)
                {
                    Console.WriteLine("Service uninstalled successfully.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Service uninstallation failed. Exit code: {process.ExitCode}");
                    Console.WriteLine($"Output: {output}");
                    Console.WriteLine($"Error: {error}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uninstalling service: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Start the Windows Service
        /// </summary>
        public static bool StartService()
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("This operation requires administrator privileges.");
                    return false;
                }

                if (!ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' does not exist.");
                    return false;
                }

                var serviceController = new ServiceController(ServiceName);
                if (serviceController.Status == ServiceControllerStatus.Running)
                {
                    Console.WriteLine("Service is already running.");
                    return true;
                }

                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
                Console.WriteLine("Service started successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting service: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stop the Windows Service
        /// </summary>
        public static bool StopService()
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("This operation requires administrator privileges.");
                    return false;
                }

                if (!ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' does not exist.");
                    return true;
                }

                var serviceController = new ServiceController(ServiceName);
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    Console.WriteLine("Service is already stopped.");
                    return true;
                }

                serviceController.Stop();
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));
                Console.WriteLine("Service stopped successfully.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping service: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Check if service exists
        /// </summary>
        public static bool ServiceExists(string serviceName)
        {
            try
            {
                var serviceController = new ServiceController(serviceName);
                var status = serviceController.Status;
                return true;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        /// <summary>
        /// Get service status
        /// </summary>
        public static string GetServiceStatus()
        {
            try
            {
                if (!ServiceExists(ServiceName))
                {
                    return "Not Installed";
                }

                var serviceController = new ServiceController(ServiceName);
                return serviceController.Status.ToString();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        /// <summary>
        /// Check if running as administrator
        /// </summary>
        private static bool IsRunningAsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        /// <summary>
        /// Get InstallUtil path
        /// </summary>
        private static string? GetInstallUtilPath()
        {
            var frameworkPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
            var installUtilPath = Path.Combine(frameworkPath, "Microsoft.NET", "Framework", "v4.0.30319", "InstallUtil.exe");
            
            if (File.Exists(installUtilPath))
            {
                return installUtilPath;
            }

            // Try 64-bit framework
            frameworkPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            installUtilPath = Path.Combine(frameworkPath, "Microsoft.NET", "Framework64", "v4.0.30319", "InstallUtil.exe");
            
            return File.Exists(installUtilPath) ? installUtilPath : null;
        }

        /// <summary>
        /// Get service executable path
        /// </summary>
        private static string GetServiceExecutablePath()
        {
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(currentPath, "InterviewAssistant.Service.exe");
        }

        /// <summary>
        /// Show service information
        /// </summary>
        public static void ShowServiceInfo()
        {
            try
            {
                if (!ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' is not installed.");
                    return;
                }

                var serviceController = new ServiceController(ServiceName);
                Console.WriteLine($"Service Name: {serviceController.ServiceName}");
                Console.WriteLine($"Display Name: {ServiceDisplayName}");
                Console.WriteLine($"Status: {serviceController.Status}");
                Console.WriteLine($"Start Type: {GetStartType(serviceController.StartType)}");
                Console.WriteLine($"Description: {ServiceDescription}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting service information: {ex.Message}");
            }
        }

        /// <summary>
        /// Get start type as string
        /// </summary>
        private static string GetStartType(ServiceStartMode startType)
        {
            return startType switch
            {
                ServiceStartMode.Automatic => "Automatic",
                ServiceStartMode.Manual => "Manual",
                ServiceStartMode.Disabled => "Disabled",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Set service start type
        /// </summary>
        public static bool SetServiceStartType(ServiceStartMode startType)
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    Console.WriteLine("This operation requires administrator privileges.");
                    return false;
                }

                if (!ServiceExists(ServiceName))
                {
                    Console.WriteLine($"Service '{ServiceName}' does not exist.");
                    return false;
                }

                Console.WriteLine("Service start type changes are not supported by this installer helper.");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting service start type: {ex.Message}");
                return false;
            }
        }
    }
}