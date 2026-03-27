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
		var config = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
			.AddEnvironmentVariables()
			.Build();

		var address =
			Environment.GetEnvironmentVariable("AVA_SMENA_GRPC_ADDRESS") ??
			Environment.GetEnvironmentVariable("Grpc__Address") ??
			config["Grpc:Address"] ??
			"http://localhost:5001";
		var apiKey =
			Environment.GetEnvironmentVariable("AVA_SMENA_API_KEY") ??
			Environment.GetEnvironmentVariable("Grpc__ApiKey") ??
			config["Grpc:ApiKey"] ??
			string.Empty;

		var grpcService = new GrpcService(address, apiKey);
		var formCache = new FormCacheService();
		Application.Run(new MainForm(grpcService, formCache));
	}
}
