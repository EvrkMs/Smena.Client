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
			.Build();

		var address = ResolveGrpcAddress(config);
		var apiKey = ResolveApiKey(config);
		var pathPrefix = config["Grpc:PathPrefix"];

		var grpcService = new GrpcService(address, apiKey, pathPrefix);
		Application.Run(new MainForm(grpcService));
	}

	private static string ResolveGrpcAddress(IConfiguration config)
	{
		return
			config["Grpc:Address"] ??
			Environment.GetEnvironmentVariable("AVA_SMENA_GRPC_ADDRESS") ??
			Environment.GetEnvironmentVariable("Grpc__Address") ??
			BuildGrpcAddressFromEnvironment() ??
			"http://localhost:5001";
	}

	private static string ResolveApiKey(IConfiguration config)
	{
		return
			config["Grpc:ApiKey"] ??
			Environment.GetEnvironmentVariable("API_KEY") ??
			Environment.GetEnvironmentVariable("AVA_SMENA_API_KEY") ??
			string.Empty;
	}

	private static string? BuildGrpcAddressFromEnvironment()
	{
		var primaryDomain = Environment.GetEnvironmentVariable("PRIMARY_DOMAIN");
		var grpcPort = Environment.GetEnvironmentVariable("GRPC_PORT");

		if (string.IsNullOrWhiteSpace(primaryDomain) || string.IsNullOrWhiteSpace(grpcPort))
		{
			return null;
		}

		return $"https://{primaryDomain.Trim()}:{grpcPort.Trim()}";
	}
}
