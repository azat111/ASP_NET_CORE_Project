using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RunGroopWeb.Helpers;
using RunGroopWeb.Interfaces;
using RunGroopWeb.Models;
using RunGroopWeb.ViewModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;

namespace RunGroopWeb.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly IClubRepository _clubRepository;

		public HomeController(ILogger<HomeController> logger, IClubRepository clubRepository)
		{
			_logger = logger;
			_clubRepository = clubRepository;
		}

		public async Task<IActionResult> Index()
		{
			var ipInfo = new IPInfo();
			var homeViewModel = new HomeViewModel();
			try
			{
				string url = "https://ipinfo.io?token=ccfc1656471174";
				var info = new WebClient().DownloadString(url);
				ipInfo = JsonConvert.DeserializeObject<IPInfo>(info);
				RegionInfo myR = new RegionInfo(ipInfo.Country);
				ipInfo.Country = myR.EnglishName;
				homeViewModel.City = ipInfo.City;
				homeViewModel.State = ipInfo.Region;
				if (homeViewModel.City!=null)
				{
					homeViewModel.Clubs = await _clubRepository.GetClubByCity(homeViewModel.City);
				}
				else
				{
					homeViewModel.Clubs = null;
				}
				return View(homeViewModel);
            }
			catch (Exception ex) 
			{
				homeViewModel.Clubs = null;
			}
			return View(homeViewModel);
		}

		public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}