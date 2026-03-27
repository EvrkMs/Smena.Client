using Microsoft.Extensions.Configuration;
using Smena.Client.Services;
using System;
using System.Windows.Forms;

namespace Smena.Client;

internal static class Program
{
	[STAThread]
	private static void Main()
	{
		Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
		Application.ThreadException += (_, e) =>
		{
			MessageBox.Show(
				$"Непредвиденная ошибка:\n{e.Exception.Message}",
				"Ошибка",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		};
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
		{
			if (e.ExceptionObject is Exception ex)
			{
				MessageBox.Show(
					$"Критическая ошибка:\n{ex.Message}",
					"Ошибка",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}
		};

		var config = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.AddEnvironmentVariables()
			.Build();

		var address =
			Environment.GetEnvironmentVariable("AVA_SMENA_GRPC_ADDRESS") ??
			config["Grpc:Address"] ??
			"http://localhost:5001";
		var apiKey =
			Environment.GetEnvironmentVariable("AVA_SMENA_API_KEY") ??
			config["Grpc:ApiKey"] ??
			string.Empty;

		var grpcService = new GrpcService(address, apiKey);
		var formCache = new FormCacheService();
		Application.Run(new MainForm(grpcService, formCache));
	}
}
