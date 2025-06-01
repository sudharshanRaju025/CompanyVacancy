using CompanyVacancy.Models;
using Microsoft.AspNetCore.Mvc;
using CompanyVacancy.Helpers;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;


namespace CompanyVacancy.Controllers
{
    public class CompanyDetailsController : Controller
    {
        Sorting Method=new Sorting();

        private readonly AddDbFile _dbContext;

        public CompanyDetailsController(AddDbFile dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Add()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> List(CompanyDetails viewModel)
        {
            
            var results = await _dbContext.CompanyDetails.ToListAsync();
            return View(results);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string query, int page = 1, int pageSize = 4)
        {
            try
            {
                IQueryable<CompanyDetails> companies = _dbContext.CompanyDetails;

                if (!string.IsNullOrWhiteSpace(query))
                {
                    string fetch = SearchQueryParser.ParseToDynamicLinq(query);
                    if (!string.IsNullOrWhiteSpace(fetch))
                    {
                        companies = companies.Where(fetch);
                    }
                }

              
                CompanyDetails[] allResults = await companies.ToArrayAsync();

                
                Sorting.MergeSort(allResults, 0, allResults.Length - 1);

                
                int totalItems = allResults.Length;
                int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedResults = allResults
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;
                ViewBag.Query = query;

                return View(pagedResults);
            }
            catch (Exception ex)
            {
                return BadRequest("Error: " + ex.Message);
            }
        }



    }

}



 



