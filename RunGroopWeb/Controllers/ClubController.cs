﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroopWeb.Data;
using RunGroopWeb.Interfaces;
using RunGroopWeb.Models;
using RunGroopWeb.Repository;
using RunGroopWeb.ViewModel;

namespace RunGroopWeb.Controllers
{
	public class ClubController : Controller
	{
        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;
		private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubController(IClubRepository clubRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
			_clubRepository = clubRepository;
			_photoService=photoService;
			_httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
		{
			IEnumerable<Club> clubs=await _clubRepository.GetAll();

            return View(clubs);
		}

		public async Task<IActionResult> Detail(int id)
		{
			Club? club = await _clubRepository.GetByIdAsync(id);
			return View(club);
		}

		public IActionResult Create()
		{
			var curUserId = _httpContextAccessor.HttpContext.User.GetUserId();
			var createClubViewModel = new CreateClubViewModel
			{
				AppUserId = curUserId
			};
			return View(createClubViewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Create(CreateClubViewModel clubVM)
		{
			if (ModelState.IsValid)
			{
				var result = await _photoService.AddPhotoAsync(clubVM.Image);

				var club = new Club
				{
					Title = clubVM.Title,
					Description = clubVM.Description,
					Image = result.Url.ToString(),
					AppUserId=clubVM.AppUserId,
					Address=new Address
					{
						City=clubVM.Address.City,
						State=clubVM.Address.State,
						Street=clubVM.Address.Street,
					}
			};
			_clubRepository.Add(club);
			return RedirectToAction("Index");
		    }
			else
			{
				ModelState.AddModelError("", "Photo upload failed");
			}
			return View(clubVM);
        }

		public async Task<IActionResult> Edit(int id)
		{
			var club = await _clubRepository.GetByIdAsync(id);
			if (club == null) return View("Error");
			var clubVm = new EditClubViewModel
			{
				Title = club.Title,
				Description = club.Description,
				AddressId = (int)club.AddressId,
				Address = club.Address,
				URL = club.Image,
				ClubCategory = club.ClubCategory
			};
			return View(clubVm);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(int id,EditClubViewModel clubMV)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "Failed to edit club");
				return View("Edit", clubMV);
			}

			var userClub = await _clubRepository.GetByIdAsyncNoTracking(id);

			if (userClub!=null)
			{
				try
				{
					await _photoService.DeletePhotoAsync(userClub.Image);
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", "Could not delete photo");
					return View(clubMV);
				}
				var photoResult = await _photoService.AddPhotoAsync(clubMV.Image);

				var club = new Club
				{
					Id = id,
					Title = clubMV.Title,
					Description = clubMV.Description,
					Image = photoResult.Url.ToString(),
					AddressId = clubMV.AddressId,
					Address = clubMV.Address
				};

				_clubRepository.Update(club);

				return RedirectToAction("Index");
			}
			else
			{
				return View(clubMV);
			}
		}

		public async Task<IActionResult> Delete(int id)
		{
			var clubDetails= await _clubRepository.GetByIdAsync(id);
			if (clubDetails==null)
			{
				return View("Error");
			}
			return View(clubDetails);
		}

		[HttpPost, ActionName("Delete")]
		public async Task<IActionResult> DeleteClub(int id)
		{
            var clubDetails = await _clubRepository.GetByIdAsync(id);
            if (clubDetails == null)
            {
                return View("Error");
            }
            _clubRepository.Delete(clubDetails);
			return RedirectToAction("Index");
        }
	}
}
